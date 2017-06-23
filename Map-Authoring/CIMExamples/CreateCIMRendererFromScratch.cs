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
    /// Provide sample to create CIMUniqueValueRenderer from scratch - when there is no access to data
    /// or no access to data is desired
    /// </summary>
    internal class CreateCIMRendererFromScratch : Button {

        private CIMRenderer _layerCimRenderer = null;

        protected override async void OnClick() {

            var usStatesLayer =
                MapView.Active.Map.GetLayersAsFlattenedList().FirstOrDefault((fl) => fl.Name == "States") as
                    FeatureLayer;
            if (usStatesLayer == null) {
                MessageBox.Show("Please add the 'States' layer to the TOC - with a 'STATE_NAME' field containing state names.", "Cannot find US States");
                return;
            }

            await QueuedTask.Run(() => {
                if (_layerCimRenderer == null)
                    _layerCimRenderer = CreateUniqueValueRendererForUSStates();
                //For examination in the debugger...
                var renderer = usStatesLayer.GetRenderer();
                string xmlDef = renderer.ToXml();
                string xmlDef2 = _layerCimRenderer.ToXml();

                usStatesLayer.SetRenderer(_layerCimRenderer);
            });

        }

        /// <summary>
        /// Warning! You must call this method on the MCT!
        /// </summary>
        /// <returns></returns>
        private CIMRenderer CreateUniqueValueRendererForUSStates() {
            //All of these methods have to be called on the MCT
            if (Module1.OnUIThread)
                throw new CalledOnWrongThreadException();

      //Create the Unique Value Renderer
      CIMUniqueValueRenderer uniqueValueRenderer = new CIMUniqueValueRenderer()
      {

        // set the value field
        Fields = new string[] { "STATE_NAME" }
      };

      //Construct the list of UniqueValueClasses
      List<CIMUniqueValueClass> classes = new List<CIMUniqueValueClass>();

            // Alabama
            List<CIMUniqueValue> alabamaValues = new List<CIMUniqueValue>();
      CIMUniqueValue alabamaValue = new CIMUniqueValue()
      {
        FieldValues = new string[] { "Alabama" }
      };
      alabamaValues.Add(alabamaValue);

            var alabamaColor = CIMColor.CreateRGBColor(255, 170, 0);

            var alabama = new CIMUniqueValueClass() {
                Values = alabamaValues.ToArray(),
                Label = "Alabama",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() {Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(alabamaColor)}
            };

            classes.Add(alabama);

            // Alaska
            List<CIMUniqueValue> alaskaValues = new List<CIMUniqueValue>();
      CIMUniqueValue alaskaValue = new CIMUniqueValue()
      {
        FieldValues = new string[] { "Alaska" }
      };
      alaskaValues.Add(alaskaValue);

            var alaskaColor = CIMColor.CreateRGBColor(255, 0, 0);

            var alaska = new CIMUniqueValueClass() {
                Values = alaskaValues.ToArray(),
                Label = "Alaska",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(alaskaColor) }
            };

            classes.Add(alaska);

            // California
            List<CIMUniqueValue> californiaValues = new List<CIMUniqueValue>();
      CIMUniqueValue californiaValue = new CIMUniqueValue()
      {
        FieldValues = new string[] { "California" }
      };
      californiaValues.Add(californiaValue);

            var californiaColor = CIMColor.CreateRGBColor(85, 255, 0);

            var california = new CIMUniqueValueClass() {
                Values = californiaValues.ToArray(),
                Label = "California",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(californiaColor) }
            };

            classes.Add(california);

            // Colorado
            List<CIMUniqueValue> coloradoValues = new List<CIMUniqueValue>();
      CIMUniqueValue coloradoValue = new CIMUniqueValue()
      {
        FieldValues = new string[] { "Colorado" }
      };
      coloradoValues.Add(coloradoValue);

            var coloradoColor = CIMColor.CreateRGBColor(0, 92, 230);

            var colorado = new CIMUniqueValueClass() {
                Values = coloradoValues.ToArray(),
                Label = "Colorado",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(coloradoColor) }
            };

            classes.Add(colorado);

      // so on and so forth for all the 51.
      //....

      //Add the classes to a group (by default there is only one group or "symbol level")
      // Unique value groups
      CIMUniqueValueGroup groupOne = new CIMUniqueValueGroup()
      {
        Heading = "State Names",
        Classes = classes.ToArray()
      };
      uniqueValueRenderer.Groups = new CIMUniqueValueGroup[] { groupOne };

            //Draw the rest with the default symbol
            uniqueValueRenderer.UseDefaultSymbol = true;
            uniqueValueRenderer.DefaultLabel = "All other values";

            var defaultColor = CIMColor.CreateRGBColor(215, 215, 215);
            uniqueValueRenderer.DefaultSymbol = new CIMSymbolReference() {
                Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(defaultColor)
            };
            
            return uniqueValueRenderer as CIMRenderer;
        }
    }
}
