//   Copyright 2023 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

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

namespace TransformationsControl
{
  /// <summary>
  /// This sample shows how to use the Pro API's Transformations Control from a Dockpane.
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
  /// 1. Make sure that the Sample data is unzipped in c:\data
  /// 1. Any project can be used with this sample
  /// 1. In Visual Studio click the Build menu.Then select Build Solution.
  /// 1. Launch the debugger to open ArcGIS Pro.
  /// 1. Open an project
  /// 1. In Add-in tab, click the "Show Transformations Control" button.
  /// ![UI](Screenshots/Screen1.png)
  /// 1. The dockpane will be displayed. 
  /// ![UI](Screenshots/Screen2.png)
  /// 1. Choose each of the configuration options and explore the control. 
  /// ![UI](Screenshots/Screen3.png)
  /// ![UI](Screenshots/Screen4.png)
  /// ![UI](Screenshots/Screen5.png)
  /// 1. After the control is configured with a transformation, click the Project button to see results of the GeometryEngine
  /// ProjectEx function on a number of geometries using the chosen datum transformation. 
  /// ![UI](Screenshots/Screen6.png)
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("TransformationsControl_Module");

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
