/*

   Copyright 2024 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       https://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.DragDrop;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DockpaneDragAndDropFeatureLayer
{
  internal class DropDockpaneViewModel : DockPane
  {
    private const string _dockPaneID = "DockpaneDragAndDropFeatureLayer_DropDockpane";

    protected DropDockpaneViewModel() { }

    /// <summary>
    /// Show the DockPane.
    /// </summary>
    internal static void Show()
    {
      DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
      if (pane == null)
        return;

      pane.Activate();
    }

    private string _tableOrFeatureClass = "Drop table or F/C here";

    private string _tableOrFeatureClassInfo = "detailed drop info";

    public string TableOrFeatureClass
    {
      get => _tableOrFeatureClass;
      set => SetProperty(ref _tableOrFeatureClass, value);
    }

		public string TableOrFeatureClassInfo
		{
			get => _tableOrFeatureClassInfo;
			set => SetProperty(ref _tableOrFeatureClassInfo, value);
		}

		private DataTable _selectedFeaturesDataTable = new DataTable();
		/// <summary>
		/// The selected data table (for tabular display)
		/// </summary>
		public DataTable SelectedFeatureDataTable
		{
			get { return _selectedFeaturesDataTable; }
			set
			{
				SetProperty(ref _selectedFeaturesDataTable, value, () => SelectedFeatureDataTable);
			}
		}

		//TODO:Implement the drag and drop overrides for the dockpane class
		public override void OnDragOver(DropInfo dropInfo)
    {
      //default is to accept our data types
      dropInfo.Effects = DragDropEffects.All;
    }

    public override void OnDrop(DropInfo dropInfo)
    {
      //eg, if you are accessing a dropped file
      string filePath = dropInfo.Items[0].Data.ToString();

      if (dropInfo.Data is List<ClipboardItem> clipboardItems) //Dropped from Catalog 
      {
				StringBuilder info = new StringBuilder();
        var thisItem = clipboardItems.FirstOrDefault();
        var itemInfo = thisItem.ItemInfoValue.typeID;
        // use the itemInfo to determine the type of item dropped
				info.AppendLine(@$"Item type dropped: {itemInfo}");
				TableOrFeatureClass = string.Empty;
				SelectedFeatureDataTable = new DataTable();
        switch (itemInfo)
        {
          case "fgdb_fc_point":
            TableOrFeatureClass = thisItem.CatalogPath;
						info.AppendLine(@$"Item is displayed in table");
						break;
          case "fgdb_fc_line":
						TableOrFeatureClass = thisItem.CatalogPath;
						info.AppendLine(@$"Item is displayed in table");
						break;
          case "fgdb_table":
						TableOrFeatureClass = thisItem.CatalogPath;
						info.AppendLine(@$"Item is displayed in table");
						break;
          default:
            info.AppendLine ("Item type not supported");
            break;
				}
				TableOrFeatureClassInfo = info.ToString();
				if (!string.IsNullOrEmpty(TableOrFeatureClass))
				{
					PopulateTableAsync(TableOrFeatureClass, itemInfo);
				}
				//set to true if you handled the drop
				dropInfo.Handled = true;
      }
		}

    private async void PopulateTableAsync (string catalogPath, string itemInfo)
    {
			try
			{
				// get the geodatabase and table path path from catalog path
				var parts = catalogPath.Split(new string[] { ".gdb\\" }, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length != 2)
				{
					ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($@"Unable to determine the geodatabase and table path from the catalog path: {catalogPath}");
				}
				string gdbPath = parts[0] + ".gdb";
				var fcNameParts = parts[1].Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
				string fcName = fcNameParts[^1];
				SelectedFeatureDataTable = await QueuedTask.Run<DataTable>(() =>
				{
					using Geodatabase projectGDB = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(Project.Current.DefaultGeodatabasePath)));
					using Table fc = projectGDB.OpenDataset<Table>(fcName);

					// Get all selected features for selectedFeatureLayer
					// and populate a DataTable with data and column headers
					var listColumnNames = new List<KeyValuePair<string, string>>();
					var listValues = new List<List<string>>();
					using (var rowCursor = fc.Search(null))
					{
						bool bDefineColumns = true;
						while (rowCursor.MoveNext())
						{
							using (var anyRow = rowCursor.Current)
							{
								if (bDefineColumns)
								{
									foreach (var fld in anyRow.GetFields().Where(fld => fld.FieldType != FieldType.Geometry))
									{
										listColumnNames.Add(new KeyValuePair<string, string>(fld.Name, fld.AliasName));
									}
								}
								var newRow = new List<string>();
								foreach (var fld in anyRow.GetFields().Where(fld => fld.FieldType != FieldType.Geometry))
								{
									newRow.Add((anyRow[fld.Name] == null) ? string.Empty : anyRow[fld.Name].ToString());
								}
								listValues.Add(newRow);
								bDefineColumns = false;
							}
						}
					}
					var newDataTable = new DataTable();
					foreach (var col in listColumnNames)
					{
						newDataTable.Columns.Add(new DataColumn(col.Key, typeof(string)) { Caption = col.Value });
					}
					foreach (var row in listValues)
					{
						var newRow = newDataTable.NewRow();
						newRow.ItemArray = row.ToArray();
						newDataTable.Rows.Add(newRow);
					}
					return newDataTable;
				});
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($@"PopulateTable Error: {ex.ToString()}");
			}

		
		}

		private static Task<bool> FeatureClassExistsAsync(string fcName)
		{
			return QueuedTask.Run(() =>
			{
				try
				{
					using (Geodatabase projectGDB = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(Project.Current.DefaultGeodatabasePath))))
					{
						using (FeatureClass fc = projectGDB.OpenDataset<FeatureClass>(fcName))
						{
							return fc != null;
						}
					}
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine($@"FeatureClassExists Error: {ex.ToString()}");
					return false;
				}
			});
		}
	}


	/// <summary>
	/// Button implementation to show the DockPane.
	/// </summary>
	internal class DropDockpane_ShowButton : Button
  {
    protected override void OnClick()
    {
      DropDockpaneViewModel.Show();
    }
  }
}
