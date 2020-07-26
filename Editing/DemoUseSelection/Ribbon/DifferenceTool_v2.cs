/*

   Copyright 2020 Esri

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
using DemoUseSelection.Helpers;

namespace DemoUseSelection.Ribbon
{
	internal class DifferenceTool_v2 : MapTool
	{
		private bool _inSelMode = false;
		private bool _sketchStarted = false;

		/// <summary>
		/// In this example we are manually toggling to selection mode using
		/// the key "W". We manually switch from sketch to selection and back
		/// using: 
		///   Task&lt;bool&gt; ActivateSelectAsync(bool activate);
		/// 
		/// rather than the built-in Shift key. Note that when we switch back
		/// to sketch mode the sketch will be restored _if_ UseSelection = true
		/// otherwise it is cleared.
		/// 
		/// To prevent the built-in Shift key behavior of the base tool intefering
		/// with our custom "W" key logic we also listen for the shift key and handle
		/// that event (as well as "W").
		/// </summary>
		public DifferenceTool_v2()
		{
			IsSketchTool = true;
			SketchType = SketchGeometryType.Polygon;
			SketchOutputMode = SketchOutputMode.Map;
			//Set UseSelection = false to clear the sketch if we toggle
			UseSelection = true;
		}

		#region Key Handling
		protected override void OnToolKeyDown(MapViewKeyEventArgs k)
		{
			System.Diagnostics.Debug.WriteLine($"OnToolKeyDown Key: {k.Key.ToString()}");
			//toggle sketch selection mode with a custom key
			if (k.Key == System.Windows.Input.Key.W)
			{
				if (!_inSelMode)
				{
					_inSelMode = true;
					k.Handled = true;
					System.Diagnostics.Debug.WriteLine($"OnToolKeyDown: ActivateSelectAsync(true)");
					//toggle the tool to select mode.
					//The sketch is saved if UseSelection = true;
					ActivateSelectAsync(true);
				}
			}
			else if (!_inSelMode)
			{
				//disable effect of Shift or Ctrl key
				//in the base class. We do not want it to intefere with our tool.
				//Mark the key event as handled to prevent further processing
				k.Handled = Module1.Current.IsShiftKey(k);
			}
		}

		protected override Task HandleKeyDownAsync(MapViewKeyEventArgs k)
		{
			//Called when we return "k.Handled = true;" from OnToolKeyDown
			//TODO any additional key down handling logic here
			return Task.FromResult(0);
		}

		protected override void OnToolKeyUp(MapViewKeyEventArgs k)
		{
			System.Diagnostics.Debug.WriteLine($"OnToolKeyUp Key: {k.Key.ToString()}");
			if (k.Key == System.Windows.Input.Key.W)
			{
				//if (_inSelMode)
				System.Diagnostics.Debug.WriteLine($"OnToolKeyUp: _inSelMode {_inSelMode}");
				if (_inSelMode)
				{
					_inSelMode = false;
					k.Handled = true;//process this one
					System.Diagnostics.Debug.WriteLine($"OnToolKeyUp: ActivateSelectAsync(false)");
					//Toggle us back to sketch mode. With UseSelection = true
					//the sketch is restored
					ActivateSelectAsync(false);

					//Check if sketching was actually started when we toggled modes
					if (!_sketchStarted)
					{
						//As sketching was not actually started we need to 
						//start it manually
						this.StartSketchAsync();
					}
				}
			}
			else if (_inSelMode)
			{
				//disable effect of Shift or Ctrl key
				//in the base class. We do not want it to intefere with our tool.
				//Mark the key event as handled to prevent further processing
				k.Handled = Module1.Current.IsShiftKey(k);
			}

		}

		protected override Task HandleKeyUpAsync(MapViewKeyEventArgs k)
		{
			//Called when we return "k.Handled = true;" from OnToolKeyUp
			//TODO any additional key up handling logic here
			return Task.FromResult(0);
		}

		#endregion Key Handling

		protected override Task<bool> OnSketchModifiedAsync()
		{
			//Mark that sketching has actually started
			_sketchStarted = true;
			return base.OnSketchModifiedAsync();
		}

		protected override Task<bool> OnSketchCancelledAsync()
		{
			//Mark that the sketch was cleared
			_sketchStarted = false;
			return base.OnSketchCancelledAsync();
		}

		protected override Task OnToolActivateAsync(bool active)
		{
			//clear the flags
			_inSelMode = false;
			_sketchStarted = false;
			return base.OnToolActivateAsync(active);
		}

		protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
		{
			var mv = MapView.Active;
			_sketchStarted = false;

			return QueuedTask.Run(() =>
			{

				//filter out non-polygons
				var sel_set = Module1.Current.FilterSelection(mv.Map.GetSelection());
				if (sel_set.Count() == 0)
					return false;

				//do the difference
				var editOp = new EditOperation()
				{
					Name = "Difference",
					ErrorMessage = "Difference failed"
				};

				foreach (var kvp in sel_set)
				{
					var diffGeom = GeometryEngine.Instance.Project(geometry,
																							kvp.Key.GetSpatialReference());
					editOp.Difference(kvp.Key, kvp.Value, diffGeom);
				}
				return editOp.Execute();
			});
		}
	}
}
