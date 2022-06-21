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

namespace VoxelSample.Examples
{
	internal class ManipulateTOC : Button
	{
		protected override void OnClick()
		{
			//Selected voxel layer
			var voxelLayer = MapView.Active.GetSelectedLayers().OfType<VoxelLayer>().FirstOrDefault();
			if (voxelLayer == null)
				return;
			QueuedTask.Run(() =>
			{
				voxelLayer.SetExpanded(!voxelLayer.IsExpanded);
				voxelLayer.SetVisibility(!voxelLayer.IsVisible);
				voxelLayer.SetIsosurfaceContainerExpanded(!voxelLayer.IsIsosurfaceContainerExpanded);
				voxelLayer.SetIsosurfaceContainerVisibility(!voxelLayer.IsIsosurfaceContainerVisible);
				voxelLayer.SetSliceContainerVisibility(!voxelLayer.IsSliceContainerVisible);
				voxelLayer.SetSectionContainerExpanded(!voxelLayer.IsSectionContainerExpanded);
				voxelLayer.SetSectionContainerVisibility(!voxelLayer.IsSectionContainerVisible);
				voxelLayer.SetLockedSectionContainerExpanded(!voxelLayer.IsLockedSectionContainerExpanded);
				voxelLayer.SetLockedSectionContainerVisibility(!voxelLayer.IsLockedSectionContainerVisible);

				//Selected status from the TOC
				var surfaces = MapView.Active.GetSelectedIsosurfaces();//MapView.Active.SelectVoxelIsosurface()
				var slices = MapView.Active.GetSelectedSlices();//MapView.Active.SelectVoxelSlice()
				var sections = MapView.Active.GetSelectedSections();//MapView.Active.SelectVoxelSection()
				var locked_sections = MapView.Active.GetSelectedLockedSections();//MapView.Active.SelectVoxelLockedSection()

				//Visualization
				voxelLayer.SetVisualization(
					voxelLayer.Visualization == VoxelVisualization.Volume ? VoxelVisualization.Surface : VoxelVisualization.Volume);

			});
		}
	}
}
