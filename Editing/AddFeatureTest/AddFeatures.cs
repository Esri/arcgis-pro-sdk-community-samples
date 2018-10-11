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

namespace AddFeatureTest
{
  internal class AddFeatures : Button
  {
    protected override void OnClick()
    {
      if (MapView.Active == null) return;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
      AddPointsAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    }
    private async Task AddPointsAsync()
    {
      try
      {
        // this doesn't work in real time because the layers are added asynchronously late
        var pointsLayer = MapView.Active.Map.GetLayersAsFlattenedList().Where((l) => l.Name == Module1.PointFcName).FirstOrDefault();
        if (pointsLayer == null) throw new Exception($@"Unable to find {Module1.PointFcName} Layer");

        var polyLayer = MapView.Active.Map.GetLayersAsFlattenedList().Where((l) => l.Name == Module1.PolyFcName).FirstOrDefault();
        if (polyLayer == null) throw new Exception($@"Unable to find {Module1.PolyFcName} Layer");

        await QueuedTask.Run(() =>
        {
          // make 5 points
          var centerPt = MapView.Active.Extent.Center;
          List<MapPoint> mapPoints = new List<MapPoint>();
          for (int pnt = 0; pnt < 5; pnt++)
          {
            mapPoints.Add(GeometryEngine.Instance.Move(centerPt, pnt * 150.0, pnt * 150.0) as MapPoint);
          }
          var editOp = new EditOperation
          {
            Name = "1. edit operation"
          };
          int iMap = 0;
          foreach (var mp in mapPoints)
          {
            var attributes = new Dictionary<string, object>
                {
                  { "Shape", mp.Clone() },
                  { "Description", $@"Map point: {++iMap}" }
                };
            editOp.Create(pointsLayer, attributes);
          }
          var result1 = editOp.Execute();
          if (result1 != true || editOp.IsSucceeded != true)
            throw new Exception($@"Edit 1 failed: {editOp.ErrorMessage}");
          MessageBox.Show("1. edit operation complete");

          editOp = new EditOperation
          {
            Name = "2. edit operation"
          };
          foreach (var mp in mapPoints)
          {
            var attributes = new Dictionary<string, object>
                {
                  { "Shape", GeometryEngine.Instance.Buffer(mp, 50.0) },
                  { "Description", $@"Polygon: {iMap--}" }
                };
            editOp.Create(polyLayer, attributes);
          }
          //Execute the operations
          var result2 = editOp.Execute();
          if (result2 != true || editOp.IsSucceeded != true)
            throw new Exception($@"Edit 2 failed: {editOp.ErrorMessage}");
          MessageBox.Show("2. edit operation complete");
        });
      }
      catch (Exception ex)
      {
        MessageBox.Show($@"Error: {ex.ToString()}");
      }
    }
  }
}
