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

namespace LockToSelectedRasters
{
	/// <summary>
	/// This sample demonstrates how to create a tool that locks to selected rasters on a mosaic layer.
	/// </summary>
	/// <remarks>
	/// 1. In Visual Studio click the Build menu. Then select Build Solution.
	/// 2. Click Start button to open ArcGIS Pro.
	/// 3. ArcGIS Pro will open. 
	/// 4. Open a map view and add one or more mosaic dataset(s) to the map. Zoom to the mosaic layer. 
	/// 5. Select the mosaic layer you want to use the tool on in the Contents pane.
	/// 6. Click on the Add-In tab on the ribbon.
	/// 7. Within this tab there is a Lock To Selected Rasters button. Click it to activate the tool.
	/// 8. On the Data tab, use the selection tools to select items in the mosaic. Alternately select items in the mosaic layer's attribute table.
	/// 9. As soon as you make a selection, the display will refresh to only show the items in the mosaic layer you have selected.
	/// 10. Click the Lock To Selected Rasters button again to deactivate the tool. The display of the mosaic layer will switch back to the original mosaic method before the tool was activated.
	/// ![UI](Screenshots/screenshot1.jpg)
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("LockToSelectedRasters_Module"));
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
