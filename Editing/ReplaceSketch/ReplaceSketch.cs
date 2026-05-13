//   Copyright 2026 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System.Linq;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace ReplaceSketch
{
  internal class ReplaceSketch : Button
  {
    protected override async void OnClick()
    {
      try
      {
        //get the screen location of the right click
        var cmdc = (System.Windows.Point)FrameworkApplication.ContextMenuDataContext;

        await QueuedTask.Run(async () =>
          {
            var mapPoint = MapView.Active.ScreenToMap(cmdc);
            //create search polygon at 25 pixels
            var searchPoly = CreateSearchPolygon(mapPoint, 25);

            var mapView = MapView.Active;
            var featureLayers = mapView?.Map?.GetLayersAsFlattenedList().OfType<FeatureLayer>();
            if (featureLayers == null || !featureLayers.Any())
              return;
            //get the sketch geometry
            var sketchGeom = await MapView.Active.GetCurrentSketchAsync();

            // Find all feature layers with the same geometry type as the sketch
            var matchingFeatureLayers = featureLayers
              .Where(fl => fl.GetFeatureClass().GetDefinition().GetShapeType() == sketchGeom.GeometryType)
              .ToList();

            // Example: do something with the matching layers
            foreach (var layer in matchingFeatureLayers)
            {
              // perform a spatial query for each layer using the searchPoly
              var spatialQuery = new SpatialQueryFilter
              {
                FilterGeometry = searchPoly,
                SpatialRelationship = SpatialRelationship.Intersects
              };
              using var featureCursor = layer.GetFeatureClass().Search(spatialQuery, false);
              while (featureCursor.MoveNext())
              {
                using var feature = featureCursor.Current as Feature;
                // Your logic here, e.g., select, highlight, etc.
                if (feature == null) continue;
                await MapView.Active.SetCurrentSketchAsync(feature.GetShape());
                break;
              }
            }
          });
      }
      catch (System.Exception ex)
      {
        MessageBox.Show(ex.Message, "Replace Sketch Error");
      }
    }

    /// <summary>
    /// Create a circular polygon around a mappoint for with a radius in pixels.
    /// </summary>
    /// <param name="mapPoint">Center of the circle as a mappoint.</param>
    /// <param name="pixels">Circle radius in screen pixels.</param>
    /// <returns>A polygon geometry.</returns>
    private Polygon CreateSearchPolygon(MapPoint mapPoint, int pixels)
    {
      //get search radius
      var screenPoint = MapView.Active.MapToScreen(mapPoint);
      var radiusScreenPoint = new System.Windows.Point((screenPoint.X + pixels), screenPoint.Y);
      var radiusMapPoint = MapView.Active.ScreenToMap(radiusScreenPoint);
      var searchRadius = GeometryEngine.Instance.Distance(mapPoint, radiusMapPoint);

      //build a search circle geometry
      var cent = new Coordinate2D(mapPoint);
      var searchGeom = EllipticArcBuilderEx.CreateCircle(cent, searchRadius, ArcOrientation.ArcClockwise, MapView.Active.Map.SpatialReference);
      var searchPB = new PolygonBuilderEx(new[] { searchGeom }, AttributeFlags.None);
      return searchPB.ToGeometry();
    }
  }
}