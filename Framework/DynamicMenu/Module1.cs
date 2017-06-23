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

namespace FeatureDynamicMenu
{/// <summary>
 /// This sample shows how to display a dynamic context menu. The sample also have an embeddabel control on the map that allows you pick the layer you want to select.   
 /// When you select features with this tool, the oids of the feaures will be displayed in a dynamic context menu. 
 /// When you click one of the OIDs in this context menu, that feature will flash on the Map View and display a pop-up. 
 /// </summary>
 /// <remarks>
 /// 1. In Visual Studio click the Build menu. Then select Build Solution.
 /// 1. This solution is using the **Extended.Wpf.Toolkit NuGet**.  If needed, you can install the NuGet from the "NuGet Package Manager Console" by using this script: "Install-Package Extended.Wpf.Toolkit".
 /// 1. Click Start button to open ArcGIS Pro.
 /// 1. ArcGIS Pro will open. 
 /// 1. Open a map view. The map should contain a few feature layers.
 /// 1. Click on the Add-In tab on the ribbon.
 /// 1. Within this tab there is a Display Dynamic Menu tool. Click it to activate the tool.
 /// 1. In the map click a point around which you want to identify features and display them in a dynamic menu.
 /// 1. A dynamic menu with the OIds of the features selected will display.
 /// 1. Click one of the items in the menu. You will see the feature flash on the map and their attributes will be displayed in a pop-up.
 ///![UI](Screenshots/DynamicMenu.png)
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("DynamicMenu_Module"));
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
