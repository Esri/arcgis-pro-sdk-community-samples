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
