/*

   Copyright 2018 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork;

using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Geoprocessing;
using System.Windows.Input;
using UtilityNetworkSamples;


namespace CategoriesUsage
{
  /// <summary>
  /// Represents the ComboBox
  /// </summary>
  internal class CategoriesComboBox : ComboBox
  {


    /// <summary>
    /// The selected utility network-based layer in the TOC
    /// </summary>
    private Layer myLayer;

    /// <summary>
    /// Categories Combo Box constructor
    /// </summary>
    public CategoriesComboBox()
    {
      Enabled = false;

      // Subscribe to the table of contents selection changed event.  
      // This allows us to populate the combo box based on the selected layer
      TOCSelectionChangedEvent.Subscribe(UpdateCategoryList);
    }


    /// <summary>
    /// This method makes sure
    /// 1. The Mapview is Active
    /// 2. There is at least one layer selected
    /// 3. That layer is either
    ///   a. A utility network layer
    ///   b. A feature layer whose feature class belongs to a utility network
    ///   c. A subtype group layer whose feature class belongs to a utility network
    /// 
    /// If all of these hold true, we populate the combo box with the list of categories that are registered with this utility network
    /// </summary>
    private async void UpdateCategoryList(MapViewEventArgs mapViewEventArgs)
    {
      // Verify that the map view is active and at least one layer is selected
      if (MapView.Active == null ||
        mapViewEventArgs.MapView.GetSelectedLayers().Count < 1)
      {
        Enabled = false;
        return;
      }

      // Verify that we have the correct kind of layer
      Layer selectedLayer = mapViewEventArgs.MapView.GetSelectedLayers()[0];
      if (!(selectedLayer is UtilityNetworkLayer) && !(selectedLayer is FeatureLayer) && !(selectedLayer is SubtypeGroupLayer))
      {
        Enabled = false;
        return;
      }

      // Switch to the MCT to access the geodatabase
      await QueuedTask.Run(() =>
      {
        // Get the utility network from the layer.  
        // It's possible that the layer is a FeatureLayer or SubtypeGroupLayer that doesn't refer to a utility network at all.
        using (UtilityNetwork utilityNetwork = UtilityNetworkUtils.GetUtilityNetworkFromLayer(selectedLayer))
        {
          if (utilityNetwork == null)
          {
            Enabled = false;
            return;
          }

          // Enable the combo box and clear out its contents
          Enabled = true;
          Clear();

          // Fill the combo box with all of the categories in the utility network
          using (UtilityNetworkDefinition utilityNetworkDefinition = utilityNetwork.GetDefinition())
          {
            IReadOnlyList<string> categories = utilityNetworkDefinition.GetAvailableCategories();
            foreach (string category in categories)
            {
              Add(new ComboBoxItem(category));
            }
          }
        }
      });

      // Store the layer 
      if (Enabled)
      {
        myLayer = selectedLayer;
      }

    }

    /// <summary>
    /// The on comboBox selection change event. This creates a new table that lists the assignments for the specified category.  This table is added to the map, selected in the TOC, and opened.
    /// </summary>
    /// <param name="item">The newly selected combo box item</param>
    protected override async void OnSelectionChange(ComboBoxItem item)
    {

      if (item == null)
        return;

      if (string.IsNullOrEmpty(item.Text))
        return;

      if (myLayer == null)
        return;

      //Construct the name of our table for the category assignment report
      string baseCategoryReportTableName = "CategoryAssignments_" + item.Text;
      string categoryReportTableName = baseCategoryReportTableName.Replace(" ", "_");

      bool needToCreateTable = true;
      bool needToAddStandaloneTable = true;

      // Switch to the MCT to access the geodatabase
      await QueuedTask.Run(() =>
      {
        // Check if the table exists

        using (Geodatabase projectWorkspace = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(Project.Current.DefaultGeodatabasePath))))
        {
          try
          {
            using (Table categoryReportTable = projectWorkspace.OpenDataset<Table>(categoryReportTableName))
            {
              // Table exists, we do not need to create it...
              needToCreateTable = false;

              // .. but we should delete the existing contents
              categoryReportTable.DeleteRows(new QueryFilter());

              // Check to see if a Standalone table exists in the map
              bool standaloneTableFound = false;
              ReadOnlyObservableCollection<StandaloneTable> initialStandaloneTables = MapView.Active.Map.StandaloneTables;
              foreach (StandaloneTable standaloneTable in initialStandaloneTables)
              {
                if (standaloneTable.Name == categoryReportTableName)
                  standaloneTableFound = true;
              }

              // Since there is already a StandaloneTable that references our category table in the map, we don't need to add it
              needToAddStandaloneTable = !standaloneTableFound;
            }
          }
          catch
          {
            //Table doesn't exist.  Not an error, but we will have to create it
          }
        }
      });

      // Create the category report table

      if (needToCreateTable)
      {
        // Create table
        IReadOnlyList<string> createParams = Geoprocessing.MakeValueArray(new object[] { Project.Current.DefaultGeodatabasePath, categoryReportTableName, null, null });
        IGPResult result = await Geoprocessing.ExecuteToolAsync("management.CreateTable", createParams);
        if (result.IsFailed)
        {
          MessageBox.Show("Unable to create category assignment table in project workspace", "Category Assignments");
          return;
        }

        // Add field for feature class alias
        IReadOnlyList<string> addFieldParams = Geoprocessing.MakeValueArray(new object[] { categoryReportTableName, "FeatureClassAlias", "TEXT", null, null, 32, "Feature Class", "NULLABLE", "NON_REQUIRED", null });
        result = await Geoprocessing.ExecuteToolAsync("management.AddField", addFieldParams);
        if (result.IsFailed)
        {
          MessageBox.Show("Unable to modify schema of category assignment table in project workspace", "Category Assignments");
          return;
        }

        // Add field for Asset Group name
        addFieldParams = Geoprocessing.MakeValueArray(new object[] { categoryReportTableName, "AssetGroupName", "TEXT", null, null, 256, "Asset Group Name", "NULLABLE", "NON_REQUIRED", null });
        result = await Geoprocessing.ExecuteToolAsync("management.AddField", addFieldParams);
        if (result.IsFailed)
        {
          MessageBox.Show("Unable to modify schema of category assignment table in project workspace", "Category Assignments");
          return;
        }

        // Add field for Asset Type name
        addFieldParams = Geoprocessing.MakeValueArray(new object[] { categoryReportTableName, "AssetTypeName", "TEXT", null, null, 256, "Asset Type Name", "NULLABLE", "NON_REQUIRED", null });
        result = await Geoprocessing.ExecuteToolAsync("management.AddField", addFieldParams);
        if (result.IsFailed)
        {
          MessageBox.Show("Unable to modify schema of category assignment table in project workspace", "Category Assignments");
          return;
        }

        needToAddStandaloneTable = false; //creating a table automatically adds it to the map
      }


      // Populate table
      // Again, we need to switch to the MCT to execute geodatabase and utility network code
      await QueuedTask.Run(() =>
      {
        using (Geodatabase projectWorkspace = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(Project.Current.DefaultGeodatabasePath))))
        using (Table categoryReportTable = projectWorkspace.OpenDataset<Table>(categoryReportTableName))
        using (UtilityNetwork utilityNetwork = UtilityNetworkSamples.UtilityNetworkUtils.GetUtilityNetworkFromLayer(myLayer))
        using (UtilityNetworkDefinition utilityNetworkDefinition = utilityNetwork.GetDefinition())
        {
          IReadOnlyList<NetworkSource> networkSources = utilityNetworkDefinition.GetNetworkSources();

          // Step through each NetworkSource
          foreach (NetworkSource networkSource in networkSources)
          {
            IReadOnlyList<AssetGroup> assetGroups = networkSource.GetAssetGroups();

            // Step through each AssetGroup
            foreach (AssetGroup assetGroup in assetGroups)
            {
              IReadOnlyList<AssetType> assetTypes = assetGroup.GetAssetTypes();

              // Step through each AssetType
              foreach (AssetType assetType in assetTypes)
              {

                // Check to see if this AssetType is assigned the Category we are looking for
                IReadOnlyList<string> assignedCategoryList = assetType.CategoryList;
                foreach (string assignedCategory in assignedCategoryList)
                {
                  if (assignedCategory == item.Text)
                  {

                    // Our Category is assigned to this AssetType.  Create a row to store in the category report table
                    using (FeatureClass networkSourceFeatureClass = utilityNetwork.GetTable(networkSource) as FeatureClass)
                    using (FeatureClassDefinition networkSourceFeatureClassDefinition = networkSourceFeatureClass.GetDefinition())
                    using (RowBuffer rowBuffer = categoryReportTable.CreateRowBuffer())
                    {
                      rowBuffer["FeatureClassAlias"] = networkSourceFeatureClassDefinition.GetAliasName();
                      rowBuffer["AssetGroupName"] = assetGroup.Name;
                      rowBuffer["AssetTypeName"] = assetType.Name;
                      categoryReportTable.CreateRow(rowBuffer).Dispose();
                    }
                  }
                }
              }
            }
          }

          // If necessary, add our category report table to the map as a standalone table
          if (needToAddStandaloneTable)
          {
            IStandaloneTableFactory tableFactory = StandaloneTableFactory.Instance;
            tableFactory.CreateStandaloneTable(categoryReportTable, MapView.Active.Map);
          }
        }

      });

      // Open category report stand alone table into a window
      ReadOnlyObservableCollection<StandaloneTable> standaloneTables = MapView.Active.Map.StandaloneTables;
      foreach (StandaloneTable standaloneTable in standaloneTables)
      {
        if (standaloneTable.Name == categoryReportTableName)
          FrameworkApplication.Panes.OpenTablePane(standaloneTable, TableViewMode.eAllRecords);
      }
    }
  

  }
}
