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
using ArcGIS.Desktop.Internal.Mapping.PropertyPages;
using ArcGIS.Desktop.Internal.Mapping.Symbology;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Element = ArcGIS.Core.Data.UtilityNetwork.Element;


namespace ConfigurationPathsAddIn
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
  internal class ConfigurationPathTool : MapTool
  {
    MapView _activeMap = null;
    FeatureLayer _pointFeatureLayer = null;
    Element _pointElement = null;
    List<string> _configurationPaths = null;
    string _currentValue = "";

    public ConfigurationPathTool()
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
      // Make sure the selected feature is from the Device or Junction Object class
      // Make sure the selected feature is configured with a terminal configration
      // that has > 1 path
      bool featureFound = false;
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
              using FeatureClass featureClass = featureLayer.GetFeatureClass();
              // Make sure this is a point feature class
              if (!Utilities.IsDesiredShapeType(featureClass, GeometryType.Point))
                continue;

              using UtilityNetwork utilityNetwork = Utilities.GetUtilityNetwork(featureClass);
              if (utilityNetwork == null)
                continue;

              // Make sure the the feature class is the Device or Junction object class in the utility network
              if (!IsPointNetworkSource(utilityNetwork, featureClass.GetName()))
                continue;

              if (featureLayerToObjectIDMapping.Value.Count > 1)
              {
                MessageBox.Show("Select only one utility network point feature.");
                return;
              }

              GetPointElement(utilityNetwork, featureClass, featureLayerToObjectIDMapping.Value[0], out Element pointElement);

              // Make sure that the feature has a terminal configuration with > 1 paths available
              if (CheckTerminalConfiguration(utilityNetwork, pointElement) == true)
              {
                // Get the current path value from the selected feature
                GetCurrentValue(utilityNetwork, pointElement);
                featureFound = true;
              }
              else
              {
                MessageBox.Show("Please select a feature that is configured with a terminal configuration that includes paths.");
              }

              _pointFeatureLayer = featureLayer;
              _pointElement = pointElement;
            }
          }
          if (_pointFeatureLayer == null)
          {
            MessageBox.Show("Select a utility network point feature from the Device or Junction object class.");
            return;
          }

        }
        catch (Exception e)
        {
          MessageBox.Show($"Exception: {e.Message}");
          return;
        }
      });

      if (featureFound == true)
      {
        // Open the dialog so the user can select a configuration path
        ConfigurationPaths configurationPath = new ConfigurationPaths(_currentValue, _configurationPaths);
        {
          configurationPath.Owner = FrameworkApplication.Current.MainWindow;
        };
        configurationPath.ShowDialog();


        using ProgressDialog progressDialog = new ProgressDialog("Performing configuration path change");
        string errorMessage = string.Empty;

        progressDialog.Show();

        await QueuedTask.Run(async () =>
        {
          if (configurationPath.ChosenConfiguration != null && configurationPath.ChosenConfiguration.FirstOrDefault().ToString() != "")
          {
            UtilityNetwork utilityNetwork = Utilities.GetUtilityNetwork(_pointFeatureLayer.GetFeatureClass());

            // get the chosen configuration path value and set it on the selected feature
            string newValue = configurationPath.ChosenConfiguration.FirstOrDefault();
            SetConfigurationValue(utilityNetwork, _pointElement.NetworkSource, _pointElement, newValue);

            // save and validate
            ValidateNetwork(utilityNetwork, _activeMap.Extent);
            await Project.Current.SaveEditsAsync();
          }

        });
      }

      _pointFeatureLayer = null;
      _pointElement = null;
      _configurationPaths = null;
      _currentValue = "";
      featureFound = false;

      return true;
    }

    private static void ValidateNetwork(UtilityNetwork utilityNetwork, Geometry extent)
    {
      utilityNetwork.ValidateNetworkTopologyInEditOperation(extent);
    }

    private async void SetConfigurationValue(UtilityNetwork utilityNetwork, NetworkSource networkSource, Element element, string newValue)
    {
      // set the TerminalConfiguration field value of clicked feature to the newvalue            
      await QueuedTask.Run(() =>
      {
        QueryFilter filter = new()
        {
          WhereClause = "OBJECTID = " + element.ObjectID
        };
        Table table = utilityNetwork.GetTable(networkSource);

        using RowCursor rowCursor = table.Search(filter, false);
        while (rowCursor.MoveNext())
        {
          using Row row = rowCursor.Current;
          EditOperation editOperation = new()
          {
            Name = "set new feature value",
          };
          string errorMessage = "";
          editOperation.Callback((context) =>
          {
            try
            {
              int intFieldPosition = row.FindField("TERMINALCONFIGURATION");
              row[intFieldPosition] = newValue;
              row.Store();
              context.Invalidate(row);
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
        return 1;
      });
    }

    private async void QueryFeature(UtilityNetwork utilityNetwork, NetworkSource networkSource, long objectID, string fieldName)
    {
      var count = await QueuedTask.Run(() =>
      {
        QueryFilter filter = new()
        {
          WhereClause = "OBJECTID = " + objectID
        };
        Table table = utilityNetwork.GetTable(networkSource);

        using RowCursor rowCursor = table.Search(filter, false);
        while (rowCursor.MoveNext())
        {
          using Row row = rowCursor.Current;
          if (row.FindField(fieldName) != -1)
          {
            if (Convert.ToString(row[fieldName]) != "")
            {
              _currentValue = Convert.ToString(row[fieldName]);
            }
          }
        }
        return 1;
      });
    }

    private void GetCurrentValue(UtilityNetwork utilityNetwork, Element pointElement)
    {
      // query the feature and return the value from the terminalconfiguration field
      QueryFeature(utilityNetwork, pointElement.NetworkSource, pointElement.ObjectID, "TERMINALCONFIGURATION");
    }

    private bool CheckTerminalConfiguration(UtilityNetwork utilityNetwork, Element pointElement)
    {
      TerminalConfiguration terminalConfiguration = pointElement.AssetType.GetTerminalConfiguration();
      IReadOnlyList<ConfigurationPath> configurationPaths = terminalConfiguration.ValidConfigurationPaths;
      if (configurationPaths.Count == 0)
      {
        return false;
      }
      else
      {
        List<string> pathList = new List<string>();
        foreach (ConfigurationPath configurationPath in configurationPaths)
        {
          pathList.Add(configurationPath.Name);
        }
        _configurationPaths = new List<string>();
        _configurationPaths = pathList;
        return true;
      }
    }

    private static void GetPointElement(UtilityNetwork utilityNetwork, FeatureClass pointFeatureClass, long pointObjectID, out Element point)
    {
      point = null;
      QueryFilter queryFilter = new QueryFilter()
      {
        ObjectIDs = new List<long>() { pointObjectID }
      };

      using RowCursor rowCursor = pointFeatureClass.Search(queryFilter, false);
      if (!rowCursor.MoveNext())
      {
        return;
      }

      using Row row = rowCursor.Current;
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

    private static bool IsPointNetworkSource(UtilityNetwork utilityNetwork, string featureClassName)
    {
      try
      {
        // This is inside a try/catch because the feature class may not be a network source (e.g. service territory).
        using UtilityNetworkDefinition utilityNetworkDefinition = utilityNetwork.GetDefinition();
        using NetworkSource networkSource = utilityNetworkDefinition.GetNetworkSource(featureClassName);
        if (networkSource.UsageType == SourceUsageType.Device || networkSource.UsageType == SourceUsageType.JunctionObject)
        {
          return true;
        }
        else
        {
          return false;
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
