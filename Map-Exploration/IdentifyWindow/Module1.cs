/*

   Copyright 2019 Esri

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

namespace IdentifyWindow
{
    /// <summary>
    /// This sample illustrates working with ArcGIS Pro's map view and how to interact.  The sample provides the following functionality
    /// 1. Show the layer for the current active map view.
    /// 1. Select features on the current active map view.
    /// 1. Display the attribute data for all selected features.
    /// 1. Display a chart control with data driven by feature selection.
    /// </summary>
    /// <remarks>
    /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data).  The sample data contains a dataset called 'Interacting with Maps' with both 2D and 3D data.  Make sure that the Sample data is unzipped in c:\data and "C:\Data\Interacting with Maps" is available.
    /// 1. Open this solution in Visual Studio.  
    /// 1. Click the build menu and select Build Solution.
    /// 1. This solution is using the **System.Windows.Controls.DataVisualization.Toolkit Nuget**.  If needed, you can install the Nuget from the "Nuget Package Manager Console" by using this script: "Install-Package System.Windows.Controls.DataVisualization.Toolkit".
    /// 1. Click the Start button to open ArCGIS Pro.  ArcGIS Pro will open.
    /// 1. Open the project "Interacting with Maps.aprx" in the "C:\Data\Interacting with Maps" folder since this project contains 2D and 3D data.
    /// 1. Click on the Add-in tab and see that a 'Show my identify' button was added.
    /// 1. The 'Show my identify' button opens the 'My Identify' pane. 
    /// 1. Click the 'Select' button and 'rubber band over the features on your map pane.
    /// ![UI](Screenshots/Screen1.png)
    /// 1. Select a single layer from the 'Select Layer' drop down.
    /// 1. Both the grid and chart controls are now displaying data for the selected feature set
    /// ![UI](Screenshots/Screen2.png)
    /// 1. Switch to the Portland 3D City map view and perform the same feature selection on the map view and then the 'select layer' drop down selection on the 'My Identify' pane
    /// ![UI](Screenshots/Screen3.png)
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("IdentifyWindow_Module"));
            }
        }

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            return true;
        }

        #endregion Overrides

    }
}
