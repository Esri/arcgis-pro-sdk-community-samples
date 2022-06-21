/*

   Copyright 2020 Esri

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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;


namespace ChangeDataSource
{
	internal class ChangeDataSourceViewModel : DockPane
	{
		private const string _dockPaneID = "ChangeDataSource_ChangeDataSource";

		protected ChangeDataSourceViewModel() { }

		private string _fcSource;
		public string FcSource
		{
			get { return _fcSource; }
			set
			{
				SetProperty(ref _fcSource, value, () => FcSource);
			}
		}

		public ICommand CmdUpdateSourceFc
		{
			get
			{
				return new RelayCommand(() => 
				{
					OpenItemDialog openFc = new OpenItemDialog
					{
						Title = "Select Source Feature Class",
						InitialLocation = @"C:\Data",
						MultiSelect = false,
						BrowseFilter = BrowseProjectFilter.GetFilter("esri_browseDialogFilters_geodatabases")
					};
					bool? ok = openFc.ShowDialog();
					if (ok.HasValue && ok.Value && openFc.Items.Count() > 0)
					{
						foreach (Item itm in openFc.Items)
						{
							FcSource = itm.Path;
						}
					}
				}, true);
			}
		}

		public ICommand CmdUpdateDatasource
		{
			get
			{
				return new RelayCommand(() =>
				{
					try
					{
						var mapView = MapView.Active;
						foreach (FeatureLayer layer in mapView.Map.Layers.OfType<FeatureLayer>())
						{
							ChangeDatasource(layer, FcSource);
						}
					}
					catch (Exception ex)
					{
						MessageBox.Show($@"Error: {ex}");
					}
				}, MapView.Active != null);
			}
		}

		private async void ChangeDatasource(FeatureLayer featLayer, string newGDB)
		{
				await QueuedTask.Run(() =>
				{
					// provide a replacement data connection object
					CIMDataConnection updatedDataConnection = new CIMStandardDataConnection()
					{
						WorkspaceConnectionString = $"DATABASE={newGDB}",
						WorkspaceFactory = WorkspaceFactory.FileGDB,
						DatasetType = esriDatasetType.esriDTFeatureClass,
						Dataset = featLayer.Name
					};
					// the updated Data connection should look like this:
					// CustomWorkspaceFactoryCLSID: null
					// Dataset: "TestMultiPoints"
					// DatasetType: esriDTFeatureClass
					// WorkspaceConnectionString: "DATABASE=C:\\Data\\FeatureTest\\FeatureTest.gdb"
					// WorkspaceFactory: FileGDB

					// overwrite the data connection
					featLayer.SetDataConnection(updatedDataConnection);
				});
		}

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
	}

	/// <summary>
	/// Button implementation to show the DockPane.
	/// </summary>
	internal class ChangeDataSource_ShowButton : Button
	{
		protected override void OnClick()
		{
			ChangeDataSourceViewModel.Show();
		}
	}
}
