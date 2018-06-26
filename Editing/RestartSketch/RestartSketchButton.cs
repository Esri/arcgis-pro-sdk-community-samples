//   Copyright 2018 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

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

using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;


namespace RestartSketch
{
  internal class RestartSketchButton : Button
  {
    protected override void OnClick()
    {
      QueuedTask.Run(async() =>
      {
        //get sketch geometry
        var sketchGeom = await MapView.Active.GetCurrentSketchAsync();

        //return if the sketch doesn't have enough points for its geometry type
        if ((sketchGeom.GeometryType == GeometryType.Polygon && sketchGeom.PointCount < 3) || (sketchGeom.GeometryType == GeometryType.Polyline && sketchGeom.PointCount < 2))
          return;

        //get the sketch as a point collection
        var pointCol = ((Multipart)sketchGeom).Points;

        //get the last point in the sketch based on its geometry type
        var lastSketchPoint = pointCol[(sketchGeom.GeometryType == GeometryType.Polygon) ? pointCol.Count -2 : pointCol.Count -1];

        //build a geometry with the last sketch point and set the sketch
        if (sketchGeom.GeometryType == GeometryType.Polygon)
        {
          //sketch polygons need two points for the initial feedback to work
          var sketchPoly = new PolygonBuilder(new[] { lastSketchPoint,lastSketchPoint });
          await MapView.Active.SetCurrentSketchAsync(sketchPoly.ToGeometry());
        }
        else
        {
          var sketchPolyline = new PolylineBuilder(new[] { lastSketchPoint });
          await MapView.Active.SetCurrentSketchAsync(sketchPolyline.ToGeometry());
        }
      });
    }
  }
}
