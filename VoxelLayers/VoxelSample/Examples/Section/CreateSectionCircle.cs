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
	/// "n" degrees apart, calculated as their orietation
	/// </summary>
	internal class CreateSectionCircle : Button
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
				foreach (var section in voxelLayer.GetSections())
					voxelLayer.DeleteSection(section);

				var volume = voxelLayer.GetVolumeSize();

				//180 degrees orientation is due South. 90 degrees orientation is due west.
				var south = 180.0;
				var num_sections = 12;
				var spacing = 1 / (double)num_sections;

				for (int s = 0; s < num_sections; s++)
				{
					var orientation = south * (s * spacing);
					voxelLayer.CreateSection(new SectionDefinition()
					{
						Name = $"Circle {s + 1}",
						VoxelPosition = new Coordinate3D(volume.Item1 / 2, volume.Item2 / 2, volume.Item3 / 2),
						Normal = voxelLayer.GetNormal(orientation, 0.0),
						IsVisible = true
					});
				}
			});
		}
	}
}
