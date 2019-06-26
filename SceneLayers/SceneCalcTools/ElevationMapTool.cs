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
using System.Windows.Input;
using System.Windows.Media;
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

namespace SceneCalcTools
{
	internal class ElevationMapTool : MapTool
	{
		public ElevationMapTool()
		{
		}

		#region Overrides

		protected override Task OnToolActivateAsync(bool hasMapViewChanged)
		{
			Module1.SceneCalcVM.ElevationToolBackground = new SolidColorBrush(Color.FromRgb(185, 209, 234));
			QueuedTask.Run(() =>
			{
				var featLayer = MapView.Active.Map.FindLayers("Clip_Polygon_Asphalt").FirstOrDefault() as FeatureLayer;

				// Get the selected records, and check/exit if there are none:
				var featSelectionOIDs = featLayer.GetSelection().GetObjectIDs();
				if (featSelectionOIDs.Count == 0)
				{
					MessageBox.Show("No features selected for layer, " + featLayer.Name + ". Elevation will not be applied. Exiting...", "Info");
				}
				else if (featSelectionOIDs.Count > 1)
				{
					MessageBox.Show("More than 1 feature selected, new elevation will be applied to all selected.", "Info");
				}

			});
			return base.OnToolActivateAsync(hasMapViewChanged);
		}

		protected override Task OnToolDeactivateAsync(bool hasMapViewChanged)
		{
			Module1.SceneCalcVM.ElevationToolBackground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
			return base.OnToolDeactivateAsync(hasMapViewChanged);
		}

		protected override void OnToolMouseDown(MapViewMouseButtonEventArgs e)
		{
			switch (e.ChangedButton)
			{
				case MouseButton.Right:
					e.Handled = true;
					break;
				case MouseButton.Left:
					e.Handled = true;
					break;
			}
		}

		protected override Task HandleMouseDownAsync(MapViewMouseButtonEventArgs e)
		{
			return QueuedTask.Run(() =>
			{
				var mapClickPnt = MapView.Active.ClientToMap(e.ClientPoint);
				double elevation = mapClickPnt.Z;

				var featLayer = MapView.Active.Map.FindLayers("Clip_Polygon_Asphalt").FirstOrDefault() as FeatureLayer;

				// Get the selected records, and check/exit if there are none.
				var featSelectionOIDs = featLayer.GetSelection().GetObjectIDs();
				if (featSelectionOIDs.Count == 0)
				{
					MessageBox.Show("No features selected for layer, " + featLayer.Name + ". Elevation not applied. Exiting...", "Info");
					return;
				}

				// Edit elevation for any/all selected polygons, and reset any existing calculated Volume and SArea values to null.
				var inspector = new ArcGIS.Desktop.Editing.Attributes.Inspector(true);
				inspector.Load(featLayer, featSelectionOIDs);
				inspector["PlaneHeight"] = elevation;
				inspector["Volume"] = null;
				inspector["SArea"] = null;
				inspector["EstWeightInTons"] = null;
				var editOp = new EditOperation
				{
					Name = $@"Edit {featLayer.Name}, {featSelectionOIDs.Count} records."
				};
				editOp.Modify(inspector);
				editOp.ExecuteAsync();
				Project.Current.SaveEditsAsync();

				Module1.SceneCalcVM.ReferencePlaneElevation = elevation;
				// Refresh the selection
				Module1.Current.FeatureSelectionChanged();
			});
		}

		#endregion

	}
}
