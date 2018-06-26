/*

   Copyright 2018 Esri

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
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace Symbology
{
    internal static class MyPolygonSymbology
    {  
        public static  async Task<Dictionary<CIMSymbol, string>> GetAllPolygonSymbolsAsync()
        {
            var PolySymbols = new Dictionary<CIMSymbol, string>
            {
                {  await CreateCrossHatchPolygonAsync(), "Cross Hatch"},
                {  await CreateDiagonalCrossPolygonAsync(), "Diagonal Hatch"},
                {  await CreateDashDotFillAsync(), "Dash Dot Fill"},
                {  await CreateGradientFillAsync(), "Gradient Fill"},
                {  await CreateProceduralPolygonSymbolAsync(), "Venice Procedural" }
            };
            return PolySymbols;
        }
        #region Snippet Cross hatch fill
        /// <summary>
        /// Create a polygon symbol with a cross hatch fill. <br/>
        /// ![PolygonSymbolCrossHatch](http://Esri.github.io/arcgis-pro-sdk/images/Symbology/polygon-crosshatch.png)
        /// </summary>
        /// <returns></returns>
        public static Task<CIMPolygonSymbol> CreateCrossHatchPolygonAsync()
        {
            return QueuedTask.Run<CIMPolygonSymbol>(() =>
            {
                var trans = 50.0;//semi transparent
                CIMStroke outline = SymbolFactory.Instance.ConstructStroke(CIMColor.CreateRGBColor(0, 0, 0, trans), 2.0, SimpleLineStyle.Solid);

                //Stroke for the hatch fill
                var dash = SymbolFactory.Instance.ConstructStroke(CIMColor.CreateRGBColor(255, 170, 0, trans), 1.0, SimpleLineStyle.Dash);

                //Mimic cross hatch
                CIMFill[] crossHatch =
                    {
                    new CIMHatchFill() {
                        Enable = true,
                        Rotation = 0.0,
                        Separation = 5.0,
                        LineSymbol = new CIMLineSymbol() { SymbolLayers = new CIMSymbolLayer[1] { dash } }
                    },
                    new CIMHatchFill() {
                        Enable = true,
                        Rotation = 90.0,
                        Separation = 5.0,
                        LineSymbol = new CIMLineSymbol() { SymbolLayers = new CIMSymbolLayer[1] { dash } }
                    }
                };
                List<CIMSymbolLayer> symbolLayers = new List<CIMSymbolLayer>();
                symbolLayers.Add(outline);
                foreach (var fill in crossHatch)
                    symbolLayers.Add(fill);
                return new CIMPolygonSymbol() { SymbolLayers = symbolLayers.ToArray() };
            });
           
        }
        #endregion

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
                List<CIMSymbolLayer> symbolLayers = new List<CIMSymbolLayer>();
                symbolLayers.Add(outline);
                foreach (var fill in diagonalCross)
                    symbolLayers.Add(fill);
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
        
        #region Snippet Gradient color fill
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
                List<CIMSymbolLayer> symbolLayers = new List<CIMSymbolLayer>();
                symbolLayers.Add(outline);
                symbolLayers.Add(solidColorHatch);

                return new CIMPolygonSymbol() { SymbolLayers = symbolLayers.ToArray() };
            });          
        }
        #endregion
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
    }
}
