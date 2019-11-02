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

namespace ProSqlExpressReader
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
				QueuedTask.Run(() => Project.Current.AddItem(item));
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
			if (MapView.Active?.Map == null)
			{
				MessageBox.Show("There is no active map");
				return;
			}
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
							case ProDataSubItem.EnumSubItemType.DataSet:
								// path is comprised for sql DB path followed by '|' and the table name
								var parts = item.Path.Split('|');
								if (parts.Length != 3)
								{
									MessageBox.Show($@"Item path can't be parsed: {item.Path}");
									break;
								}
								var sqlPath = parts[0];
                                var sqlConStr = parts[1];
								var dataset = parts[2];
								var conSql = new PluginDatasourceConnectionPath("ProSqlExpressPluginDatasource",
																			  new Uri(item.Path.Replace(";", "||"), UriKind.Absolute));
								using (var pluginSql = new PluginDatastore(conSql))
								{
									foreach (var tn in pluginSql.GetTableNames())
									{
										if (tn.StartsWith($@"\{dataset}\"))
										{
											using (var table = pluginSql.OpenTable(tn))
											{
												if (table is FeatureClass)
												{
													//Add as a layer to the active map or scene
													LayerFactory.Instance.CreateFeatureLayer((FeatureClass)table, MapView.Active.Map);
												}
												else
												{
													//add as a standalone table
													StandaloneTableFactory.Instance.CreateStandaloneTable(table, MapView.Active.Map);
												}
											}
										}
									}
								}
								break;
							case ProDataSubItem.EnumSubItemType.SqlType:
								// path is comprised for sql DB path followed by '|' and the table name
								parts = item.Path.Split('|');
								if (parts.Length < 2)
								{
									MessageBox.Show($@"Item path can't be parsed: {item.Path}");
									break;
								}
                                sqlPath = parts[0];
                                sqlConStr = parts[1];
                                var tableName = string.Empty;
                                if (parts.Length == 3) tableName = parts[2];

                                conSql = new PluginDatasourceConnectionPath("ProSqlExpressPluginDatasource",
																			  new Uri(item.Path.Replace(";", "||"), UriKind.Absolute));
								using (var pluginSql = new PluginDatastore(conSql))
								{
                                    var tableNames = new List<string>();
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        tableNames = new List<string>(pluginSql.GetTableNames());
                                    }
                                    else tableNames.Add(tableName);
                                    foreach (var tn in tableNames)
                                    {
                                        System.Diagnostics.Debug.Write($"Open table: {tn}\r\n");
                                        //open the table
                                        using (var table = pluginSql.OpenTable(tn))
                                        {
                                            if (table is FeatureClass)
                                            {
                                                //Add as a layer to the active map or scene
                                                LayerFactory.Instance.CreateFeatureLayer((FeatureClass)table, MapView.Active.Map);
                                            }
                                            else
                                            {
                                                //add as a standalone table
                                                StandaloneTableFactory.Instance.CreateStandaloneTable(table, MapView.Active.Map);
                                            }
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
