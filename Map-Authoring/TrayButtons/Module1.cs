/*

   Copyright 2022 Esri

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

namespace TrayButtons
{
  /// <summary>
  /// This sample illustrates how to build custom tray buttons.  
  /// Three samples are included.
  /// The first is a simple map tray button, ZoomToVisibleExtentTrayButton. This tray button will change the map extent to the visible extent of all layers in the map.
  /// The second sample is the layout tray toggle button.  This tray button is the Toggle button type created to be used in layout only. 
  /// When toggled on and off layout guides that have been authored in the current layout will change from being visible to invisible or vice versa depending on the toggle state of the tray button.
  /// The third sample is a map tray popup button. This tray button toggles the editing mini toolbar visibility and re-creates on the popup, UI from the Editing backstage options controlling the mini toolbar.
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu.Then select Build Solution.
  /// 1. Launch the debugger to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open, select a project that has editable layers in the map. 
  /// 1. Change the map extent and test out the tray button with the zoom to extent icon and see that the map extent will update to the extent off all visible layers. 
  /// 1. Open a layout, or insert a new layout and add a map frame to the layout.    
  /// 1. Right click on the ruler and select 'Add Guides..' In the dialog select Orientation 'Both' and Placement 'Evenly spaced' and click OK.
  /// ![UI](Screenshots/LayoutGuides.PNG) 
  /// 1. Click on the tray toggle button 'LayoutGuideToggle', which had the ruler icon and is directly to the right of the snapping tray button in the bottom left of the map. 
  /// ![UI](Screenshots/LayoutGuidesTrayButton.png)     
  /// 1. The first click turns on the Guides, so click a second time to toggle them off, a third click will toggle them back on. 
  /// 1. Activate the map view again and open the Create Features dialog and select a template to start sketching. The editing mini toolbar will appear on the map. 
  /// 1. Hover over the red square tray button icon, which is the Mini Toolbar Tray Button defined in this sample. 
  ///  ![UI](Screenshots/ZoomAndToolbarTrayButtons.png)   
  /// 1. Click on a radio button to change the position and size of the mini toolbar and check Magnify. Notice the toolbar is updated to match the new properties.
  /// 1. Toggle the tray button and notice the mini toolbar is no longer visible.     
  /// 1. Revert any properties change to the mini toobar if not desired. 
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("TrayButtons_Module");

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
