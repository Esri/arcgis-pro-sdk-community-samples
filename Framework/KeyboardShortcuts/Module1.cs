//   Copyright 2023 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
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
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace KeyboardShortcuts
{
  /// <summary>
  /// This sample provides an overview of the Keyboard Shortcuts framework. Examples
  /// include: Global Accelerator, keyUp shortcut, conditional shortcut, 
  /// Pane and DockPane shortcut via keyCommand
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 2. Click Start button to open ArcGIS Pro.
  /// 3. ArcGIS Pro will open.
  /// 4. Open a project with a map or create a blank template and insert a map.
  /// 5. Open a map view. 
  /// 6. Launch the Keyboard Shortcuts dialog with F12.Take a moment to inspect the Shortcut Tables. 
  /// 7. Expand the "Global" group.Scroll down to "SampleButton." This is a read-only accelerator added by this AddIn.
  /// 8. Uncheck the "Show currently active shortcuts" checkbox at the top of the dialog.
  /// 9. Expand the Custom group. This is an example of a Custom category added in this sample.
  /// 10. Close the Keyboard Shortcuts dialog.
  /// 11. Press `h` to invoke the Accelerator command.A message will appear.
  /// 12. Press "h" to invoke the Accelerator command.
  /// 13. Activate the Map View.
  /// 14. Press "k" to invoke a shortcut targeting the Map View.
  /// 15. With the Map View still activated, press "Shift + j" to trigger a shortcut
  ///    that opens a Sample DockPane.
  /// 16. With the DockPane activated, press "a" - this will trigger a keyCommanmd 
  ///    shortcut on the DockPane.
  /// 17. Click on the Add-In ribbon tab and then click on the "Open SamplePane"
  ///     button in the Shortcuts Sample group. This will open a sample Pane.
  /// 18. With the SamplePane activated, click the "StateAButton" in the
  ///     Shortcuts Sample group. This will satisfy ConditionA which will allow
  ///     invocation of a conditional shortcut.
  /// 19. With the SamplePane activated, press "l" - this will invoke the
  ///     conditional shortcut.
  /// 20. Dismiss the MessageBox and, with the SamplePane still activated, press
  ///     "r" - this will trigger a keyCommanmd shortcut on the Pane.
  /// 21. Activate the Map View - this will enable the "Open Sample Tool" button.
  ///     Click the button to simulate tool activation. Press "n" to invoke
  ///     a shortcut targeted at the tool.
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("KeyboardShortcuts_Module");

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
