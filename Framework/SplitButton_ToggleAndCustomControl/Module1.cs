/*

   Copyright 2026 Esri

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

using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;


namespace SplitButton_ToggleAndCustomControl
{
  /// <summary>
  /// Hosts an editing-toolbar interface inside a ribbon split button and synchronizes its visibility with the split button's toggle command.
  /// 
  /// The split button toggles the editing mini toolbar visibility and re-creates on the dropdown, UI from 
  /// the Editing backstage options controlling the mini toolbar options.
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Click Start button to debug ArcGIS Pro.
  /// 1. Open any ArcGIS Pro project. Navigate to the Add-In tab on the ribbon. 
  /// 1. Click the dropdown part of the Editing Toolbar button.  This will display the custom control
  /// showing the mini toolbar options. 
  /// ![UI](Screenshots/Screenshot1.png)
  /// 1. Turn the mini toolbar on by clicking the checkbox in the upper right of the dropdown. 
  /// 1. Open the Create Features dockpane and activate a line or 
  /// polygon template to initiate an editing action.The editing toolbar should be displayed.
  /// 1. Modify the radio buttons and checkbox on the dropdown UI of the split button and verify that the 
  /// editing toolbar changes position and size accordingly. 
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("SplitButton_ToggleAndCustomControl_Module");

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
