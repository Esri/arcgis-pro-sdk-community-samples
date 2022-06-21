/*

   Copyright 2019 Esri

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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.DragDrop;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;


namespace DragAndDrop
{
	internal class DragAndDropDockpane1ViewModel : DockPane, IDragSource
	{
		private const string _dockPaneID = "DragAndDrop_DragAndDropDockpane1";

		protected DragAndDropDockpane1ViewModel() { }

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

		/// <summary>
		/// Text shown near the top of the DockPane.
		/// </summary>
		private string _heading = "Drag and Drop Dockpane";
		public string Heading
		{
			get { return _heading; }
			set
			{
				SetProperty(ref _heading, value, () => Heading);
			}
		}
		private string _name = "Drag and Drop File GDB Here";
		public string Name
		{
			get { return _name; }
			set
			{
				SetProperty(ref _name, value, () => Name);
			}
		}

		private ObservableCollection<GDBBaseItem> _gdbItems = new ObservableCollection<GDBBaseItem>();
		public ObservableCollection<GDBBaseItem> GDBItems
		{
			get { return _gdbItems; }
			set
			{
				SetProperty(ref _gdbItems, value, () => GDBItems);
			}
		}

		#region Drag and Drop handler
		public override async void OnDrop(DropInfo dropInfo)
		{
			//eg, if you are accessing a dropped file
			string filePath = dropInfo.Items[0].Data.ToString();
			if (dropInfo.Data is List<ClipboardItem> clipboardItems) //Dropped from Catalog
			{
				var thisItem = clipboardItems.FirstOrDefault();
				var itemInfo = thisItem.ItemInfoValue.typeID;
				if (itemInfo != "database_fgdb") //Not a file gdb
				{
					dropInfo.Handled = false;
					ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($"Drag and drop File GDB only here");
					return;
				}
				Name = thisItem.CatalogPath;
			}
			else //Dropped from File Explorer
			{
				FileInfo file = new FileInfo(filePath);
				if (string.Compare(file.Extension, ".gdb", true) == 0) //.gdb
				{
					Name = filePath;
					dropInfo.Handled = true;
				}
				else //Not a .gdb
				{
					dropInfo.Handled = false;
					ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($"Drag and drop File GDB only here");
					return;
				}
			}

			GDBItems = await GetGDBItemsAsync();
			//set to true if you handled the drop
			dropInfo.Handled = true;

		}
		public override void OnDragOver(DropInfo dropInfo)
		{
			//default is to accept our data types
			dropInfo.Effects = DragDropEffects.All;
		}
		#endregion

		#region Private methods
		private async Task<ObservableCollection<GDBBaseItem>> GetGDBItemsAsync()
		{
			var gdbItems = await QueuedTask.Run<ObservableCollection<GDBBaseItem>>(() =>
			{
				List<GDBBaseItem> lstGDBBaseItems = new List<GDBBaseItem>();
				//Database becomes the root node
				var path = Name;
				var root = new DatabaseGDBItem { DBName = System.IO.Path.GetFileName(Name), Name = path, Path = path };
				lstGDBBaseItems.Add(root);
				// use the geodatabase to get all layers
				var fGdbPath = new FileGeodatabaseConnectionPath(new Uri(Name, UriKind.Absolute));
				using (var gdb = new Geodatabase(fGdbPath))
				{
					IReadOnlyList<Definition> fcList = gdb.GetDefinitions<FeatureClassDefinition>();
					//Feature class
					foreach (FeatureClassDefinition fcDef in fcList)
					{
						var fc = gdb.OpenDataset<FeatureClass>(fcDef.GetName());
						var fd = fc.GetFeatureDataset();
						var fc_path = (fd == null) ? path : path + @"\" + fd.GetName();
						fc.Dispose();
						fd?.Dispose();
						switch (fcDef.GetShapeType())
						{
							case GeometryType.Point:
								var pointfcItem = new PointFCGDBItem { Name = fcDef.GetName(), Path = fc_path };
								root.Children.Add(pointfcItem);
								break;
							case GeometryType.Polyline:
								var linefcItem = new LineFCGDBItem { Name = fcDef.GetName(), Path = fc_path };
								root.Children.Add(linefcItem);
								break;
							case GeometryType.Polygon:
								var polyfcItem = new PolygonFCGDBItem { Name = fcDef.GetName(), Path = fc_path };
								root.Children.Add(polyfcItem);
								break;
						}
					}
				}
				root.IsExpanded = true;

				return new ObservableCollection<GDBBaseItem>(lstGDBBaseItems);
			});

			return gdbItems;
		}

		public void StartDrag(DragInfo dragInfo)
		{
			var sourceItem = dragInfo.SourceItem;
			if (sourceItem == null)
				return;
			var gdbItem = dragInfo.SourceItem as GDBBaseItem;
			if (gdbItem == null)
				return;

			if (gdbItem is DatabaseGDBItem)
				return;//no dragging of the gdb

			List<ClipboardItem> clip_items = new List<ClipboardItem>();
			clip_items.Add(new ClipboardItem()
			{
				ItemInfoValue = gdbItem.GetItemInfoValue()
			});
			dragInfo.Data = clip_items;
			dragInfo.Effects = DragDropEffects.Copy;
		}
		#endregion
	}

	/// <summary>
	/// Button implementation to show the DockPane.
	/// </summary>
	internal class DragAndDropDockpane1_ShowButton : Button
	{
		protected override void OnClick()
		{
			DragAndDropDockpane1ViewModel.Show();
		}
	}
}
