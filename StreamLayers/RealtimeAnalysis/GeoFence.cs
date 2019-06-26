/*

   Copyright 2019 Esri

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
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.Realtime;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace RealtimeAnalysis
{
	internal class GeoFence : Button
	{
		private RealtimeCursor _rtfc = null;
		private Dictionary<int, bool> _featuresGeoFenced = null;

		protected async override void OnClick()
		{
			#region initial setup
			if ((_rtfc != null) && (_rtfc.GetState() == RealtimeCursorState.Subscribed))
			{
				_rtfc.Unsubscribe();
				_featuresGeoFenced = null;
				return;
			}

			Map map = MapView.Active.Map;
			Polygon geofence = null;
			_featuresGeoFenced = new Dictionary<int, bool> { { 1, false }, { 2, false } };
			#endregion

			#region getting geofence geometry
			FeatureLayer flyr = map.Layers[1] as FeatureLayer;
			await QueuedTask.Run(() =>
			{
				RowCursor rc = flyr.Search(null);
				rc.MoveNext();
				Feature f = rc.Current as Feature;
				geofence = f.GetShape() as Polygon;
			});
			#endregion

			#region Setting geo-fencing using a spatial filter
			StreamLayer streamLayer = map.Layers[0] as StreamLayer;
			SpatialQueryFilter sf = new SpatialQueryFilter
			{
				SpatialRelationship = SpatialRelationship.Intersects,
				FilterGeometry = geofence
			};

			await QueuedTask.Run(async () =>
	  {
		  RealtimeFeatureClass rtfcls = streamLayer.GetFeatureClass();

		  //Subscribing with a spatial filter
		  _rtfc = rtfcls.Subscribe(sf, true);

		  while (await _rtfc.WaitForRowsAsync())
		  {
			  while (_rtfc.MoveNext())
			  {
				  switch (_rtfc.Current.GetRowSource())
				  {
					  case RealtimeRowSource.EventInsert:
						  RealtimeFeature rtfeat = _rtfc.Current as RealtimeFeature;
						  int featureID = (int)rtfeat["TrackID"];
						  if (!_featuresGeoFenced[featureID])
						  {
							  _featuresGeoFenced[featureID] = true;
							  ShowAlert(featureID.ToString());
						  }
						  continue;
					  default:
						  continue;
				  }
			  }
		  }
	  });
			#endregion
		}

		//delegate to clear notifications and reset stream layer notification counter
		private void ClearAll()
		{
			NotificationManager.ClearNotification();
			foreach (var item in _featuresGeoFenced.Keys.ToArray())
				_featuresGeoFenced[item] = false;
		}

		private void ShowAlert(string v)
		{
			//show notification
			var notification = new NotificationItem
			  (
				$"Geo-fencing Alert - {v}",
				false,
				$"Alert!!!\nFeature with id#{v} has entered the restricted zone!",
				NotificationType.Warning,
				"Clear All",
				(Action)(() => ClearAll()),
				null
			  );
			NotificationManager.AddNotification(notification);

			//Showing toast notification
			FrameworkApplication.AddNotification(new Notification()
			{
				Title = "Geo-fencing Alert",
				Message = $"Alert!!!\nFeature with id#{v} has entered the restricted zone!",
				ImageUrl = @"pack://application:,,,/ArcGIS.Desktop.Resources;component/Images/DiagDirtyFeatures32.png",
			});
		}
	}
}


