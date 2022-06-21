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

namespace VoxelSample.Examples.Slice
{
	internal class CreateSlice : Button
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
				if (voxelLayer.Visualization != VoxelVisualization.Volume)
					voxelLayer.SetVisualization(VoxelVisualization.Volume);
				voxelLayer.SetSliceContainerExpanded(true);
				voxelLayer.SetSliceContainerVisibility(true);

				//To stop the Voxel Exploration Dockpane activating use:
				//voxelLayer.AutoShowExploreDockPane = false;
				//This is useful if u have your own dockpane activated...
				//
				//voxelLayer.AutoShowExploreDockPane = true; to set the
				//behavior back to "usual"...

				//Create a slice that cuts the volume in two
				var volume = voxelLayer.SelectedVariableProfile.Volume;
				var vol_size = volume.GetVolumeSize();
				var normal = voxelLayer.GetNormal(90, 0.0);
				volume.CreateSlice(new SliceDefinition()
				{
					Name = "Middle Slice",
					VoxelPosition = new Coordinate3D(vol_size.X / 2, vol_size.Y / 2,
																					 vol_size.Z / 2),
					Normal = normal,
					IsVisible = true
				});
			});
		}
	}
}
