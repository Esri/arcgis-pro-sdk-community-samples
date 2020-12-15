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
				foreach (var section in voxelLayer.GetSections())
					voxelLayer.DeleteSection(section);

				var volume = voxelLayer.GetVolumeSize();

				//Make the Normals - each is a Unit Vector (x, y, z)
				var north_south = new Coordinate3D(1, 0, 0);
				var east_west = new Coordinate3D(0, 1, 0);
				var horizontal = new Coordinate3D(0, 0, 1);

				int n = 0;
				foreach (var normal in new List<Coordinate3D> { north_south, east_west, horizontal })
				{
					voxelLayer.CreateSection(new SectionDefinition()
					{
						Name = $"Cross {++n}",
						VoxelPosition = new Coordinate3D(volume.Item1 / 2, volume.Item2 / 2, volume.Item3 / 2),
						Normal = normal,
						IsVisible = true
					});
				}
				
			});
		}
	}
}
