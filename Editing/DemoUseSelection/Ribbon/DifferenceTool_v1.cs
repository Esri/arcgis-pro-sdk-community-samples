/*

   Copyright 2020 Esri

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
using System.Windows.Input;
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
	/// <summary>
	/// Sketches a polygon that does a difference against underlying polygon features.
	/// Illustrates the use of UseSelection = true and the built-in behavior of the
	/// SHIFT key to be placed into select mode.
	/// </summary>
	/// <remarks>If lines and points are also selected then they are unaffected by
	/// the difference.</remarks>
	internal class DifferenceTool_v1 : MapTool
	{

		public DifferenceTool_v1()
		{
			IsSketchTool = true;
			SketchType = SketchGeometryType.Polygon;
			SketchOutputMode = SketchOutputMode.Map;

			//To allow activation of "selection" mode
			//Hold down the SHIFT key to toggle into 
			//selection mode
			UseSelection = true;//set to false to "turn off"
		}

		protected override Task OnToolActivateAsync(bool active)
		{
			return base.OnToolActivateAsync(active);
		}

		protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
		{
			var mv = MapView.Active;

			return QueuedTask.Run(() =>
			{

				//filter out non-polygons
				var sel_set = Module1.Current.FilterSelection(mv.Map.GetSelection().ToDictionary());
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
