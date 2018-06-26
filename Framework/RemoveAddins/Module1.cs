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

namespace RemoveAddins
{

    /// <summary>
    /// This sample shows how to customize the backstage by adding a new tab. 
    /// In this example we will add a new tab in the backstage called Remove Add-Ins. Add-ins installed on that machine will be listed in that tab and you will be able to delete them.
    /// Adding and removing add-ins requires an application restart to take effect.
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 2. Click Start button to open ArcGIS Pro.
    /// 3. ArcGIS Pro will open. 
    /// 4. Open a blank project.
    /// 5. Click the Project tab to display the backstage.
    /// 6. Select the new Remove Add-in tab added to the backstage on the left.
    /// 7. The Add-in Folder drop down will display all the "well-known" add-in folders registered on your machine. 
    /// 8. Add-ins available list box will display all the add-ins found in each well-known add-in folder.
    /// 9. You can select any add-in(s) and click the Delete button to remove them.
    ///Note: Adding and removing add-ins requires an application restart to take effect.
    ///![UI](screenshots/RemoveAdd-Ins.png)
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("RemoveAddins_Module"));
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
