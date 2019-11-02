/*

   Copyright 2017 Esri

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
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.PluginDatastore;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProDataReader
{

	internal class DelFromProject : Button
	{
		protected override void OnClick()
		{
			var catalog = Project.GetCatalogPane();
			var items = catalog.SelectedItems;
			var item = items.OfType<ProDataProjectItem>().FirstOrDefault();
			if (item == null) return;
			try
			{
				QueuedTask.Run(() => Project.Current.RemoveItem(item));
			}
			catch (Exception ex)
			{
				MessageBox.Show($@"Unable to remove from project: {ex.Message}");
			}
		}
	}

	internal class AddToProject : Button
	{
		protected override void OnClick()
		{
			var catalog = Project.GetCatalogPane();
			var items = catalog.SelectedItems;
			var item = items.OfType<ProDataProjectItem>().FirstOrDefault();
			if (item == null) return;
			try
			{
				QueuedTask.Run(() => Project.Current.AddItem(item.Clone()));
			}
			catch (Exception ex)
			{
				MessageBox.Show($@"Unable to add to project: {ex.Message}");
			}
		}
	}


	internal class AddToCurrentMap : Button
	{
		protected async override void OnClick()
		{
			var catalog = Project.GetCatalogPane();
			var items = catalog.SelectedItems;
			var ProDataSubItems = items.OfType<ProDataSubItem>();
			foreach (var item in ProDataSubItems)
			{
				try
				{
					await QueuedTask.Run(() =>
					{
						switch (item.SubItemType)
						{
							case ProDataSubItem.EnumSubItemType.DirType:
								break;
							case ProDataSubItem.EnumSubItemType.GpxType:
								var conGpx = new PluginDatasourceConnectionPath("ProGpxPluginDatasource",
												 new Uri(item.Path, UriKind.Absolute));
								using (var pluginGpx = new PluginDatastore(conGpx))
								{
									System.Diagnostics.Debug.Write($"Table: {item.Path}\r\n");
									foreach (var tn in pluginGpx.GetTableNames())
									{
										using (var table = pluginGpx.OpenTable(tn))
										{
											//Add as a layer to the active map or scene
											LayerFactory.Instance.CreateFeatureLayer((FeatureClass)table, MapView.Active.Map);
										}
									}
								}
								break;
							case ProDataSubItem.EnumSubItemType.ImgDirType:
							case ProDataSubItem.EnumSubItemType.ImgType:
								var conJpg = new PluginDatasourceConnectionPath("ProJpgPluginDatasource",
												 new Uri(item.Path, UriKind.Absolute));
								using (var pluginJpg = new PluginDatastore(conJpg))
								{
									System.Diagnostics.Debug.Write($"Table: {item.Path}\r\n");
									//open each table....use the returned table name
									//or just pass in the name of a csv file in the workspace folder
									foreach (var tn in pluginJpg.GetTableNames())
									{
										using (var table = pluginJpg.OpenTable(tn))
										{
											//Add as a layer to the active map or scene
											LayerFactory.Instance.CreateFeatureLayer((FeatureClass)table, MapView.Active.Map);
										}
									}
								}
								break;
						}
					});
				}
				catch (Exception ex)
				{
					MessageBox.Show($@"Unable to add to map: {ex.Message}");
				}
			}
		}
	}
}
