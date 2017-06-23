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

namespace CIMExamples {
    /// <summary>
    /// Change the selection color for the given feature layer
    /// </summary>
    internal class ChangeLayerSelectionColor : Button {

        protected override async void OnClick() {

            //So, arbitrarily, we pick the first feature layer and we just assume there is one ;-)
            var layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>()
                .FirstOrDefault((fl) => fl.ShapeType == esriGeometryType.esriGeometryPolygon || fl.ShapeType == esriGeometryType.esriGeometryPolyline);
            if (layer == null) {
                MessageBox.Show("Please add a polygon or polyline layer to the Map to use this sample", "No Polygon or Polyline Layer Found");
                return;
            }

            await QueuedTask.Run(() => {
                var layerDef = layer.GetDefinition() as CIMBasicFeatureLayer;
                layerDef.UseSelectionSymbol = false;
                layerDef.SelectionColor = ColorFactory.Instance.RedRGB;
                layer.SetDefinition(layerDef);
                if (!layer.IsVisible) layer.SetVisibility(true);
                 //Do a selection
                MapView.Active.SelectFeatures(MapView.Active.Extent);
            });

        }
    }
}
