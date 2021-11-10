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
  internal class RemoveReplicas : Button
  {
    protected async override void OnClick()
    {
	  //get the active map .
	  var map = MapView.Active.Map;

	  await QueuedTask.Run(() =>
	  {
		//Does the map have local syncable content that can be unregistered from the feature service.
		var canRemove = GenerateOfflineMap.Instance.GetCanRemoveReplicas(map);
		if (canRemove)
		{
		  //Remove the replicas 
		  GenerateOfflineMap.Instance.RemoveReplicas(map);
		  canRemove = GenerateOfflineMap.Instance.GetCanRemoveReplicas(map);
		}
		else
		{
		  try
		  {
			GenerateOfflineMap.Instance.RemoveReplicas(map);
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
