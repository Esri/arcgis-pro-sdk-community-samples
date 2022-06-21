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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using ArcGIS.Desktop.Mapping;
using ContextMenu.Menus;

namespace ContextMenu.Ribbon
{
	internal class ContextMenuTool3 : MapTool
	{

		private Tuple<string, List<long>> _lineFeatureInfo = null;

		public System.Windows.Point ClientPoint { get; set; }

		public ContextMenuTool3()
		{
			IsSketchTool = true;
			SketchType = SketchGeometryType.Line;
			SketchOutputMode = SketchOutputMode.Map;
		}

		protected override Task OnToolActivateAsync(bool active)
		{
			return QueuedTask.Run(() =>
			{
				_lineFeatureInfo = null;
				var select = MapView.Active.GetFeatures(MapView.Active.Extent);
				foreach (var kvp in select.ToDictionary())
				{
					var featureLayer = kvp.Key as FeatureLayer;
					if (featureLayer.ShapeType == esriGeometryType.esriGeometryPolyline)
					{
						_lineFeatureInfo = new Tuple<string, List<long>>(kvp.Key.URI, kvp.Value);
						break;
					}
				}
			});
		}

		protected override void OnToolMouseDown(MapViewMouseButtonEventArgs e)
		{
			if (e.ChangedButton == System.Windows.Input.MouseButton.Right)
				e.Handled = true;
		}

		protected async override Task HandleMouseDownAsync(MapViewMouseButtonEventArgs e)
		{
			MapPoint clickedPoint = await QueuedTask.Run(() =>
			{
				this.ClientPoint = e.ClientPoint;
				return MapView.Active.ClientToMap(this.ClientPoint);
			});
			//There is always a sketch if the tool is active though it can be
			//empty...
			var sketch = await MapView.Active.GetCurrentSketchAsync();
			ShowContextMenu(sketch, clickedPoint);
		}

		private void ShowContextMenu(Geometry sketch, MapPoint clickedPoint)
		{
			var contextMenu = FrameworkApplication.CreateContextMenu(
														 "ContextMenu_Menus_UpdateSketch", () => ClientPoint);

			LineFeaturesDynamicMenu.SetFeaturesForMenu(sketch, _lineFeatureInfo, clickedPoint);

			contextMenu.Closed += (o, e) =>
			{
				//TODO, any clean up associated with your dynamic menu closing
			};
			contextMenu.IsOpen = true;
		}


		protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
		{
			return base.OnSketchCompleteAsync(geometry);
		}

	}
}
