using ActiproSoftware.Windows.Controls;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Element = ArcGIS.Core.Data.UtilityNetwork.Element;
using Geometry = ArcGIS.Core.Geometry.Geometry;
using NetworkSource = ArcGIS.Core.Data.UtilityNetwork.NetworkSource;
using QueryFilter = ArcGIS.Core.Data.QueryFilter;

namespace ToFromWarehouse
{
  //   Copyright 2019 Esri
  //   Licensed under the Apache License, Version 2.0 (the "License");
  //   you may not use this file except in compliance with the License.
  //   You may obtain a copy of the License at

  //       https://www.apache.org/licenses/LICENSE-2.0

  //   Unless required by applicable law or agreed to in writing, software
  //   distributed under the License is distributed on an "AS IS" BASIS,
  //   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  //   See the License for the specific language governing permissions and
  //   limitations under the License.

  /// <summary>
  /// This class is used when someone wants to click on a feature and
  /// have it moved to a warehouse.
  /// </summary>
  internal class MoveToWarehouseTool : MapTool
  {
    Dictionary<string, string> _WarehouseNames = new();
    FeatureLayer _pointFeatureLayer = null;
    Element _pointElement = null;
    Element _warehouseElement = null;
    MapView _activeMap = null;

    public MoveToWarehouseTool()
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Point;
      SketchOutputMode = SketchOutputMode.Map;
      Cursor = Cursors.Cross;
    }

    protected override Task OnToolActivateAsync(bool active)
    {
      return base.OnToolActivateAsync(active);
    }

    protected async override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      // make sure they clicked on a UN class and feature
      // popup a dialog that will let them choose a warehouse from a list
      // > list is based on features that have the correct category "Warehouse"
      // get all clicked feature's associations and delete them
      // Move the features geometry to match that of the warehouse feature chosen
      // Create new containment association between the clicked feature and the warehouse feature
      MapView activeMap = MapView.Active;
      if (activeMap == null)
      {
        // Shouldn't happen
        MessageBox.Show("No active map");
        return false;
      }
      _activeMap = activeMap;

      await QueuedTask.Run(() =>
      {
        try
        {
          SelectionSet featureSelections = activeMap.GetFeaturesEx(geometry);
          Dictionary<MapMember, List<long>> featureLayerToObjectIDMappings = featureSelections.ToDictionary();
          if (featureLayerToObjectIDMappings.Count == 0)
          {
            MessageBox.Show("Select a utility network feature.");
            return;
          }

          foreach (KeyValuePair<MapMember, List<long>> featureLayerToObjectIDMapping in featureLayerToObjectIDMappings)
          {
            if (featureLayerToObjectIDMapping.Key is FeatureLayer featureLayer)
            {
              using (FeatureClass featureClass = featureLayer.GetFeatureClass())
              {
                // Make sure this is a point feature class
                if (!Utilities.IsDesiredShapeType(featureClass, GeometryType.Point))
                  continue;

                using (UtilityNetwork utilityNetwork = Utilities.GetUtilityNetwork(featureClass))
                {
                  if (utilityNetwork == null)
                    continue;

                  // Make sure the the feature class is the Device or Junction class in the utility network
                  if (!IsPointNetworkSource(utilityNetwork, featureClass.GetName()))
                    continue;

                  if (featureLayerToObjectIDMapping.Value.Count > 1)
                  {
                    MessageBox.Show("Select only one utility network point feature.");
                    return;
                  }

                  GetPointElement(utilityNetwork, featureClass, featureLayerToObjectIDMapping.Value[0], out Element pointElement);
                  if (Utilities._WarehouseNames.Count == 0)
                  {
                    Utilities.GetWarehouseNames(utilityNetwork);
                    _WarehouseNames = Utilities._WarehouseNames;
                  }
                  else
                  {
                    _WarehouseNames = Utilities._WarehouseNames;
                  }

                  if (_WarehouseNames.Count == 0)
                  {
                    MessageBox.Show("No warehouse features found, possibly missing the Name field.");
                    return;
                  }

                  _pointFeatureLayer = featureLayer;
                  _pointElement = pointElement;
                }
              }
            }
          }
          if (_pointFeatureLayer == null)
          {
            MessageBox.Show("Select a utility network point feature from the Device or Junction class.");
          }

        }
        catch (Exception e)
        {
          MessageBox.Show($"Exception: {e.Message}");
        }
      });

      // Open the dialog so the user can choose an existing warehouse feature
      SelectWarehouse selectWarehouse = new(_WarehouseNames);
      {
        selectWarehouse.Owner = FrameworkApplication.Current.MainWindow;
      };
      selectWarehouse.ShowDialog();

      if (selectWarehouse.ChosenWarehouse.Count == 0)
      {
        return false;
      }

      using (ProgressDialog progressDialog = new ProgressDialog("Performing move"))
      {
        string errorMessage = string.Empty;

        progressDialog.Show();

        await QueuedTask.Run(() =>
        {
          UtilityNetwork utilityNetwork = Utilities.GetUtilityNetwork(_pointFeatureLayer.GetFeatureClass());

          // delete associations for the selected feature
          Utilities.DeleteAssociations(utilityNetwork, _pointElement, _pointFeatureLayer.GetFeatureClass());

          // move geometry
          MoveGeometry(selectWarehouse.ChosenWarehouse, utilityNetwork);

          // add containment to the warehouse
          AddContainment(utilityNetwork);

        });
      }

