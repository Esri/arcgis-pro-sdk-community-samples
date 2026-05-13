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
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;

namespace ConstructionSplitTool
{
  internal class LineSplitsLines : MapTool
  {
    public LineSplitsLines()
    {
      IsSketchTool = true;
      UseSnapping = true;
      SketchType = SketchGeometryType.Line;
      UsesCurrentTemplate = true;
      FireSketchEvents = true;
    }
    protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      return QueuedTask.Run(() => ExecuteCreateLineAndSplitLines(geometry));
    }
    private Task<bool> ExecuteCreateLineAndSplitLines(Geometry geometry)
    {
      if (CurrentTemplate == null || geometry == null)
        return Task.FromResult(false);

      var mapView = ActiveMapView;
      if (mapView == null || mapView.Map == null)
        return Task.FromResult(false);

      var LineSplitsIntersectingLines = new EditOperation
      {
        Name = $"Create {CurrentTemplate.Layer.Name} and split intersecting lines",
        SelectNewFeatures = true,
        SelectModifiedFeatures = true
      };

      var selectionSet = mapView.GetFeaturesEx(geometry);

      var intersectingPolylines = new List<Polyline>();

      foreach (var kvp in selectionSet.ToDictionary())
      {
        var featureLayer = kvp.Key as FeatureLayer;

        if (featureLayer == null || featureLayer.ShapeType != esriGeometryType.esriGeometryPolyline)
          continue; 

        var cutOIDs = new List<long>();

        var table = featureLayer.GetTable();

        var featurelayerSR = featureLayer.GetSpatialReference();
        Geometry geometrySketch = geometry;
        if (featurelayerSR == null || geometry.SpatialReference == null || !geometry.SpatialReference.IsEqual(featurelayerSR))
        {
          geometrySketch = GeometryEngine.Instance.Project(geometry, featurelayerSR);
        }

        var QueryFilter = new QueryFilter
        {
          ObjectIDs = kvp.Value
        };

        using (var rowCursor = table.Search(QueryFilter,false))
        {
          while (rowCursor.MoveNext())
          {
            var feature = rowCursor.Current as Feature;
            if (feature?.GetShape() is Polyline polyline)
            {
              var splitResult = GeometryEngine.Instance.Cut(polyline, (Polyline)geometrySketch);
              if (splitResult != null)
              {
                int validCount = 0;              
                foreach (var part in splitResult)
                {
                  if (part is Polyline poly && !poly.IsEmpty)
                    validCount++;
                }
                bool isMultipartOriginal = polyline.Parts.Count > 1;
                if (isMultipartOriginal)
                {                
                  bool meaningfulSplit = false;
                  
                  foreach (var originalPart in polyline.Parts)
                  {
                   
                    var originalPartPolyline = PolylineBuilder.CreatePolyline(originalPart, polyline.SpatialReference);
 
                    var partCutResult = GeometryEngine.Instance.Cut(originalPartPolyline, geometrySketch as Polyline);
                 
                    int partValidCount = 0;
                    if (partCutResult != null)
                    {
                      foreach (var part in partCutResult)
                      {
                        if (part is Polyline poly && !poly.IsEmpty)
                          partValidCount++;
                      }
                    }
                   
                    if (partValidCount > 1)
                    {
                      meaningfulSplit = true;
                      break;
                    }
                  }
                  if (meaningfulSplit)
                  {
                    cutOIDs.Add(feature.GetObjectID());
                    intersectingPolylines.Add(polyline);
                  }
                }
                
                else
                {
                  if (validCount >= 2)
                  {
                    cutOIDs.Add(feature.GetObjectID());
                    intersectingPolylines.Add(polyline);
                  }
                }                
              }
            }
          }
        }

        if (cutOIDs.Count > 0)
          LineSplitsIntersectingLines.Split(featureLayer, cutOIDs, geometrySketch);
      }

      if (intersectingPolylines.Count > 0)
      {
        var sketchSpatialReference = geometry.SpatialReference;
        var projIntersectingLines = new List<Polyline>(intersectingPolylines.Count);
        foreach (var line in intersectingPolylines)
        {
          if (line.SpatialReference == sketchSpatialReference)
            projIntersectingLines.Add(line);
          else
            projIntersectingLines.Add((Polyline)GeometryEngine.Instance.Project(line, sketchSpatialReference));
        }

        var unionGeometry = GeometryEngine.Instance.Union(projIntersectingLines) as Polyline;
        var cutResult = GeometryEngine.Instance.Cut((Polyline)geometry, unionGeometry);

        if (cutResult != null && cutResult?.Count > 1)
        {
          foreach (var geom in cutResult)
          {
            if (geom is Polyline polyline)
            {
              foreach (var part in polyline.Parts)
              {
                var explodedPolyline = PolylineBuilder.CreatePolyline(part, polyline.SpatialReference);
                if (!GeometryEngine.Instance.IsSimpleAsFeature(explodedPolyline))
                  explodedPolyline = GeometryEngine.Instance.SimplifyAsFeature(explodedPolyline) as Polyline;
                LineSplitsIntersectingLines.Create(CurrentTemplate, explodedPolyline);
              }             
            }
          }       
        }
      }

      if (!LineSplitsIntersectingLines.Execute())
      {
        MessageBox.Show("Split failed: An error occurred while attempting to split the lines.",
            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
      }
      return Task.FromResult(false);
    }
  }
}
