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
using ArcGIS.Core.Internal.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;

namespace ConstructionSplitTool
{
  internal class LineSplitsPolygons : MapTool
  {
    public LineSplitsPolygons()
    {
      IsSketchTool = true;
      UseSnapping = true;
      SketchType = SketchGeometryType.Line;
      UsesCurrentTemplate = true;
      FireSketchEvents = true;
    }
    protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      return QueuedTask.Run(() => ExecuteCreateLineAndSplitPolygons(geometry));
    }
    protected Task<bool> ExecuteCreateLineAndSplitPolygons(Geometry geometry)
    {
      if (CurrentTemplate == null || geometry == null)
        return Task.FromResult(false);

      var mapView = ActiveMapView;
      if (mapView == null || mapView.Map == null)
        return Task.FromResult(false);

      var LineSplitsIntersectingPolygons = new EditOperation
      {
        Name = string.Format("Create {0} and split intersecting Polygons", CurrentTemplate.Layer.Name),
        SelectNewFeatures = true,
        SelectModifiedFeatures = true
      };

      var selectionSet = mapView.GetFeaturesEx(geometry);

      var cutPolygons = new List<Polygon>();

      foreach (var kvp in selectionSet.ToDictionary())
      {
        var featureLayer = kvp.Key as FeatureLayer;

        if (featureLayer == null || featureLayer.ShapeType != esriGeometryType.esriGeometryPolygon)
          continue;

        var cutOIDs = new List<long>();
        var table = featureLayer.GetTable();

        var QueryFilter = new QueryFilter
        {
          ObjectIDs = kvp.Value,
        };

        using (var rowCursor = table.Search(QueryFilter, false))
        {
          while (rowCursor.MoveNext())
          {
            var feature = rowCursor.Current as Feature;
            if (feature?.GetShape() is Polygon polygon)
            {
              cutPolygons.Add(polygon);
              cutOIDs.Add(feature.GetObjectID());
            }
          }
        }
        if (cutOIDs.Count > 0)
          LineSplitsIntersectingPolygons.Split(featureLayer, cutOIDs, geometry);
      }

      if (cutPolygons.Count > 0)
      {
        var sketchSR = geometry.SpatialReference;
        var projCutPolygons = new List<Polygon>(cutPolygons.Count);
        foreach (var polygon in cutPolygons)
        {
          if (polygon.SpatialReference != sketchSR)
          {
            projCutPolygons.Add(GeometryEngine.Instance.Project(polygon, sketchSR) as Polygon);
          }
          else
          {
            projCutPolygons.Add(polygon);
          }
        }

        var unionedPolygon = GeometryEngine.Instance.Union(projCutPolygons) as Polygon;

        if (unionedPolygon != null && !unionedPolygon.IsEmpty &&
           unionedPolygon.SpatialReference.IsEqual(geometry.SpatialReference))
        {
          var boundaryPolyline = GeometryEngine.Instance.Boundary(unionedPolygon) as Polyline;

          if (boundaryPolyline != null)
          {
            var cutResult = GeometryEngine.Instance.Cut((Polyline)geometry, boundaryPolyline);

            if (cutResult != null && cutResult?.Count > 1)
            {
              foreach (Polyline part in cutResult)
              {
                if (part == null || part.IsEmpty) continue;
                foreach (var singlePart in part.Parts)
                {
                  var explodedPolyline = PolylineBuilder.CreatePolyline(singlePart, part.SpatialReference);

                  if (!GeometryEngine.Instance.IsSimpleAsFeature(explodedPolyline))
                    explodedPolyline = GeometryEngine.Instance.SimplifyAsFeature(explodedPolyline) as Polyline;
                  LineSplitsIntersectingPolygons.Create(CurrentTemplate, explodedPolyline);
                }
              }
            }
          }
        }
      }

      if (!LineSplitsIntersectingPolygons.Execute())
      {
        MessageBox.Show("Split failed: An error occurred while attempting to split the polygons.",
            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        return Task.FromResult(false);
      }
      return Task.FromResult(false);
    }
  }
}
