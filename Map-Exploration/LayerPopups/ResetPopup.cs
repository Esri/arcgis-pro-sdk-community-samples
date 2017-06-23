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
  /// <remarks>Requires the States dataset from the AdminData.gdb downloadable from:
  /// &lt;a href=&quot;https://github.com/Esri/arcgis-pro-sdk-community-samples/releases&quot;/&gt;</remarks>
  internal class ResetPopup : Button {
        protected override void OnClick() {
            string layerName = "U.S. States";
            var usStatesLayer = MapView.Active.Map.GetLayersAsFlattenedList()
                .FirstOrDefault((lyr) => lyr.Name.StartsWith(layerName)) as FeatureLayer;

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
