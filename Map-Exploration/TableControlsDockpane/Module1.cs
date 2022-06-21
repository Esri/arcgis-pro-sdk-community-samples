/*

   Copyright 2020 Esri

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace TableControlsDockpane
{
    /// <summary>
    /// This sample demonstrates using multiple TableControls on a dockpane. 
    /// The example shows multiple table controls selectable by a tab control.  A TableControl is filled with content from selected items on the map's Contents TOC dockpane. 
    /// A context menu is added to each table control allowing access to row functionality in the table control. 
    /// </summary>
    /// <remarks>
    /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
    /// 1. Make sure that the Sample data is unzipped in c:\data 
    /// 1. The project used for this sample is 'C:\Data\FeatureTest\FeatureTest.aprx'
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 1. Launch the debugger to open ArcGIS Pro.
    /// 1. Select the FeatureTest.aprx project
    /// 1. Open the current active map's Contents dockpane and select the layers for which to view the attribute tables:
    /// ![UI](Screenshots/Screenshot1.png)
    /// 1. Click on the ADD-IN tab and the click the 'Show AttributeTables' button.
    /// 1. For each selected layer on the Content TOC a tab and the corresponding TableControl is added on the Attribute table viewer dockpane:
    /// ![UI](Screenshots/Screenshot2.png)
    /// 1. Click any row in one of those tables then right click and select 'zoom to feature' from the context menu.
    /// 1. The map will zoom to the selected feature.
    /// ![UI](Screenshots/Screenshot3.png)
    /// 1. Select the 'Edit' tab on the ArcGIS Pro ribbon and 'Create' new features
    /// 1. On the 'Create Features' pane select the 'TestLines' feature layer and create a new line feature on the map.
    /// 1. Note that the table control on the Attribute Table Viewer dockpane is automatically updated to show the newly added feature.
    /// ![UI](Screenshots/Screenshot4.png)
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("TableControlsDockpane_Module"));
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
