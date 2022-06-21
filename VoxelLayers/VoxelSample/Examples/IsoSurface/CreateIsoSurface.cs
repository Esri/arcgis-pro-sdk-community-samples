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
using ArcGIS.Desktop.Mapping.Voxel;

namespace VoxelSample.Examples.IsoSurface
{
	/// <summary>
	/// Illustrates how to create an isosurface
	/// </summary>
	internal class CreateIsoSurface : Button
	{
		protected override void OnClick()
		{
			//Selected voxel layer
			var voxelLayer = MapView.Active.GetSelectedLayers().OfType<VoxelLayer>().FirstOrDefault();
			if (voxelLayer == null)
			{
				voxelLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<VoxelLayer>().FirstOrDefault();
				if (voxelLayer == null)
					return;
			}

			QueuedTask.Run(() =>
			{
				var selProfile = voxelLayer.SelectedVariableProfile;

				//Visualization must be surface or CanCreateIsosurface will be false
				if (voxelLayer.Visualization != VoxelVisualization.Surface)
					voxelLayer.SetVisualization(VoxelVisualization.Surface);

				if (selProfile.CanCreateIsosurface)
				{
					//To stop the Voxel Exploration Dockpane activating use:
					//voxelLayer.AutoShowExploreDockPane = false;
					//This is useful if u have your own dockpane activated...
					//
					//voxelLayer.AutoShowExploreDockPane = true; to set the
					//behavior back to "usual"...

					//stats - data range
					var min = selProfile.Statistics.MinimumValue;
					var max = selProfile.Statistics.MaximumValue;
					var mid = (max + min) / 2;

					//color range (i.e. values that are being rendered)
					var renderer = selProfile.Renderer as CIMVoxelStretchRenderer;
					var color_min = renderer.ColorRangeMin;
					var color_max = renderer.ColorRangeMax;
					//keep the surface within the current color range
					if (mid < color_min)
					{
						mid = renderer.ColorRangeMin;
					}
					else if (mid > color_max)
					{
						mid = renderer.ColorRangeMax;
					}
					//To change ColorRange, set the relevant values on the renderer and then
					//use - selProfile.SetRenderer(renderer) - to update the renderer

					//Create the iso surface
					var suffix = Math.Truncate(mid * 100) / 100;
					selProfile.CreateIsosurface(new IsosurfaceDefinition()
					{
						Name = $"Surface {suffix}",
						Value = mid,
						IsVisible = true
					});
				}
			});
		}
	}
}
