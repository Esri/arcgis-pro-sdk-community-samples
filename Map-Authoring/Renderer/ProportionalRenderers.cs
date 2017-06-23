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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Renderer.Helpers;

namespace Renderer
{
    internal static class ProportionalRenderers
    {
        #region Snippet Proportional symbols renderer.
        /// <summary>
        /// Renders a feature layer using proportional symbols to draw quantities.
        /// ![Proportional Symbols renderer](http://Esri.github.io/arcgis-pro-sdk/images/Renderers/proportional-renderer.png)
        /// </summary>
        /// <remarks></remarks>
        /// <param name="featureLayer"></param>
        /// <returns></returns>
        internal static async Task ProportionalRenderer(FeatureLayer featureLayer)
        {
            await QueuedTask.Run(() =>
            {
                //Gets the first numeric field of the feature layer
                var firstNumericFieldOfFeatureLayer = SDKHelpers.GetNumericField(featureLayer);
                //Gets the min and max value of the field
                var sizes = SDKHelpers.GetFieldMinMax(featureLayer, firstNumericFieldOfFeatureLayer);
                ProportionalRendererDefinition prDef = new ProportionalRendererDefinition()
                {
                    Field = firstNumericFieldOfFeatureLayer,
                    MinimumSymbolSize = 4,
                    MaximumSymbolSize = 50,                   
                    LowerSizeStop = Convert.ToDouble(sizes.Item1),
                    UpperSizeStop = Convert.ToDouble(sizes.Item2)
                };
                CIMProportionalRenderer propRndr = (CIMProportionalRenderer)featureLayer.CreateRenderer(prDef);
                featureLayer.SetRenderer(propRndr);
            });
        }
        #endregion
    }
}
