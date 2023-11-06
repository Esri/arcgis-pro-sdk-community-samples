using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Core.Geometry;
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Element = ArcGIS.Core.Data.UtilityNetwork.Element;

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
  /// This class is used when someone wants to get a feature that exists
  /// inside a warehouse and move it to a location clicked on the map.
  /// </summary>
  internal class MoveFromWarehouseTool : MapTool
  {
    MapView _activeMap = null;
    Dictionary<string, string> _WarehouseNames = null;
    FeatureLayer _featureLayer = null;
    Dictionary<Element, string> _elementList = null;

    public MoveFromWarehouseTool()
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Point;
      SketchOutputMode = SketchOutputMode.Map;
    }

    protected override Task OnToolActivateAsync(bool active)
    {
      return base.OnToolActivateAsync(active);
    }

    protected async override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      // pop up a dialog that lets the user choose a warehouse and then a feature in it
      // remove the containment association between the chosen feature and the warehouse
      // move the chosen feature to the geometry where the user clicked on the map
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
          ReadOnlyObservableCollection<Layer> layers = _activeMap.Map.Layers;
          foreach (Layer layer in layers)
          {
            if (layer is FeatureLayer featureLayer)
            {

              using (FeatureClass featureClass = featureLayer.GetFeatureClass())
              {
                using (UtilityNetwork utilityNetwork = Utilities.GetUtilityNetwork(featureClass))
                {
                  if (utilityNetwork == null)
                    continue;

                  if (Utilities._WarehouseNames.Count == 0)
                  {
                    Utilities.GetWarehouseNames(utilityNetwork);
                    _WarehouseNames = Utilities._WarehouseNames;
                  }
                  else
                  {
                    _WarehouseNames = Utilities._WarehouseNames;
                  }
                  _elementList = GetElements(utilityNetwork);
                  _featureLayer = featureLayer;
                  return;
                }
              }
            }
            else if (layer is SubtypeGroupLayer subtypeGroupLayer)
            {
              IReadOnlyList<Layer> subtypeLayers = subtypeGroupLayer.GetLayersAsFlattenedList();
              foreach (Layer subtypeLayer in subtypeLayers)
              {
                if (subtypeLayer is FeatureLayer featureLayer2)
                {

                  using (FeatureClass featureClass = featureLayer2.GetFeatureClass())
                  {
                    using (UtilityNetwork utilityNetwork = Utilities.GetUtilityNetwork(featureClass))
                    {
                      if (utilityNetwork == null)
                        continue;

                      if (Utilities._WarehouseNames.Count == 0)
                      {
                        Utilities.GetWarehouseNames(utilityNetwork);
                        _WarehouseNames = Utilities._WarehouseNames;
                      }
                      else
                      {
                        _WarehouseNames = Utilities._WarehouseNames;
                      }
                      _elementList = GetElements(utilityNetwork);
                      _featureLayer = featureLayer2;
                      return;
                    }
                  }
                }
              }
            }
          }


        }
        catch
        {
          MessageBox.Show("failed in building initail lists");
        }
      });

      // Open the dialog so the user can choose an existing warehouse & feature
      SelectWarehouseFeature selectWarehouseFeature = new SelectWarehouseFeature(_elementList);
      {
        selectWarehouseFeature.Owner = FrameworkApplication.Current.MainWindow;
      };
      selectWarehouseFeature.ShowDialog();


      if (selectWarehouseFeature.ChosenFeature.Count == 0)
      {
        return false;
      }

      using (ProgressDialog progressDialog = new ProgressDialog("Performing move"))
      {
        string errorMessage = string.Empty;

        progressDialog.Show();

        await QueuedTask.Run(() =>
        {
          UtilityNetwork utilityNetwork = Utilities.GetUtilityNetwork(_featureLayer.GetFeatureClass());

          // delete association between selected feature and warehouse
          Utilities.DeleteAssociations(utilityNetwork, selectWarehouseFeature.ChosenFeature.Keys.First(), _featureLayer.GetFeatureClass());

          // move geometry
          MoveGeometry(selectWarehouseFeature.ChosenFeature.Keys.First(), utilityNetwork, geometry);

        });
      }


      return true;
    }

    private async void MoveGeometry(Element element, UtilityNetwork utilityNetwork, Geometry geometry)
    {
      // set geometry of selected feature to that of the location clicked on the map
      await QueuedTask.Run(() =>
      {
        QueryFilter filter = new();
        filter.WhereClause = "OBJECTID = " + element.ObjectID;
        Table table = utilityNetwork.GetTable(element.NetworkSource);

        using (RowCursor rowCursor = table.Search(filter, false))
        {
          while (rowCursor.MoveNext())
          {
            using (Row row = rowCursor.Current)
            {
              Feature pointFeature = (Feature)row;

              EditOperation editOperation = new EditOperation()
              {
                Name = "move feature",
              };
              string errorMessage = "";
              editOperation.Callback((context) =>
                    {
                        try
                        {
                          pointFeature.SetShape(geometry);
                          pointFeature.Store();
                          context.Invalidate(pointFeature);
                        }
                        catch (Exception e)
                        {
                          errorMessage = $"Exception: {e.Message}";
                        }
                      }, _featureLayer);

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

    // Create a dictionary of features contained in each warehouse
    private Dictionary<Element, string> GetElements(UtilityNetwork utilityNetwork)
    {
      Dictionary<Element, string> elementList = new();

      // for each warehouse, get all content and load dictionary
      var utilityNetworkDefinition = utilityNetwork.GetDefinition();

      foreach (KeyValuePair<string, string> warehouse in _WarehouseNames)
      {
        NetworkSource networkSource = utilityNetworkDefinition.GetNetworkSource(warehouse.Value);

        QueryFilter filter = new QueryFilter();
        filter.WhereClause = "Name = '" + warehouse.Key + "'";
        Table table = utilityNetwork.GetTable(networkSource);

        using (RowCursor rowCursor = table.Search(filter, false))
        {
          while (rowCursor.MoveNext())
          {
            using (Row row = rowCursor.Current)
            {
              Element warehouseElement = utilityNetwork.CreateElement(row);
              IReadOnlyList<Association> assocations = utilityNetwork.GetAssociations(warehouseElement, AssociationType.Containment);
              foreach (Association association in assocations)
              {
                if (association.ToElement != warehouseElement)
                {
                  elementList.Add(association.ToElement, warehouse.Key);
                }
              }
            }
          }
        }
      }

      return elementList;
    }

  }

}
