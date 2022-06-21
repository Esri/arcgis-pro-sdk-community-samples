//Copyright 2020 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace LayerSnapModes
{

  /// <summary>
  /// This sample illustrates the use of layer snap modes, which provides the ability to set the snap mode (vertex, edge, end etc) on a per layer basis.
  /// The layer snap modes are presented in a datagrid hosted by a simple dockpane.
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio, build the solution.
  /// 1. Launch the debugger to open ArcGIS Pro.
  /// 1. In Pro, open a map with feature layers or create a new map and add some.
  /// 1. Turn on the application snap modes Vertex, Edge and End from the snapping dropdown on the editing tab.
  /// 1. On the add-in tab, click Show LayerSnapModes to display the LayerSnapModes dockpane.
  /// ![UI](Screenshots/lsm_dockpane.png)
  /// 1. Toggle some of the layer snap modes and examine the effect when snapping to the layer with a tool such as the measure tool.
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("LayerSnapModes_Module"));
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
