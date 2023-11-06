/*

   Copyright 2023 Esri

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

namespace ProWindowModal
{
  /// <summary>
  /// This sample demonstrates a modal MVVM ProWindow and a modal non-MVVM ProWindow. 
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio rebuild this Solution.
  /// 1. Launch the debugger to open ArcGIS Pro.
  /// 1. Open any ArcGIS Pro project.
  /// 1. Click on the 'Pro Windows' tab 
  /// ![UI](Screenshots/Screen1.png)
  /// 1. Click on the 'Modal ProWindow' button to bring up the non MVVM modal ProWindow.  User input is focused on this ProWindow until the ProWindow is closed.
  /// ![UI](Screenshots/Screen2.png)
  /// 1. Click on the 'Modal MVVM ProWindow' button to bring up the MVVM modal ProWindow.  User input is focused on this ProWindow until the ProWindow is closed.
  /// ![UI](Screenshots/Screen3.png)
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("ProWindowModal_Module");

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
