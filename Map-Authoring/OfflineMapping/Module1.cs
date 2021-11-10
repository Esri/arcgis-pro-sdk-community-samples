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

namespace OfflineMapping
{
  /// <summary>
  /// This sample demonstrates how maps in Pro containing sync-enabled feature service content can be taken "offline".
  /// </summary>
  /// <remarks>
  /// Using the sample:
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open. 
  /// 1. Open a new Map project. 
  /// 1. Add any sync enabled feature service to the map.
  /// 1. In the Offline Map tab, notice there is a group created: Take a map offline. It contains the Download Map custom control and two disabled buttons - Sync Replicas and Remove Replicas.
  /// ![UI](screenshots/offlinemap.png)
  /// 1. Click the Download Map button to display its contents. 
  /// ![UI](screenshots/download.png)
  /// 1. Turn on the check box to "Include basemaps". Pick any scale you want to use.
  /// ![UI](screenshots/basemapsScales.png)
  /// 1. Click the Download button. This will make a local copy (replica) of your feature service. You can make any edits you need to this feature service.
  /// 1. The Sync replica button on the group is now enabled. This button allows you to sync your local replica to the feature service.
  /// ![UI](screenshots/sync.png)
  /// 1. After you are done with the edits to the replica, you can click the "Remove Replica" button to remove all the replicas from the local map content.
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("OfflineMapping_Module"));
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
