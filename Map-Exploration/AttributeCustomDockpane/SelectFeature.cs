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
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace AttributeCustomDockpane
{
	internal class SelectFeature : MapTool
	{
		public SelectFeature()
		{
			IsSketchTool = true;
			UseSnapping = true;
			// Select the type of construction tool you wish to implement.  
			// Make sure that the tool is correctly registered with the correct component category type in the daml 
			SketchType = SketchGeometryType.Point;
		}

		/// <summary>
		/// Prepares the content for the embeddable control when the tool is activated.
		/// </summary>
		/// <param name="hasMapViewChanged"></param>
		/// <returns></returns>
		protected override Task OnToolActivateAsync(bool hasMapViewChanged)
		{
			// show the attribute dockpane
			ShowAttributeViewModel.Show();

			return Task.FromResult(true);
		}

		/// <summary>
		/// Called when the sketch finishes. This is where we will create the sketch operation and then execute it.
		/// </summary>
		/// <param name="geometry">The geometry created by the sketch.</param>
		/// <returns>A Task returning a Boolean indicating if the sketch complete event was successfully handled.</returns>
		protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
		{
            // select the first point feature layer in the active map
            var polyLayer = ActiveMapView.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().
                Where(lyr => lyr.ShapeType == esriGeometryType.esriGeometryPolygon).FirstOrDefault();

            if (polyLayer == null)
                return Task.FromResult(true);

            // execute the select on the MCT
            QueuedTask.Run(() =>
            {
                // define the spatial query filter
                var spatialQuery = new SpatialQueryFilter() { FilterGeometry = geometry, SpatialRelationship = SpatialRelationship.Intersects };

                // gather the selection
                var pointSelection = polyLayer.Select(spatialQuery);

                List<long> oids = pointSelection.GetObjectIDs().ToList();
                if (oids.Count == 0)
                    return;

				// show the first selected item in the attribute inspector
				var inspector = Module1.AttributeInspector;
				inspector?.LoadAsync(polyLayer, oids[0]);
				Module1.AttributeViewModel.Geometry = inspector?.Shape;
				// update the heading 
				Module1.AttributeViewModel.Heading = $@"Loaded [OID]:{oids[0]}";
			});

            return Task.FromResult(true);
        }
	}
}
