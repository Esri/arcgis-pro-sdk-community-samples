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
using ArcGIS.Desktop.Mapping.Controls;
using ArcGIS.Desktop.Mapping.Events;

namespace OverviewMapControl
{
    /// <summary>
    /// This sample shows how to author a map control.  
    /// In this example we will be creating a map control that will act as an overview window of the active map. 
    /// When the view changes, the map control will display the changed view. You will also be able navigate inside the map control and see the Active map view mirror the map control's extent.
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 2. Click Start button to open ArcGIS Pro.
    /// 3. ArcGIS Pro will open. 
    /// 4. Open any project with map views. Make sure a map is active.
    /// 5. Within the "Map Control" tab click the "Show Map Control" button.
    /// 6. A dockpane will open up with an embedded Map Control displaying the overview of the Active map view. A red overview rectangle is displayed inside the map control showing the extent of the active map view.
    /// 7. Pan or zoom inside the active map view.  Notice that the Map conrol extent changes to reflect this.
    /// 8. Pan or zoom inside the map control. Notice that the active map view changes to reflect this.
    ///
    ///![UI](screenshots/mapcontrol.png)
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("OverviewMapControl_Module"));
            }
        }      
    }
}
