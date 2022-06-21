//   Copyright 2019 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Events;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace  FeatureDynamicMenu
{
	/// <summary>
	/// Implementation of custom Map tool.
	/// </summary>
	class FeatureSelectionDynamic : MapTool
	{

		private static readonly IDictionary<BasicFeatureLayer, List<long>> Selection = new Dictionary<BasicFeatureLayer, List<long>>();
		private static readonly object LockSelection = new object();
		private System.Windows.Point _clickPoint;

		public System.Windows.Point MouseLocation => _clickPoint;

		/// <summary>
		/// Define the tool as a sketch tool that draws a point in screen space on the view.
		/// </summary>
		public FeatureSelectionDynamic()
		{
			IsSketchTool = true;
			SketchType = SketchGeometryType.Rectangle;
			SketchOutputMode = SketchOutputMode.Map;

			// binding sync: we need to lock this selection collection since it will be updated
			// asynchronously from a worker thread
			BindingOperations.EnableCollectionSynchronization(Selection, LockSelection);
			//Set the embeddable's control's DAML ID to show on the mapview when the tool is active.
			OverlayControlID = "FeatureDynamicMenu_EmbeddedControl";
			//Allow the embeddable control to be re-sized.
			OverlayControlCanResize = true;
			//Specify a ratio of 0 to 1 to place the control
			OverlayControlPositionRatio = new Point(0, 0);  //top left
		}

		internal static IDictionary<BasicFeatureLayer, List<long>> FeatureSelection => Selection;

		private void ShowContextMenu()
		{
			_clickPoint = MouseCursorPosition.GetMouseCursorPosition();
			System.Diagnostics.Debug.WriteLine($@"MouseLocation: {_clickPoint.X} {_clickPoint.Y}");
			var contextMenu = FrameworkApplication.CreateContextMenu("DynamicMenu_DynamicFeatureSelection", () => MouseLocation);
			contextMenu.DataContext = this;
			contextMenu.Closed += (o, e) =>
			{
							// clear the list asynchronously
							lock (LockSelection)
				{
					Selection.Clear();
				}
			};
			contextMenu.IsOpen = true;
		}

		protected override Task OnToolActivateAsync(bool hasMapViewChanged)
		{
			GetLayersFromActiveMap();
			return base.OnToolActivateAsync(hasMapViewChanged);
		}

		protected override Task OnToolDeactivateAsync(bool hasMapViewChanged)
		{
			return base.OnToolDeactivateAsync(hasMapViewChanged);
		}

		protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
		{
			List<long> oids = new List<long>();
			var vm = OverlayEmbeddableControl as EmbeddedControlViewModel;

			return QueuedTask.Run(() =>
						{
							SpatialQueryFilter spatialQueryFilter = new SpatialQueryFilter
							{
								FilterGeometry = geometry,
								SpatialRelationship = SpatialRelationship.Intersects,

							};
							var selection = vm?.SelectedLayer.Select(spatialQueryFilter);
							if (selection == null)
								return false;
							oids.AddRange(selection.GetObjectIDs());

							lock (LockSelection)
							{
								Selection.Add(vm?.SelectedLayer, oids);
							}

							Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() => ShowContextMenu()));

							return true;
						});
		}

		private void GetLayersFromActiveMap()
		{
			var vm = OverlayEmbeddableControl as EmbeddedControlViewModel;
			vm?.Layers.Clear();
			Map map = MapView.Active.Map;
			if (map == null)
				return;
			vm.MapName = map.Name;
			var layers = map.GetLayersAsFlattenedList().OfType<BasicFeatureLayer>();
			lock (LockSelection)
			{
				foreach (var layer in layers)
				{
					vm.Layers.Add(layer);
				}

				if (vm.Layers.Count > 0)
					vm.SelectedLayer = vm.Layers[0];
			}
		}
	}
}
