/*

   Copyright 2018 Esri

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

using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace MapToolZoom
{
    /// <summary>
    /// ProGuide example for simple MapView interaction is demonstrated in this sample with a tool that allows to zoom in and out of the current MapView.  The left mouse click will zoom in and the right mouse click will zoom out.  Unlike the other Map Tool samples this example does not use the sketch capabilities of the MapTool base class instead the sample overrides mouse and keyboard events.  
    /// </summary>
    /// <remarks>
    /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data).  The sample data contains a dataset called 'Interacting with Maps' with both 2D and 3D data.  Make sure that the Sample data is unzipped in c:\data and "C:\Data\Interacting with Maps" is available.
    /// 1. Open this solution in Visual Studio 2015.  
    /// 1. Click the build menu and select Build Solution.
    /// 1. Click the Start button to open ArCGIS Pro.  ArcGIS Pro will open.
    /// 1. Open the project "Interacting with Maps.aprx" in the "C:\Data\Interacting with Maps" folder since this project contains 2D and 3D data.
    /// 1. Open the 2D crime map
    /// 1. Click on the Add-in tab 
    /// 1. Click the 'Zoom In/Out' button and left click on the map somewhere off the map center.
    /// ![UI](Screenshots/3MapTool2D.png)
    /// 1. Validate the that the mouse click point is now at the center of the map view and that the view has zoomed in.
    /// ![UI](Screenshots/3MapTool2D-2.png)
    /// 1. Switch to the Portland 3D City map view and perform the zoom in/out on the 3D scene.
    /// ![UI](Screenshots/3MapTool3D.png)
    /// 1. Validate the zoom in/out functionality.
    /// ![UI](Screenshots/3MapTool3D-2.png)
    /// 1. Use the cursor up and cursor down keys and validate that the zoom in/out is working.
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("MapToolZoom_Module"));
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
