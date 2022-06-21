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

namespace VoxelSample.Examples.IsoSurface
{
	/// <summary>
	/// Illustrates how to update an isosurface
	/// </summary>
	internal class UpdateIsoSurface : Button
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
				//Change the color of the first surface
				var surface = selProfile.GetIsosurfaces().FirstOrDefault();
				if (surface != null) 
				{

					if (voxelLayer.Visualization != VoxelVisualization.Surface)
						voxelLayer.SetVisualization(VoxelVisualization.Surface);

					//get a random color
					var count = new Random().Next(0, 100);
					var colors = ColorFactory.Instance.GenerateColorsFromColorRamp(
						((CIMVoxelStretchRenderer)selProfile.Renderer).ColorRamp, count) ;

					var idx = new Random().Next(0, count - 1);
					surface.Color = colors[idx];
					surface.IsCustomColor = true;//set the custom color flag true as we picked the color
					selProfile.UpdateIsosurface(surface);
				}
				
			});
		}
	}
}
