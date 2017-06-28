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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace CIMExamples {
    /// <summary>
    /// Show how to change out the Data Connection (equivalent to changing "DataSource" in ArcObjects)
    /// </summary>
    /// <remarks>So, we assume that the feature class referenced in the path exists - 
    /// simply change it to a path to a FileGDB and feature class on your local system</remarks>
    internal class ChangeLayerDataSource : Button {
        protected override  async void OnClick() {

            //So, arbitrarily, we pick the first feature layer and we just assume there is one ;-)
            var layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().ToList()[0];

            //If you got this far you have a feature layer
            string path = @"C:\Data\Admin\AdminSample.gdb";
            if (!Directory.Exists(path)) {
                MessageBox.Show(
                    string.Format("Please change the path in the sample to a GDB folder that exists: {0}", path),
                    "Folder Not Found");
                return;
            }

            string fcSource = System.IO.Path.Combine(path, "intrstat");
            try
            {
                await ChangeUSHighwaysLayerDataConnectionAsync(layer, fcSource);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private Task ChangeUSHighwaysLayerDataConnectionAsync(FeatureLayer featureLayer, string catalogPath) {
            return QueuedTask.Run(() => {
                CIMDataConnection currentDataConnection = featureLayer.GetDataConnection();

                string connection = System.IO.Path.GetDirectoryName(catalogPath);
                string suffix = System.IO.Path.GetExtension(connection).ToLower();

                var workspaceConnectionString = string.Empty;
                WorkspaceFactory wf = WorkspaceFactory.FileGDB;
                if (suffix == ".sde") {
                    wf = WorkspaceFactory.SDE;
                    var dbGdbConnection = new DatabaseConnectionFile(new Uri(connection, UriKind.Absolute));
                    workspaceConnectionString = new Geodatabase(dbGdbConnection).GetConnectionString();
                }
                else
                {
                    var dbGdbConnectionFile = new FileGeodatabaseConnectionPath (new Uri(connection, UriKind.Absolute));
                    workspaceConnectionString = new Geodatabase(dbGdbConnectionFile).GetConnectionString();
                }

                string dataset = System.IO.Path.GetFileName(catalogPath);
                // provide a replace data connection method
                CIMStandardDataConnection updatedDataConnection = new CIMStandardDataConnection() {
                    WorkspaceConnectionString = workspaceConnectionString,
                    WorkspaceFactory = wf,
                    Dataset = dataset,
                    DatasetType = esriDatasetType.esriDTFeatureClass
                };

                featureLayer.SetDataConnection(updatedDataConnection);

                //For a RDBMS, it might look like this:
                //string connection = "C:\\Work\\temp.sde";
                //Geodatabase sde = new Geodatabase(connection);

                //// provide a replace data connection method
                //CIMStandardDataConnection updatedDataConnection = new CIMStandardDataConnection();
                //updatedDataConnection.WorkspaceConnectionString = sde.GetConnectionString();
                //updatedDataConnection.WorkspaceFactory = WorkspaceFactory.SDE;
                //updatedDataConnection.Dataset = "vtest.usa.states";
                //updatedDataConnection.DatasetType = esriDatasetType.esriDTFeatureClass;



                //// Alternatively, use Layer.FindAndReplaceWorkspacePath()
                ////Note: this will not allow changing the dataset name or workspace type
                ////
                ////string connection = "C:\\Work\\temp.sde";
                ////Geodatabase sde = new Geodatabase(connection);
                ////featureLayer.FindAndReplaceWorkspacePath(((CIMStandardDataConnection)currentDataConnection).WorkspaceConnectionString, 
                ////                        sde.GetConnectionString(), true);


                //////////////////////////////////////////////
                ////Please Read
                ////
                //ok, so at this point we have a couple of bugs at 1.1 AND 1.2.....
                //
                //#1: if you switched to a Datasource that invalidates the Renderer, the Renderer does
                //not get invalidated in the UI
                //(eg You had a UniqueValueRenderer on a Field called "CATEGORY", the new datasource
                //does NOT have that field and so the renderer is invalid).
                //
                //#2: By default, Layers are added with a permanent cache. The cache is NOT automatically
                //invalidated so data (eg in the Attribute table, on the screen for draws) does NOT get
                //Refreshed so you have to invalidate the cache manually...

                //So, Bug #1 - we arbitrarily switch the Renderer to a simple renderer as a work around for that...
                featureLayer.SetRenderer(featureLayer.CreateRenderer(new SimpleRendererDefinition()));

                //Bug #2, we manually invalidate the cache
                featureLayer.ClearDisplayCache();
            });
        }
    }
}
