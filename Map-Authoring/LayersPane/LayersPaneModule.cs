/*

   Copyright 2016 Esri

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
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace LayersPane
{
    /// <summary>
    /// The Layers pane sample uses a docking pane to query and display feature classes and their content.  Access to feature classes is provided through the layers of the active map.
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 2. Click Start button to open ArcGIS Pro.
    /// 3. ArcGIS Pro will open. 
    /// 4. Open any project file that contains a map with layers.  
    /// 5. Click on the Add-in tab on the ribbon and then on the "Open LayersPane" button.
    /// 6. Click the 'Search button' and the pane's grid will display the selected feature layer's columns and data.
    /// 7. Enter a valid SQL where clause like 'objectid = 1' in the text box next to the search button and click Search again.
    /// 8. The data displayed is now restricted to records that match the given where clause.
    /// </remarks>
    internal class LayersPaneModule : Module
    {
        private static LayersPaneModule _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static LayersPaneModule Current
        {
            get
            {
                return _this ?? (_this = (LayersPaneModule)FrameworkApplication.FindModule("LayersPane_Module"));
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
