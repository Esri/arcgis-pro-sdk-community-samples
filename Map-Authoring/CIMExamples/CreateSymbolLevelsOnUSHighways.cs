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
    /// In this sample we show how to create the equivalent of symbol levels in Pro.
    /// We assume the project contains a dataset for US Highways. We will create a symbol level
    /// for each US Highway value.<br/>
    /// It is a prerequisite that the layer MUST HAVE a Unique Value Renderer or other Class "type" Renderer
    /// with a corresponding number of values to which groups can be assigned
    /// </summary>
    internal class CreateSymbolLevelsOnUSHighways : Button {

        private CIMRenderer _layerCimRenderer = null;

        protected override async void OnClick() {
         var usHighwaysLayer =
                MapView.Active.Map.GetLayersAsFlattenedList().FirstOrDefault((fl) => fl.Name == "USHighways") as
                    FeatureLayer;
         if (usHighwaysLayer == null) {
                MessageBox.Show("Please add the US HIghways layer to the TOC", "Cannot find US Highways");
                return;
            }

            await QueuedTask.Run(() => {
                if (_layerCimRenderer == null)
                    _layerCimRenderer = CreateniqueValueRendererForUSHighwaysUsingDefinition(usHighwaysLayer);
                //For examination in the debugger...
                var renderer = usHighwaysLayer.GetRenderer();
                //We will use a UniqueValue renderer on the US Highways
                if ((renderer as CIMUniqueValueRenderer) == null)
                    usHighwaysLayer.SetRenderer(_layerCimRenderer);

                //Now, create the symbol levels
                SetUpSymbolLevelsForUSHighways(usHighwaysLayer);
            });
        }

        /// <summary>
        /// Call this method to apply symbol groups to the featureLayer - one group per value in the renderer.
        /// The first group to be added will be the first group to be drawn
        /// </summary>
        /// <param name="featureLayer"></param>
        private void SetUpSymbolLevelsForUSHighways(FeatureLayer featureLayer) {

            //All of these methods have to be called on the MCT
            if (Module1.OnUIThread)
                throw new CalledOnWrongThreadException();

            CIMBaseLayer baseLayer = featureLayer.GetDefinition();
            //We need CIMGeoFeatureLayerBase because this class controls whether or not we
            //use 'groups' (ie Pro Symbol Levels) with the renderer
            CIMGeoFeatureLayerBase geoFeatureLayer = baseLayer as CIMGeoFeatureLayerBase;

            // assume the unique value renderer was created using the CreateCIMRenderer()
            CIMUniqueValueRenderer uniqueValueRenderer = geoFeatureLayer.Renderer as CIMUniqueValueRenderer;

            CIMSymbolLayerDrawing symbolLayerDrawing = new CIMSymbolLayerDrawing()
            {

                // This flag controls the drawing code and forces it to use defined symbol layers.
                //It must be set 'true'
                UseSymbolLayerDrawing = true
            };

            // setup the symbol layers.
            List<CIMSymbolLayerIdentifier> symbolLayers = new List<CIMSymbolLayerIdentifier>();

            // this will be a for loop that will iterate over the unique value classes and updating the symbol in each class        
            int index = 0;
            foreach (CIMUniqueValueGroup nextGroup in uniqueValueRenderer.Groups) {
                foreach (CIMUniqueValueClass nextClass in nextGroup.Classes) {
                    CIMMultiLayerSymbol multiLayerSymbol = nextClass.Symbol.Symbol as CIMMultiLayerSymbol;
                    if (multiLayerSymbol == null) //This check probably is not needed
                        continue;
                    //Each group must be uniquely named
                    string uniqueName = "Group_" + index.ToString();
                    nextClass.Symbol.SymbolName = uniqueName;

                    for (int i = 0; i < multiLayerSymbol.SymbolLayers.Length; i++)
                        //Assign the unique name to all of the layers in the symbol
                        multiLayerSymbol.SymbolLayers[i].Name = uniqueName;

                    index++;
                    //Assign the name to a 'CIMSymbolLayerIdentifier'. This is the equivalent
                    //of a KeyValuePair in a Dictionary. The Names of each SymbolLayerIdentifier
                    //will be matched up in the renderer to a corresponding symbol (via nextClass.Symbol.SymbolName)
                    //So that each SymbolLayer is associated with a specific symbol for a specific value
                    CIMSymbolLayerIdentifier nextSymbolLayer = new CIMSymbolLayerIdentifier() {
                        SymbolLayerName = uniqueName
                    };

                    symbolLayers.Add(nextSymbolLayer);
                    
                }
            }
            //This is where the symbol layers get added to the feature layer definition
            symbolLayerDrawing.SymbolLayers = symbolLayers.ToArray();
            geoFeatureLayer.SymbolLayerDrawing = symbolLayerDrawing;

            // update the featureLayer definition.
            featureLayer.SetDefinition(geoFeatureLayer as  CIMBaseLayer);
        }


        private CIMRenderer CreateniqueValueRendererForUSHighwaysUsingDefinition(FeatureLayer featureLayer) {

            //All of these methods have to be called on the MCT
            if (Module1.OnUIThread)
                throw new CalledOnWrongThreadException();

            // color ramp
            CIMICCColorSpace colorSpace = new CIMICCColorSpace() {
                URL = "Default RGB"
            };

            CIMContinuousColorRamp continuousColorRamp = new CIMLinearContinuousColorRamp()
            {
                FromColor = CIMColor.CreateRGBColor(0, 0, 255), // yellow
                ToColor = CIMColor.CreateRGBColor(255, 0, 0),     // red
                ColorSpace = colorSpace
            };
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
                ColorRamp = continuousColorRamp, //randomHSVColorRamp,
                UseDefaultSymbol = true,
                ValueFields = new string[] {"ROUTE_NUM"}
            };
            //Configure the Renderer using the featureLayer and the contents of the STATENAM
            //field
            return featureLayer.CreateRenderer(uvRendererDef);      
        }
    }
}
