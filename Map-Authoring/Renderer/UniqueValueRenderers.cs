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
    internal static class UniqueValueRenderers
    {
        #region Snippet Unique Value Renderer for a feature layer
        /// <summary>
        /// Renders a feature layer using unique values from one or multiple fields
        /// </summary>
        /// <remarks>
        /// ![Unique Value renderer](http://Esri.github.io/arcgis-pro-sdk/images/Renderers/unique-value.png)
        /// </remarks>
        /// <param name="featureLayer"></param>
        /// <returns>
        /// ![Unique Value renderer](http://Esri.github.io/arcgis-pro-sdk/images/Renderers/unique-value.png)
        /// </returns>
        internal static async Task UniqueValueRenderer(FeatureLayer featureLayer)
        {
            await QueuedTask.Run(() =>
            {                
                //construct unique value renderer definition                
                UniqueValueRendererDefinition uvr = new
                   UniqueValueRendererDefinition()
                {
                    ValueFields = new string[] { SDKHelpers.GetDisplayField(featureLayer) }, //multiple fields in the array if needed.
                    ColorRamp = SDKHelpers.GetColorRamp(), //Specify color ramp
                };

                //Creates a "Renderer"
                var cimRenderer = featureLayer.CreateRenderer(uvr);

                //Sets the renderer to the feature layer
                featureLayer.SetRenderer(cimRenderer);
            });
        }
        #endregion              
    }
}
