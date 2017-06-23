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
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;

namespace CustomRasterIdentify
{
    /// <summary>
    /// A map tool to identify raster pixel values and display the results in a custom pop-up window. The 
    /// popup window will show pixel values for the rendered raster and the source raster dataset.
    /// </summary>
    internal class CustomRasterIdentifyTool : MapTool
    {
        public CustomRasterIdentifyTool()
        {
            // Indicate the tool is a sketch tool.
            IsSketchTool = true;
            // Set the sketch type of the tool to be Point.
            SketchType = SketchGeometryType.Point;
            // Set the output mode of the sketch to be in map coordinates.
            SketchOutputMode = SketchOutputMode.Map;
        }

        protected override Task OnToolActivateAsync(bool active)
        {
            return base.OnToolActivateAsync(active);
        }

        /// <summary>
        /// Function called when the tool has finished drawing on the map. The function then identifies pixel 
        /// values and shows them using a custom popup.
        /// </summary>
        /// <param name="geometry">The geometry object that is returned by the tool.</param>
        /// <returns>Task that returns true if the function succeeds and false otherwise.</returns>
        protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            // Check if the geometry is a point.
            MapPoint identifyPoint = null;
            if (geometry.GeometryType != GeometryType.Point)
                // Pass the call onwards.
                return await base.OnSketchCompleteAsync(geometry);
            else
            {
                // If the geomtry is a point,
                identifyPoint = geometry as MapPoint;
                // Show the custom popup based on the point.
                CustomRasterIdentifyVM.CustomRasterIdentify(identifyPoint);
                // Pass the call onwards.
                return await base.OnSketchCompleteAsync(geometry);
            }
        }
    }
}
