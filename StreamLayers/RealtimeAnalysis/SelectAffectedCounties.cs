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
	internal class SelectAffectedCounties : Button
	{
		public RealtimeCursor _rtCursor = null;

		protected async override void OnClick()
		{
			Map map = MapView.Active.Map;
			if ((_rtCursor != null) && (_rtCursor.GetState() == RealtimeCursorState.Subscribed))
			{
				_rtCursor.Unsubscribe();
				return;
			}

			StreamLayer streamFLyr = map.Layers[0] as StreamLayer;
			SpatialQueryFilter sf = new SpatialQueryFilter();
			sf.SpatialRelationship = SpatialRelationship.Intersects;

			FeatureLayer countiesFLyr = map.Layers[1] as FeatureLayer;

			//var serviceConnectionProperties =
			//  new RealtimeServiceConnectionProperties(new Uri("https://zihans.esri.com:6443/arcgis/rest/services/Florence-Polygon-Out/StreamServer"), RealtimeDatastoreType.StreamService);
			//RealtimeDatastore realtimeDatastore = null;
			//string tableName = "";
			RealtimeFeatureClass realTimeFC = null;
			await QueuedTask.Run(async () =>
			{
				realTimeFC = streamFLyr.GetFeatureClass();
				_rtCursor = realTimeFC.SearchAndSubscribe(null, true);

				while (await _rtCursor.WaitForRowsAsync())
				{
					while (_rtCursor.MoveNext())
					{
                        using (var rtFeature = _rtCursor.Current as RealtimeFeature)
                        {
                            switch (rtFeature.GetRowSource())
                            {
                                case RealtimeRowSource.EventInsert:
                                    Polygon searchGeom = rtFeature.GetShape() as Polygon;
                                    sf.FilterGeometry = searchGeom;
                                    countiesFLyr.Select(sf);
                                    continue;
                                default:
                                    continue;
                            }
                        }
					}
				}
			});
		}
	}
}
