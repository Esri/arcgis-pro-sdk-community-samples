//Copyright 2020 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace CreatePointsAlongLine3D
{
  /// <summary>
  /// This sample provides a point construction tool to create points along a 3D line.
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio, build the solution.
  /// 1. Click the Visual Studio Start button to open ArcGIS Pro.
  /// 1. In Pro, open a 2D map containing Z aware polylines and Z aware points or create a new 2D map and add some.
  /// 1. Select a Z aware polyline that you want to create points along.
  /// 1. Open the editing create features pane and select a template for your z aware points.
  /// 1. Click the 'create points along a 3D line tool' to display the tools pane.
  /// ![UI](ScreenShots/cpal.png)
  /// 1. Enter the construction options for the number of points you wish to create, then click 'Create'.
  /// 1. Z aware points will be created using the 3D distance along the selected line. The tool can be used to select other Z aware polylines.
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("CreatePointsAlongLine3D_Module"));
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
