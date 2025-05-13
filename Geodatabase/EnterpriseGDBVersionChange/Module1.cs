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

namespace EnterpriseGDBVersionChange
{
	/// <summary>
	/// This sample illustrates how to switch versions for an Enterprise Geodatabase connection.
	/// </summary>
	/// <remarks>
	/// 1. Open this solution in Visual Studio.  
	/// 1. Click the build menu and select Build Solution.
	/// 1. Launch the debugger to open ArCGIS Pro.
	/// 1. Open any project "that contains a map with an Enterprise Geodatabase connection".
	/// 1. Open the "Enterprise GDB" tab on the Pro ribbon
	/// 1. Click on the "Show Version Change" button to open the "Switch GDB Version" dockpane.
	/// ![UI](Screenshots/Screen1.png)
	/// 1. Select any feature class with an Enterprise Geodatabase connection from the map's TOC.
	/// 1. The "Switch GDB Version" dockpane is updated with the selected feature class's version information.
	/// 1. Select a version from the drop-down list as the 'Switch To' version.
	/// 1. Click the "Switch Version" button to switch the selected feature class to the selected "To version".
	/// </remarks>
	internal class Module1 : Module
    {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("EnterpriseGDBVersionChange_Module");

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
