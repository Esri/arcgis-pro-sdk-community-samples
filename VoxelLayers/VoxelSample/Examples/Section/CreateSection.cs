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
	/// Illustrates how to create a section
	/// </summary>
	internal class CreateSection : Button
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
				//To stop the Voxel Exploration Dockpane activating use:
				//voxelLayer.AutoShowExploreDockPane = false;
				//This is useful if u have your own dockpane activated...
				//
				//voxelLayer.AutoShowExploreDockPane = true; to set the
				//behavior back to "usual"...

				if (voxelLayer.Visualization != VoxelVisualization.Surface)
					voxelLayer.SetVisualization(VoxelVisualization.Surface);
				voxelLayer.SetSectionContainerExpanded(true);
				voxelLayer.SetSectionContainerVisibility(true);

				//delete all sections
				foreach (var section in voxelLayer.GetSections())
					voxelLayer.DeleteSection(section);

				//Create a section that cuts the volume in two
				var volume = voxelLayer.GetVolumeSize();
				var normal = voxelLayer.GetNormal(90, 0.0);
				voxelLayer.CreateSection(new SectionDefinition()
				{
					Name = "Middle Section",
					VoxelPosition = new Coordinate3D(volume.Item1 / 2, volume.Item2 / 2, volume.Item3 / 2),
					Normal = normal,
					IsVisible = true
				});
			});
		}
	}
}
