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

namespace VoxelSample.Examples.LockSection
{
	/// <summary>
	/// Illustrates how to unlock an (locked) sections
	/// </summary>
	internal class UnlockSections : Button
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

				voxelLayer.SetLockedSectionContainerExpanded(true);
				voxelLayer.SetSectionContainerExpanded(true);
				voxelLayer.SetSectionContainerVisibility(true);

				//Note: Just the selected locked section: 
				//var locked_section = MapView.Active.GetSelectedLockedSections().FirstOrDefault()
				foreach (var locked_section in voxelLayer.GetLockedSections())
				{
					if (locked_section.Layer.CanUnlockSection(locked_section))
						locked_section.Layer.UnlockSection(locked_section);
				}

			});
		}
	}
}
