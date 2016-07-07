//   Copyright 2016 Esri
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

namespace MagnifierWindow
{
    /// <summary>
    /// This sample provides a map tool and a window with map control.
    /// The position of the map control window is updated based on the current map tool position.
    /// The map control content is created from the currently active MAP view and the map control shows a magnified view of where the mouse is positioned in the active MAP view.
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open. 
    /// 1. Open a 2D map view for example: C:\Data\FeatureTest\FeatureTest.aprx from the sample dataset.
    /// 1. Click on the ADD-IN tab
    /// 1. Click on the Magnifier tool
    /// 1. A map control window will open up that shows the same content as in your main 2D map view
    /// 1. Move the mouse in the main view and the camera in map control window will update to show you a magnified view of where you are in the main 2D map view. Additionally, the current geo-coordinates in the main view are also displayed in the lower left corner of the map control window
    /// 1. You can press the ESC key to deactivate the Magnifier tool
    ///![UI](Screenshots/Screen1.png)
    /// NOTE - the magnifier tool only works in 2D map views but can be enhanced to work in 3D too.
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("MagnifierWindow_Module"));
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
