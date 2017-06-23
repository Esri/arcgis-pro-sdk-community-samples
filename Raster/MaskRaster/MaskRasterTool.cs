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
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.Data.Raster;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Core.Data;
using System.IO;
using ArcGIS.Desktop.Core;

namespace MaskRaster
{
    /// <summary>
    /// A map tool that masks raster pixels based on a rectangle drawn by the user and saves the masked raster 
    /// to a folder in the current project folder. The tool only works on raster layers.
    /// </summary>
    internal class MaskRasterTool : MapTool
    {
        public MaskRasterTool()
        {
            // Indicate the tool is a sketch tool.
            IsSketchTool = true;
            // Set the sketch type of the tool to be Rectangle.
            SketchType = SketchGeometryType.Rectangle;
            // Set the output mode of the sketch to be in map coordinates.
            SketchOutputMode = SketchOutputMode.Map;
        }

        protected override Task OnToolActivateAsync(bool active)
        {
            return base.OnToolActivateAsync(active);
        }

        /// <summary>
        /// Function called when the tool has finished drawing on the map. The function then masks
        /// raster pixels and saves the output masked raster to the project folder.
        /// </summary>
        /// <param name="geometry">The geometry object that is returned by the tool.</param>
        /// <returns>Task that returns true if the function succeeds and false otherwise.</returns>
        protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            // Mask the raster based on the geometry.
            MaskRasterVM.MaskRaster(geometry);
            // Pass the call onwards.
            return await base.OnSketchCompleteAsync(geometry);
        }
    }
}
