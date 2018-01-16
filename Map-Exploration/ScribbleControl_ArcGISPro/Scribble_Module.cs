//   Copyright 2014 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

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
    /// 1. This solution is using the **Extended.Wpf.Toolkit NuGet**.  If needed, you can install the NuGet from the "NuGet Package Manager Console" by using this script: "Install-Package Extended.Wpf.Toolkit".
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open. 
    /// 1. Open a map view 
    /// 1. Click on ADD-IN tab and click the "Add Canvas" button in the "Scribble" group
    /// 1. Scribble control will be added to the top of the map view
    /// 1. By default, the control is the same width as the current map-view pane and the default height is just enough to show the toolbar
    /// 1. Grab the handle at bottom-center of the control to expand the drawing/canvas area
    /// 1. Now you should be able to draw with the pencil tool (left-mouse-down and drag)
    /// 1. Shapes can be inserted by right-clicking in the canvas
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
