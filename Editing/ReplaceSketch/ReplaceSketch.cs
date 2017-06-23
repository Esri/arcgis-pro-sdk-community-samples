//   Copyright 2017 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System.Linq;

using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace ReplaceSketch
{
  internal class ReplaceSketch : Button
  {
    protected override void OnClick()
    {
      //get the screen location of the right click
      var cmdc = (System.Windows.Point)FrameworkApplication.ContextMenuDataContext;

      QueuedTask.Run(async() =>
        {
          var mapPoint = MapView.Active.ScreenToMap(cmdc);
          
          //create search polygon at 5 pixels
          var searchPoly = CreateSearchPolygon(mapPoint, 5);

          //find features under the search geometry
          var searchFeatures = MapView.Active.GetFeatures(searchPoly);

          //get the sketch geometry
          var sketchGeom = await MapView.Active.GetCurrentSketchAsync();

          //loop through underlying features for the first one that matches sketch geometry type
          foreach (var layer in searchFeatures.Keys)
          {
            //load the first feature into the inspector
            var insp = new Inspector();
            insp.Load(layer, searchFeatures[layer].First());

            if (sketchGeom.GeometryType == insp.Shape.GeometryType)
            {
              await MapView.Active.SetCurrentSketchAsync(insp.Shape);
              break;
            }
          }
        });
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
      var searchGeom = EllipticArcBuilder.CreateEllipticArcSegment(cent, searchRadius, esriArcOrientation.esriArcClockwise, MapView.Active.Map.SpatialReference);
      var searchPB = new PolygonBuilder(new[] { searchGeom });
      return searchPB.ToGeometry();
    }
  }
}