//   Copyright 2020 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

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
using ArcGIS.Desktop.Mapping.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SketchRemoveCurves
{
  internal class SketchRemoveCurves : Button
  {
    ArcGIS.Core.Events.SubscriptionToken srcToken = null;
    protected override void OnClick()
    {
      //subscribe to BeforeSketchCompleted event once
      if (srcToken == null)
        srcToken = ArcGIS.Desktop.Mapping.Events.BeforeSketchCompletedEvent.Subscribe(OnBeforeSketchCompletedEvent);
    }

    private Task OnBeforeSketchCompletedEvent(BeforeSketchCompletedEventArgs arg)
    {
      //replace curved sketch segments with straight segments

      //return if sketch geometry is not polygon or polyline 
      if (!(arg.Sketch.GeometryType == GeometryType.Polyline || arg.Sketch.GeometryType == GeometryType.Polygon))
        return Task.CompletedTask;

      var sketchMP = arg.Sketch as Multipart;
      //if the sketch doesnt have curves then return
      if (!sketchMP.HasCurves)
        return Task.CompletedTask;

      //itterate through each sketch part
      var newParts = new List<List<Segment>>();
      foreach (var sketchPart in sketchMP.Parts)
      {
        //itterate through each sketch segment
        var newSegments = new List<Segment>();
        foreach (var sketchSegment in sketchPart)
        {
          if (sketchSegment.IsCurve)
            newSegments.Add(LineBuilder.CreateLineSegment(sketchSegment.StartPoint, sketchSegment.EndPoint));
          else
            newSegments.Add(sketchSegment);
        }
        newParts.Add(newSegments);
      }

      //create the new sketch geometry based on sketch type and set back on the sketch
      if (arg.Sketch.GeometryType == GeometryType.Polyline)
      {
        var polyline = PolylineBuilder.CreatePolyline(newParts);
        arg.SetSketchGeometry(polyline);
      }
      else
      {
        var polygon = PolygonBuilder.CreatePolygon(newParts);
        arg.SetSketchGeometry(polygon);
      }

      return Task.CompletedTask;
    }
  }
}
