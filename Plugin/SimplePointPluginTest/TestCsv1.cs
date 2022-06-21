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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using ArcGIS.Core.Data.PluginDatastore;

namespace SimplePointPluginTest
{
  /// <summary>
  /// Example of accessing a Plugin Workspace via the ArcGIS.Core.Data API
  /// </summary>
  internal class TestCsv1 : Button
  {
    protected async override void OnClick()
    {
      //Change this path to the path of your sample csv data
      string csv_path = @"C:\Data\SimplePointPlugin\SimplePointData";

      await QueuedTask.Run(() =>
      {

        using (PluginDatastore pluginws = new PluginDatastore(
             new PluginDatasourceConnectionPath("SimplePointPlugin_Datasource",
                   new Uri(csv_path, UriKind.Absolute))))
        {
          System.Diagnostics.Debug.Write("==========================\r\n");
          foreach (var table_name in pluginws.GetTableNames())
          {
            System.Diagnostics.Debug.Write($"Table: {table_name}\r\n");
            //open each table....use the returned table name
            //or just pass in the name of a csv file in the workspace folder
            using (var table = pluginws.OpenTable(table_name))
            {
              //get information about the table
              using (var def = table.GetDefinition() as FeatureClassDefinition)
              {

              }
              //query and return all rows
              //TODO - use a QueryFilter and Whereclause
              //var qf = new QueryFilter()
              //{
              //  WhereClause = "OBJECTID > 0"
              //};
              //var rc = table.Search(qf);

              using (var rc = table.Search(null))
              {
                while (rc.MoveNext())
                {
                  using (var feat = rc.Current as Feature)
                  {
                    //Get information from the feature
                    var oid = feat.GetObjectID();
                    var shape = feat.GetShape();

                    //Access all the values
                    var count = feat.GetFields().Count();
                    for (int i = 0; i < count; i++)
                    {
                      var val = feat[i];
                      //TODO use the value(s)
                    }
                  }
                }
              }
            }
          }
        }


      });

    }
  }
}
