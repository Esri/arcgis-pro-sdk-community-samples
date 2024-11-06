//
// Copyright 2024 Esri 
//
// 
//   Licensed under the Apache License, Version 2.0 (the "License"); 
//   you may not use this file except in compliance with the License. 
//   You may obtain a copy of the License at 
//
//       https://www.apache.org/licenses/LICENSE-2.0 
//
//   Unless required by applicable law or agreed to in writing, software 
//   distributed under the License is distributed on an "AS IS" BASIS, 
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//   See the License for the specific language governing permissions and 
//   limitations under the License. 

using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Editing.Templates;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KnowledgeGraphConstructionTools
{


  internal class RelateToolHelper
  {
    // cache (layer, oid) pairs for the origin and destination entities
    //  also cache the display expression 
    private string _displayExpr0 = "";
    private string _displayExpr1 = "";
    private Layer _layer0;
    private Layer _layer1;
    private long _oid0 = -1L;
    private long _oid1 = -1L;

   
    #region Sketch Tip / Messaging

    // Gets a message to use as a Sketch Tip according to the tool state
    internal string GetSketchTip(RelateToolState state, EditingTemplate template)
    {
      switch (state)
      {
        case RelateToolState.IdentifyOriginFeature: return Format0(template);
        case RelateToolState.IdentifyDestinationFeature: return Format1(template, _displayExpr0, _layer0);
        case RelateToolState.CreateRelationship:
          return Format2(template, _displayExpr0, _layer0, _displayExpr1, _layer1);
      }
      return "";
    }

    private static string msg0 => "Click on from entity to create {0} relationship.";
    private static string msg1 => "Create {0} relationship from '{1}' in {2}";
    private static string msg2 => "Create {0} relationship from '{1}' in {2} \n to '{3}' in {4}";
    private static string msg3 => "Create {0} relationship";

    private static string Format(string s, params object[] args) => string.Format(s, args).Replace("\\n", Environment.NewLine);
    private static string Format0(EditingTemplate template) => Format(msg0, template?.Name ?? "");
    private static string Format1(EditingTemplate template, string expr, MapMember member) => Format(msg1, template?.Name ?? "", expr, member?.Name ?? "");
    private static string Format2(EditingTemplate template, string expr1, MapMember member1, string expr2, MapMember member2)
      => Format(msg2, template?.Name ?? "", expr1, member1?.Name ?? "", expr2, member2?.Name ?? "");
    private static string Format3(EditingTemplate template) => Format(msg3, template?.Name ?? "");
    #endregion

    #region Origin / Destination Entity Management

    internal Layer Layer0 => _layer0;
    internal long OID0 => _oid0;
    internal Layer Layer1 => _layer1;
    internal long OID1 => _oid1;

    // Sets the internal display expression, layer, oid variables according to the tool state
    internal void SetFeature(RelateToolState state, string disp, Layer layer, long oid)
    {
      if (state == RelateToolState.IdentifyOriginFeature)
      {
        _displayExpr0 = disp;
        _layer0 = layer;
        _oid0 = oid;
      }
      else if (state == RelateToolState.IdentifyDestinationFeature)
      {
        _displayExpr1 = disp;
        _layer1 = layer;
        _oid1 = oid;
      }
    }

    internal bool HasOriginDestinationGeometries()
    {
      if ((_oid0 != -1) && (_oid1 != -1))
      {
        var g0 = GetGeometry(_layer0, _oid0) as MapPoint;
        var g1 = GetGeometry(_layer1, _oid1) as MapPoint;

        return (g0 != null) && (g1 != null);
      }
      return false;
    }

    private Geometry GetGeometry(Layer layer, long oid)
    {
      var insp = new Inspector();
      insp.Load(layer, oid);
      var shape = insp.Shape;

      // project to map before returning
      var projGeometry = GeometryEngine.Instance.Project(shape, MapView.Active.Map.SpatialReference);
      return projGeometry;
    }

    #endregion

    #region Searching
    internal Task<(Layer, long, MapPoint)> FindEntityFeature(System.Windows.Point mouseLocation)
      => Process(mouseLocation);

    private async Task<(Layer, long, MapPoint)> Process(System.Windows.Point mouseLocation)
    {
      var mapPoint = MapView.Active.ClientToMap(mouseLocation);
      // get features around the cursor
      var selSet = MapView.Active.GetFeatures(mapPoint);  

      // filter for entities only
      // these are displayed on link charts as point layers
      Layer layer = null;
      List<long> oids = null;
      var selSetDict = selSet.ToDictionary();
      foreach (var mm in selSetDict.Keys)
      {
        if (mm is FeatureLayer fLayer && (fLayer.ShapeType == esriGeometryType.esriGeometryPoint))
        {
          layer = fLayer;
          oids = selSetDict[mm];
        }
      }
      // no information if no point layer found
      if (layer == null)
      {
        return (null as Layer, -1L, mapPoint);
      }
      // no information if more than 1 feature found
      if (oids.Count > 1)
      {
        return (null as Layer, -1L, mapPoint);
      }
      // success - return the information
      var oid = oids[0];
      return (layer, oid, mapPoint);
    }
    #endregion

    #region Symbols
    private CIMSymbolReference _symbol;
    private CIMSymbolReference _disabledSymbol;

    internal static CIMColor DefaultFeedbackColor() => CIMColor.CreateRGBColor(51, 153, 255);

    internal void InitSymbols()
    {
      _symbol ??= CreateLineSymbol(DefaultFeedbackColor(), 3.0);
      _disabledSymbol ??= CreateLineSymbol(new CIMRGBColor() { R = 100, G = 100, B = 100, Alpha = 10 }, 2.0);
    }
    internal void ClearSymbols()
    {
      _symbol = null;
      _disabledSymbol = null;
    }

    internal static CIMSymbolReference CreateLineSymbol(CIMColor color, double width)
    {
      var symbol = SymbolFactory.Instance.ConstructLineSymbol(color, width);
      return new CIMSymbolReference { Symbol = symbol };
    }
    #endregion

    #region Overlay Management
    private IDisposable _overlay = null;

    // Adds a polyline between the origin and destination features to the overlay.
    // Nothing is added if the origin and destination feature point geometries cannot be determined.
    internal void AddRelationshipLineToOverlay()
    {
      var g0 = GetGeometry(_layer0, _oid0) as MapPoint;
      if (_oid1 != -1)
      {
        var g1 = GetGeometry(_layer1, _oid1) as MapPoint;
        if (g0 != null && g1 != null)
        {
          var pts = new List<MapPoint>() { g0, g1 };
          var line = ArcGIS.Core.Geometry.PolylineBuilderEx.CreatePolyline(pts);
          var graphic = new ArcGIS.Core.CIM.CIMLineGraphic
          {
            Line = line,
            Symbol = _symbol
          };
          _overlay = MapView.Active.AddOverlay(graphic);
        }
      }
    }

    // Adds a polyline between the origin feature and the "pt" to the overlay. 
    // Nothing is added if the origin feature point geometry cannot be determined.
    internal void AddLineOverlay(MapPoint pt)
    {
      var g0 = GetGeometry(_layer0, _oid0) as MapPoint;
      if (g0 != null && pt != null)
      {
        var pts = new List<MapPoint>() { g0, pt };
        var line = ArcGIS.Core.Geometry.PolylineBuilderEx.CreatePolyline(pts);
        var graphic = new ArcGIS.Core.CIM.CIMLineGraphic
        {
          Line = line,
          Symbol = _disabledSymbol
        };
        _overlay = MapView.Active.AddOverlay(graphic);
      }
    }

    internal void ClearOverlay() => SafeDispose(System.Threading.Interlocked.Exchange(ref _overlay, null));

    internal static void SafeDispose(object o)
    {
      if (o is IDisposable disp)
        disp.Dispose();
      else if (o is IEnumerable enumerable)
      {
        foreach (object x in enumerable)
          SafeDispose(x);
      }
    }
    #endregion
  }
}


