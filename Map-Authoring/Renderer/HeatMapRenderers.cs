/*

   Copyright 2019 Esri

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
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Renderer.Helpers;

namespace Renderer
{
    internal class HeatMapRenderers
    {
        #region Snippet Heat map renderer
        /// <summary>
        /// Renders a point feature layer using a continuous color gradient to represent density of points.
        /// </summary>
        /// <remarks>
        /// ![Heat map renderer](http://Esri.github.io/arcgis-pro-sdk/images/Renderers/heat-map.png)
        /// </remarks>
        /// <returns>
        /// </returns>
        internal static Task HeatMapRenderersAsync()
        {
            //Check feature layer name
            //Code works with the U.S. Cities feature layer available with the ArcGIS Pro SDK Sample data
            var featureLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(f => f.Name == "U.S. Cities");
            if (featureLayer == null)
            {
              MessageBox.Show("This renderer works with the U.S. Cities feature layer available with the ArcGIS Pro SDK Sample data", "Data missing");
              return Task.FromResult(0);
            }
            return QueuedTask.Run(() =>
            {
                //defining a heatmap renderer that uses values from Population field as the weights
                HeatMapRendererDefinition heatMapDef = new HeatMapRendererDefinition()
                {
                    Radius = 20,
                    WeightField = SDKHelpers.GetNumericField(featureLayer),
                    ColorRamp = SDKHelpers.GetColorRamp(),
                    RendereringQuality = 8,
                    UpperLabel = "High Density",
                    LowerLabel = "Low Density"
                };

                CIMHeatMapRenderer heatMapRndr = (CIMHeatMapRenderer)featureLayer.CreateRenderer(heatMapDef);
                featureLayer.SetRenderer(heatMapRndr);
            });
        }
        #endregion
    }
}
