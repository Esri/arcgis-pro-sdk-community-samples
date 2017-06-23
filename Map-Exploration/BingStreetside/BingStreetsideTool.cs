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
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace BingStreetside
{
    internal class BingStreetsideTool : MapTool
    {
        /// <summary> 
        ///  Constructor 
        /// </summary> 
        public BingStreetsideTool()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Point;
            SketchOutputMode = SketchOutputMode.Map;
        }


        protected override Task OnToolActivateAsync(bool active)
        {
            return base.OnToolActivateAsync(active);
        }

        //Get the map coordinates from the click point and set the property on the ViewModel. 
        /// <summary> 
        ///  On sketch completion find the intersecting features, flash features and show the number of features selected per layer 
        /// </summary> 
        /// <param name="geometry"></param> 
        /// <returns></returns> 
        protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            double lat;
            double lng;
            var coord = GeometryEngine.Instance.Project(geometry, SpatialReferences.WGS84) as MapPoint;
            if (coord != null)
            {
                lng = coord.X;
                lat = coord.Y;
                BingStreetsideModule.SetMapLocationOnBingMaps(lng, lat);
            }
            var ret = QueuedTask.Run(() =>
            {
                BingStreetsideModule.ShowCurrentBingMapCoord(coord);
                //await ActiveMapView.ZoomToAsync(camera, new TimeSpan(0, 0, 1));
                return true;
            });

            return ret;
        }
    }
}
