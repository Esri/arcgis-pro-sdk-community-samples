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
using ArcGIS.Desktop.Mapping.Offline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfflineMapping
{
  internal class SyncReplica : Button
  {
    protected async override void OnClick()
    {
	  //get the active map.
	  var map = MapView.Active.Map;
	  await QueuedTask.Run(() =>
	  {
		//Does the local map contain local syncable content?
		var canSync = GenerateOfflineMap.Instance.GetCanSynchronizeReplicas(map);
		if (canSync)
		{
		  //Sync
		  GenerateOfflineMap.Instance.SynchronizeReplicas(map);
		  canSync = GenerateOfflineMap.Instance.GetCanSynchronizeReplicas(map);
		}
		else
		{
		  try
		  {
			GenerateOfflineMap.Instance.SynchronizeReplicas(map);
		  }
		  catch (Exception ex)
		  {
			System.Diagnostics.Debug.WriteLine(ex.ToString());
		  }
		}
	  });
	}
  }
}
