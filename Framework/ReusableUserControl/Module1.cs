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

namespace ReusableUserControl
{
  /// <summary>
  /// This sample shows how to create a user control that can be utilized in an ArcGIS Pro Dockpane and an ArcGIS Pro ProWindow.
  /// </summary>
  /// <remarks>
  /// 1. Open this solution in Visual Studio.  
  /// 1. Click the build menu and select Build Solution.
  /// 1. Click the Start button to open ArCGIS Pro.  ArcGIS Pro will open.
  /// 1. Click on the Add-in tab and see that a 'Reusable User Control' with two buttons was added.
  /// 1. The 'Show Dockpane With UserControl' button opens the 'Dockpane With UserControl' pane. 
  /// ![UI](Screenshots/Screen1.png)
  /// 1. Note that the Age field is updated asynchronously.
  /// 1. Click the 'Save Info' button to view the current data managed by the user control.
  /// ![UI](Screenshots/Screen2.png)
  /// 1. The 'Show ProWindow With UserControl' button opens the 'ProWindow With UserControl' Window. 
  /// ![UI](Screenshots/Screen3.png)
  /// 1. Click the 'Save Info' button to view the current data managed by the user control within the ProWindow.
  /// ![UI](Screenshots/Screen4.png)
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("ReusableUserControl_Module"));
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
