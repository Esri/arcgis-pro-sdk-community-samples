/*

   Copyright 2019 Esri

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System.Windows;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data.Analyst3D;

namespace MapToolWithDynamicMenu
{
  internal class MapToolShowMenu : MapTool
  {

    private CIMPolygonSymbol _PolySymbol = null;
    private CIMPointSymbol _PointSymbol = null;
    private CIMLineSymbol _LineSymbol = null;
    private IDisposable _graphic = null;

    public MapToolShowMenu()
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Point;
      SketchOutputMode = SketchOutputMode.Screen;
    }

    protected override Task OnToolActivateAsync(bool active)
    {
      return base.OnToolActivateAsync(active);
    }

    protected override Task OnToolDeactivateAsync(bool hasMapViewChanged)
    {
      _PolySymbol = null;
      if (_graphic != null)
      {
        _graphic.Dispose();
        _graphic = null;
      }
      return base.OnToolDeactivateAsync(hasMapViewChanged);
    }

    protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      Point bottomRight = new(0, 0); 
      if (geometry is not MapPoint clickedPnt) return false;
      try
      {
        IList<Tuple<string, string, long>> tripleTuplePoints = new List<Tuple<string, string, long>>();
        var hasSelection = await QueuedTask.Run(() =>
        {
          // geometry is a point
          if (geometry is not MapPoint clickedPnt) return false;
          // pixel tolerance
          var toleranceInScreenUnits = 10;
          // Use bottomRight to popup the dynamic menu result
          bottomRight = new Point(clickedPnt.X + toleranceInScreenUnits, clickedPnt.Y + toleranceInScreenUnits);
          var toleranceInMapUnits = GetScreenPointsInMapUnits(toleranceInScreenUnits, MapView.Active);

          // Create a search circle around the click point using the pixel tolerance as a radius
          var pnt = MapView.Active.ClientToMap(new Point(clickedPnt.X, clickedPnt.Y));
          var arcSegment = EllipticArcBuilderEx.CreateCircle(pnt.Coordinate2D, toleranceInMapUnits, ArcOrientation.ArcClockwise, pnt.SpatialReference);
          var circlePolygon = PolygonBuilderEx.CreatePolygon(new[] { arcSegment });

          //Get the features that intersect the search circle polygon
          var result = ActiveMapView.GetFeatures(circlePolygon);
          foreach (var kvp in result.ToDictionary())
          {
            if (kvp.Key is not BasicFeatureLayer bfl) continue;
            // only look at points
            if (bfl.ShapeType != esriGeometryType.esriGeometryPoint) continue;
            var layerName = bfl.Name;
            var oidName = bfl.GetTable().GetDefinition().GetObjectIDField();
            foreach (var oid in kvp.Value)
            {
              tripleTuplePoints.Add(new Tuple<string, string, long>(layerName, oidName, oid));
            }
          }
          return true;
        });

        // show the tolerance polygon
        if (_graphic != null)
          _graphic.Dispose();
        {
          await QueuedTask.Run(() =>
          {
            // pixel tolerance
            var toleranceInScreenUnits = 10;
            var toleranceInMapUnits = GetScreenPointsInMapUnits(toleranceInScreenUnits, MapView.Active);
            //Get the client point edges
            var pnt = MapView.Active.ClientToMap(new Point(clickedPnt.X, clickedPnt.Y));
            var arcSegment = EllipticArcBuilderEx.CreateCircle(pnt.Coordinate2D, toleranceInMapUnits, ArcOrientation.ArcClockwise, pnt.SpatialReference);
            var circlePolygon = PolygonBuilderEx.CreatePolygon(new[] { arcSegment });

            _graphic = MapView.Active.AddOverlay(circlePolygon, CIMSelectionPolySymbol.MakeSymbolReference());
          });
        }
        if (hasSelection)
        {
          ShowContextMenu(bottomRight, tripleTuplePoints);
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show($@"Exception: {ex}");
      }
      return true;
    }

    private void ShowContextMenu(System.Windows.Point screenLocation, IList<Tuple<string, string, long>> tripleTuplePoints)
    {
      var contextMenu = FrameworkApplication.CreateContextMenu("DynamicMenu_SelectPoint", () => screenLocation);
      if (contextMenu == null) return;
      DynamicSelectPointMenu.SetMenuPoints(tripleTuplePoints);
      contextMenu.Closed += (o, e) =>
      {
        // nothing to do
        System.Diagnostics.Debug.WriteLine(e);
      };
      contextMenu.IsOpen = true;
    }


    private CIMPolygonSymbol CIMSelectionPolySymbol
    {
      get
      {
        if (_PolySymbol != null) return _PolySymbol;

        //Create a solid fill polygon symbol for the callout.
        var aquaBackground = ColorFactory.Instance.CreateRGBColor(190, 255, 232, 100);
        _PolySymbol = SymbolFactory.Instance.ConstructPolygonSymbol(aquaBackground, SimpleFillStyle.DiagonalCross);
        return _PolySymbol;
      }
    }

    private CIMLineSymbol CIMSelectionLineSymbol
    {
      get
      {
        if (_LineSymbol != null) return _LineSymbol;

        //Create a solid fill polygon symbol for the callout.
        var aquaBackground = ColorFactory.Instance.CreateRGBColor(190, 255, 232, 100);
        _LineSymbol = SymbolFactory.Instance.ConstructLineSymbol(ColorFactory.Instance.RedRGB, 2, SimpleLineStyle.Solid);
        return _LineSymbol;
      }
    }

    private CIMPointSymbol CIMSelectionPointSymbol
    {
      get
      {
        if (_PointSymbol != null) return _PointSymbol;

        _PointSymbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.RedRGB, 20, SimpleMarkerStyle.Circle);
        return _PointSymbol;
      }
    }

    private double GetScreenPointsInMapUnits (double pixels, MapView mapView)
    {
      var left = new Point(0, 0);
      var right = new Point(0, pixels);
      var pntLeft = mapView.ClientToMap(left);
      var pntRight = mapView.ClientToMap(right);
      var line = LineBuilderEx.CreateLineSegment (pntLeft, pntRight);
      return line.Length;
    }
  }
}
