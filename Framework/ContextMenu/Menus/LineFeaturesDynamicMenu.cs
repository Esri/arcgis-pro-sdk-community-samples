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
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContextMenu.Menus
{
	/// <summary>
	/// Dynamic menu implementation for Example 3. One menu item per
	/// line feature passed in to the <see cref="SetFeaturesForMenu(Geometry, Tuple{string, List{long}}, MapPoint)"/>
	/// static method is created when the menu is shown.
	/// </summary>
	class LineFeaturesDynamicMenu : DynamicMenu
	{

		private static Tuple<string, List<long>> _featureInfo = null;
		private static MapPoint _insertPoint = null;
		private static Geometry _sketch = null;

		/// <summary>
		/// Used by the map tool to pass in the line features to add to the dynamic menu along
		/// with the current sketch geometry and right-click location
		/// </summary>
		/// <param name="sketch"></param>
		/// <param name="featureInfo"></param>
		/// <param name="insertPoint"></param>
		public static void SetFeaturesForMenu(Geometry sketch, Tuple<string, List<long>> featureInfo, MapPoint insertPoint)
		{
			_featureInfo = featureInfo;
			_insertPoint = insertPoint;
			_sketch = null;
			_sketch = sketch;
		}

		/// <summary>
		/// Override to handle the popup callback when the dynamic menu is about to be shown
		/// </summary>
		/// <remarks>Create the menu items for the menu here</remarks>
		protected override void OnPopup()
		{
			if (_featureInfo == null)
			{
				//Place holder
				this.Add("No features found");
			}
			else
			{
				//One menu item per line feature
				foreach (var oid in _featureInfo.Item2)
				{
					var caption = $"Update sketch with feature {oid}";
					this.Add(caption);//Adds a menu item to the collection of
					                  //menu items
				}
			}
		}

		/// <summary>
		/// Implement the on-click behavior of the individual menu items. Alternatively, addins can
		/// implement on-click behavior via a delegate assigned to each menu item (refer to the dynamic
		/// menu Add method overloads).
		/// </summary>
		/// <param name="index">The 0-based index of the clicked menu item</param>
		/// <remarks>Retrieve the feature geometry (for the clicked menu item) and
		/// append it into the sketch.</remarks>
		protected override void OnClick(int index)
		{
			if (_featureInfo == null)
				return;
			var fl = MapView.Active.Map.FindLayer(_featureInfo.Item1) as FeatureLayer;
			QueuedTask.Run(() =>
			{
				//Use inspector to retrieve the feature shape
				var insp = new Inspector();
				insp.Load(fl, _featureInfo.Item2[index]);

				//Project
				var temp_line = 
				     GeometryEngine.Instance.Project(insp["SHAPE"] as Polyline, 
						         MapView.Active.Map.SpatialReference) as Polyline;

				//Move the beginning of the shape to the right-click
				//location...
				var first_point = temp_line.Points[0];
				var dx = _insertPoint.X - first_point.X;
				var dy = _insertPoint.Y - first_point.Y;
				var mv_line = GeometryEngine.Instance.Move(temp_line, dx, dy);

				//match the geometry sr with the sketch sr
				Polyline finalLine = 
				         GeometryEngine.Instance.Project(mv_line,
										 _sketch.SpatialReference) as Polyline;

				//Sketch might be empty but it is never null...
				//assumes single part polyline here...
				var points = ((Polyline)_sketch).Points.ToList();
				points.AddRange(finalLine.Points);
				var bldr = new PolylineBuilderEx(points.Select(p => p.Coordinate3D).ToList(), 
					   _sketch.SpatialReference);
				//ensure the geometry is Z enabled to be used as for
				//the sketch
				bldr.HasZ = true;
				_sketch = bldr.ToGeometry();
				MapView.Active.SetCurrentSketchAsync(_sketch);
			});
		}

	}
}
