/*

   Copyright 2017 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace CoordinateSystemAddin
{
  /// <summary>
  /// This sample provides a re-usable Coordinate Picker User Control. Embed within your Add-in 
  /// if you need to provide a UI component similar to the core Coordinate Picker on the Pro Map 
  /// property sheet. The list of Coordinate systems is loaded the first time the dialog is 
  /// opened and remains cached for the duration of the Pro session. When a user picks a 
  /// Coordinate System, and clicks OK, it can be retrieved via the SpatialReference property on 
  /// the dialog.
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open.
  /// 1. Either create a new blank project OR open an existing project.
  /// 1. Click on the ADD-IN TAB.
  /// 1. Click on the *Pick Coord Sys.* button. ![Coordinate systems loading](Screenshots/Screen1.png)
  /// 1. The Pick Coord System dialog will open up.
  /// 1. The code behind queries for all available coordinate systems.  ![Coordinate systems dialog](Screenshots/Screen2.png)
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("CoordinateSystemPicker_Module"));
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
