//   Copyright 2019 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System.Collections.Generic;
using System.Threading.Tasks;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace GeoprocessingExecuteAsync
{
    /// <summary>
    /// This sample a map tool to Addin Tab. You click the tool and draw a line
    /// on the map. The tool then passes the line geometry to Geoprocessing ExecuteToolAsync
    /// method which draws a buffer around that line.
    /// </summary>
    /// <remarks>
    ///1. In Visual Studio click the Build menu. Then select Build Solution.
    ///2. Click Start button to open ArcGIS Pro.
    ///3. ArcGIS Pro will open. 
    ///4. Open a map view and zoom in to your area of interest. Click on BufferGeometryTool on the ADD-IN TAB 
    ///and click several places to draw a line. End the line with a double-click.
    ///5. Double-click ends the line and ExecuteAsync tool is called with this line
    ///as the input to Buffer (Analysis Tools toolbox) tool.
    ///6. Once the execution of Buffer tool is complete, the buffered polygon as added to display.
    /// </remarks>
    internal class BufferGeometryTool : MapTool
    {
        /// <summary>
        /// Constructor of BufferGeometry tool
        /// </summary>
        public BufferGeometryTool()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Line;   // Create a line geometry
            SketchOutputMode = SketchOutputMode.Map;
        }

        /// <summary>
        /// Constructs the value array to be passed as parameter to ExecuteToolAsync
        /// Runs the Buffer tool of Analysis toolbox
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns>Geoprocessing result object as a Task</returns>
        protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            var valueArray = await QueuedTask.Run(() =>
            {
                var g = new List<object>() { geometry, };
                // Creates a 8000-meter buffer around the geometry object
                // null indicates a default output name is used
                return Geoprocessing.MakeValueArray(g, null, @"800 Meters");
            });

            await Geoprocessing.ExecuteToolAsync("analysis.Buffer", valueArray);
            return true;
        }
    }
}
