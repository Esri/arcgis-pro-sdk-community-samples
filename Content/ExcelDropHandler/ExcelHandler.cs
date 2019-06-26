//   Copyright 2018 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.DragDrop;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;


namespace ExcelDropHandler
{
    internal class ExcelHandler : DropHandlerBase
    {
        public override void OnDragOver(DropInfo dropInfo)
        {
            dropInfo.Effects = DragDropEffects.All;
        }

        public override void OnDrop(DropInfo dropInfo)
        {

            #region Implement Your OnDrop Here
            if (FrameworkApplication.Panes.ActivePane is IMapPane)
            {
                //a drag is being made on the active map
                var view = ((IMapPane)FrameworkApplication.Panes.ActivePane).MapView;
                // globe or local
                if (view.ViewingMode == MapViewingMode.SceneGlobal ||
                  view.ViewingMode == MapViewingMode.SceneLocal)
                {
                    //we are in 3D
                    OnDrop3D(dropInfo);
                }
                else
                {
                    //we are in 2D
                    OnDrop2D(dropInfo);
                }
            }

           
            #endregion

            dropInfo.Handled = true;
        }

        private async void OnDrop2D(DropInfo dropInfo)
        {

            string xlsName = dropInfo.Items[0].Data.ToString();
            string xlsLayerName = Module1.GetUniqueLayerName(xlsName);
            string xlsSheetName = Module1.GetUniqueStandaloneTableName(xlsName);
            string xlsTableName = xlsName + "\\" + xlsSheetName;

            // set overwrite flag           
            var environments = Geoprocessing.MakeEnvironmentArray(overwriteoutput: true);

            //To use Excel files in Pro, you need Microsoft Access Database Engine 2016. 
            //Refer to the Pro Help topic "Work with Microsoft Excel files" for more information on dowloading the required driver.
            //https://prodev.arcgis.com/en/pro-app/help/data/excel/work-with-excel-in-arcgis-pro.htm

            #region Geoprocessing.ExecuteToolAsync(MakeXYEventLayer_management)
            var cts = new CancellationTokenSource();
            var result = await Geoprocessing.ExecuteToolAsync("MakeXYEventLayer_management", new string[] {
                xlsTableName,
                "POINT_X",
                "POINT_Y",
                xlsLayerName,
                "WGS_1984"
            }, environments, cts.Token,
                        (eventName, o) =>
                        {
                            System.Diagnostics.Debug.WriteLine($@"GP event: {eventName}");
                        });

            #endregion

            #region Assign Symbology (from Location layer)
            var featureLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(fl => fl.Name.Contains(xlsLayerName)).FirstOrDefault();
            await ModifyLayerSymbologyFromLyrFileAsync(featureLayer, @"E:\Data\SDK\Default2DPointSymbols.lyrx");
            //  @"C:\Data\SDK\Default2DPointSymbols.lyrx"

            #endregion

        }

        private async void OnDrop3D(DropInfo dropInfo)
        {

            string xlsName = dropInfo.Items[0].Data.ToString();
            string xlsLayerName = Module1.GetUniqueLayerName(xlsName);
            string xlsSheetName = Module1.GetUniqueStandaloneTableName(xlsName);
            string xlsTableName = xlsName + "\\" + xlsSheetName;

            // set overwrite flag           
            var environments = Geoprocessing.MakeEnvironmentArray(overwriteoutput: true);

            //To use Excel files in Pro, you need Microsoft Access Database Engine 2016. 
            //Refer to the Pro Help topic "Work with Microsoft Excel files" for more information on dowloading the required driver.
            //https://prodev.arcgis.com/en/pro-app/help/data/excel/work-with-excel-in-arcgis-pro.htm

            #region await Geoprocessing.ExecuteToolAsync(MakeXYEventLayer_management)
            var cts = new CancellationTokenSource();
            var result = await Geoprocessing.ExecuteToolAsync("MakeXYEventLayer_management", new string[] {
                  xlsTableName,
                  "POINT_X",
                  "POINT_Y",
                  xlsLayerName,
                  "WGS_1984"
              }, environments, cts.Token,
                        (eventName, o) =>
                        {
                            System.Diagnostics.Debug.WriteLine($@"GP event: {eventName}");
                        });

            #endregion

            #region Assign Symbology from lyr file
            var featureLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(fl => fl.Name.Contains(xlsLayerName)).FirstOrDefault();
            await ModifyLayerSymbologyFromLyrFileAsync(featureLayer, @"E:\Data\SDK\Default3DPointSymbols.lyrx");           

            #endregion
        }

        private static async Task ModifyLayerSymbologyFromLyrFileAsync(FeatureLayer featureLayer, string layerFile)
        {
            await QueuedTask.Run(() => {
                //Does layer file exist
                if (!System.IO.File.Exists(layerFile))
                    return;

                //Get the Layer Document from the lyrx file
                var lyrDocFromLyrxFile = new LayerDocument(layerFile);
                var cimLyrDoc = lyrDocFromLyrxFile.GetCIMLayerDocument();

                //Get the renderer from the layer file
                var rendererFromLayerFile = ((CIMFeatureLayer)cimLyrDoc.LayerDefinitions[0]).Renderer as CIMSimpleRenderer;

                if (rendererFromLayerFile == null)
                    return;

                //Apply the renderer to the feature layer
                featureLayer?.SetRenderer(rendererFromLayerFile);

            });
        }

    }
}
