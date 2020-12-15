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

namespace VoxelSample.Examples.IsoSurface
{
	/// <summary>
	/// Illustrates how to delete an isosurface
	/// </summary>
	internal class DeleteIsoSurface : Button
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
				var selProfile = voxelLayer.SelectedVariableProfile;
				//Change the color of the first surface
				var surface = selProfile.GetIsosurfaces().LastOrDefault();
				
				if (surface != null)
				{
					selProfile.DeleteIsosurface(surface);
				}
				if (selProfile.GetIsosurfaces().Count() == 0)
				{
					voxelLayer.SetVisualization(VoxelVisualization.Volume);
				}
			});
		}
	}
}
