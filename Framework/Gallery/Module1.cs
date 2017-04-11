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

namespace GalleryDemo
{
    /// <summary>
    /// This sample shows how to author a gallery on the ArcGIS Pro Ribbon.  
    /// In this example we will be creating an inline gallery control that will host a collection of web maps downloaded from ArcGIS Online. 
    /// When you click on a web map item in the gallery, it will open in a map pane in ArcGIS Pro. 
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 2. This solution is using the Newtonsoft.Json Nuget. If needed, you can install the Nuget from the "Nuget Package Manager Console" by using this script: "Install-Package Newtonsoft.Json".
    /// 2. Click Start button to open ArcGIS Pro.
    /// 3. ArcGIS Pro will open. 
    /// 4. Open a blank project.
    /// 5. Within the "Gallery Demo" tab you will notice an in-line gallery control with a collection of 10 web maps from ArcGIS Online.
    /// 6. Click any one of the web map items to select it.
    /// 7. The map will be downloaded from ArcGIS Online and opened in a map pane in the application.    
    ///
    ///![UI](screenshots/webmapgallery.png)
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("GalleryDemo_Module"));
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
