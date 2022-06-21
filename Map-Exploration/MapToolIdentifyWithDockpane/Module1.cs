/*

   Copyright 2019 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       https://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/

using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace MapToolIdentifyWithDockpane
{
    /// <summary>
    /// ProGuide example of a simple tool that performs feature identify function on a map.  The sample tool allows the operator to view the number of features by drawing a rectangle on the map.  I Dockpane is used to display the feature data.  
    /// </summary>
    /// <remarks>
    /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data).  The sample data contains a dataset called 'Interacting with Maps' with both 2D and 3D data.  Make sure that the Sample data is unzipped in c:\data and "C:\Data\Interacting with Maps" is available.
    /// 1. Open this solution in Visual Studio.  
    /// 1. Click the build menu and select Build Solution.
    /// 1. Launch the debugger to open ArCGIS Pro.
    /// 1. Open the project "Interacting with Maps.aprx" in the "C:\Data\Interacting with Maps" folder since this project contains 2D and 3D data.
    /// 1. Open the 2D crime map
    /// 1. Click on the Add-in tab 
    /// 1. Click the 'Identify Features' button and draw a rectangle over the features to show in the dockpane.
    /// ![UI](Screenshots/2MapTool2D.png)
    /// 1. Validate the result.
    /// ![UI](Screenshots/2MapTool2D-2.png)
    /// 1. Switch to the Portland 3D City map view and perform the identify feature function on the 3D scene.
    /// 1. Validate the result.
    /// ![UI](Screenshots/2MapTool3D.png)
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("MapToolIdentifyWithDockpane_Module"));
            }
        }

        /// <summary>
        /// Link to Dockpane view model which allows to update properties of dockpane
        /// </summary>
        internal static MapToolIdentifyDockpaneViewModel MapToolIdentifyDockpaneVM { get; set; }

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
