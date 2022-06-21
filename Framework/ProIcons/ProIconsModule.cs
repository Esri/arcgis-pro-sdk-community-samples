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

namespace ProIcons
{
  /// <summary>
  /// This sample shows a list of all icons defined in ArcGIS Pro.  It allows to get the pack URI for those icons and has a utility to inspect the icons using the light and dark themes.
  /// </summary>
  /// <remarks>
  /// 1. In Visual studio rebuild the solution.
  /// 1. Debug the add-in by clicking the "Start" button.
  /// 1. ArcGIS Pro opens, select any Map project
  /// 1. Under the "Pro Icons" tab there are two buttons that show the "Show All Icons" and "Verify Icon" Panes.
  /// ![UI](Screenshots/Screen1.png)
  /// 1. The "Show All Icons" pane displays all icons defined in ArcGIS Pro.  Click on any icon to get the icons name (or URI if you click the checkbox).. 
  /// ![UI](Screenshots/Screen2.png)
  /// 1. The "Verify Icon" pane allows you to enter an icon name (use the "Show All Icons" pane to get any icon name and verify the icon image using light and dark themes. 
  /// </remarks>
  internal class ProIconsModule : Module
  {
    private static ProIconsModule _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static ProIconsModule Current
    {
      get
      {
        return _this ?? (_this = (ProIconsModule)FrameworkApplication.FindModule("ProIcons_Module"));
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
