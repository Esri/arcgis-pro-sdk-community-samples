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
using System.Threading.Tasks;
using System.Windows;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Geoprocessing;
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

            if (dropInfo.TargetModel != null)
            {
                if (dropInfo.TargetModel is MapView)
                {
                    MapView view = dropInfo.TargetModel as MapView;
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

            #region Geoprocessing.ExecuteToolAsync(MakeXYEventLayer_management)

            var result = await Geoprocessing.ExecuteToolAsync("MakeXYEventLayer_management", new string[] {
                xlsTableName,
                "POINT_X",
                "POINT_Y",
                xlsLayerName,
                "WGS_1984"
            }, environments);

            #endregion

            #region Assign Symbology (from Location layer)

            await Geoprocessing.ExecuteToolAsync("ApplySymbologyFromLayer_management", new string[] {
              xlsLayerName, 
              @"C:\Data\SDK\Default2DPointSymbols.lyrx"
            });

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


            #region await Geoprocessing.ExecuteToolAsync(MakeXYEventLayer_management)

            var result = await Geoprocessing.ExecuteToolAsync("MakeXYEventLayer_management", new string[] {
                  xlsTableName,
                  "POINT_X",
                  "POINT_Y",
                  xlsLayerName,
                  "WGS_1984"
              }, environments);

            #endregion

            #region await Geoprocessing.ExecuteToolAsync(ApplySymbologyFromLayer_management)

            result = await Geoprocessing.ExecuteToolAsync("ApplySymbologyFromLayer_management", new string[] {
              xlsLayerName, 
              @"C:\Data\SDK\Default3DPointSymbols.lyrx"
            });

            #endregion
        }

    }
}
