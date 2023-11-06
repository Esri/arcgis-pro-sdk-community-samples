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

namespace LASDatasetAPISamples.LASRenderers
{
  /// <summary>
  /// Symbolizes your LAS layer using Points, based on Elevation. 
  /// </summary>
  internal class Points_Elevation : Button
  {
    protected async override void OnClick()
    {
      var map = MapView.Active?.Map;
      if (map == null) return;
      var lasDatasetLayer = map.GetLayersAsFlattenedList().OfType<LasDatasetLayer>().FirstOrDefault();
      if (lasDatasetLayer == null) return;
      await QueuedTask.Run(() => {
        if (lasDatasetLayer != null)
        {
          var lasStretchRendererDefn = new LasStretchRendererDefinition(LASStretchAttribute.Elevation, LASStretchType.MinimumMaximum, 0);
          lasStretchRendererDefn.ModulateUsingIntensity = false;

          lasStretchRendererDefn.ColorRamp = Utilitites.GetColorRamp();
          if (lasDatasetLayer.CanCreateRenderer(lasStretchRendererDefn))
          {
            var elevationLasRenderer = lasDatasetLayer.CreateRenderer(lasStretchRendererDefn) as CIMLASStretchRenderer;
            if (lasDatasetLayer.CanSetRenderer(elevationLasRenderer, SurfaceRendererTarget.Points))
            {
              lasDatasetLayer.SetRenderer(elevationLasRenderer, SurfaceRendererTarget.Points);
            }
          }
        }
      });
    }
  }
  /// <summary>
  /// Symbolizes your LAS layer using Points, based on Classification Codes. 
  /// </summary>
  internal class Points_Class : Button
  {
    protected async override void OnClick()
    {
      var map = MapView.Active?.Map;
      if (map == null) return;
      var lasDatasetLayer = map.GetLayersAsFlattenedList().OfType<LasDatasetLayer>().FirstOrDefault();
      if (lasDatasetLayer == null) return;
      await QueuedTask.Run(() => {
        if (lasDatasetLayer != null)
        {
          var pointSymbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.BlackRGB, 2);
          var lasUVRRendererDefn = new LasUniqueValueRendererDefinition(LasAttributeType.Classification, false, pointSymbol.MakeSymbolReference(), 0, Utilitites.GetColorRamp("Enamel"));

          if (lasDatasetLayer.CanCreateRenderer(lasUVRRendererDefn))
          {
            var elevationLasRenderer = lasDatasetLayer.CreateRenderer(lasUVRRendererDefn) as CIMLASUniqueValueRenderer;
            if (lasDatasetLayer.CanSetRenderer(elevationLasRenderer, SurfaceRendererTarget.Points))
            {
              lasDatasetLayer.SetRenderer(elevationLasRenderer, SurfaceRendererTarget.Points);
            }
          }
        }
      });
    }
  }
  /// <summary>
  /// Symbolizes your LAS layer using Points, based on Return Values. 
  /// </summary>
  internal class points_Returns : Button
  {
    protected async override void OnClick()
    {
      var map = MapView.Active?.Map;
      if (map == null) return;
      var lasDatasetLayer = map.GetLayersAsFlattenedList().OfType<LasDatasetLayer>().FirstOrDefault();
      if (lasDatasetLayer == null) return;
      await QueuedTask.Run(() => {
        if (lasDatasetLayer != null)
        {
          var pointSymbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.BlackRGB, 2);
          var lasUVRRendererDefn = new LasUniqueValueRendererDefinition(LasAttributeType.ReturnNumber, false, pointSymbol.MakeSymbolReference(), 0, Utilitites.GetColorRamp("Enamel"));

          if (lasDatasetLayer.CanCreateRenderer(lasUVRRendererDefn))
          {
            var elevationLasRenderer = lasDatasetLayer.CreateRenderer(lasUVRRendererDefn) as CIMLASUniqueValueRenderer;
            if (lasDatasetLayer.CanSetRenderer(elevationLasRenderer, SurfaceRendererTarget.Points))
            {
              lasDatasetLayer.SetRenderer(elevationLasRenderer, SurfaceRendererTarget.Points);
            }
          }
        }
      });
    }
  }
  /// <summary>
  /// Symbolize your LAS layer surface, based on Elevation.
  /// </summary>
  internal class surface_Elevation : Button
  {
    protected async override void OnClick()
    {
      var map = MapView.Active?.Map;
      if (map == null) return;
      var lasDatasetLayer = map.GetLayersAsFlattenedList().OfType<LasDatasetLayer>().FirstOrDefault();
      if (lasDatasetLayer == null) return;
      await QueuedTask.Run(() =>
      {
        if (lasDatasetLayer != null)
        {
          TinFaceClassBreaksRendererDefinition lasDatasetElevationFaceClassBreakRendererDefn = new TinFaceClassBreaksRendererDefinition(TerrainDrawCursorType.FaceElevation, ClassificationMethod.NaturalBreaks, 9, null, Utilitites.GetColorRamp("Multipart Color Scheme"));
          
          if (lasDatasetLayer.CanCreateRenderer(lasDatasetElevationFaceClassBreakRendererDefn))
          {
            var elevationFaceClassBreakLasDatasetRenderer = lasDatasetLayer.CreateRenderer(lasDatasetElevationFaceClassBreakRendererDefn) as CIMTinFaceClassBreaksRenderer;
            elevationFaceClassBreakLasDatasetRenderer.CursorType = TerrainDrawCursorType.FaceElevation;
            if (lasDatasetLayer.CanSetRenderer(elevationFaceClassBreakLasDatasetRenderer, SurfaceRendererTarget.Surface))
            {
              lasDatasetLayer.SetRenderer(elevationFaceClassBreakLasDatasetRenderer, SurfaceRendererTarget.Surface);
            }
          }
        }
      });
    }
  }
  /// <summary>
  /// Symbolize your LAS layer surface, based on Aspect Values.
  /// </summary>
  internal class surface_Aspect : Button
  {
    protected async override void OnClick()
    {
      var map = MapView.Active?.Map;
      if (map == null) return;
      var lasDatasetLayer = map.GetLayersAsFlattenedList().OfType<LasDatasetLayer>().FirstOrDefault();
      if (lasDatasetLayer == null) return;
      await QueuedTask.Run(() =>
      {
        if (lasDatasetLayer != null)
        {
          TinFaceClassBreaksAspectRendererDefinition lasDatasetElevationFaceClassBreakRendererDefn = new TinFaceClassBreaksAspectRendererDefinition(); //TinFaceClassBreaksAspectRendererDefinition 
          if (lasDatasetLayer.CanCreateRenderer(lasDatasetElevationFaceClassBreakRendererDefn))
          {
            var elevationFaceClassBreakLasDatasetRenderer = lasDatasetLayer.CreateRenderer(lasDatasetElevationFaceClassBreakRendererDefn) as CIMTinFaceClassBreaksRenderer;
            elevationFaceClassBreakLasDatasetRenderer.CursorType = TerrainDrawCursorType.FaceAspect;
            if (lasDatasetLayer.CanSetRenderer(elevationFaceClassBreakLasDatasetRenderer, SurfaceRendererTarget.Surface))
            {
              lasDatasetLayer.SetRenderer(elevationFaceClassBreakLasDatasetRenderer, SurfaceRendererTarget.Surface);
            }
          }
        }
      });
    }
  }
  /// <summary>
  /// Symbolize your LAS layer surface, based on Slope Values.
  /// </summary>
  internal class surface_slope : Button
  {
    protected async override void OnClick()
    {
      var map = MapView.Active?.Map;
      if (map == null) return;
      var lasDatasetLayer = map.GetLayersAsFlattenedList().OfType<LasDatasetLayer>().FirstOrDefault();
      if (lasDatasetLayer == null) return;
      await QueuedTask.Run(() =>
      {
        if (lasDatasetLayer != null)
        {
          TinFaceClassBreaksRendererDefinition lasDatasetElevationFaceClassBreakRendererDefn = new TinFaceClassBreaksRendererDefinition(TerrainDrawCursorType.FaceSlope, ClassificationMethod.NaturalBreaks, 9, null, Utilitites.GetColorRamp("Multipart Color Scheme")); //TinFaceClassBreaksAspectRendererDefinition 
          
          if (lasDatasetLayer.CanCreateRenderer(lasDatasetElevationFaceClassBreakRendererDefn))
          {
            var elevationFaceClassBreakLasDatasetRenderer = lasDatasetLayer.CreateRenderer(lasDatasetElevationFaceClassBreakRendererDefn) as CIMTinFaceClassBreaksRenderer;
            elevationFaceClassBreakLasDatasetRenderer.CursorType = TerrainDrawCursorType.FaceSlope;
            if (lasDatasetLayer.CanSetRenderer(elevationFaceClassBreakLasDatasetRenderer, SurfaceRendererTarget.Surface))
            {
              lasDatasetLayer.SetRenderer(elevationFaceClassBreakLasDatasetRenderer, SurfaceRendererTarget.Surface);
            }
          }
        }
      });
    }
  }
  /// <summary>
  /// Symbolize your LAS layer using Contour Lines.
  /// </summary>
  internal class lines_contour : Button
  {
    protected async override void OnClick()
    {
      var lasDatasetLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<LasDatasetLayer>().FirstOrDefault();
      if (lasDatasetLayer == null) return;
      await QueuedTask.Run(() =>
      {
        if (lasDatasetLayer != null)
        {
          var indexContourLineSymbol = SymbolFactory.Instance.ConstructLineSymbol(ColorFactory.Instance.RedRGB, 1.7);
          var regularContourLineSymbol = SymbolFactory.Instance.ConstructLineSymbol(ColorFactory.Instance.BlackRGB, 1);
          var contourRendererDefn = new TinContourRendererDefinition(regularContourLineSymbol.MakeSymbolReference(), "Regular Contour", "API: Regular Contour",
            indexContourLineSymbol.MakeSymbolReference(), "Index Contour", "API: Index Contour");
          contourRendererDefn.ContourInterval = 5;
          contourRendererDefn.ContourFactor = 5;
          contourRendererDefn.ReferenceHeight = 0;
          if (lasDatasetLayer.CanCreateRenderer(contourRendererDefn))
          {
            var contourRenderer = lasDatasetLayer.CreateRenderer(contourRendererDefn) as CIMTinContourRenderer;
            if (lasDatasetLayer.CanSetRenderer(contourRenderer, SurfaceRendererTarget.Contours))
            {
              lasDatasetLayer.SetRenderer(contourRenderer, SurfaceRendererTarget.Contours);
            }
          }
        }
      });
    }
  }
  /// <summary>
  /// Symbolize your LAS layer using Edges
  /// </summary>
  internal class lines_edges : Button
  {
    protected async override void OnClick()
    {
      var map = MapView.Active?.Map;
      if (map == null) return;
      var lasDatasetLayer = map.GetLayersAsFlattenedList().OfType<LasDatasetLayer>().FirstOrDefault();
      if (lasDatasetLayer == null) return;
      await QueuedTask.Run(() =>
      {
        if (lasDatasetLayer != null)
        {
          var EdgeLineSymbol = SymbolFactory.Instance.ConstructLineSymbol(ColorFactory.Instance.CreateRGBColor(133,0,66), 1);
          //var regularContourLineSymbol = SymbolFactory.Instance.ConstructLineSymbol(ColorFactory.Instance.BlackRGB, 1);
          var simpleEdgeRendererDefn = new TinEdgeRendererDefintion(EdgeLineSymbol.MakeSymbolReference(), "Edge Simple", "Edge Simple: Created by an add-in");
          if (lasDatasetLayer.CanCreateRenderer(simpleEdgeRendererDefn))
          {
            var simpleEdgeRenderer = lasDatasetLayer.CreateRenderer(simpleEdgeRendererDefn) as CIMTinEdgeRenderer;
            if (lasDatasetLayer.CanSetRenderer(simpleEdgeRenderer, SurfaceRendererTarget.Edges))
            {
              lasDatasetLayer.SetRenderer(simpleEdgeRenderer, SurfaceRendererTarget.Edges);
            }
          }
        }
      });
    }
  }
  /// <summary>
  /// Utility methods 
  /// </summary>
  internal class Utilitites
  {
    /// <summary>
    /// Get a color ramp from a style file
    /// </summary>
    /// <param name="colorRampName"></param>
    /// <returns></returns>
    internal static CIMColorRamp GetColorRamp(string colorRampName = "Multipart Color Scheme")
    {
      StyleProjectItem style =
        Project.Current.GetItems<StyleProjectItem>().FirstOrDefault(s => s.Name == "ArcGIS Colors");
      if (style == null) return null;
      //var colorRampList = style.SearchColorRamps("Heat Map 4 - Semitransparent");
      var colorRampList = style.SearchColorRamps(colorRampName);
      if (colorRampList == null || colorRampList.Count == 0) return null;
      return colorRampList[0].ColorRamp;
    }
  }
}
