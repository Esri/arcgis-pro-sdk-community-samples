//   Copyright 2020 Esri
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

namespace GeometryControl
{
    /// <summary>
    /// This sample demonstrates the UI GeometryControl.  You can use this control to view sketch vertices or geometry vertices. 
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio, build the solution.
    /// 1. Launch the debugger to open ArcGIS Pro.
    /// 1. Open any project containing data.
    /// 1. Activate the Add-in tab.
    /// ![UI](Screenshots/UI_Ribbon.png)
    /// 1. Click the 'Show GeometryView Dockpane' button.  A new dockpane should be displayed. 
    /// 1. Use the Select tool on the Map tab to select a feature. The vertices of the feature's geometry should be displayed in the dockpane. 
    /// ![UI](Screenshots/GeometryVertices.png)
    /// 1. Click the 'Sketch Vertices' tool button. An empty control will be overlayed on the map.
    /// ![UI](Screenshots/SketchVertices.png)
    /// 1. Start sketching with the tool.  The vertices of the sketch should be displayed.
    /// ![UI](Screenshots/Sketchvertices2.png)
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("GeometryControl_Module"));
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
