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

namespace CustomStyling
{
    /// <summary>
    /// Show how to apply custom styles to a UserControl that work in both Light and Dark modes.
    /// </summary>
    /// <remarks>
    /// New at 1.4, ArcGIS Pro supports a Light theme, Dark theme, and a variant of the Dark theme for use with High Contrast. Developers who want their Add-ins to blend in with Pro must likewise style their Add-in UIs to provide a Light and Dark themeing. In most cases, developers need only apply 'ESRI' styles to their UserControl content (refer to the ProGuide at [ProGuide: Style Guide](https://github.com/esri/arcgis-pro-sdk/wiki/proguide-style-guide). However, there may be situations where you need to derive your own styles and have them switch between a custom Light and Dark mode to mirror the current theme being applied to Pro. This sample shows you how.  
    /// The sample contains a custom UserControl as well as a custom Light and Dark theme that is applied at runtime in conjunction with the Light or Dark theme applied to Pro.  
    /// Steps:  
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. The style applied to the Dockpane custom UserControl should match the Light or Dark mode of Pro.
    ///![UI](Screenshots/Screen1.png)
    /// 1. Toggle the Pro theme (via backstage->Options->Application->General settings). Restart Pro.
    /// 1. Re-start the Debugger and Re-open the Dockpane in ArcGIS Pro.
    /// 1. The custom style should have changed (Light to Dark or vice versa) to match the corresponding change made to Pro.
    ///![UI](Screenshots/Screen2.png)
    /// 1. Please also refer to the companion ProGuide at [ProGuide: Applying Custom Styles](https://github.com/Esri/arcgis-pro-sdk/wiki/ProGuide-Applying-Custom-Styles) for more information.  
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("CustomStyling_Module"));
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
