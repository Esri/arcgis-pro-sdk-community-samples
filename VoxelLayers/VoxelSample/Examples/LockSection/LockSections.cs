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

namespace VoxelSample.Examples.LockSection
{
	/// <summary>
	/// Illustrates how to lock (dynamic) sections
	/// </summary>
	internal class LockSections : Button
	{

		protected override void OnClick()
		{

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
				voxelLayer.SetLockedSectionContainerExpanded(true);
				voxelLayer.SetLockedSectionContainerVisibility(true);

				//Note: Just the selected section: 
				//var section = MapView.Active.GetSelectedSections().FirstOrDefault()
				foreach (var section in voxelLayer.GetSections())
				{
					if (section.Layer.CanLockSection(section))
						section.Layer.LockSection(section);
				}

			});
		}
	}
}
