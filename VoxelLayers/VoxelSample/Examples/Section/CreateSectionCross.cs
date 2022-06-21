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

namespace VoxelSample.Examples.Section
{
	/// <summary>
	/// Another section create example. In this example, sections are placed
	/// bisecting the voxel vertically and horizontally
	/// </summary>
	internal class CreateSectionCross : Button
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
				if (voxelLayer.Visualization != VoxelVisualization.Surface)
					voxelLayer.SetVisualization(VoxelVisualization.Surface);
				voxelLayer.SetSectionContainerExpanded(true);
				voxelLayer.SetSectionContainerVisibility(true);

				//delete all sections
				var volume = voxelLayer.SelectedVariableProfile.Volume;
				foreach (var section in volume.GetSections())
					volume.DeleteSection(section);

				var vol_size = volume.GetVolumeSize();

				//Make the Normals - each is a Unit Vector (x, y, z)
				var north_south = new Coordinate3D(1, 0, 0);
				var east_west = new Coordinate3D(0, 1, 0);
				var horizontal = new Coordinate3D(0, 0, 1);

				int n = 0;
				foreach (var normal in new List<Coordinate3D> { north_south, east_west, horizontal })
				{
					volume.CreateSection(new SectionDefinition()
					{
						Name = $"Cross {++n}",
						VoxelPosition = new Coordinate3D(vol_size.X / 2, vol_size.Y / 2,
																							vol_size.Z / 2),
						Normal = normal,
						IsVisible = true
					});
				}
				
			});
		}
	}
}
