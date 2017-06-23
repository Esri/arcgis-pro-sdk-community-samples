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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace MainConnectorManhole
{
  internal class Mcm : MapTool
  {
    public Mcm() : base()
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Line;
      SketchOutputMode = ArcGIS.Desktop.Mapping.SketchOutputMode.Map;
      UseSnapping = true;
    }

    protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      //Run on MCT
      return QueuedTask.Run(() =>
      {
        //get the templates
        var map = MapView.Active.Map;
        IEnumerable<Layer> layers = map.GetLayersAsFlattenedList().AsEnumerable();
        Layer mainLayer = layers.FirstOrDefault(l => l.Name == "main");
        Layer mhLayer = layers.FirstOrDefault(l => l.Name == "Manhole");
        Layer conLayer = layers.FirstOrDefault(l => l.Name == "Connector");

        if ((mainLayer == null) || (mhLayer == null) || (conLayer == null))
          return false;

        var mainTemplate = mainLayer.GetTemplate("main");
        var mhTemplate = mhLayer.GetTemplate("Manhole");
        var conTemplate = conLayer.GetTemplate("Connector");

        if ((mainTemplate == null) || (mhTemplate == null) || (conTemplate == null))
          return false;

        var op = new EditOperation()
        {
          Name = "Create main-connector-manhole",
          SelectModifiedFeatures = false,
          SelectNewFeatures = false
        };

        //create the main geom
        var mainGeom = GeometryEngine.Instance.Move(geometry, 0, 0, -20);
        op.Create(mainTemplate, mainGeom);

        //create manhole points and connector
        foreach (var pnt in ((Polyline)geometry).Points)
        {
          //manhole point at sketch vertex
          op.Create(mhTemplate, pnt);

          //vertical connector between mahole and main
          var conPoints = new List<MapPoint>
          {
            pnt, //top of vertical connector
            GeometryEngine.Instance.Move(pnt, 0, 0, -20) as MapPoint //bottom of vertical connector
          };
          var conPolyLine = PolylineBuilder.CreatePolyline(conPoints);
          op.Create(conTemplate, conPolyLine);
        }
        return op.Execute();
      });
    }
  }
}
