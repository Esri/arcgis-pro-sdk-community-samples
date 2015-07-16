//   Copyright 2015 Esri
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

namespace FontStyles
{
    /// <summary>
    /// This Sample provides a new dock pane which shows the different font styles available in ArcGIS Pro.
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 2. Click Start button to open ArcGIS Pro.
    /// 3. ArcGIS Pro will open. 
    /// 4. Open a map view. Click on the ADD-IN tab on the ribbon.
    /// 5. Within this tab there is a "Show DockPane" button that when clicked will open a DockPane.
    /// 6. The dock pane has a tabbed control with 2 tabs 'Controls' and 'Xaml'.
    /// 7. The Controls tab shows set of textboxes, Labels and Links.
    /// ![Controls](Screenshots/Controls.png)
    /// 8. Hover the mouse on any control and the style used for that control will be shown as Tooltip 
    /// 9. The Xaml tab shows the xaml code used for creating the controls in the controls tab.
    /// ![Controls](Screenshots/XAML.png)
    /// 10. The Xaml code can be copied for reuse.
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
