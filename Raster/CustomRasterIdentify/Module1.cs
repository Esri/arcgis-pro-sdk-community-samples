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
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace CustomRasterIdentify
{
  /// <summary>
  /// This sample shows how to author a tool that can be used to identify raster pixel values and display 
  /// the results in a custom pop-up window. The popup window will show pixel values for the 
  /// rendered raster and the source raster dataset.
  /// Note: The identify is authored to query any raster, mosaic or image service layer(s) in the map that 
  /// are selected in the Contents pane.
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 2. Click Start button to open ArcGIS Pro.
  /// 3. ArcGIS Pro will open. 
  /// 4. Open a map view and add a raster, mosaic dataset or image service to the map. Select the layer(s) you want to identify in the Contents pane.
  /// 5. Click on the Add-In tab on the ribbon.
  /// 5. Within this tab there is a Custom Raster Identify tool. Click it to activate the tool.
  /// 6. In the map click a point on the raster you want to identify pixel values for.
  /// 7. The pop-up window should display and you should see the results of the identify.
  /// 8. You can click through the popup pages if you have multiple layers selected. Each page shows you the results for a selected raster, mosaic or image service layer.
  /// 9. Press the escape key if you want to deactivate the tool.
  /// ![UI](Screenshots/Screenshot1.png)
  /// </remarks>
  internal class Module1 : Module
    {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current
        {
            get
            {
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("CustomRasterIdentify_Module"));
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
