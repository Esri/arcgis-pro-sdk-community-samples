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

namespace CrowdPlannerTool
{
    /// <summary>
    /// This sample shows the use of a construction tool to implement a crowd planning workflow.
    /// </summary>
    /// <remarks>
    /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data).  The sample data contains a map package called 'CrowdPlannerProject.ppkx' which is required for this sample.  Make sure that the Sample data is unzipped in c:\data and "C:\Data\CrowdPlanner" is available.
    /// 1. Open this solution in Visual Studio 2015.  
    /// 1. Click the build menu and select Build Solution.
    /// 1. This solution is using the **System.Windows.Controls.DataVisualization.Toolkit Nuget**.  If needed, you can install the Nuget from the "Nuget Package Manager Console" by using this script: "Install-Package System.Windows.Controls.DataVisualization.Toolkit".
    /// 1. Click the Start button to open ArCGIS Pro.  ArcGIS Pro will open.
    /// 1. Open the map package located in the "CrowdPlannerProject.ppkx" in the "C:\Data\CrowdPlanner" folder since this project contains all required data.
    /// 1. Click on the Add-in tab and see that a 'Crowd Planner Summary' button was added.
    /// 1. The 'Crowd Planner Summary' button opens the 'Crowd Planner' pane. 
    /// ![UI](Screenshots/Screen1.png)
    /// 1. Click the 'Populate Values' button and note that now the pane entry fields have been populated using data from the sample record in the Crowd Planning feature class.
    /// ![UI](Screenshots/Screen2.png)
    /// 1. Select the 'Edit' tab and create a new 'CrowdPlanning' feature by using the CP construction tool.
    /// ![UI](Screenshots/Screen3.png)
    /// 1. Digitize a new polygon and note that its attributes are automatically populated. 
    /// ![UI](Screenshots/Screen4.png)
    /// 1. Finally you can also play with the various value adjustment buttons that are provided on the 'Crowd Planner' pane.
    /// ![UI](Screenshots/Screen5.png)
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("CrowdPlannerTool_Module"));
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
