/*

   Copyright 2026 Esri

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
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolarSuitability
{
  internal class MapToolTargetPropertyTrigger : MapTool
  {
    public MapToolTargetPropertyTrigger()
    {
      OverlayControlID = "SolarSuitability_TargetPropertyInformation";
      OverlayControlPositionRatio = new System.Windows.Point(0, 0); //top left

      IsSketchTool = true;
      SketchType = SketchGeometryType.Point;
      SketchOutputMode = SketchOutputMode.Map;
    }

    protected override Task OnToolActivateAsync(bool active)
    {
      return base.OnToolActivateAsync(active);
    }

    protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
			// find the BuildingFootprints feature layer and set the selection under the clicked point, then update the TargetPropertyInformationViewModel
      var map = MapView.Active.Map;
			if (map != null)
			{
				var buildingFootprintsLayer = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(lyr => lyr.Name == "RPHDBuildingFootprints");

				if (Module1.TargetPropInfoVM != null )
				{
					if (Module1.TargetPropInfoVM.ControlVisibility == System.Windows.Visibility.Collapsed)
					{
						// select the building footprint under the clicked point and show the TargetPropertyInformationViewModel
						if (buildingFootprintsLayer != null)
						{
							QueuedTask.Run(() =>
							{
								var spatialQueryFilter = new SpatialQueryFilter
								{
									FilterGeometry = geometry,
									SpatialRelationship = SpatialRelationship.Intersects,
									SubFields = "*"
								};
								var selectionResult = buildingFootprintsLayer.Select(spatialQueryFilter, SelectionCombinationMethod.New);
								var building = buildingFootprintsLayer.Inspect(selectionResult.GetObjectIDs().First());

                System.Diagnostics.Trace.WriteLine($@"{selectionResult.GetCount()}");
							});
						}
						Module1.TargetPropInfoVM.ControlVisibility = System.Windows.Visibility.Visible;
					}
					else
					{
						// clear the selection and hide the TargetPropertyInformationViewModel
						if (buildingFootprintsLayer != null)
						{
							QueuedTask.Run(() => buildingFootprintsLayer.ClearSelection());
						}
						Module1.TargetPropInfoVM.ControlVisibility = System.Windows.Visibility.Collapsed;
					}
				}
			}
      return base.OnSketchCompleteAsync(geometry);
    }
  }
}
