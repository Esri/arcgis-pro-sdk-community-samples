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
  internal class PointSplitsLines : MapTool
  {
    public PointSplitsLines()
    {
      IsSketchTool = true;
      UseSnapping = true;
      SketchType = SketchGeometryType.Point;
      UsesCurrentTemplate = true;
      FireSketchEvents = true;
    }

    protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      return QueuedTask.Run(() => ExecuteCreatePointAndSplitLines(geometry));
    }
    private Task<bool> ExecuteCreatePointAndSplitLines(Geometry geometry)
    {
      if (CurrentTemplate == null || geometry == null)
        return Task.FromResult(false);
      var mapView = ActiveMapView;
      if (mapView == null || mapView.Map == null)
        return Task.FromResult(false);

      var PointToSplitLines = new EditOperation
      {
        Name = string.Format("Create {0} and split intersecting lines", CurrentTemplate.Layer.Name),
        SelectModifiedFeatures = true,
        SelectNewFeatures = true,
      };

      PointToSplitLines.Create(CurrentTemplate, geometry);

      var featuresIntersect = mapView.GetFeaturesEx(geometry);
  
      foreach (var kvp in featuresIntersect.ToDictionary())
      {
        var featureLayer = kvp.Key as FeatureLayer;
        var cutOids = kvp.Value;

        if (featureLayer == null || featureLayer.ShapeType != esriGeometryType.esriGeometryPolyline)
          continue;

        var featurelayerSR = featureLayer.GetSpatialReference();
        Geometry sketchpointgeometry = geometry;
        if (featurelayerSR == null || geometry.SpatialReference == null || !geometry.SpatialReference.IsEqual(featurelayerSR))
        {
          sketchpointgeometry = GeometryEngine.Instance.Project(geometry, featurelayerSR);
        }
        PointToSplitLines.Split(featureLayer,cutOids, sketchpointgeometry);       
      }

      if (!PointToSplitLines.Execute())
      {       
        MessageBox.Show("Split failed: An error occurred while attempting to split the lines.",
            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        return Task.FromResult(false);
      }
      return Task.FromResult(false);
    }
  }
}
