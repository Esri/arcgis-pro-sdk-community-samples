//   Copyright 2018 Esri
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
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace WorkingWithRasterLayers
{
    /// <summary>
    /// This sample demonstrates how to work with raster layers in ArcGIS Pro.  The samples includes these functions:
    /// 1.) Create a new image service layer and add the layer to the current map.
    /// 2.) Set the compression type and quality to JPEG and 85 on the selected image service layer.
    /// 3.) Set the processing template to Natural Color on the selected image service layer.
    /// 4.) Set the stretch type to Percent Clip and enable DRA on the selected image service layer.
    /// 5.) Set the resampling type to Nearest Neighbor on the selected image service layer.
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 2. Click Start button to open ArcGIS Pro.
    /// 3. ArcGIS Pro will open. 
    /// 4. Open a new map view using the Map tempalte (using this path: "CIMPATH=map/map.xml")
    /// 5. Click on the ADD-IN tab. 
    /// 6. Click the Add Raster Layer button to add a new image service layer to the map.
    /// 7. Click on the other buttons described above to set different properties in the image service layer.
    /// ![UI](Screenshots/Screen.png)
    /// </remarks>
    internal class AddinModule : Module
    {
        private static AddinModule _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static AddinModule Current
        {
            get
            {
                return _this ?? (_this = (AddinModule)FrameworkApplication.FindModule("WorkingWithRasterLayers_Module"));
            }
        }

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

        #endregion Overrides

    }
}
