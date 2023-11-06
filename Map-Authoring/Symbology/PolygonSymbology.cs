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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace Symbology
{
  internal static class MyPolygonSymbology
  {
    public static async Task<Dictionary<CIMSymbol, string>> GetAllPolygonSymbolsAsync()
    {
      var PolySymbols = new Dictionary<CIMSymbol, string>
            {
              {  await CreateHatchFillPolygonAsync(), "Cross Hatch"},
              {  await CreateDiagonalCrossPolygonAsync(), "CIMFill CrossHatch"},
              {  await CreateDashDotFillAsync(), "Dash Dot Fill"},
              {  await CreateGradientTwoColorsPolygonAsync(), "Gradient 2 colors" },
              {  await CreateGradientColorRampPolygonAsync(), "Gradient color ramp" },
              {  await CreateGradientFillAsync(), "CIMGradientFill"},
              {  await CreateProceduralPolygonSymbolAsync(), "Venice Procedural" },
              {  await CreatePictureFillPolygonAsync(), "Picture"},
              {  await CreateWaterFillPolygonAsync(), "Water" },
              {  await CreateRippleFillPolygonAsync(), "Ripple" },
              {  await CreateStippleFillPolygonAsync(), "Stipple"},
              {  await CreatePenInkCrossHatchFillPolygonAsync(), "PN CrossHatch"}
            };
      return PolySymbols;
    }
    // cref: ArcGIS.Desktop.Mapping.ISymbolFactory.ConstructStroke(ArcGIS.Core.CIM.CIMColor,System.Double,ArcGIS.Desktop.Mapping.SimpleLineStyle)
    // cref: ArcGIS.Core.CIM.CIMFill
    // cref: ArcGIS.Core.CIM.CIMHatchFill
    // cref: ArcGIS.Core.CIM.CIMLineSymbol
    #region Snippet Diagonal cross hatch fill
    /// <summary>
    /// Create a polygon symbol with a diagonal cross hatch fill. <br/>
    /// ![PolygonSymbolDiagonalCrossHatch](http://Esri.github.io/arcgis-pro-sdk/images/Symbology/polygon-diagonal-crosshatch.png)
    /// </summary>
    /// <returns></returns>
    public static Task<CIMPolygonSymbol> CreateDiagonalCrossPolygonAsync()
    {
      return QueuedTask.Run<CIMPolygonSymbol>(() =>
      {
        var trans = 50.0;//semi transparent
        CIMStroke outline = SymbolFactory.Instance.ConstructStroke(CIMColor.CreateRGBColor(0, 0, 0, trans), 2.0, SimpleLineStyle.Solid);

        //Stroke for the fill
        var solid = SymbolFactory.Instance.ConstructStroke(CIMColor.CreateRGBColor(255, 0, 0, trans), 1.0, SimpleLineStyle.Solid);

        //Mimic cross hatch
        CIMFill[] diagonalCross =
                  {
                    new CIMHatchFill() {
                        Enable = true,
                        Rotation = 45.0,
                        Separation = 5.0,
                        LineSymbol = new CIMLineSymbol() { SymbolLayers = new CIMSymbolLayer[1] { solid } }
                    },
                    new CIMHatchFill() {
                        Enable = true,
                        Rotation = -45.0,
                        Separation = 5.0,
                        LineSymbol = new CIMLineSymbol() { SymbolLayers = new CIMSymbolLayer[1] { solid } }
                    }
          };
        List<CIMSymbolLayer> symbolLayers = new List<CIMSymbolLayer>
          {
                    outline
          };
        foreach (var fill in diagonalCross)
          symbolLayers.Add(fill);
        return new CIMPolygonSymbol() { SymbolLayers = symbolLayers.ToArray() };
      });
    }
    #endregion
    // cref: ArcGIS.Desktop.Mapping.ISymbolFactory.ConstructStroke(ArcGIS.Core.CIM.CIMColor,System.Double,ArcGIS.Desktop.Mapping.SimpleLineStyle)
    // cref: ArcGIS.Core.CIM.CIMSymbolLayer
    // cref: ArcGIS.Desktop.Mapping.SymbolFactory.ConstructHatchFill(ArcGIS.Core.CIM.CIMStroke,System.Double,System.Double,System.Double)
    // cref: ArcGIS.Core.CIM.CIMPolygonSymbol
    #region Snippet Cross hatch
    /// <summary>
    /// Create a polygon symbol using the ConstructHatchFill method . <br/>
    /// ![PolygonSymbolDiagonalCrossHatch](http://Esri.github.io/arcgis-pro-sdk/images/Symbology/ConstructHatchFill.png)
    /// </summary>
    /// <returns></returns>
    private static Task<CIMPolygonSymbol> CreateHatchFillPolygonAsync()
    {
      return QueuedTask.Run<CIMPolygonSymbol>(() =>
      {
        CIMStroke lineStroke = SymbolFactory.Instance.ConstructStroke(CIMColor.CreateRGBColor(51, 51, 51, 60), 4, SimpleLineStyle.Solid);
        //gradient
        var hatchFill = SymbolFactory.Instance.ConstructHatchFill(lineStroke, 45, 6, 0);

        List<CIMSymbolLayer> symbolLayers = new List<CIMSymbolLayer>
        {
          hatchFill
        };
        return new CIMPolygonSymbol() { SymbolLayers = symbolLayers.ToArray() };
      });
    }
    #endregion

    #region Snippet Dash dot fill
    /// <summary>
    /// Create a polygon symbol with a dash dot fill. <br/>
    /// ![PolygonSymbolDashDot](http://Esri.github.io/arcgis-pro-sdk/images/Symbology/polygon-dash-dot.png)
    /// </summary>
    /// <returns></returns>
    public static Task<CIMPolygonSymbol> CreateDashDotFillAsync()
    {
      return QueuedTask.Run<CIMPolygonSymbol>(() =>
      {
        var trans = 50.0;//semi transparent
        CIMStroke outline = SymbolFactory.Instance.ConstructStroke(CIMColor.CreateRGBColor(0, 0, 0, trans), 2.0, SimpleLineStyle.Solid);

        //Stroke for the fill            
        var dashDot = SymbolFactory.Instance.ConstructStroke(ColorFactory.Instance.RedRGB, 1.0, SimpleLineStyle.DashDotDot);
        //Mimic cross hatch
        CIMFill[] solidColorHatch =
              {

                 new CIMHatchFill()
                {
                    Enable = true,
                    Rotation = 0.0,
                    Separation = 2.5,
                    LineSymbol = new CIMLineSymbol(){SymbolLayers = new CIMSymbolLayer[1] {dashDot } }
                },
                 new CIMSolidFill()
                {
                    Enable = true,
                    Color = ColorFactory.Instance.CreateRGBColor(255, 255, 0)
                },
      };
        List<CIMSymbolLayer> symbolLayers = new List<CIMSymbolLayer>();
        symbolLayers.Add(outline);
        foreach (var fill in solidColorHatch)
          symbolLayers.Add(fill);
        return new CIMPolygonSymbol() { SymbolLayers = symbolLayers.ToArray() };
      });
    }
    #endregion
    // cref: ArcGIS.Desktop.Mapping.ISymbolFactory.ConstructStroke(ArcGIS.Core.CIM.CIMColor,System.Double,ArcGIS.Desktop.Mapping.SimpleLineStyle)
    // cref: ArcGIS.Desktop.Mapping.ColorFactory.ConstructColorRamp(ArcGIS.Desktop.Mapping.ColorRampAlgorithm,ArcGIS.Core.CIM.CIMColor,ArcGIS.Core.CIM.CIMColor)
    // cref: ArcGIS.Core.CIM.CIMGradientFill
    // cref: ArcGIS.Core.CIM.CIMSymbolLayer
    // cref: ArcGIS.Core.CIM.CIMFill
    // cref: ArcGIS.Core.CIM.CIMStroke
    // cref: ArcGIS.Core.CIM.CIMSymbolLayer
    #region Snippet Gradient color fill using CIMGradientFill
    /// <summary>
    /// Create a polygon symbol with a gradient color fill. <br/>
    /// ![PolygonSymbolGradientColor](http://Esri.github.io/arcgis-pro-sdk/images/Symbology/polygon-gradient-color.png)
    /// </summary>
    /// <remarks>
    /// 1. Create a solid colored stroke with 50% transparency
    /// 1. Create a fill using gradient colors red through green
    /// 1. Apply both the stroke and fill as a symbol layer array to the new PolygonSymbol
    /// </remarks>
    /// <returns></returns>
    public static Task<CIMPolygonSymbol> CreateGradientFillAsync()
    {
      return QueuedTask.Run<CIMPolygonSymbol>(() =>
      {
        var trans = 50.0;//semi transparent
        CIMStroke outline = SymbolFactory.Instance.ConstructStroke(CIMColor.CreateRGBColor(0, 0, 0, trans), 2.0, SimpleLineStyle.Solid);
        //Mimic cross hatch
        CIMFill solidColorHatch =
               new CIMGradientFill()
           {
             ColorRamp = ColorFactory.Instance.ConstructColorRamp(ColorRampAlgorithm.LinearContinuous,
                                      ColorFactory.Instance.RedRGB, ColorFactory.Instance.GreenRGB)
           };
        List<CIMSymbolLayer> symbolLayers = new List<CIMSymbolLayer>
          {
                    outline,
                    solidColorHatch
          };

        return new CIMPolygonSymbol() { SymbolLayers = symbolLayers.ToArray() };
      });
    }
    #endregion
    // cref: ArcGIS.Desktop.Mapping.SymbolFactory.ConstructGradientFill(ArcGIS.Core.CIM.CIMColorRamp,ArcGIS.Core.CIM.GradientFillMethod,System.Int32)
    #region Snippet Gradient fill between two colors
    /// <summary>
    /// Create a polygon symbol using the ConstructGradientFill method. Constructs a gradient fill between two colors passed to the method. <br/>
    /// ![PolygonSymbolTwoColors](http://Esri.github.io/arcgis-pro-sdk/images/Symbology/PolygonSymbolTwoColors.png)
    /// </summary>
    /// <returns></returns>
    public static Task<CIMPolygonSymbol> CreateGradientTwoColorsPolygonAsync()
    {
      return QueuedTask.Run<CIMPolygonSymbol>(() =>
      {
        //gradient fill between 2 colors
        var gradientFill = SymbolFactory.Instance.ConstructGradientFill(CIMColor.CreateRGBColor(235, 64, 52), CIMColor.NoColor(), GradientFillMethod.Linear);
        List<CIMSymbolLayer> symbolLayers = new List<CIMSymbolLayer>
          {
                    gradientFill
          };
        return new CIMPolygonSymbol() { SymbolLayers = symbolLayers.ToArray() };
      });

    }
    #endregion
    // cref: ArcGIS.Desktop.Mapping.ISymbolFactory.ConstructStroke(ArcGIS.Core.CIM.CIMColor,System.Double,ArcGIS.Desktop.Mapping.SimpleLineStyle)
    // cref: ArcGIS.Desktop.Mapping.SymbolFactory.ConstructGradientFill(ArcGIS.Core.CIM.CIMColorRamp,ArcGIS.Core.CIM.GradientFillMethod,System.Int32)
    #region Snippet Gradient fill using Color ramp
    /// <summary>
    /// Create a polygon symbol using the ConstructGradientFill method. Constructs a gradient fill using the specified color ramp. <br/>
    /// ![PolygonSymbolColorRamp](http://Esri.github.io/arcgis-pro-sdk/images/Symbology/PolygonSymbolColorRamp.png)
    /// </summary>
    /// <returns></returns>
    public static Task<CIMPolygonSymbol> CreateGradientColorRampPolygonAsync()
    {
      return QueuedTask.Run<CIMPolygonSymbol>(() =>
      {
        //outine
        CIMStroke outline = SymbolFactory.Instance.ConstructStroke(CIMColor.CreateRGBColor(49, 49, 49), 2.0, SimpleLineStyle.Solid);

        //gradient fill using a color ramp
        var gradientFill = SymbolFactory.Instance.ConstructGradientFill(GetColorRamp(), GradientFillMethod.Linear);

        List<CIMSymbolLayer> symbolLayers = new List<CIMSymbolLayer>
          {
                    outline,
                    gradientFill
          };
        return new CIMPolygonSymbol() { SymbolLayers = symbolLayers.ToArray() };
      });

    }
    #endregion
    // cref: ArcGIS.Desktop.Mapping.SymbolFactory.ConstructPictureFill(System.String,System.Double)
    // cref: ArcGIS.Desktop.Mapping.SymbolFactory.ConstructStroke(ArcGIS.Core.CIM.CIMColor,System.Double,ArcGIS.Desktop.Mapping.SimpleLineStyle)
    #region Snippet Picture fill
    /// <summary>
    /// Constructs a picture fill with the specified parameters.
    /// ![ConstructPictureFill](http://Esri.github.io/arcgis-pro-sdk/images/Symbology/ConstructPictureFill.png)
    /// </summary>
    /// <returns></returns>
    private static Task<CIMPolygonSymbol> CreatePictureFillPolygonAsync()
    {
      return QueuedTask.Run<CIMPolygonSymbol>(() =>
      {
        CIMStroke outline = SymbolFactory.Instance.ConstructStroke(CIMColor.CreateRGBColor(110, 110, 110), 2.0, SimpleLineStyle.Solid);
        //picture
        var imgPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Images\CaliforniaEmblem.png");
        var pictureFill = SymbolFactory.Instance.ConstructPictureFill(imgPath, 64);

        List<CIMSymbolLayer> symbolLayers = new()
        {
          outline,
          pictureFill
        };
        return new CIMPolygonSymbol() { SymbolLayers = symbolLayers.ToArray() };
      });
    }
    #endregion
    // cref: ArcGIS.Desktop.Mapping.ISymbolFactory.ConstructWaterFill(ArcGIS.Core.CIM.CIMColor,ArcGIS.Core.CIM.WaterbodySize,ArcGIS.Core.CIM.WaveStrength) 
    // cref: ArcGIS.Desktop.Mapping.SymbolFactory.ConstructStroke(ArcGIS.Core.CIM.CIMColor,System.Double,ArcGIS.Desktop.Mapping.SimpleLineStyle)
    #region Snippet Animation water
    /// <summary>
    /// Constructs a water fill of specific color, waterbody size and wave strength. This fill can be used on polygon feature classes in a Scene view only.
    /// ![ConstructWaterFill](https://github.com/ArcGIS/arcgis-pro-sdk/blob/master/Images/waterAnimation.gif)
    /// </summary>
    /// <returns></returns>
    private static Task<CIMPolygonSymbol> CreateWaterFillPolygonAsync()
    {
      return QueuedTask.Run<CIMPolygonSymbol>(() =>
      {
        CIMStroke outline = SymbolFactory.Instance.ConstructStroke(CIMColor.CreateRGBColor(49, 49, 49, 50.0), 2.0, SimpleLineStyle.Solid);
        var waterFill = SymbolFactory.Instance.ConstructWaterFill(CIMColor.CreateRGBColor(3, 223, 252), WaterbodySize.Large, WaveStrength.Rippled);
        List<CIMSymbolLayer> symbolLayers = new List<CIMSymbolLayer>
          {
                    outline,
                    waterFill
          };
        return new CIMPolygonSymbol() { SymbolLayers = symbolLayers.ToArray() };
      });
    }
    #endregion
    // cref: ArcGIS.Desktop.Mapping.SymbolFactory.ConstructPolygonSymbolWithPenInkRipple(ArcGIS.Core.CIM.CIMColor) 
    #region Snippet Pen and Ink: Ripple
    /// <summary>
    /// Constructs a polygon symbol in the specified color representing a pen and ink ripple water fill. See https://www.esri.com/arcgis-blog/products/arcgis-pro/mapping/please-steal-this-pen-and-ink-style/
    /// ![polygonRipple.png](http://Esri.github.io/arcgis-pro-sdk/images/Symbology/polygonRipple.png)
    /// </summary>
    /// <returns></returns>
    private static Task<CIMPolygonSymbol> CreateRippleFillPolygonAsync()
    {
      return QueuedTask.Run<CIMPolygonSymbol>(() =>
      {
        //Ripple pen and ink
        var penInkRipple = SymbolFactory.Instance.ConstructPolygonSymbolWithPenInkRipple(CIMColor.CreateRGBColor(13, 24, 54));
        return penInkRipple;
      });
    }
    #endregion
    // cref: ArcGIS.Desktop.Mapping.SymbolFactory.ConstructPolygonSymbolWithPenInkStipple(ArcGIS.Core.CIM.CIMColor,System.Boolean) 

    #region Snippet Pen and Ink: Stipple
    /// <summary>
    /// Constructs a polygon symbol in the specified color representing a pen and ink stipple effect. See https://www.esri.com/arcgis-blog/products/arcgis-pro/mapping/please-steal-this-pen-and-ink-style/
    /// ![polygonStipple.png](http://Esri.github.io/arcgis-pro-sdk/images/Symbology/polygonStipple.png)
    /// </summary>
    /// <returns></returns>
    private static Task<CIMPolygonSymbol> CreateStippleFillPolygonAsync()
    {
      return QueuedTask.Run<CIMPolygonSymbol>(() =>
      {
        //Stipple pen and ink
        var penInkRipple = SymbolFactory.Instance.ConstructPolygonSymbolWithPenInkStipple(CIMColor.CreateRGBColor(78, 133, 105), true);
        return penInkRipple;
      });
    }
    #endregion

    // cref: ArcGIS.Desktop.Mapping.SymbolFactory.ConstructPolygonSymbolWithPenInkCrossHatch(ArcGIS.Core.CIM.CIMColor,System.Boolean) 
    #region Snippet Pen and Ink: Cross Hatch
    /// <summary>
    /// Constructs a polygon symbol in the specified color representing a pen and ink cross hatch effect. See https://www.esri.com/arcgis-blog/products/arcgis-pro/mapping/please-steal-this-pen-and-ink-style/
    /// ![polygonPNHatch.png](http://Esri.github.io/arcgis-pro-sdk/images/Symbology/polygonPNHatch.png)
    /// </summary>
    /// <returns></returns>
    private static Task<CIMPolygonSymbol> CreatePenInkCrossHatchFillPolygonAsync()
    {
      return QueuedTask.Run<CIMPolygonSymbol>(() =>
      {
        //Cross Hatch pen and ink
        var penkInkCrossHatch = SymbolFactory.Instance.ConstructPolygonSymbolWithPenInkCrossHatch(CIMColor.CreateRGBColor(168, 49, 22), true);
        return penkInkCrossHatch;
      });
    }
    #endregion

    // cref: ArcGIS.Core.CIM.CIMProceduralSymbolLayer
    // cref: ArcGIS.Desktop.Mapping.SymbolFactory.ConstructPolygonSymbol
    #region Snippet Procedural Symbol
    /// <summary>
    /// Create a procedural symbol that can be applied to a polygon building footprint layer
    /// ![ProceduralSymbol](http://Esri.github.io/arcgis-pro-sdk/images/Symbology/polygon-procedural.png)        
    /// </summary>    
    /// <remarks>Note: The rule package used in this method can be obtained from the Sample Data included in the arcgis-pro-sdk-community-samples repository.</remarks>
    private static readonly string _rulePkgPath = @"C:\Data\RulePackages\Venice_2014.rpk";
    public static Task<CIMPolygonSymbol> CreateProceduralPolygonSymbolAsync()
    {
      return QueuedTask.Run<CIMPolygonSymbol>(() =>
      {
        //Polygon symbol to hold the procedural layer
        var myPolygonSymbol = SymbolFactory.Instance.ConstructPolygonSymbol();

        //Array of layers to hold a procedural symbol layer
        CIMSymbolLayer[] proceduralSymbolLyr =
              {
                    new CIMProceduralSymbolLayer()
                    {
                        PrimitiveName = "Venice Rule package 2014",
                        RulePackage = _rulePkgPath,
                        RulePackageName = "Venice_2014",
                    }
          };
        myPolygonSymbol.SymbolLayers = proceduralSymbolLyr;
        return myPolygonSymbol;
      });
    }
    #endregion
    internal static CIMColorRamp GetColorRamp()
    {
      StyleProjectItem style =
        Project.Current.GetItems<StyleProjectItem>().FirstOrDefault(s => s.Name == "ArcGIS Colors");
      if (style == null) return null;
      var colorRampList = style.SearchColorRamps("Spectrum-Full Bright");
      if (colorRampList == null || colorRampList.Count == 0) return null;
      return colorRampList[0].ColorRamp;
    }
  }
}
