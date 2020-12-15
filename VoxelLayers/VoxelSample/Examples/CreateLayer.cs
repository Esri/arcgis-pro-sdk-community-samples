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
	internal class CreateLayer : Button
	{

		private string _path = "";
		protected override void OnClick()
		{
			//Set path to a .NetCDF
			_path = @"E:\Data\SDK\Test\ForSamples\2.7\VoxelData\china_epa.nc";
			var map = MapView.Active.Map;

			QueuedTask.Run(() =>
			{
				
				var cim_connection = new CIMVoxelDataConnection()
				{
					URI = _path
				};
				//Can also just use the path....
				var createParams = VoxelLayerCreationParams.Create(cim_connection);
				createParams.IsVisible = true;

				//VoxelLayerCreationParams allows you to enumerate the variables within the voxel
				var variables = createParams.Variables;
				foreach (var variable in variables)
				{
					var line = $"{variable.Variable}: {variable.DataType}, " +
										 $"{variable.Description}, {variable.IsDefault}, {variable.IsSelected}";
					System.Diagnostics.Debug.WriteLine(line);
				}
				//You can also pick the default variable
				createParams.SetDefaultVariable(variables.Last());

				//Create the layer - map must be a local scene!
				LayerFactory.Instance.CreateLayer<VoxelLayer>(createParams, map);
			});
		}
	}
}
