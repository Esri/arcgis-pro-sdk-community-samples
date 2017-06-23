// Copyright 2017 Esri 
//
// 
//   Licensed under the Apache License, Version 2.0 (the "License"); 
//   you may not use this file except in compliance with the License. 
//   You may obtain a copy of the License at 
//
//       http://www.apache.org/licenses/LICENSE-2.0 
//
//   Unless required by applicable law or agreed to in writing, software 
//   distributed under the License is distributed on an "AS IS" BASIS, 
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//   See the License for the specific language governing permissions and 
//   limitations under the License. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.CIM;

namespace ConstructionToolWithOptions
{
  internal class CircleTool : MapTool
  {
    public CircleTool()
    {
      IsSketchTool = true;
      UseSnapping = true;
      // Select the type of construction tool you wish to implement.  
      SketchType = SketchGeometryType.Point;
    }

    #region Tool Options
    private ReadOnlyToolOptions ToolOptions => CurrentTemplate?.GetToolOptions(ID);

    private double Radius
    {
      get
      {
        if (ToolOptions == null)
          return CircleToolOptionsViewModel.DefaultRadius;

        return ToolOptions.GetProperty(CircleToolOptionsViewModel.RadiusOptionName, CircleToolOptionsViewModel.DefaultRadius);
      }
    }

    #endregion

    /// <summary>
    /// Called when the sketch finishes. This is where we will create the sketch operation and then execute it.
    /// </summary>
    /// <param name="geometry">The geometry created by the sketch.</param>
    /// <returns>A Task returning a Boolean indicating if the sketch complete event was successfully handled.</returns>
    protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      if (CurrentTemplate == null || geometry == null)
        return Task.FromResult(false);

      return QueuedTask.Run(() =>
      {
        //build a circular arc
        var cent = new Coordinate2D(geometry as MapPoint);
        var circleEAB = EllipticArcBuilder.CreateEllipticArcSegment(cent, Radius, esriArcOrientation.esriArcClockwise, MapView.Active.Map.SpatialReference);

        // find the source layer and determine whether polyline/polygon.  Create the appropriate shape
        var lyr = CurrentTemplate.Layer as BasicFeatureLayer;
        Geometry circleGeom = null;
        if (lyr.ShapeType == esriGeometryType.esriGeometryPolygon)
          circleGeom = PolygonBuilder.CreatePolygon(new[] { circleEAB });
        else
          circleGeom = PolylineBuilder.CreatePolyline(circleEAB);

        // Create an edit operation
        var createOperation = new EditOperation();
        createOperation.Name = string.Format("Create circular {0}", CurrentTemplate.Layer.Name);
        createOperation.SelectNewFeatures = true;

        // Queue feature creation
        createOperation.Create(CurrentTemplate, circleGeom);

        // Execute the operation
        return createOperation.ExecuteAsync();
      });
    }
  }
}