      return true;
    }



    private void AddContainment(UtilityNetwork utilityNetwork)
    {
      Association association = new Association(AssociationType.Containment, _warehouseElement, _pointElement);

      EditOperation editOperation = new EditOperation()
      {
        Name = "add association",
      };
      string errorMessage = "";
      editOperation.Callback((context) =>
      {
        try
        {
          utilityNetwork.AddAssociation(association);
        }
        catch (Exception e)
        {
          errorMessage = $"Exception: {e.Message}";
        }
      }, _pointFeatureLayer);

      if (editOperation.IsEmpty || !editOperation.Execute())
      {
        return;
      }
      if (errorMessage != "")
      {
        MessageBox.Show(errorMessage);
      }
    }

    private async void MoveGeometry(ObservableCollection<string> ChosenWarehouse, UtilityNetwork utilityNetwork)
    {
      // get geometry of chosen warehouse
      string warehouseName = ChosenWarehouse.First<string>();
      Geometry warehouseGeometry = null;

      _WarehouseNames.TryGetValue(warehouseName, out string warehouseNetworkSource);

      var utilityNetworkDefinition = utilityNetwork.GetDefinition();
      NetworkSource networkSource = utilityNetworkDefinition.GetNetworkSource(warehouseNetworkSource);

      // query for the chosen warehouse to get the feature
      var count = await QueuedTask.Run(() =>
      {
        QueryFilter filter = new QueryFilter();
        filter.WhereClause = "Name = '" + warehouseName + "'";
        Table table = utilityNetwork.GetTable(networkSource);

        using (RowCursor rowCursor = table.Search(filter, false))
        {
          while (rowCursor.MoveNext())
          {
            using (Row row = rowCursor.Current)
            {
              Feature warehouseFeature = (Feature)row;
              warehouseGeometry = warehouseFeature.GetShape();

              Element warehouseElement = utilityNetwork.CreateElement(row);
              _warehouseElement = warehouseElement;
            }
          }
        }
        return 1;
      });

      // set geometry of clicked feature to that of the warehouse
      await QueuedTask.Run(() =>
      {
        QueryFilter filter = new QueryFilter();
        filter.WhereClause = "OBJECTID = " + _pointElement.ObjectID;
        Table table = utilityNetwork.GetTable(_pointElement.NetworkSource);

        using (RowCursor rowCursor = table.Search(filter, false))
        {
          while (rowCursor.MoveNext())
          {
            using (Row row = rowCursor.Current)
            {
              Feature pointFeature = (Feature)row;
              MapPoint mapPoint = GeometryEngine.Instance.Centroid(warehouseGeometry);

              // check to make sure we aren't stacking features on top of each other
              while (_activeMap.GetFeaturesEx(mapPoint).Count > 1)
              {
                MapPoint testMapPoint = MapPointBuilderEx.CreateMapPoint(mapPoint.X + 1, mapPoint.Y + 1, mapPoint.SpatialReference);
                mapPoint = testMapPoint;
              }
              MapPoint mapPointNew = MapPointBuilderEx.CreateMapPoint(mapPoint.X, mapPoint.Y, 0, mapPoint.SpatialReference);

              EditOperation editOperation = new EditOperation()
              {
                Name = "move feature",
              };
              string errorMessage = "";
              editOperation.Callback((context) =>
                    {
                        try
                        {
                          pointFeature.SetShape(mapPointNew);
                          pointFeature.Store();
                          context.Invalidate(pointFeature);
                        }
                        catch (Exception e)
                        {
                          errorMessage = $"Exception: {e.Message}";
                        }
                      }, _pointFeatureLayer);

              if (editOperation.IsEmpty || !editOperation.Execute())
              {
                //return;
              }
              if (errorMessage != "")
              {
                MessageBox.Show(errorMessage);
              }
            }
          }
        }
        return 1;
      });
    }

    private void GetPointElement(UtilityNetwork utilityNetwork, FeatureClass pointFeatureClass, long pointObjectID, out Element point)
    {
      point = null;
      QueryFilter queryFilter = new QueryFilter()
      {
        ObjectIDs = new List<long>() { pointObjectID }
      };

      using (RowCursor rowCursor = pointFeatureClass.Search(queryFilter, false))
      {
        if (!rowCursor.MoveNext())
        {
          return;
        }

        using (Row row = rowCursor.Current)
        {
          try
          {
            point = utilityNetwork.CreateElement(row);
          }
          catch (Exception e)
          {
            MessageBox.Show(e.Message);
            return;
          }
        }
      }
    }

    private bool IsPointNetworkSource(UtilityNetwork utilityNetwork, string featureClassName)
    {
      try
      {
        // This is inside a try/catch because the feature class may not be a network source (e.g. service territory).

        using (UtilityNetworkDefinition utilityNetworkDefinition = utilityNetwork.GetDefinition())
        using (NetworkSource networkSource = utilityNetworkDefinition.GetNetworkSource(featureClassName))
        {
          if (networkSource.UsageType == SourceUsageType.Device || networkSource.UsageType == SourceUsageType.Junction)
          {
            return true;
          }
          else
          {
            return false;
          }
        }
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message);
        return false;
      }
    }

  }
}
