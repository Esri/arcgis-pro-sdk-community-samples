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
	/// Another section create example. In this example, sections are evenly spaced
	/// along a diagonal crossing the voxel
	/// </summary>
	internal class CreateSectionDiagonal : Button
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
				var voxel_pos = new Coordinate3D(0, 0, volume.Item3);
				var voxel_pos_ur = new Coordinate3D(volume.Item1, volume.Item2, volume.Item3);

				//Make the diagonal in voxel space we will be using
				var lineBuilder = new LineBuilder(voxel_pos, voxel_pos_ur, null);
				var diagonal = PolylineBuilder.CreatePolyline(lineBuilder.ToSegment());

				var num_sections = 12;
				var spacing = 1 / (double)num_sections;

				//change as needed
				var orientation = 20.0;
				var tilt = -15.0;
				var normal = voxelLayer.GetNormal(orientation, tilt);

				for (int s = 0; s < num_sections; s++)
				{
					Coordinate2D end_pt = new Coordinate2D(0, 0);
					if (s > 0)
					{
						//position each section along the diagonal
						var segments = new List<Segment>() as ICollection<Segment>;
						var part = GeometryEngine.Instance.GetSubCurve3D(
								diagonal, 0.0, s * spacing, AsRatioOrLength.AsRatio);
						part.GetAllSegments(ref segments);
						end_pt = segments.First().EndCoordinate;
					}

					voxelLayer.CreateSection(new SectionDefinition()
					{
						Name = $"Diagonal {s + 1}",
						VoxelPosition = new Coordinate3D(end_pt.X, end_pt.Y, volume.Item3),
						Normal = normal,
						IsVisible = true
					});
				}
			});
		}
	}
}
