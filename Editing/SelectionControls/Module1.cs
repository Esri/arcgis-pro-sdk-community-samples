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

namespace SelectionControls
{
  /// <summary>
  /// Hosts SelectionToolPicker and Selection controls in a dockpane for choosing selection tools and managing the active map's feature selection.
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Click Start button to debug ArcGIS Pro.
  /// 1. Open any ArcGIS Pro project. Navigate to the Add-In tab on the ribbon. 
  /// ![UI](screenshots/Screenshot1.png)
  /// 1. Click the Show MapSelection button.  This will display the Map Selection dockpane.  The SelectionToolPicker will display "Select one or more features" or "Change the selection"
  /// depending on whether a selection exists in the map. The current map selection (if any) will be displayed in the treeview.
  /// 1. Activate the SelectionToolPicker so the selection tool becomes active. 
  /// ![UI](screenshots/Screenshot2.png)
  /// 1. Make a selection on the map and see the features displayed in the Selection treeview. 
  /// ![UI](screenshots/Screenshot3.png)
  /// 1. Make additional selections and notice the treeview changing. Clear the selection and see the treeview cleared and the SelectionToolPicker text change. 
  /// 1. Choose a different selection tool from the dropdown and make other selections. 
  /// ![UI](screenshots/Screenshot4.png)
  /// 1. You can also activate a different map and the dockpane will respond and display the current selection of that map. 
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("SelectionControls_Module");

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
