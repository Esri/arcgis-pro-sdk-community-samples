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
using LayerPopups.Helpers;

namespace LayerPopups {
    /// <summary>
    /// Resets the popup definition for the U.S. States layer back to 'default'
    /// </summary>
    // <remarks>Requires the States dataset from the AdminData.gdb downloadable from:
    /// <a href="https://github.com/Esri/arcgis-pro-sdk-community-samples/releases"/></remarks>
    internal class ResetPopup : Button {
        protected override void OnClick() {
            string layerName = "States";
            var usStatesLayer = MapView.Active.Map.GetLayersAsFlattenedList()
                .FirstOrDefault((lyr) => lyr.Name == layerName) as FeatureLayer;
            if (usStatesLayer == null) {
                MessageBox.Show(
                    "Please add the 'States' layer to the TOC from the Pro community samples AdminData.gdb geodatabase.",
                    "Cannot find US States");
                return;
            }

            //No need to await this
            QueuedTask.Run(() => {
                //To reset the popup back to its "out-of-box" default
                //set the popup info to null
                usStatesLayer.SetPopupInfo(null);
            });

            MessageBox.Show(
                "The popup for U.S. States has been reset",
                "U.S. States Popup Reset");
        }
    }
}
