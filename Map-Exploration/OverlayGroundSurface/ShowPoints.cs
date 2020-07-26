/*

   Copyright 2020 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

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

namespace OverlayGroundSurface
{
  internal class ShowPoints : Button
  {
    protected override async void OnClick()
    {
      try
      {
        // do this for all active map panes
        var mapPanes = ProApp.Panes.OfType<IMapPane>();
        foreach (IMapPane mapPane in mapPanes)
        {
          if (!(mapPane as Pane).Initialized || mapPane.MapView == null)
          {
            System.Diagnostics.Debug.WriteLine($@"Not map but table? {mapPane.Caption}");
            continue;
          }
          Module1.CurrentMapView = mapPane.MapView;
          System.Diagnostics.Debug.WriteLine($@"Drawing graphics on: {Module1.CurrentMapView.Map.Name}");
          var polygon = Module1.Polygon;
          var fishnetPolygons = await QueuedTask.Run(async () =>
            {
              List<Geometry> polygons = null;
              if (Module1.HasGeometries && Module1.Geometries.Count > 0)
              {
                polygons = Module1.Geometries;
              }
              else polygons = Module1.MakeFishnetPolygons(polygon);
              bool bFirst = true;
              foreach (var fishnetPolygon in polygons)
              {
                var poly = fishnetPolygon as Polygon;
                if (Module1.Is3D)
                {
                  var result = await Module1.CurrentMapView?.Map?.GetZsFromSurfaceAsync(poly);
                  if (result.Status == SurfaceZsResultStatus.Ok)
                  {
                    poly = result.Geometry as Polygon;
                    poly = GeometryEngine.Instance.Move(poly, 0, 0, Module1.Elevation) as Polygon;
                  }
                }
                var pnts = poly.Points;
                foreach (var pnt in pnts)
                {
                  Module1.MultiAddOrUpdateOverlay(bFirst, pnt, Module1.GetPointSymbolRef());
                  bFirst = false;
                }
              }
              return polygons;
            });
        }
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($@"Exception: {ex}");
      }
    }
  }
}
