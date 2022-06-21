/*

   Copyright 2022 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       https://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
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
	internal class ChangeVariableProfile : Button
	{
		protected override void OnClick()
		{
			//Here, we are going to change the current active variable profile for
			//the selected voxel layer

			var voxelLayer = MapView.Active.GetSelectedLayers().OfType<VoxelLayer>().FirstOrDefault();
			if (voxelLayer == null)
			{
				//just get the first one if there is a voxel layer in the TOC
				voxelLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<VoxelLayer>().FirstOrDefault();
				if (voxelLayer == null)
					return;
			}

			QueuedTask.Run(() =>
			{
				//Get the available profiles
				var profiles = voxelLayer.GetVariableProfiles();
				var sel_profile = voxelLayer.SelectedVariableProfile;//Currently selected one...

				var not_selected = profiles.Where(p => p.Variable != sel_profile.Variable).ToList();
				//Randomly assign a different variable profile
				if (not_selected.Count() > 0)
				{
					var rand = new Random();
					var idx = rand.Next(0, not_selected.Count());
					if (idx >= not_selected.Count())
						idx = not_selected.Count - 1;
					//Set the active variable profile
					voxelLayer.SetSelectedVariableProfile(not_selected[idx]);
				}

			});
		}
	}
}
