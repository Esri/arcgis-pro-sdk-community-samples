//   Copyright 2016 Esri
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
            var popupContent = await QueuedTask.Run(async () =>
            {
                var mapView = MapView.Active;
                if (mapView == null)
                    return null;

                //Get the features that intersect the sketch geometry.
                var features = mapView.GetFeatures(geometry);

                if (features.Count == 0)
                    return null;
                
                var firstLyr =
                     MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(); //get the first layer in the map

                if (firstLyr == null)
                    return null;
                var gdb = await GetGDBFromLyrAsync(firstLyr);

                LayersInMapFeatureClassMap = Module1.GetMapLayersFeatureClassMap(gdb);

                var oidList = features[firstLyr]; //gets the OIds of all the features selected.
                var oid = firstLyr.GetTable().GetDefinition().GetObjectIDField(); //gets the OId field
                var qf = new QueryFilter() //create the query filter
                {
                    WhereClause = string.Format("({0} in ({1}))", oid, string.Join(",", oidList))
                };

                //Create the new selection
                Selection selection = firstLyr.Select(qf);
                var relateInfo = new RelateInfo(firstLyr, selection); 

                return await relateInfo.GetPopupContent(features); //passes the selection to gather the relationShip class information.

            });

            MapView.Active.ShowCustomPopup(popupContent);
            return true;
        }

        //private static Dictionary<string, FeatureClass> _layersInMapFeatureClassMap = new Dictionary<string, FeatureClass>();

        public static Dictionary<string, FeatureClass> LayersInMapFeatureClassMap = new Dictionary<string, FeatureClass>();
        
        private async Task<Geodatabase> GetGDBFromLyrAsync(BasicFeatureLayer lyr)
        {
            Geodatabase geodatabase = null;
            await QueuedTask.Run(() => geodatabase = (lyr.GetTable().GetDatastore() as Geodatabase));
            return geodatabase;
        }
    }
}
