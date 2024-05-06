/*

   Copyright 2024 Esri

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

namespace CreateLineOfSight
{
  /// <summary>
  /// This sample demonstrates how to perform a line of sight analysis between an observer and a target point on a TIN surface.
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.  
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open. 
  /// 1. Open CreateLineOfSight.aprx from the Sample Data
  /// 1. Activate the "Line of Sight Demo" map if it is not already active. 
  /// 1. Observe the green observer point push pin and the 3 red target point push pins on the map.
  /// 1. Click the Los Demo tab on the ribbon.
  /// 1. Click the "Sight Lines" button. This will run the Line of sight analysis between the observer and target points.
  /// 1. Three sight lines will be drawn from the observer point to the 3 target points. Each sight line will have a green visible section, a red invisible section and an optional purple Obstruction point (if any).
  /// ![UI](screenshots/screen1.png)
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("CreateLineOfSight_Module");

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

    protected override bool Initialize()
    {
      return base.Initialize();
    }
    #endregion Overrides
    public static void CallButtonClick(string buttonName)
    {
      var button = FrameworkApplication.GetPlugInWrapper(buttonName);
      if (button != null)
      {
        var cmd = button as ICommand;
        if (cmd.CanExecute(null))
          cmd.Execute(null);
      }
    }
  }
}
