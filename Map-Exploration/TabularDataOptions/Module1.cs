/*

   Copyright 2025 Esri

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
using System.Threading.Tasks;
using System.Windows.Input;
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
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;

namespace TabularDataOptions
{
	/// <summary>
	/// This sample illustrates Visualization Options for Tabular Data:
	/// - .Net (WPF out-of-box) options to display tabular data:
	///   - WPF DataGrid Control – read only
	/// - ArcGIS Pro options to display tabular data:
	///   - TableView – read / write
	///   - TableControl – read only
	/// - View one record(row) at a time
	///   - Inspector – read / write
	///   - Geometry Control – read only
	/// </summary>
	/// <remarks>
	/// 1. Download the Community Sample data (see under the "Resources" section for downloading sample data).  The sample data contains a project called "AdminSample.ppkx" with data suitable for this sample.  Make sure that the Sample data is unzipped in c:\data and "C:\Data\TabularDataOptions\AdminSample.ppkx" is available.
	/// 1. Open this solution in Visual Studio.
	/// 1. Click the build menu and select Build Solution.
	/// 1. Click the Start button to open ArCGIS Pro. ArcGIS Pro will open.  
	/// 1. Open the "C:\Data\TabularDataOptions\AdminSample.ppkx" project.
	/// 1. Open the "Map" view and click the "Tabular Data" tab.
	/// ![UI](Screenshots/Screen1.png)
	/// 1. Click the "WPF DataGrid Sample" button to open the WPF DataGrid sample dockpane.  In the dockpane, chose a map member from the drop-down to load the data into the WPF DataGrid controls.
	/// ![UI](Screenshots/Screen2.png)
	/// 1. Click the "TableView Sample" button to open the TableView sample dockpane.  In the dockpane, chose a map member from the drop-down, click "Open TableView" to open the table view for the corresponding map member.  Use the Table View function buttons to test the associated table view function.
	/// ![UI](Screenshots/Screen3.png)
	/// 1. Click the "TableControl Sample" button to open the TableControl sample dockpane.  In the dockpane, chose a map member from the drop-down, click "Open TableControl" to open the table control for the corresponding map member.  Use the Table Control function buttons to test the associated table control function.
	/// ![UI](Screenshots/Screen4.png)
	/// 1. Click the "Inspector Sample" button to open the Inspector sample dockpane.  In the dockpane, chose a map member from the drop-down to load the first row (by object id) into the inspector control.  Use the up and down arrows to scroll through the corresponding rows of data.
	/// ![UI](Screenshots/Screen5.png)
	/// 1. Click the "Geometry Control Sample" button to open the Geometry Control sample dockpane.  In the dockpane, chose a map member from the drop-down to load the first row's geometry (by object id) into the Geometry Control.  Use the up and down arrows to scroll through the corresponding rows of data.
	/// ![UI](Screenshots/Screen6.png)
	/// </remarks>
	internal class Module1 : Module
    {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("TabularDataOptions_Module");

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            //return false to ~cancel~ Application close
            return true;
        }

        #endregion Overrides

    }
}
