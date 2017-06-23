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

namespace OpenMapViews
{    /// <summary>
     /// This sample shows how to show / hide mapview panes associated with multiple maps. 
     /// </summary>
     /// <remarks>
     /// 1. In Visual Studio click the Build menu. Then select Build Solution.
     /// 1. Click Start button to open ArcGIS Pro.
     /// 1. ArcGIS Pro will open. 
     /// 1. Open the "C:\Data\Admin\AdminSample.aprx" project.
     /// 1. Click on the Add-In tab on the ribbon.
     /// 1. Within this tab there is a **Show Open Mapviews Dockpane** button. Click on the button to activate the Open Mapviews Dockpane.
     /// ![UI](Screenshots/Screen1.png)
     /// 1. On the dockpane click on one of the states to view the associated mapview pane.
     /// ![UI](Screenshots/Screen2.png)
     /// 1. On the dockpane click on one of the states where 'Has Mapview' to hide the associated mapview pane.
     /// ![UI](Screenshots/Screen3.png)
     /// 1. Open the catalog view to see which maps were added to your project.
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("OpenMapViews_Module"));
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
