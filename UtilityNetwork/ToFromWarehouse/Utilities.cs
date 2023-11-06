using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
  /// This class contains generic functions that are used by the 
  /// MoveFromWarehouseTool and MoveToWarehouseTool classes.
  /// </summary>
  internal class Utilities
  {
    // Change this string if you want to find a different Category
    internal static string _warehouseString = "Warehouse";
    // Change this string if the field that you want to use to show warehouse names is not 'Name'
    internal static string _warehouseNameField = "Name";

    internal static Dictionary<string, string> _WarehouseNames = new Dictionary<string, string>();

    // Generic function to check if a feature class definitions shape type matches an expected shape type
    internal static bool IsDesiredShapeType(FeatureClass featureClass, GeometryType desiredShapeType)
    {
      using (FeatureClassDefinition featureClassDefinition = featureClass.GetDefinition())
      {
        return featureClassDefinition.GetShapeType() == desiredShapeType;
      }
    }

    // Retrieve the utility network from a feature class
    internal static UtilityNetwork GetUtilityNetwork(FeatureClass featureClass)
    {
      IReadOnlyList<Dataset> controllerDatasets = featureClass.GetControllerDatasets();

      bool foundUtilityNetwork = false;

      UtilityNetwork utilityNetwork = null;

      // Make sure to dispose all non-utility network controller datasets.

      foreach (Dataset dataset in controllerDatasets)
      {
        if (!foundUtilityNetwork && dataset.Type == DatasetType.UtilityNetwork && dataset is UtilityNetwork)
        {
          foundUtilityNetwork = true;
          utilityNetwork = dataset as UtilityNetwork;
        }
        else
        {
          dataset.Dispose();
        }
      }

      return utilityNetwork;
    }

    // Delete all associations for an element
    internal static bool DeleteAssociations(UtilityNetwork utilityNetwork, Element pointElement, FeatureClass featureClass)
    {
      IReadOnlyList<Association> associations = utilityNetwork.GetAssociations(pointElement);

      if (associations.Count > 0)
      {
        EditOperation editOperation = new EditOperation()
        {
          Name = "remove associations",
        };
        string errorMessage = "";
        editOperation.Callback((context) =>
        {
          try
          {
            foreach (Association association in associations)
            {
              utilityNetwork.DeleteAssociation(association);
            }
          }
          catch (Exception e)
          {
            errorMessage = $"Exception: {e.Message}";
          }
        }, featureClass);

        if (editOperation.IsEmpty || !editOperation.Execute())
        {
          return false;
        }

        if (errorMessage != "")
        {
          MessageBox.Show(errorMessage);
          return false;
        }

      }
      return true;
    }

    // Loop through the utility network configuration to find asset types that are assigned the Warehouse category
    // For each asset type found, query the table and add any warehouse names found to a dictionary
    internal static async void GetWarehouseNames(UtilityNetwork utilityNetwork)
    {
      bool found = false;
      Dictionary<string, string> warehouseNames = new Dictionary<string, string>();
      var utilityNetworkDefinition = utilityNetwork.GetDefinition();
      IReadOnlyList<string> categories = utilityNetworkDefinition.GetAvailableCategories();

      foreach (string category in categories)
      {
        if (category == _warehouseString)
        {
          found = true;
        }
      }

      if (found == false)
      {
        MessageBox.Show("Need to add Warehouse to utility network categories.");
      }

      IReadOnlyList<NetworkSource> networkSources = utilityNetworkDefinition.GetNetworkSources();

      foreach (NetworkSource networkSource in networkSources)
      {
        IReadOnlyList<AssetGroup> assetGroups = networkSource.GetAssetGroups();

        foreach (AssetGroup assetGroup in assetGroups)
        {
          IReadOnlyList<AssetType> assetTypes = assetGroup.GetAssetTypes();

          foreach (AssetType assetType in assetTypes)
          {
            IReadOnlyList<string> assignedCategoryList = assetType.CategoryList;
            foreach (string assignedCategory in assignedCategoryList)
            {
              if (assignedCategory == _warehouseString)
              {
                var count = await QueuedTask.Run(() =>
                {
                  QueryFilter filter = new QueryFilter();
                  filter.WhereClause = "ASSETTYPE = " + assetType.Code;
                  Table table = utilityNetwork.GetTable(networkSource);

                  using (RowCursor rowCursor = table.Search(filter, false))
                  {
                    while (rowCursor.MoveNext())
                    {
                      using (Row row = rowCursor.Current)
                      {
                        if (row.FindField(_warehouseNameField) != -1)
                        {
                          if (Convert.ToString(row[_warehouseNameField]) != "")
                          {
                            warehouseNames.Add(Convert.ToString(row[_warehouseNameField]), networkSource.Name);
                          }
                        }
                      }
                    }
                  }
                  return 1;
                });
              }
            }
          }
        }
      }

      _WarehouseNames = warehouseNames;
    }


  }
}
