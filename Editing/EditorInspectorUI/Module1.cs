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
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace EditorInspectorUI
{
  /// <summary>
  /// This sample demonstrates how to use the Inspector class to display field names and values of a feature in a grid like manner.
  /// You can also use the InspectorProvider class to create a more customized version of the inspector grid.
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.  
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open. 
  /// 1. Open Data\Editing\InspectorUI.ppkx from the Sample Data
  /// 1. Activate the "NYC Building permits" map if it is not already active. 5 features will be selected in this map.
  /// 1. Click the "Inspector UI" tab on the ribbon.
  /// 1. Click the "My Inspector Grid" button. This will display the Inspector dockpane.
  /// ![UI](screenshots/screen1.png)
  /// 1. You can now modify this sample to use the InsepctorProvider class to customize the Inspector Grid. InsepctorProvider base class allows you to create a more customized version of the inspector grid. Open the MyProvider.cs class. Notice how it derives from the InspectorProvider base class and implements various callbacks to customize that specific aspect of the UI.
  /// 1. Given below are some examples of how you can customize the Inspector Grid using the InspectorProvider class callbacks:
  ///      * Change field visibility, 
  ///      * Sets fields to be read-only, highlighted, 
  ///      * Customizes field order in the grid,
  ///      * Adds validation
  ///      * Modifies field name display.
  /// 1. Open the DockpaneInspectorUIViewModel.cs.Follow the "TODO" comments in the "Show" method in this class file to use the Inspector Provider class to customize the Inspector Grid.
  /// 1. Hot reload the change in the code and click the "My Inspector Grid" tab on the ribbon. Notice the customized Inspector UI.
  /// ![UI](screenshots/screen2.png)
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("EditorInspectorUI_Module");

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
