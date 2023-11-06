/*

   Copyright 2023 Esri

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
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
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


namespace TINApiSamples.TinRenderers
{
  #region Symbolize your layer using points
  /// <summary>
  /// Points: Simple Renderer (Same Symbol)
  /// </summary>
  /// <remarks>
  /// CIMTinNodeRenderer/TinNodeRendererDefinition
  /// </remarks>
  internal class TinLayerPoints_Simple : Button
  {
    protected override void OnClick()
    {
      var map = MapView.Active?.Map;
      if (map == null) return;
      var tinLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<TinLayer>().FirstOrDefault();
      if (tinLayer == null) return;
      QueuedTask.Run(() =>
      {
        var pointSymbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.GreyRGB, 2, SimpleMarkerStyle.Star);
        pointSymbol.UseRealWorldSymbolSizes = true;
        var tinNodeRendererDefn = new TinNodeRendererDefinition();
        tinNodeRendererDefn.Label = "TIN Node renderer";
        tinNodeRendererDefn.Description = "TIN Node Renderer created with the API";
        tinNodeRendererDefn.SymbolTemplate = pointSymbol.MakeSymbolReference();
        //Alternatively, you can use this constructor
        //var tinNodeRendererDefn = new TinNodeRendererDefinition(pointSymbol.MakeSymbolReference(), "tin points", "Renderer created with the API");
        if (tinLayer.CanCreateRenderer(tinNodeRendererDefn))
        {
          var simpleTinPointRenderer = tinLayer.CreateRenderer(tinNodeRendererDefn) as CIMTinNodeRenderer;
          if (tinLayer.CanSetRenderer(simpleTinPointRenderer, SurfaceRendererTarget.Points))
          {
            tinLayer.SetRenderer(simpleTinPointRenderer, SurfaceRendererTarget.Points);
          }
        }
      });
    }
  }
  /// <summary>
  /// Points: Elevation
  /// </summary>
  /// <remarks>
  /// CIMTinNodeElevationRenderer/TinNodeClassBreaksRendererDefinition.
  /// Classification method: Standard Deviation using a Deviation Interval of One Half.
  /// </remarks>
  internal class TinLayerPoints_Elevation : Button
  {
    protected override void OnClick()
    {
      var map = MapView.Active?.Map;
      if (map == null) return;
      var tinLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<TinLayer>().FirstOrDefault();
      if (tinLayer == null) return;
      QueuedTask.Run(() =>
      {
        var pointSymbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.GreyRGB, 4, SimpleMarkerStyle.Star);
        pointSymbol.UseRealWorldSymbolSizes = true;
        var tinNodeElevationRendererDefn = new TinNodeClassBreaksRendererDefinition();
        tinNodeElevationRendererDefn.SymbolTemplate = pointSymbol.MakeSymbolReference();
        tinNodeElevationRendererDefn.ClassificationMethod = ClassificationMethod.StandardDeviation;
        tinNodeElevationRendererDefn.ColorRamp = Helpers.Utilitites.GetColorRamp();
        tinNodeElevationRendererDefn.DeviationInterval = StandardDeviationInterval.OneHalf;
        if (tinLayer.CanCreateRenderer(tinNodeElevationRendererDefn))
        {
          var elevationTinPointRenderer = tinLayer.CreateRenderer(tinNodeElevationRendererDefn) as CIMTinNodeElevationRenderer;
          if (tinLayer.CanSetRenderer(elevationTinPointRenderer, SurfaceRendererTarget.Points))
          {
            tinLayer.SetRenderer(elevationTinPointRenderer, SurfaceRendererTarget.Points);
          }
        }
      });
    }
  }
  #endregion
  #region Symbolize your layer using lines
  /// <summary>
  /// Lines: Contours
  /// </summary>  
  /// <remarks>
  /// CIMTinContourRenderer/TinContourRendererDefinition
  /// </remarks>
  internal class TinLayerLines_Contour : Button
    {
      protected override void OnClick()
      {
        var map = MapView.Active?.Map;
        if (map == null) return;
        var tinLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<TinLayer>().FirstOrDefault();
        if (tinLayer == null) return;
      QueuedTask.Run(() =>
        {
          if (tinLayer != null)
          {
            var indexContourLineSymbol = SymbolFactory.Instance.ConstructLineSymbol(ColorFactory.Instance.RedRGB, 1.7);
            var regularContourLineSymbol = SymbolFactory.Instance.ConstructLineSymbol(ColorFactory.Instance.BlackRGB, 1);
            var contourRendererDefn = new TinContourRendererDefinition(regularContourLineSymbol.MakeSymbolReference(), "Symbol Label: Regular Contour", "Symbol Description: Regular Contour",
              indexContourLineSymbol.MakeSymbolReference(), "Index Label: Index Contour", "Index Description: Index Contour");
            contourRendererDefn.ContourInterval = 10;
            contourRendererDefn.ContourFactor = 10;
            contourRendererDefn.ReferenceHeight = 7;
            if (tinLayer.CanCreateRenderer(contourRendererDefn))
            {
              var contourRenderer = tinLayer.CreateRenderer(contourRendererDefn) as CIMTinContourRenderer;
              if (tinLayer.CanSetRenderer(contourRenderer, SurfaceRendererTarget.Contours))
              {
                tinLayer.SetRenderer(contourRenderer, SurfaceRendererTarget.Contours);
              }
            }
          }
        });
      }
  }
  /// <summary>
  /// Lines: Simple Edges
  /// </summary>
  /// <remarks>
  /// CIMTinEdgeRenderer/TinEdgeRendererDefintion
  /// </remarks>
  internal class TinLayerLines_SimpleEdge : Button
  {
    protected override void OnClick()
    {
      var map = MapView.Active?.Map;
      if (map == null) return;
      var tinLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<TinLayer>().FirstOrDefault();
      if (tinLayer == null) return;
      QueuedTask.Run(() =>
      {
        if (tinLayer != null)
        {
          var edgeSimpleLines = SymbolFactory.Instance.ConstructLineSymbol(ColorFactory.Instance.RedRGB, 1.7, SimpleLineStyle.Solid, Simple3DLineStyle.Square);
          TinEdgeRendererDefintion tinEdgeRendererDefintion = new TinEdgeRendererDefintion(edgeSimpleLines.MakeSymbolReference(), "Simple Edges", "Simple Edge Renderer created by the API");
          if (tinLayer.CanCreateRenderer(tinEdgeRendererDefintion))
          {
            CIMTinEdgeRenderer cIMTinEdgeRenderer = tinLayer.CreateRenderer(tinEdgeRendererDefintion) as CIMTinEdgeRenderer;
            if (tinLayer.CanSetRenderer(cIMTinEdgeRenderer, SurfaceRendererTarget.Edges))
            {
              tinLayer.SetRenderer(cIMTinEdgeRenderer, SurfaceRendererTarget.Edges);
            }
          }
        }
      });

    }
  }
  /// <summary>
  /// Line Edges: Edge Types
  /// </summary>
  /// <remarks>
  /// TinBreaklineRendererDefinition/CIMTinBreaklineRenderer
  /// </remarks>

  internal class TinLayerLines_EdgeType : Button
    {
    protected override void OnClick()
    {
      var map = MapView.Active?.Map;
      if (map == null) return;
      var tinLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<TinLayer>().FirstOrDefault();
      if (tinLayer == null) return;
      QueuedTask.Run(() =>
      {
        if (tinLayer != null)
        {
          var regularEdgeLineSymbol = SymbolFactory.Instance.ConstructLineSymbol(ColorFactory.Instance.CreateRGBColor(39, 163, 116), 1);
          var softEdgeLineSymbol = SymbolFactory.Instance.ConstructLineSymbol(ColorFactory.Instance.CreateRGBColor(230, 0, 0), 1);
          var hardEdgeLineSymbol = SymbolFactory.Instance.ConstructLineSymbol(ColorFactory.Instance.CreateRGBColor(0, 77, 168), 1);
          var outsideEdgeLineSymbol = SymbolFactory.Instance.ConstructLineSymbol(ColorFactory.Instance.CreateRGBColor(225, 225, 225), 1);
          var defaultEdgeLineSymbol = SymbolFactory.Instance.ConstructLineSymbol(ColorFactory.Instance.GreyRGB, 1);
          TinBreaklineRendererDefinition tinBreaklineRendererDefinition = new TinBreaklineRendererDefinition();
          tinBreaklineRendererDefinition.RegularEdgeSymbol = regularEdgeLineSymbol.MakeSymbolReference();
          tinBreaklineRendererDefinition.SoftEdgeSymbol = softEdgeLineSymbol.MakeSymbolReference();
          tinBreaklineRendererDefinition.HardEdgeSymbol = hardEdgeLineSymbol.MakeSymbolReference();
          tinBreaklineRendererDefinition.OutsideEdgeSymbol = outsideEdgeLineSymbol.MakeSymbolReference();
          tinBreaklineRendererDefinition.SymbolTemplate = defaultEdgeLineSymbol.MakeSymbolReference();
          tinBreaklineRendererDefinition.UseDefaultSymbol = true;

          if (tinLayer.CanCreateRenderer(tinBreaklineRendererDefinition))
          {
            CIMTinBreaklineRenderer cIMTinBreaklineRenderer = tinLayer.CreateRenderer(tinBreaklineRendererDefinition) as CIMTinBreaklineRenderer;
            if (tinLayer.CanSetRenderer(cIMTinBreaklineRenderer, SurfaceRendererTarget.Edges))
            {
              tinLayer.SetRenderer(cIMTinBreaklineRenderer, SurfaceRendererTarget.Edges);
            }
          }
        }
      });
    }
  }
  #endregion

  #region Symbolize your layer using a surface
  /// <summary>
  /// Surface: Simple
  /// </summary>
  /// <remarks>
  /// TinFaceRendererDefinition/CIMTinFaceRenderer
  /// </remarks>
  internal class TinLayerSurface_Simple: Button
  {
    protected override void OnClick()
    {
      var map = MapView.Active?.Map;
      if (map == null) return;
      var tinLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<TinLayer>().FirstOrDefault();
      if (tinLayer == null) return;
      QueuedTask.Run(() =>
      {
        if (tinLayer != null)
        {
          var surfacePolygonSymbol = SymbolFactory.Instance.ConstructPolygonSymbol(ColorFactory.Instance.CreateRGBColor(56, 168, 0));
          TinFaceRendererDefinition faceRendererDefinition = new TinFaceRendererDefinition(surfacePolygonSymbol.MakeSymbolReference(), "Simple Surface", "Created by the API");
          if (tinLayer.CanCreateRenderer(faceRendererDefinition))
          {
            CIMTinFaceRenderer cIMTinFaceRenderer = tinLayer.CreateRenderer(faceRendererDefinition) as CIMTinFaceRenderer;
            if (tinLayer.CanSetRenderer(cIMTinFaceRenderer, SurfaceRendererTarget.Surface)) {
              tinLayer.SetRenderer(cIMTinFaceRenderer, SurfaceRendererTarget.Surface);
            }
          }
        }
      });
    }
  }
  /// <summary>
  /// Surface: Elevation
  /// </summary>
  /// <remarks>TinFaceClassBreaksRendererDefinition/CIMTinFaceClassBreaksRenderer
  /// Classification Method: Standard Deviation with a Deviation Interval of One Quarter 
  /// </remarks> 
  internal class TinLayerSurface_Elevation : Button
  {
    protected override void OnClick()
    {
      var map = MapView.Active?.Map;
      if (map == null) return;
      var tinLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<TinLayer>().FirstOrDefault();
      if (tinLayer == null) return;
      QueuedTask.Run(() =>
      {
        if (tinLayer != null )
        {
          TinFaceClassBreaksRendererDefinition tinFaceClassBreaksRendererDefinition = new TinFaceClassBreaksRendererDefinition(TerrainDrawCursorType.FaceElevation);
          tinFaceClassBreaksRendererDefinition.ClassificationMethod = ClassificationMethod.StandardDeviation;
          tinFaceClassBreaksRendererDefinition.DeviationInterval = StandardDeviationInterval.OneQuarter;
          tinFaceClassBreaksRendererDefinition.ColorRamp = Helpers.Utilitites.GetColorRamp();
          if (tinLayer.CanCreateRenderer(tinFaceClassBreaksRendererDefinition))
          {
            CIMTinFaceClassBreaksRenderer cIMTinFaceClassBreaksRenderer = tinLayer.CreateRenderer(tinFaceClassBreaksRendererDefinition) as CIMTinFaceClassBreaksRenderer;
            if (tinLayer.CanSetRenderer(cIMTinFaceClassBreaksRenderer, SurfaceRendererTarget.Surface))
            {
              tinLayer.SetRenderer(cIMTinFaceClassBreaksRenderer, SurfaceRendererTarget.Surface);
            }
          }
                                                                               
 
        }
      });
    }
  }
  /// <summary>
  /// Surface: Slope
  /// </summary> 
  /// <remarks>TinFaceClassBreaksRendererDefinition/CIMTinFaceClassBreaksRenderer
  /// Classification Method: DefinedInterval with a Interval Size of 13 
  /// </remarks>
  internal class TinLayerSurface_Slope : Button
  {
    protected override void OnClick()
    {
      var map = MapView.Active?.Map;
      if (map == null) return;
      var tinLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<TinLayer>().FirstOrDefault();
      if (tinLayer == null) return;
      QueuedTask.Run(() =>
      {
        if (tinLayer != null)
        {
          TinFaceClassBreaksRendererDefinition tinFaceClassBreaksRendererDefinition = new TinFaceClassBreaksRendererDefinition(TerrainDrawCursorType.FaceSlope);
          tinFaceClassBreaksRendererDefinition.ClassificationMethod = ClassificationMethod.DefinedInterval;
          tinFaceClassBreaksRendererDefinition.IntervalSize = 13;
          tinFaceClassBreaksRendererDefinition.ColorRamp = Helpers.Utilitites.GetColorRamp();
          if (tinLayer.CanCreateRenderer(tinFaceClassBreaksRendererDefinition))
          {
            CIMTinFaceClassBreaksRenderer cIMTinFaceClassBreaksRenderer = tinLayer.CreateRenderer(tinFaceClassBreaksRendererDefinition) as CIMTinFaceClassBreaksRenderer;
            if (tinLayer.CanSetRenderer(cIMTinFaceClassBreaksRenderer, SurfaceRendererTarget.Surface))
            {
              tinLayer.SetRenderer(cIMTinFaceClassBreaksRenderer, SurfaceRendererTarget.Surface);
            }
          }
        }
      });
    }
  }
  /// <summary>
  /// Surface: Aspect
  /// </summary>
  /// <remarks>TinFaceClassBreaksAspectRendererDefinition/CIMTinFaceClassBreaksRenderer
  /// </remarks>
  internal class TinLayerSurface_Aspect : Button
  {
    protected override void OnClick()
    {
      var map = MapView.Active?.Map;
      if (map == null) return;
      var tinLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<TinLayer>().FirstOrDefault();
      if (tinLayer == null) return;
      QueuedTask.Run(() =>
      {
        if (tinLayer != null)
        {
          TinFaceClassBreaksAspectRendererDefinition tinFaceClassBreaksRendererDefinition = new TinFaceClassBreaksAspectRendererDefinition();         
          if (tinLayer.CanCreateRenderer(tinFaceClassBreaksRendererDefinition))
          {
            CIMTinFaceClassBreaksRenderer cIMTinFaceClassBreaksRenderer = tinLayer.CreateRenderer(tinFaceClassBreaksRendererDefinition) as CIMTinFaceClassBreaksRenderer;
            cIMTinFaceClassBreaksRenderer.CursorType = TerrainDrawCursorType.FaceAspect;
            if (tinLayer.CanSetRenderer(cIMTinFaceClassBreaksRenderer, SurfaceRendererTarget.Surface))
            {
              tinLayer.SetRenderer(cIMTinFaceClassBreaksRenderer, SurfaceRendererTarget.Surface);
            }
          }
        }
      });
    }
  }
  #endregion
}

