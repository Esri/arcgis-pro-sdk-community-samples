/*

   Copyright 2018 Esri

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
using System.IO;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.PluginDatastore;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace SimplePointPluginTest
{
  internal class TestCsv2 : Button
  {
    protected async override void OnClick()
    {
      try
      {
        // Change this path to the path of your sample csv data
        string csvPath = @"C:\Data\SimplePointPlugin\SimplePointData";
        var dirCsv = new DirectoryInfo(csvPath);
        if (!dirCsv.Exists) throw new Exception($@"The sample cannot find and csv files in this folder: {csvPath}");
        var csvFiles = dirCsv.GetFiles();
        if (csvFiles.Length <= 0) throw new Exception($@"The test folder has no csv files: {csvPath}");
        await QueuedTask.Run(() =>
        {
          using var pluginws = new PluginDatastore(
                new PluginDatasourceConnectionPath("SimplePointPlugin_Datasource",
                      new Uri(csvPath, UriKind.Absolute)));
          System.Diagnostics.Debug.Write("==========================\r\n");
          foreach (var table_name in pluginws.GetTableNames())
          {
            System.Diagnostics.Debug.Write($"Table: {table_name}\r\n");
            //open each table....use the returned table name
            //or just pass in the name of a csv file in the workspace folder
            using var table = pluginws.OpenTable(table_name);
            //Add as a layer to the active map or scene
            LayerFactory.Instance.CreateLayer<FeatureLayer>(new FeatureLayerCreationParams((FeatureClass)table), MapView.Active.Map);
          }
        });
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }
  }
}

