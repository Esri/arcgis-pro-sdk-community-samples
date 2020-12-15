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
