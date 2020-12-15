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
	/// Illustrates how to update sections
	/// </summary>
	internal class UpdateSections : Button
	{

		private double tilt_diff = 10.0;

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

				//update all sections - change the tilt
				var sections = voxelLayer.GetSections();
				if (sections.Count() == 0)
					return;
				
				(double orientation, double current_tilt) = voxelLayer.GetOrientationAndTilt(sections.First().Normal);

				//This really only works for the section (or sections)
				//created with this sample because they all get created with the same (orientation and) tilt
				var tilt = ConfigureTilt(current_tilt);
				
				foreach (var section in voxelLayer.GetSections())
				{
					section.Normal = voxelLayer.GetNormal(orientation, tilt);
					voxelLayer.UpdateSection(section);
				}
			});
		}

		private double ConfigureTilt(double current_tilt)
		{
			current_tilt += tilt_diff;
			//to flip "in a circle" tilt goes 0 -> 90 -> 0
			if (current_tilt >= 91.0)
			{
				//the section was vertical
				current_tilt = 90.0 - (current_tilt - 90.0);
				tilt_diff *= -1;//80,70,60,50, etc
			}
			else if (current_tilt <= -91.0)
			{
				//the section was vertical
				current_tilt = -90 - (current_tilt + 90.0);
				tilt_diff *= -1;//-80,-70,-60,-50, etc
			}
			return current_tilt;
		}
	}
}
