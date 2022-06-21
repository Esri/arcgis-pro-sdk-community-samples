//   Copyright 2019 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System.Collections;

namespace ScribbleControl_ArcGISPro
{
  /// <summary>
  /// This sample provides a control that allows you to do on-screen drawing 
  /// on top of map/scene views. This sample is an example of adding custom
  /// overlay controls to map views.
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. This solution is using the **DotNetProjects.Extended.Wpf.Toolkit**.  
  /// 1. Launch debugger to open ArcGIS Pro.
  /// 1. Open a map view 
  /// 1. Click on Add-in tab and click the "Add Canvas" button in the "Scribble" group
  /// 1. A Scribble control will be added on top of the map view
  /// 1. The Scribble control will resize to fit the MapView (notice the faint gray border around the edges of the MapView)
  /// 1. Scribble with any of the toolbar scribble tools on top of the map view
  /// 1. Shapes can also be inserted by right-clicking on the Scribble canvas
  /// 1. To remove the Scribble overlay, click "Remove Canvas"
  /// ![UI](Screenshots/Screen.png)
  /// </remarks>
  internal class Scribble_Module : Module
  {
    private static Scribble_Module _this = null;
    public static IDictionary projectControls = new Dictionary<string, MapViewOverlayControl>();

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Scribble_Module Current
    {
      get
      {
        return _this ?? (_this = (Scribble_Module)FrameworkApplication.FindModule("ScribbleControl_ArcGISPro_Module"));
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
