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
using System.Windows.Input;
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
  /// <summary>
  /// OverlayGroundSurface illustrates how to draw geometries over a ground surface using AddOverlay.  
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data).  The sample data contains a folder called 'C:\Data\Configurations\Projects' with sample data required for this solution.  Make sure that the Sample data (specifically CommunitySampleData-3D-mm-dd-yyyy.zip) is unzipped into c:\data and c:\data\PolyTest is available.
  /// 1. This solution is using the **Newtonsoft.Json NuGet**.  If needed, you can install the NuGet from the "NuGet Package Manager Console" by using this script: "Install-Package Newtonsoft.Json".
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Click Start button to debug ArcGIS Pro.
  /// 1. Open the Pro project file: PolyTest.aprx in the C:\Data\PolyTest\ folder.  Open the 'Graphic Test' tab in ArcGIS Pro.
  /// ![UI](Screenshots/Screen1.png)
  /// 1. Make sure that the 'Scene' is selected as the active map in ArcGIS Pro and then click on the 'Create 3D Polygon Tool'.
  /// 1. Digitize a polygon on the map.  Once a polygon has been digitized the buttons under the 'Polygon 2 Graphic' group are now enabled.  These buttons can be used to 'slice' the digitized polygon into smaller parts.
  /// ![UI](Screenshots/Screen2.png)
  /// 1. Note that the number of slices across the X axis can be controlled with the Resolution pull-down.  The 'Elevation' pull-down can be used add an additional elevation offset to the Z axis when the polygon is rendered in a scene.
  /// 1. Select: Resolution 4 and Elevation 5 on the respective pull-down and click the 'Make Ring MultiPatch' button.
  /// 1. The Add-in now converts the digitized polygon into a multipatch geometry and renders the multipatch in 2D and 3D.
  /// ![UI](Screenshots/Screen3.png)
  /// 1. Select: Resolution 50 and Elevation 20 on the respective pull-down and click the 'Make Ring MultiPatch' button.
  /// 1. The Add-in now converts the digitized polygon into a higher resolution multipatch geometry and renders the multipatch in 3D and 3D.
  /// ![UI](Screenshots/Screen4.png)
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current
    {
      get
      {
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("OverlayGroundSurface_Module"));
      }
    }

    internal static MapView CurrentMapView { get; set; }

    #region UI properties and states

    private static List<Geometry> _geometries = new List<Geometry>();
    private static bool _hasImportData = false;

    internal static bool HasImportData
    {
      get { return _hasImportData; }
      set { _hasImportData = value; }
    }

    internal static bool HasGeometries
    {
      get { return HasImportData && _geometries.Count > 0; }
    }

    internal static List<Geometry> Geometries
    {
      get
      {
        return _geometries;
      }
      set
      {
        if (value == null) _geometries = null;
        else
        {
          _geometries.Clear();
          if (value != null)
          {
            foreach (var geom in value)
            {
              _geometries.Add(geom.Clone());
            }
          }
        }
        SetState("has_geometry_state", _geometries != null && _geometries.Count > 0);
      }
    }

    private static Polygon _polygon = null;

    internal static Polygon Polygon
    {
      get
      {
        return _polygon;
      }
      set
      {
        if (value == null) _polygon = null;
        else _polygon = value.Clone() as Polygon;
        SetState("has_polygon_state", _polygon != null);
        foreach (var mv in _graphics.Keys)
        {
          ClearGraphics(mv);
        }
        var mvs = new List<MapView>();
        foreach (var mv in _graphic.Keys)
        {
          if (_graphic[mv] != null) mvs.Add(mv);
        }
        foreach (var mv in mvs)
        {
          _graphic[mv]?.Dispose();
          _graphic[mv] = null;
        }
        HasImportData = false;
      }
    }

    internal static void SetState(string stateName, bool active)
    {
      if (FrameworkApplication.State.Contains(stateName) == active) return;
      // toggle the state
      if (FrameworkApplication.State.Contains(stateName))
      {
        //deactivates the state
        FrameworkApplication.State.Deactivate(stateName);
      }
      else
      {
        //activates the state
        FrameworkApplication.State.Activate(stateName);
      }
    }

    internal static double Resolution { get; set; } = 1.00;

    internal static double Elevation { get; set; } = 1.00;

    #endregion UI properties and states

    #region Overlay Helpers

    private static Dictionary<MapView, IDisposable> _graphic = new Dictionary<MapView, IDisposable>();

    internal static void AddOrUpdateOverlay(Geometry geom, CIMSymbolReference symRef)
    {
      var mapView = Module1.CurrentMapView;
      if (mapView == null) return;
      ClearGraphics(mapView);
      if (!_graphic.ContainsKey(mapView))
      {
        _graphic.Add(mapView, mapView.AddOverlay(geom, symRef));
        return;
      }
      if (_graphic[mapView] == null)
      {
        if (geom is Multipatch)
        {
          var cimMpGraphic = new CIMMultiPatchGraphic
          {
            MultiPatch = geom as Multipatch,
            Symbol = symRef,
            Transparency = 64
          };
          _graphic[mapView] = mapView.AddOverlay(geom, symRef);
        }
        else
          _graphic[mapView] = mapView.AddOverlay(geom, symRef);
      }
      else
        mapView.UpdateOverlay(_graphic[mapView], geom, symRef);
    }

    internal static void ClearGraphic(MapView mapView)
    {
      if (mapView == null) return;
      if (_graphic.ContainsKey(mapView))
      {
        _graphic[mapView]?.Dispose();
        _graphic[mapView] = null;
      }
    }

    private static Dictionary<MapView, List<IDisposable>> _graphics = new Dictionary<MapView, List<IDisposable>>();

    private static void ClearGraphics(MapView mapView)
    {
      if (_graphics.ContainsKey(mapView))
      {
        foreach (var g in _graphics[mapView]) g.Dispose();
        _graphics[mapView].Clear();
      }
    }

    internal static void MultiAddOrUpdateOverlay(bool bFirst, Geometry geom, CIMSymbolReference symRef)
    {
      var mapView = Module1.CurrentMapView;
      if (mapView == null) return;
      if (bFirst)
      {
        ClearGraphic(mapView);
        ClearGraphics(mapView);
      }
      if (!_graphics.ContainsKey(mapView))
      {
        _graphics.Add(mapView, new List<IDisposable>());
      }
      _graphics[mapView].Add(Module1.CurrentMapView?.AddOverlay(geom, symRef));
    }

    #endregion Overlay Helpers

    #region Utility Helpers

    internal static bool Is3D => (Module1.CurrentMapView?.ViewingMode == MapViewingMode.SceneGlobal || Module1.CurrentMapView?.ViewingMode == MapViewingMode.SceneLocal);

    internal static double GetFishnetIntervalDistance(Envelope env)
    {
      return ((env.XMax - env.XMin) + (env.XMax - env.XMin) * 0.01) / Resolution;
    }

    #endregion Utility Helpers

    #region Polygon Helpers

    internal static List<Geometry> MakeFishnetPolygons(Polygon inputPoly)
    {
      List<Geometry> polygons = new List<Geometry>();
      Envelope envPoly = inputPoly.Extent;
      var interval = GetFishnetIntervalDistance(envPoly);
      for (var dX = envPoly.XMin; dX < envPoly.XMax + interval; dX += interval)
      {
        for (var dY = envPoly.YMin; dY < envPoly.YMax + interval; dY += interval)
        {
          var cutEnv = EnvelopeBuilder.CreateEnvelope(dX, dY, dX + interval, dY + interval, envPoly.SpatialReference);
          if (GeometryEngine.Instance.Intersects(cutEnv, inputPoly))
          {
            var addPolygonPart = GeometryEngine.Instance.Clip(inputPoly, cutEnv);
            polygons.Add(addPolygonPart);
          }
        }
      }
      return polygons;
    }

    internal static Geometry MakeFishnetPolygon(Polygon inputPoly)
    {
      Envelope envPoly = inputPoly.Extent;
      var interval = GetFishnetIntervalDistance(envPoly);
      var pb = new PolygonBuilder(inputPoly.SpatialReference)
      {
        HasZ = true
      };
      for (var dX = envPoly.XMin; dX < envPoly.XMax + interval; dX += interval)
      {
        for (var dY = envPoly.YMin; dY < envPoly.YMax + interval; dY += interval)
        {
          var cutEnv = EnvelopeBuilder.CreateEnvelope(dX, dY, dX + interval, dY + interval, envPoly.SpatialReference);
          if (GeometryEngine.Instance.Intersects(cutEnv, inputPoly))
          {
            var addPolygonPart = GeometryEngine.Instance.Clip(inputPoly, cutEnv) as Polygon;
            if (addPolygonPart.Area < 0)
            {
              System.Diagnostics.Debug.WriteLine($@"area: {addPolygonPart.Area}");
            }
            pb.AddPart(addPolygonPart.Points);
          }
        }
      }
      return pb.ToGeometry();
    }

    #endregion Polygon Helpers

    #region MultiPatch Helpers

    internal static async Task<Geometry> MakeFishnetMultiPatchAsync(Polygon inputPoly)
    {
      Envelope envPoly = inputPoly.Extent;
      var interval = GetFishnetIntervalDistance(envPoly);

      var mColor = System.Windows.Media.Colors.LightCyan;
      var materialRed = new BasicMaterial
      {
        Color = mColor,
        EdgeColor = mColor,
        EdgeWidth = 0
      };
      // create the multipatchBuilderEx object
      var mpb = new MultipatchBuilderEx();
      // create a list of patch objects
      var patches = new List<Patch>();
      for (var dX = envPoly.XMin; dX < envPoly.XMax + interval; dX += interval)
      {
        for (var dY = envPoly.YMin; dY < envPoly.YMax + interval; dY += interval)
        {
          var cutEnv = EnvelopeBuilder.CreateEnvelope(dX, dY, dX + interval, dY + interval,
                                                      envPoly.SpatialReference);
          if (GeometryEngine.Instance.Intersects(cutEnv, inputPoly))
          {
            var addPolygonPart = GeometryEngine.Instance.Clip(inputPoly, cutEnv) as Polygon;
            if (addPolygonPart.Area < 0)
            {
              System.Diagnostics.Debug.WriteLine($@"area: {addPolygonPart.Area}");
            }
            var result = await Module1.CurrentMapView.Map.GetZsFromSurfaceAsync(addPolygonPart);
            if (result.Status == SurfaceZsResultStatus.Ok)
            {
              addPolygonPart = GeometryEngine.Instance.Move(result.Geometry, 0, 0, Module1.Elevation) as Polygon;
            }
            var patch = mpb.MakePatch(esriPatchType.TriangleStrip);
            patch.Coords = GetPolygonAsTriangleStrip(addPolygonPart);
            patch.Material = materialRed;
            patches.Add(patch);
          }
        }
      }            // assign the patches to the multipatchBuilder
      mpb.Patches = patches;
      // call ToGeometry to get the multipatch
      return mpb.ToGeometry();
    }

    internal static async Task<Multipatch> MakeFishnetRingMultiPatchAsync(Polygon inputPoly)
    {
      Envelope envPoly = inputPoly.Extent;
      var interval = GetFishnetIntervalDistance(envPoly);

      var mColor = System.Windows.Media.Colors.LightCyan;
      var materialRed = new BasicMaterial
      {
        Color = mColor,
        EdgeColor = mColor,
        EdgeWidth = 0
      };
      // create the multipatchBuilderEx object
      var mpb = new MultipatchBuilderEx();
      // create a list of patch objects
      var patches = new List<Patch>();
      var first = true;
      for (var dX = envPoly.XMin; dX < envPoly.XMax + interval; dX += interval)
      {
        for (var dY = envPoly.YMin; dY < envPoly.YMax + interval; dY += interval)
        {
          var cutEnv = EnvelopeBuilder.CreateEnvelope(dX, dY, dX + interval, dY + interval,
                                                      envPoly.SpatialReference);
          if (GeometryEngine.Instance.Intersects(cutEnv, inputPoly))
          {
            var addPolygonPart = GeometryEngine.Instance.Clip(inputPoly, cutEnv) as Polygon;
            if (addPolygonPart.Area < 0)
            {
              System.Diagnostics.Debug.WriteLine($@"area: {addPolygonPart.Area}");
            }
            var result = await Module1.CurrentMapView.Map.GetZsFromSurfaceAsync(addPolygonPart);
            if (result.Status == SurfaceZsResultStatus.Ok)
            {
              addPolygonPart = GeometryEngine.Instance.Move(result.Geometry, 0, 0, Module1.Elevation) as Polygon;
            }
            var patch = mpb.MakePatch(first ? esriPatchType.FirstRing : esriPatchType.Ring);
            first = false;
            patch.InsertPoints(0, addPolygonPart.Points);
            patch.Material = materialRed;
            patches.Add(patch);
          }
        }
      }            // assign the patches to the multipatchBuilder
      mpb.Patches = patches;
      // call ToGeometry to get the multipatch
      return mpb.ToGeometry() as Multipatch;
    }

    internal static List<Coordinate3D> GetPolygonAsTriangleStrip(Polygon poly)
    {
      if (poly.PointCount < 4)
      {
        throw new Exception($@"Polygon 2 Triangle failed, too few vertices: {poly.PointCount}");
      }
      int maxPoints = poly.PointCount - 1;
      Coordinate3D[] coords = new Coordinate3D[maxPoints];
      int iNext = 0;
      foreach (var coord in poly.Copy3DCoordinatesToList())
      {
        if (iNext < maxPoints) coords[iNext++] = coord;
      }
      var lstCoords = new List<Coordinate3D>();
      int iTop = maxPoints - 1;
      int iBottom = 0;
      lstCoords.Add(DebugCoord(iBottom, coords[iBottom++]));
      lstCoords.Add(DebugCoord(iBottom, coords[iBottom++]));
      lstCoords.Add(DebugCoord(iTop, coords[iTop--]));
      var tick = true;
      for (; iTop >= iBottom; tick = !tick)
      {
        lstCoords.Add(tick ? DebugCoord(iBottom, coords[iBottom++]) : DebugCoord(iTop, coords[iTop--]));
      }
      // get the coordinates in triangle strip order
      return lstCoords;
    }

    internal static Coordinate3D DebugCoord(int idx, Coordinate3D coord)
    {
      // System.Diagnostics.Debug.WriteLine($@"Pnt: {idx} {coord.X} {coord.Y} {coord.Z} ");
      return coord;
    }

    /// <summary>
    /// Call from MCT.
    /// </summary>
    /// <returns></returns>
    internal static CIMSymbolReference GetDefaultMeshSymbol()
    {
      // find a 3d symbol
      var spi = Project.Current.GetItems<StyleProjectItem>().First(s => s.Name == "ArcGIS 3D");
      if (spi == null) return null;
      // create new structural features
      var style_item = spi.SearchSymbols(StyleItemType.MeshSymbol, "").FirstOrDefault(si => si.Name.StartsWith("Gray 40%"));
      var symbol = style_item?.Symbol as CIMMeshSymbol;
      if (symbol == null) return null;
      var meshSymbol = symbol.MakeSymbolReference();
      return meshSymbol;
    }

    #endregion MultiPatch Helpers

    #region Symbol Helpers

    private static CIMSymbolReference _polySymbolRef = null;

    internal static CIMSymbolReference GetPolygonSymbolRef()
    {
      if (_polySymbolRef != null) return _polySymbolRef;
      //Creating a polygon with a red fill and blue outline.
      CIMStroke outline = SymbolFactory.Instance.ConstructStroke(
           ColorFactory.Instance.BlueRGB, 2.0, SimpleLineStyle.Solid);
      _polySymbolRef = SymbolFactory.Instance.ConstructPolygonSymbol(
           ColorFactory.Instance.CreateRGBColor(255, 190, 190),
           SimpleFillStyle.Solid, outline).MakeSymbolReference();
      return _polySymbolRef;
    }

    private static CIMSymbolReference _lineSymbolRef = null;

    /// <summary>
    /// Get a line symbol
    /// Must be called from the MCT.  Use QueuedTask.Run.
    /// </summary>
    /// <returns></returns>
    internal static CIMSymbolReference GetLineSymbolRef()
    {
      if (_lineSymbolRef != null) return _lineSymbolRef;
      _lineSymbolRef = SymbolFactory.Instance.ConstructLineSymbol(
                          ColorFactory.Instance.RedRGB, 4,
                          SimpleLineStyle.Solid).MakeSymbolReference();
      return _lineSymbolRef;
    }

    private static CIMSymbolReference _point3DSymbolRef = null;
    private static CIMSymbolReference _point2DSymbolRef = null;

    internal static CIMSymbolReference GetPointSymbolRef()
    {
      if (Is3D)
      {
        if (_point3DSymbolRef != null) return _point3DSymbolRef;
        var pointSymbol = GetPointSymbol("ArcGIS 3D", @"Pushpin 2");
        CIMPointSymbol pnt3DSym = pointSymbol.Symbol as CIMPointSymbol;
        pnt3DSym.SetSize(200);
        pnt3DSym.SetRealWorldUnits(true);
        _point3DSymbolRef = pnt3DSym.MakeSymbolReference();
        return _point3DSymbolRef;
      }
      if (_point2DSymbolRef != null) return _point2DSymbolRef;
      CIMPointSymbol pntSym = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.RedRGB, 8, SimpleMarkerStyle.Circle);
      _point2DSymbolRef = pntSym.MakeSymbolReference();
      return _point2DSymbolRef;
    }

    private static SymbolStyleItem GetPointSymbol(string styleProjectItemName, string symbolStyleName)
    {
      var style3DProjectItem = Project.Current.GetItems<StyleProjectItem>().Where(p => p.Name == styleProjectItemName).FirstOrDefault();
      var symbolStyle = style3DProjectItem.SearchSymbols(StyleItemType.PointSymbol, symbolStyleName).FirstOrDefault();
      return symbolStyle;
    }

    #endregion Symbol Helper

    #region Overrides
    /// <summary>
    /// Called by Framework when ArcGIS Pro is closing
    /// </summary>
    /// <returns>False to prevent Pro from closing, otherwise True</returns>
    protected override bool CanUnload()
    {
      //TODO - add your business logic
      //return false to ~cancel~ Application close
      return true;
    }

    #endregion Overrides

  }
}
