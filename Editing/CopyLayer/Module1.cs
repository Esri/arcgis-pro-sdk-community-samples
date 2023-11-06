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

namespace CopyLayer
{
  /// <summary>
  /// This sample shows how to take an existing feature layer and create a copy of the feature class in the default geodatabase.  The spatial column's attributes are recreated in the new feature class and two columns are added.  Finally the existing spatial data is copied and the new feature class is added to the map's layers.
  /// </summary>
  /// <remarks>   
  /// 1. Open this solution in Visual Studio.  
  /// 1. Click the build menu and select Build Solution.
  /// 1. Launch the debugger to open ArCGIS Pro. 
  /// 1. Open any project with a map that contains either Point, Line or Polygon feature layers.
  /// 1. Click on the 'Add-in' tab and note the 'Copy Feature Layer' group.
  /// ![UI](Screenshots/Screen1.png)
  /// 1. Click the 'Copy Layer' button to create a copy of the first feature layer in the current active map's table of content.
  /// 1. The add-in creates a copy of the first feature layer's feature class in the default geodatabase using the layer's spatial attributes.
  /// 1. Then the add-in copies the existing spatial data to the new feature class and finally adds the new feature class to the map's table of content.
  /// ![UI](Screenshots/Screen2.png)
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("CopyLayer_Module");

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
