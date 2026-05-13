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
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolGetFeatures
{
  internal class GetFeaturesMapTool : MapTool
  {
    private IDisposable _graphic = null;
    private CIMSymbol _polygonSymbol = null;

    public GetFeaturesMapTool()
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Point;
      SketchOutputMode = SketchOutputMode.Map;
    }

    protected override Task OnToolActivateAsync(bool active)
    {
      return base.OnToolActivateAsync(active);
    }

    protected override Task OnToolDeactivateAsync(bool active)
    {
      if (_graphic != null)
      {
        _graphic.Dispose();
        _graphic = null;
      }
      if (_polygonSymbol != null)
      {
        _polygonSymbol = null;
      }
      return base.OnToolDeactivateAsync(active);
    }

    protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      try
      {
        if (geometry is not MapPoint mapPoint)
        {
          // Fix: Await the base method and return its result
          return await base.OnSketchCompleteAsync(geometry);
        }
        var foundMsg = await QueuedTask.Run<string>(async () =>
        {
          //create a circular polygon around the mappoint with a radius of 20 pixels
          var searchPoly = CreateSearchPolygon(mapPoint, 20);
          if (_polygonSymbol == null)
          {
            //create a polygon symbol
            var stroke = SymbolFactory.Instance.ConstructStroke(ColorFactory.Instance.RedRGB, 2, SimpleLineStyle.Solid);
            _polygonSymbol = SymbolFactory.Instance.ConstructPolygonSymbol(ColorFactory.Instance.BlackRGB, SimpleFillStyle.Null, stroke);
          }
          if (_graphic != null)
          {
            //update the graphic overlay
            this.UpdateOverlay(_graphic, searchPoly, _polygonSymbol.MakeSymbolReference());
          }
          else
          {
            _graphic = this.AddOverlay(searchPoly, _polygonSymbol.MakeSymbolReference());
          }
          StringBuilder sb = new();
          var mapView = MapView.Active;
          var foundFeatures = mapView.GetFeatures(searchPoly);
          if (foundFeatures.IsEmpty)
            return "No feature layers were found!";
          var dictFoundFeatures = foundFeatures.ToDictionary();
          foreach (var layers in dictFoundFeatures.Keys)
          {
            sb.AppendLine($"Layer: {layers.Name} - Features Found: {dictFoundFeatures[layers].Count}");
          }
          if (sb.Length == 0)
            sb.AppendLine("No underlying features were found.");
          return sb.ToString();
        });
        MessageBox.Show(foundMsg);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
      // Fix: Await the base method and return its result
      return await base.OnSketchCompleteAsync(geometry);
    }

    /// <summary>
    /// Create a circular polygon around a mappoint for with a radius in pixels.
    /// </summary>
    /// <param name="mapPoint">Center of the circle as a mappoint.</param>
    /// <param name="pixels">Circle radius in screen pixels.</param>
    /// <returns>A polygon geometry.</returns>
    private Polygon CreateSearchPolygon(MapPoint mapPoint, int pixels)
    {
      //get search radius
      var screenPoint = MapView.Active.MapToScreen(mapPoint);
      var radiusScreenPoint = new System.Windows.Point((screenPoint.X + pixels), screenPoint.Y);
      var radiusMapPoint = MapView.Active.ScreenToMap(radiusScreenPoint);
      var searchRadius = GeometryEngine.Instance.Distance(mapPoint, radiusMapPoint);

      //build a search circle geometry
      var cent = new Coordinate2D(mapPoint);
      var searchGeom = EllipticArcBuilderEx.CreateCircle(cent, searchRadius, ArcOrientation.ArcClockwise, MapView.Active.Map.SpatialReference);
      var searchPB = new PolygonBuilderEx(new[] { searchGeom }, AttributeFlags.None, mapPoint.SpatialReference);
      return searchPB.ToGeometry();
    }
  }
}
