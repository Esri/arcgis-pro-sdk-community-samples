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

namespace COGOLineFeatures
{
  /// <summary>
  /// This custom feature template construction tool is used to click two locations on the map and presents measured COGO attribute values, then allows those attributes to be edited without altering the end point location of the created line. This can be used as an enhanced Direction Distance constraint tool or as an enhanced measure tool that holds fixed the locations snapped on the map, while also maintaining the entered values.
  /// </summary>
  /// <remarks>
  /// 1. Build or debug the sample through Visual Studio.  
  /// 1. In Pro, add a line layer or a COGO enabled line layer to the map.
  /// 1. Click the Edit tab and click Create Features button in the Feature group.
  /// 1. Click the line feature template in the Create Features pane, and click the COGO Line tool.  
  /// ![UI](Screenshots/Screen01.png)
  /// 1. Snap to or click in the map for the first point of the COGO line, then snap to or click in the map for the second point.
  /// 1. The COGO line dialog shows the measured Direction and Distance values.
  /// ![UI](Screenshots/Screen02.png)  
  /// 1. Change the values as needed to match the source document; these are usually small changes to the numbers.
  /// 1. The first Direction entry box has the focus, you can press the Enter key to move the focus to the Distance field, and press Enter again to create the line.
  /// 1. The check box "Hold end point position" is turned on by default. Uncheck the box to have the end point location updated based on changed values.
  /// 1. You can type a negative distance to reverse the direction of the line.
  /// 1. To create the line you can click the OK button, or you can press Enter when in the Distance entry field. The dialog will automatically close.
  /// ![UI](Screenshots/Screen03.png)
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("COGOLineFeatures_Module"));
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
