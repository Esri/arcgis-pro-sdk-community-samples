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
using ArcGIS.Core;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace CIMExamples {
    /// <summary>
    /// In this case, we will make the same renderer as was made from scratch with <b>CreateCIMRendererFromScratch</b> except, in this
    /// case we will use the UniqueValueRendererDefinition class and the layer to configure the underlying Renderer
    /// </summary>
    internal class CreateCIMRendererWithDefinition : Button {

        private CIMRenderer _layerCimRenderer = null;

        protected override async void OnClick() {
            var usStatesLayer =
                MapView.Active.Map.GetLayersAsFlattenedList().FirstOrDefault((fl) => fl.Name == "States") as
                    FeatureLayer;
            if (usStatesLayer == null) {
                MessageBox.Show("Please add the US States layer to the TOC", "Cannot find US States");
                return;
            }

            await QueuedTask.Run(() => {
                if (_layerCimRenderer == null)
                    _layerCimRenderer = CreateniqueValueRendererForUSStatesUsingDefinition(usStatesLayer);
                //For examination in the debugger...
                var renderer = usStatesLayer.GetRenderer();
                string xmlDef = renderer.ToXml();
                string xmlDef2 = _layerCimRenderer.ToXml();

                usStatesLayer.SetRenderer(_layerCimRenderer);
            });
        }

        private CIMRenderer CreateniqueValueRendererForUSStatesUsingDefinition(FeatureLayer featureLayer) {

            //All of these methods have to be called on the MCT
            if (Module1.OnUIThread)
                throw new CalledOnWrongThreadException();

            // color ramp
            CIMICCColorSpace colorSpace = new CIMICCColorSpace() {
                URL = "Default RGB"
            };

            CIMContinuousColorRamp continuousColorRamp = new CIMLinearContinuousColorRamp();
            continuousColorRamp.FromColor = CIMColor.CreateRGBColor(255,255,100); // yellow
            continuousColorRamp.ToColor = CIMColor.CreateRGBColor(255,0,0);     // red
            continuousColorRamp.ColorSpace = colorSpace;

            CIMRandomHSVColorRamp randomHSVColorRamp = new CIMRandomHSVColorRamp() {
                ColorSpace = colorSpace,
                MinAlpha = 100,
                MaxAlpha = 100,
                MinH = 0,
                MaxH = 360,
                MinS = 15,
                MaxS = 30,
                MinV = 99,
                MaxV = 100,
                Seed = 0
            };

            UniqueValueRendererDefinition uvRendererDef = new UniqueValueRendererDefinition() {
                ColorRamp = continuousColorRamp, // randomHSVColorRamp,
                UseDefaultSymbol = true,
                ValueFields = new string[] { "TOTPOP2010" }
            };
            //Configure the Renderer using the layer and the contents of the STATENAM
            //field
            return featureLayer.CreateRenderer(uvRendererDef);      
        }
    }
}
