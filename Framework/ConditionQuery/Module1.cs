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
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace ConditionQuery {
    /// <summary>
    /// This Sample queries the application state to determine which conditions are currently enabled.
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 1. This solution is using the **AvalonEdit Nuget**.  If needed, you can install the Nuget from the "Nuget Package Manager Console" by using this script: "Install-Package AvalonEdit".
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open.
    /// 1. Open any ArcGIS Pro project file containing data.
    /// 1. If the project doesn't have a map view add a new map view, and if the project doesn't have a layout view add a layout view.
    /// 1. Click on the Add-in tab and see the 'Show Conditions' button.
    /// ![UI](Screenshots/Screenshot1.png)
    /// 1. Click the 'Show Conditions' button to bring up the 'Conditions' dockpane into view.
    /// 1. Open the 'Active States' and the 'Selected Condition XML' panes and select a condition under 'Enabled Conditions'.  This will show the 'Condition XML' for the selected condition, if a XML condition has been defined for the selected condition.
    /// ![UI](Screenshots/Screenshot2.png)
    /// 1. Select the map view as the active view and click the refresh button on the Condition dock pane.  You should now find the 'esri_mapping_mapPane' condition under 'Active States'.
    /// ![UI](Screenshots/Screenshot3.png)
    /// 1. Select the layout view as the active view and click the refresh button on the Condition dock pane.  You should now find that the 'esri_mapping_mapPane' condition is not listed under 'Active States' anymore.
    /// ![UI](Screenshots/Screenshot4.png)
    /// </remarks>
    internal class Module1 : Module {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current
        {
            get
            {
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("ConditionQuery_Module"));
            }
        }

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload() {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

        #endregion Overrides

    }
}
