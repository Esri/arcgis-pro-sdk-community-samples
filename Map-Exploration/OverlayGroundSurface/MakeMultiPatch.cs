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
  internal class MakeMultiPatch : Button
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
          var fishnetPolygon = await QueuedTask.Run(async () =>
          {
            var multiPatchPoly = await Module1.MakeFishnetMultiPatchAsync(polygon);
            //if (Module1.Is3D)
            //{
            //  // 3D
            //  var result = await Module1.CurrentMapView.Map.GetZsFromSurfaceAsync(multiPatchPoly);
            //  if (result.Status == SurfaceZsResultStatus.Ok)
            //  {
            //    var movedZup = GeometryEngine.Instance.Move(result.Geometry, 0, 0, Module1.Elevation) as Polygon;
            //    Module1.AddOrUpdateOverlay(movedZup, Module1.GetDefaultMeshSymbol());
            //  }
            //}
            //else
            //{
            // 2D
            Module1.AddOrUpdateOverlay(multiPatchPoly, Module1.GetDefaultMeshSymbol());
            //}
            return multiPatchPoly;
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
