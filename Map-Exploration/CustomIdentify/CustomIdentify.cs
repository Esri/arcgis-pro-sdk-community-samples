//   Copyright 2017 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace CustomIdentify
{
    /// <summary>
    /// Implementation of custom pop-up tool.
    /// </summary>
    class CustomIdentify : MapTool
    {
        /// <summary>
        /// Define the tool as a sketch tool that draws a rectangle in screen space on the view.
        /// </summary>
        public CustomIdentify()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Rectangle;
            SketchOutputMode = SketchOutputMode.Screen;
        }
        /// <summary>
        /// Called when a sketch is completed.
        /// </summary>
        protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            var popupContent = await QueuedTask.Run(() =>
            {
                var popupContents = new List<PopupContent>();
                var mapView = MapView.Active;
                if (mapView != null) {
                    //Get the features that intersect the sketch geometry.
                    var features = mapView.GetFeatures(geometry);
                    if (features.Count > 0) {
                        foreach (var kvp in features) {
                            var bfl = kvp.Key;
                            var oids = kvp.Value;
                            foreach (var objectID in oids) {
                                popupContents.Add(new DynamicPopupContent(bfl, objectID));
                            }
                        }
                    }
                }
                return popupContents;
            });

            MapView.Active.ShowCustomPopup(popupContent);
            return true;
        }

        //private static Dictionary<string, FeatureClass> _layersInMapFeatureClassMap = new Dictionary<string, FeatureClass>();

        //public static Dictionary<string, FeatureClass> LayersInMapFeatureClassMap = new Dictionary<string, FeatureClass>();
        
        //private Task<Geodatabase> GetGDBFromLyrAsync(BasicFeatureLayer lyr)
        //{
        //    return QueuedTask.Run(() =>  lyr.GetTable().GetDatastore() as Geodatabase);
        //}
    }
}
