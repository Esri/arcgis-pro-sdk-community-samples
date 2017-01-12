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


using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace FontStyles
{
    /// <summary>
    /// This Sample provides a new dock pane which shows the different font styles available in ArcGIS Pro.
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 1. This solution is using the **AvalonEdit Nuget**.  If needed, you can install the Nuget from the "Nuget Package Manager Console" by using this script: "Install-Package AvalonEdit".
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open. 
    /// 1. Open a map view. Click on the ADD-IN tab on the ribbon.
    /// 1. Within this tab there is a "Show DockPane" button that when clicked will open a DockPane.
    /// 1. The dock pane has a tabbed control with 2 tabs 'Controls' and 'Xaml'.
    /// 1. The Controls tab shows set of textboxes, Labels and Links.
    /// ![Controls](Screenshots/Controls.png)
    /// 1. Hover the mouse on any control and the style used for that control will be shown as Tooltip 
    /// 1. The Xaml tab shows the xaml code used for creating the controls in the controls tab.
    /// ![Controls](Screenshots/XAML.png)
    /// 1. The Xaml code can be copied for reuse.
    /// </remarks>
    internal class Module1 : Module
    {
        private static Module1 _this = null;

        
        public static Module1 Current
        {
            get
            {
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("FontStyles_Module"));
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
