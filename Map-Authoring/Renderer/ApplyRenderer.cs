/*

   Copyright 2019 Esri

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
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace Renderer
{
    internal enum EnumRenderType 
    {
        UniqueValueRenderer,
        ClassBreakRenderer,
        BarChartRenderer,
        PieChartRenderer,
        StackedBarChartRenderer,
        HeatMapRenderer,
        SimpleRendererPolygon,
        ProportionalRenderer
    };

    internal class ApplyRenderer : Button
    {
        private static EnumRenderType renderType = EnumRenderType.UniqueValueRenderer;

        protected async override void OnClick()
        {
            //TODO: This line below gets the first point layer in the project to apply a renderer.  
            //You can modify it to use other layers with polygon or line geometry if needed.
            var lyr = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(s => s.ShapeType == esriGeometryType.esriGeometryPolygon);
            //TODO: Modify this line below to experiment with the different renderers
            switch (renderType)
            {
                case EnumRenderType.UniqueValueRenderer:
                    await UniqueValueRenderers.UniqueValueRendererAsync(lyr);
                    renderType = EnumRenderType.ClassBreakRenderer;
                    break;
                case EnumRenderType.ClassBreakRenderer:
                    await ClassBreakRenderers.CBRendererGraduatedColorsOutlineAsync(lyr);
                    renderType = EnumRenderType.BarChartRenderer;
                    break;
                case EnumRenderType.BarChartRenderer:
                    await ChartRenderers.BarChartRendererAsync();
                    renderType = EnumRenderType.PieChartRenderer;
                    break;
                case EnumRenderType.PieChartRenderer:
                    await ChartRenderers.PieChartRendererAsync();
                    renderType = EnumRenderType.StackedBarChartRenderer;
                    break;
                case EnumRenderType.StackedBarChartRenderer:
                    await ChartRenderers.StackedBarChartRendererAsync();
                    renderType = EnumRenderType.HeatMapRenderer;
                    break;
                case EnumRenderType.HeatMapRenderer:
                    await HeatMapRenderers.HeatMapRenderersAsync(lyr);
                    renderType = EnumRenderType.UniqueValueRenderer;
                    break;
                case EnumRenderType.SimpleRendererPolygon:
                    await SimpleRenderers.SimpleRendererPolygon(lyr);
                    renderType = EnumRenderType.ProportionalRenderer;
                    break;
                case EnumRenderType.ProportionalRenderer:
                    await ProportionalRenderers.ProportionalRendererAsync(lyr);
                    renderType = EnumRenderType.UniqueValueRenderer;
                    break;
                default:
                    renderType = EnumRenderType.UniqueValueRenderer;
                    break;

            }

            
            //
    }      
    }
}
