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

namespace Shortcuts
{
  /// <summary>
  /// This sample provides an overview of the Shortcuts framework. Examples
  /// include: Global Accelerator, keyUp shortcut, conditional shortcut, 
  /// Pane and DockPane shortcut via keyCommand
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 2. Click Start button to open ArcGIS Pro.
  /// 3. ArcGIS Pro will open. 
  /// 4. Open a project with a map or create a blank template and insert a map.
  /// 5. Press "h" to invoke the Accelerator. ----
  /// 6. Activate the Map Pane.
  /// 7. Press "k" to invoke a shortcut targeting the Map Pane.
  /// 8. With the Map Pane still activated, press "Shift + j" to trigger a shortcut
  ///    that opens a Sample DockPane.
  /// 9. With the DockPane activated, press "a" - this will trigger a keyCommanmd 
  ///    shortcut on the DockPane.
  /// 10. Click on the Add-In ribbon tab and then click on the "Open SamplePane"
  ///     button in the Shortcuts Sample group. This will open a sample Pane.
  /// 11. With the SamplePane activated, click the "StateAButton" in the
  ///     Shortcuts Sample group. This will satisfy ConditionA which will allow
  ///     invocation of a conditional shortcut.
  /// 12. With the SamplePane activated, press "l" - this will invoke the
  ///     conditional shortcut.
  /// 13. Dismiss the MessageBox and, with the SamplePane still activated, press
  ///     "r" - this will trigger a keyCommanmd shortcut on the Pane.
  /// 14. Activate the Map Pane - this will enable the "Open Sample Tool" button.
  ///     Click the button to simulate tool activation. Press "n" to invoke
  ///     a shortcut targeted at the tool.
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("Shortcuts_Module");

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
