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

namespace TransformCADLayer
{
  /// <summary>
  /// This sample can be used to transform a CAD layer by entering a scale factor, rotation, local coordinates and corresponding grid coordinates. The parameters are entered and a world file is created. The CAD layer's map location is updated based on the new world file transformation parameters corresponding with the entered values. 
  /// </summary>
  /// <remarks>
  /// 1. Build or debug the sample through Visual Studio.
  /// 1. In Pro, select the CAD layer in the table of contents.  
  /// 1. The contextual ribbon for the CAD layer becomes available to access the add-in button for Transform from the Alignment group.
  /// ![UI](Screenshots/Screen1.png)
  /// 1. Click the button and enter the parameters for local coordinates, grid coordinates, scale and rotation values.
  /// ![UI](Screenshots/Screen2.png)
  /// 1. Check the box "Update ground to grid corrections" to apply the scale and rotation values to the map's ground to grid correction settings.
  /// ![UI](Screenshots/Screen3.png)
  /// 1. Click the Reset button to set all the transformation parameters back to starting values.
  /// 1. Click the OK button to transform the CAD layer based on the entered transformation parameters.
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("TransformCADLayer_Module"));
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
