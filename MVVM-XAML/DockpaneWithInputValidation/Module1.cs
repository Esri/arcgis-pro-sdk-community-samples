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
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.KnowledgeGraph;

namespace DockpaneWithInputValidation
{
	/// <summary>
	/// This sample illustrates who to validate data input in a dockpane using the INotifyDataErrorInfo interface for feedback.
	/// </summary>
	/// <remarks>
	/// 1. Open this solution in Visual Studio.
	/// 1. Click the build menu and select Build Solution.
	/// 1. Click the Start button to open ArCGIS Pro. ArcGIS Pro will open.  
	/// 1. Open the ArcGIS Pro ribbon click the "Input Validation" tab then click the "Show Input Validation" button to open the "Input Validation" dockpane.
	/// ![UI](Screenshots/Screen1.png)
	/// 1. Try to input invalid data in the "Integer Input" textbox. You will see an error message appear below the textbox.
	/// </remarks>
	internal class Module1 : Module {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("DockpaneWithInputValidation_Module");

  #region Overrides
  /// <summary>
  /// Called by Framework when ArcGIS Pro is closing
  /// </summary>
  /// <returns>False to prevent Pro from closing, otherwise True</returns>
  protected override bool CanUnload() {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

#endregion Overrides

    }
}
