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
  internal class MakeManyPolygons : Button
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
            var polygons = Module1.MakeFishnetPolygons(polygon);
            bool bFirst = true;
            foreach (var fishnetPolygon in polygons)
            {
              if (Module1.Is3D)
              {
                // 3D
                var result = await Module1.CurrentMapView?.Map?.GetZsFromSurfaceAsync(fishnetPolygon);
                if (result.Status == SurfaceZsResultStatus.Ok)
                {
                  var movedZup = GeometryEngine.Instance.Move(result.Geometry, 0, 0, Module1.Elevation) as Polygon;
                  Module1.MultiAddOrUpdateOverlay(bFirst, movedZup, Module1.GetPolygonSymbolRef());
                }
              }
              else
              {
                // 2D
                Module1.MultiAddOrUpdateOverlay(bFirst, fishnetPolygon, Module1.GetPolygonSymbolRef());
              }
              bFirst = false;
            }
            return polygons;
          });
        }
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($@"Exception: {ex.ToString()}");
      }
    }
  }
}
