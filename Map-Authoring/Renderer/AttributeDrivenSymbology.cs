/*

   Copyright 2020 Esri

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

namespace Renderer
{
    internal class AttributeDrivenSymbology : Button
    {
        protected override void OnClick()
        {
            //Get the specific FlightPathPoint layer. This is available in the community samples data in the C:\Data\SDK folder.
            var lyr = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(f => f.Name == "FlightPathPoints").FirstOrDefault();
            if (lyr == null) return;
            QueuedTask.Run(() => {
                //Get the layer's renderer
                var renderer = lyr.GetRenderer() as CIMSimpleRenderer;
                //Create the attributes to use for the Rotation Visual Variables.
                var cimExpressionInfoX = new CIMExpressionInfo { Expression = "$feature.Tilt" };
                var cimVisualVariableInfoX = new CIMVisualVariableInfo { VisualVariableInfoType = VisualVariableInfoType.Expression, ValueExpressionInfo = cimExpressionInfoX };

                var cimExpressionInfoY = new CIMExpressionInfo { Expression = "$feature.Y" };
                var cimVisualVariableInfoY = new CIMVisualVariableInfo { VisualVariableInfoType = VisualVariableInfoType.Expression, ValueExpressionInfo = cimExpressionInfoY };

                var cimVisualVariableInfoZ = new CIMVisualVariableInfo { VisualVariableInfoType = VisualVariableInfoType.None };

                var listCIMVisualVariables = new List<CIMVisualVariable>
                {
                    new CIMRotationVisualVariable {
                        VisualVariableInfoX = cimVisualVariableInfoX,
                        VisualVariableInfoY = cimVisualVariableInfoY,
                        VisualVariableInfoZ = cimVisualVariableInfoZ
                    }
                };
                //Apply the visual variables to the renderer's VisualVariables property
                renderer.VisualVariables = listCIMVisualVariables.ToArray();
                //Apply the renderer to the feature layer
                lyr.SetRenderer(renderer);
            });
        }
    }
}
