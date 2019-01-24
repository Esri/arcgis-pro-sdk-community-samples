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
using ArcGIS.Desktop.Mapping;

namespace LayerPopups.Helpers {
    /// <summary>
    /// Layer extensions to support updating the popup info
    /// </summary>
    /// <remarks>Mimics the same pattern used by RendererDefinitions in the API</remarks>
    public static class LayerExtensions {

        /// <summary>
        /// Set the popupinfo on the layer definition
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="popupInfo"></param>
        public static void SetPopupInfo(this Layer layer, CIMPopupInfo popupInfo) {
            var layerDef = layer.GetDefinition();
            layerDef.PopupInfo = popupInfo;
            layer.SetDefinition(layerDef);
        }

        /// <summary>
        /// Create a CIMPopupInfo from the given definition 
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="popupDefinition"></param>
        /// <returns></returns>
        public static CIMPopupInfo CreatePopupInfo(this Layer layer, PopupDefinition popupDefinition) {
            return popupDefinition.CreatePopupInfo();
        }
    }
}
