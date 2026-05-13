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

namespace EditOperationRowEvent
{
  internal class CreateZone : Button
  {
    protected override void OnClick()
    {
      QueuedTask.Run(() =>
      {
        // create a new record

        // Get the active map view
        var activeMapView = MapView.Active;

        // Check if there is an active map view
        if (activeMapView == null)
        {
          MessageBox.Show("No active map view found.", "Error");
          return;
        }

        // Get the first feature layer in the current map
        var featureLayer = activeMapView.Map.GetLayersAsFlattenedList()
                                           .OfType<FeatureLayer>()
                                           .FirstOrDefault();

        // Check if a feature layer was found
        if (featureLayer == null)
        {
          MessageBox.Show("No feature layer found in the current map.", "Error");
          return;
        }

        // Use the feature layer as needed
        MessageBox.Show($"Feature Layer Found: {featureLayer.Name}", "Success");

        // Determine the type of geometry supported by the feature layer
        var shapeType = featureLayer.GetFeatureClass().GetDefinition().GetShapeType();

        switch(shapeType)
        {
          case GeometryType.Point:
            MessageBox.Show("The feature layer supports Point geometry. This sample only works with polygon layers", "Geometry Type");
            break;
          case GeometryType.Multipoint:
            MessageBox.Show("The feature layer supports Multipoint geometry. This sample only works with polygon layers", "Geometry Type");
            break;
          case GeometryType.Polyline:
            MessageBox.Show("The feature layer supports Polyline geometry. This sample only works with polygon layers", "Geometry Type");
            break;
          case GeometryType.Polygon:
            MessageBox.Show("The feature layer supports Polygon geometry.", "Geometry Type");

            // Example: Create a polygon geometry
            var geom = MapView.Active.Extent.Expand(0.1, 0.1, true);
            var poly = new PolygonBuilderEx(geom).ToGeometry();

            // Create an edit operation and execute
            var editOp = new EditOperation();
            editOp.Name = "Create crowd plan";
            editOp.Create(featureLayer, poly);
            editOp.Execute();
            break;
          default:
            MessageBox.Show("Unknown geometry type.", "Geometry Type");
            break;
        }

      });
    }
  }
}
