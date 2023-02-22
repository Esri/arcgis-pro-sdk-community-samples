/*

   Copyright 2022 Esri

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
using System.Windows.Input;

namespace DockpaneWithListCheckbox
{  /// <summary>
   /// This MVVM / XAML sample shows how to implement a multi-select ListBox in an ArcGIS Pro API Dockpane using MVVM.
   /// </summary>
   /// <remarks>
   /// 1. In Visual Studio build the solution and run the debugger to open ArcGIS Pro.
   /// 1. In ArcGIS Pro open any project or a new map.  
   /// 1. Select the 'Add-in' tab, and then click on 'Show TestListbox' to open the test dockpane.
   /// ![Screen1](Screenshots/Screen1.png)
   /// 1. Click the 'Populate List' button to populate the list box.
   /// ![Screen2](Screenshots/Screen2.png)
   /// 1. Select a few items and click the 'Read Selcted Items' button to test the selection.
   /// ![Screen3](Screenshots/Screen3.png)
   /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("DockpaneWithListCheckbox_Module");

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
