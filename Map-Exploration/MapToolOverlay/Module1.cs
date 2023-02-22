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

namespace MapToolOverlay
{
  /// <summary>
  /// This sample used map tools to demontrate graphic overlays (for MapTool) in form of Text, line, point, and arrow line graphic overlays.
  /// </summary>
  /// <remarks>
  /// 1. In Visual studio rebuild the solution.
  /// 1. Debug the add-in.
  /// 1. ArcGIS Pro opens, open any project.
  /// 1. Click on the Add-In Tab.
  /// 1. Exercise each MapTool by selecting the tool and the then click on the map, hold down the mouse button and move the mouse across the map.
  /// 1. First the Text Overlay:
  /// ![UI](Screenshots/Screen1.png)
  /// 1. The Point Overlay
  /// ![UI](Screenshots/Screen2.png)
  /// 1. The Line Overlay 
  /// ![UI](Screenshots/Screen3.png)
  /// 1. The Arrow Overlay
  /// ![UI](Screenshots/Screen4.png)
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("MapToolOverlay_Module");

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
