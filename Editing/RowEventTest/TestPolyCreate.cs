/*

   Copyright 2024 Esri

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
using ArcGIS.Core.Events;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Editing.Events;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RowEventTest
{
  internal class TestPolyCreate : Button
  {
    private int _CreatedAnnoCount = 0;
    private int _CreatedNonAnnoCount = 0;

    protected override async void OnClick()
    {
      try
      {
        // show the event log
        ShowEventsViewModel.Show();
        _CreatedAnnoCount = 0;
        _CreatedNonAnnoCount = 0;
        Module1.EventCreatedCount = 0;
        await Module1.StartListening();
        await CreatePolyFeatures();
        await Module1.StopListening();
        Module1.AddEntry($"RowEvent / created counts are equal: {_CreatedAnnoCount + _CreatedNonAnnoCount == Module1.EventCreatedCount}");
        Module1.AddEntry($"Annotation Rows created: {_CreatedAnnoCount}");
        Module1.AddEntry($"Non-Annotation Rows created: {_CreatedNonAnnoCount}");
        Module1.AddEntry($"Total RowEvents count: {Module1.EventCreatedCount}");
      }
      catch (Exception ex)
      {
        MessageBox.Show($@"Error: {ex.Message}");
      }
    }

    private async Task CreatePolyFeatures()
    {
      var result = await QueuedTask.Run<string>(() =>
      {
        // create a list to hold 20 random coordinates in map extent
        var coordinateList = CreateRandomCoordinateList(Module1.TestPolyCycles);
        EditOperation op = null;
        var featureLayers = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where((fl) => fl.ShapeType == esriGeometryType.esriGeometryPolygon);
        if (!featureLayers.Any())
        {
          return "No polygon layers found in the current map";
        }
        foreach (var featureLayer in featureLayers)
        {
          var featTemplate = featureLayer.GetTemplatesAsFlattenedList().FirstOrDefault();
          foreach (Coordinate2D coordinate in coordinateList)
          {
            var coordPnt = MapPointBuilderEx.CreateMapPoint(coordinate, MapView.Active.Map.SpatialReference);
            var gi = GeometryEngine.Instance;
            var pnt = gi.Project(coordPnt, featureLayer.GetSpatialReference()) as MapPoint;
            // create a rectangle on the screen
            var clientPnt = MapView.Active.MapToClient(pnt);
            var xy = GetRandomXy();
            var pntForDistance = MapView.Active.ClientToMap(new System.Windows.Point(xy, xy));
            List<MapPoint> points = new() { pnt, gi.Move(pnt, 0, xy) as MapPoint, gi.Move(pnt, xy, xy) as MapPoint, gi.Move(pnt, xy, 0) as MapPoint };
            var rect = PolygonBuilderEx.CreatePolygon(points);

            // create a rect and move rect to 
            // create the edit operation
            op ??= new EditOperation
            {
              Name = "Create with text field",
              SelectModifiedFeatures = true,
              SelectNewFeatures = false
            };
            var insp = featTemplate.Inspector;
            // Find the first text field
            var firstTextField = featureLayer.GetFeatureClass().GetDefinition().GetFields().FirstOrDefault(f => f.FieldType == FieldType.String);
            if (firstTextField != null)
            {
              // change the text 
              Dictionary<string, object> attrs = new()
                {
                  {firstTextField.Name, DateTime.Now.ToLongTimeString()}
                };
              //insp.Shape = rect;
              // call modify
              op.Create(featureLayer, rect, attrs);
              _CreatedNonAnnoCount++;
            }
          }
        }
        // execute the operation
        if ((op != null) && !op.IsEmpty)
        {
          var result = op.Execute();
          var errors = result ? string.Empty : op.ErrorMessage;
          return $"Created {_CreatedAnnoCount} annos & features with result: {result} {errors}";
        }
        return "No annotation features were selected";
      });
      Module1.AddEntry(result);
    }

    // create a random number generator for the new point locations
    private Random randomGenerator = new();

    private IEnumerable<Coordinate2D> CreateRandomCoordinateList(int iCoords)
    {
      IList<Coordinate2D> coordinateList = new List<Coordinate2D>(iCoords);
      for (int i = 0; i < iCoords; i++)
      {
        coordinateList.Add(randomGenerator.NextCoordinate2D(MapView.Active.Extent));
      }
      return coordinateList;
    }

    private double GetRandomXy()
    {
      return randomGenerator.NextDouble(40, 60);
    }
  }
}
