/*

   Copyright 2024 Esri

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DockpaneDragAndDropFeatureLayer
{
	/// <summary>
	/// This sample shows how to process drag and drop events in an ArcGIS Pro dockpane extension.
	/// </summary>
	/// <remarks>
	/// 1. In Visual Studio click the Build menu. Then select Build Solution.
	/// 1. Launch the debugger to open ArcGIS Pro.
	/// 1. Open a map view. The map should contain a few feature classes, preferably also a Geodatabase Table.  For example:  C:\Data\Admin\AdminSample.aprx.
	/// 1. Click on the Add-In tab on the ribbon and then on the 'Show Drop Target Dockpane' button to open the 'Drop Target Dockpane'.
	/// 1. Click on a File Geodatabase feature class or table in the Catalog pane and drag it onto the dockpane.
	/// ![UI](Screenshots/Screen1.png)
	/// ![UI](Screenshots/Screen2.png)
	/// 1. The dockpane will display the name of the feature class or table if the type is implemented and it will also populate a table control with the data.
	/// ![UI](Screenshots/Screen3.png)
  /// </remarks>
	internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("DockpaneDragAndDropFeatureLayer_Module");

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
