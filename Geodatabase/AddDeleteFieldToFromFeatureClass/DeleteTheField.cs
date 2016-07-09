/*

   Copyright 2016 Esri

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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Mapping;

namespace AddDeleteFieldToFromFeatureClass
{
    internal class DeleteTheField : Button
    {
        protected override async void OnClick()
        {
            try
            {
                BasicFeatureLayer layer = null;
                await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
                {
                    //find first layer
                    layer = MapView.Active.Map.Layers.FirstOrDefault() as BasicFeatureLayer;

                });
                if (layer == null)
                    MessageBox.Show("Unable to find a feature class at the first layer of the active map");
                else
                {
                    MessageBox.Show($@"{layer.Name} was found .... deleting the newly added field");
                    await
                        ExecuteDeleteFieldTool(layer, "AddedField");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private async Task<bool> ExecuteDeleteFieldTool(BasicFeatureLayer theLayer, string fieldName)
        {
            try
            {
                return await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
                {
                    var inTable = theLayer.Name;
                    var table = theLayer.GetTable();
                    var dataStore = table.GetDatastore();
                    var workspaceNameDef = dataStore.GetConnectionString();
                    var workspaceName = workspaceNameDef.Split('=')[1];

                    var parameters = Geoprocessing.MakeValueArray(inTable, fieldName);
                    var env = Geoprocessing.MakeEnvironmentArray(workspace: workspaceName);
                    var cts = new CancellationTokenSource();
                    var results = Geoprocessing.ExecuteToolAsync("management.DeleteField", parameters, env, cts.Token,
                        (eventName, o) =>
                        {
                            System.Diagnostics.Debug.WriteLine($@"GP event: {eventName}");
                            if (eventName == "OnMessage")
                            {

                                System.Diagnostics.Debug.WriteLine($@"Msg: {o}");
                            }
                        });
                    return true;

                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
        }

    }
}
