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
  /// This sample provides a new tab and controls that allow you to set the 
  ///time in the map view, step through time, and navigate between time 
  ///enabled bookmarks in the map.
  /// </summary>
  /// <remarks>
  ///1. In Visual Studio click the Build menu. Then select Build Solution.
  ///2. Click Start button to open ArcGIS Pro.
  ///3. ArcGIS Pro will open. 
  ///4. Open a map view that contains time aware data. Click on the new 
  ///Navigation tab within the 
  ///Time tab group on the ribbon.
  ///![UI](screenshots/UICommands.png)
  ///6. Within this tab there are 3 groups that provide functionality to 
  ///navigate through time.
  ///7. The Map Time group provides two date picker controls to set the 
  ///start and end time in the map.
  ///8. The Time Step group provides two combo boxes to set the time step 
  ///interval. The previous and next 
  ///button can be used to offset the map time forward or back by the 
  ///specified time step interval.
  ///9.. The Bookmarks group provides a gallery of time enabled bookmarks 
  ///for the map. Clicking a bookmark in the 
  ///gallery will zoom the map to that location and time. 
  ///It also provides play, previous and next buttons that can be used to 
  ///navigate between the time enabled bookmarks. 
  ///These commands are only enabled when there are at least 2 bookmarks 
  ///in the map. Finally it provides a 
  ///slider that can be used to set how quickly to move between 
  ///bookmarks during playback.
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
