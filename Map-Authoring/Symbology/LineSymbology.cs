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
using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace Symbology
{
    internal static class MyLineSymbology
    {

        public static async Task<Dictionary<CIMSymbol, string>> GetAllLineSymbolsAsync()
        {
            var lineSymbols = new Dictionary<CIMSymbol, string>
            {
                {await CreateMyMarkerLineSymbolAsync(), "Angled Hatch line" },
                {await CreateLineDashTwoMarkersAync(), "Dash Two Markers" },
                {await CreateLineDashTwoMarkers2Async(), "Dash Two Markers 2" }

            };
            return lineSymbols;
        }
        // cref: ArcGIS.Core.CIM.CIMLineSymbol
        // cref: ArcGIS.Desktop.Mapping.SymbolFactory.ConstructMarker(System.Int32,System.String,System.String,System.Int32)
        // cref: ArcGIS.Desktop.Mapping.SymbolFactory.ConstructLineSymbol(ArcGIS.Core.CIM.CIMColor,System.Double,ArcGIS.Desktop.Mapping.SimpleLineStyle)
        // cref: ArcGIS.Core.CIM.CIMMarkerPlacementAlongLineSameSize
        #region Snippet Markers placed at a 45 degree angle
        /// <summary>
        /// Create a line symbol with the markers placed at a 45 degree angle. <br/>  
        /// ![LineSymbolAngleMarker](http://Esri.github.io/arcgis-pro-sdk/images/Symbology/line-marker-angle.png)
        /// </summary>
        /// <returns></returns>
        internal static Task<CIMLineSymbol> CreateMyMarkerLineSymbolAsync()
        {
            return QueuedTask.Run<CIMLineSymbol>(() =>
            {
                //Create a marker from the "|" character.  This is the marker that will be used to render the line layer.
                var lineMarker = SymbolFactory.Instance.ConstructMarker(124, "Agency FB", "Regular", 12);

                //Default line symbol which will be modified 
                var blackSolidLineSymbol = SymbolFactory.Instance.ConstructLineSymbol(ColorFactory.Instance.BlackRGB, 2, SimpleLineStyle.Solid);

                //Modifying the marker to align with line
                //First define "markerplacement"
                CIMMarkerPlacementAlongLineSameSize markerPlacement = new CIMMarkerPlacementAlongLineSameSize()
                {
                    AngleToLine = true,
                    PlacementTemplate = new double[] { 5 }
                };
                //assign the markerplacement to the marker
                lineMarker.MarkerPlacement = markerPlacement;
                //angle the marker if needed
                lineMarker.Rotation = 45;

                //assign the marker as a layer to the line symbol
                blackSolidLineSymbol.SymbolLayers[0] = lineMarker;

                return blackSolidLineSymbol;
            });
        }
        #endregion
        // cref: ArcGIS.Core.CIM.CIMLineSymbol
        // cref: ArcGIS.Core.CIM.CIMSolidStroke
        // cref: ArcGIS.Core.CIM.CIMGeometricEffect
        // cref: ArcGIS.Core.CIM.CIMGeometricEffectDashes
        // cref: ArcGIS.Core.CIM.CIMGeometricEffectOffset
        // cref: ArcGIS.Core.CIM.CIMMarkerPlacementAlongLineSameSize

        #region Snippet Dash line with two markers - Method I
        /// <summary>
        /// Create a line symbol with a dash and two markers.<br/>          
        /// </summary>
        /// <remarks>
        /// This line symbol comprises three symbol layers listed below: 
        /// 1. A solid stroke that has dashes.
        /// 1. A circle marker.
        /// 1. A square marker.
        /// ![LineSymbolTwoMarkers](http://Esri.github.io/arcgis-pro-sdk/images/Symbology/line-dash-two-markers.png)
        /// </remarks>
        /// <returns></returns>
        internal static Task<CIMLineSymbol> CreateLineDashTwoMarkersAync()
        {
            return QueuedTask.Run<CIMLineSymbol>(() =>
            {

                var dash2MarkersLine = new CIMLineSymbol();

                var mySymbolLyrs = new CIMSymbolLayer[]
                {
                    new CIMSolidStroke()
                    {
                        Color = ColorFactory.Instance.BlackRGB,
                        Enable = true,
                        ColorLocked = true,
                        CapStyle = LineCapStyle.Round,
                        JoinStyle = LineJoinStyle.Round,
                        LineStyle3D = Simple3DLineStyle.Strip,
                        MiterLimit = 10,
                        Width = 1,
                        CloseCaps3D = false,
                        Effects = new CIMGeometricEffect[]
                        {
                            new CIMGeometricEffectDashes()
                            {
                                CustomEndingOffset = 0,
                                DashTemplate = new double[] {20, 10, 20, 10},
                                LineDashEnding = LineDashEnding.HalfPattern,
                                OffsetAlongLine = 0,
                                ControlPointEnding = LineDashEnding.NoConstraint
                            },
                            new CIMGeometricEffectOffset()
                            {
                                Method = GeometricEffectOffsetMethod.Bevelled,
                                Offset = 0,
                                Option = GeometricEffectOffsetOption.Fast
                            }
                        },
                    },
                    CreateCircleMarkerPerSpecs(),
                    CreateSquareMarkerPerSpecs()
                };
                dash2MarkersLine.SymbolLayers = mySymbolLyrs;
                return dash2MarkersLine;
            });
        }
        private static CIMMarker CreateCircleMarkerPerSpecs()
        {
            var circleMarker = SymbolFactory.Instance.ConstructMarker(ColorFactory.Instance.BlackRGB, 5, SimpleMarkerStyle.Circle) as CIMVectorMarker;
            //Modifying the marker to align with line
            //First define "markerplacement"
            CIMMarkerPlacementAlongLineSameSize markerPlacement = new CIMMarkerPlacementAlongLineSameSize()
            {
                AngleToLine = true,
                Offset = 0,
                Endings = PlacementEndings.Custom,
                OffsetAlongLine = 15,
                PlacementTemplate = new double[] { 60 }
            };
            //assign the markerplacement to the marker
            circleMarker.MarkerPlacement = markerPlacement;
            return circleMarker;
        }
        private static CIMMarker CreateSquareMarkerPerSpecs()
        {
            var squareMarker = SymbolFactory.Instance.ConstructMarker(ColorFactory.Instance.BlueRGB, 5, SimpleMarkerStyle.Square) as CIMVectorMarker;
            CIMMarkerPlacementAlongLineSameSize markerPlacement2 = new CIMMarkerPlacementAlongLineSameSize()
            {
                AngleToLine = true,
                Endings = PlacementEndings.Custom,
                OffsetAlongLine = 45,
                PlacementTemplate = new double[] { 60 },
            };
            squareMarker.MarkerPlacement = markerPlacement2;
            return squareMarker;
        }

        #endregion
        // cref: ArcGIS.Core.CIM.CIMLineSymbol
        // cref: ArcGIS.Desktop.Mapping.SymbolFactory.ConstructMarker(System.Int32,System.String,System.String,System.Int32)
        // cref: ArcGIS.Core.CIM.CIMGeometricEffect
        // cref: ArcGIS.Core.CIM.CIMGeometricEffectDashes
        // cref: ArcGIS.Core.CIM.CIMSolidStroke
        // cref: ArcGIS.Core.CIM.CIMVectorMarker

        #region Snippet Dash line with two markers - Method II
        /// <summary>
        /// Create a line symbol with a dash and two markers. <br/>
        /// In this pattern of creating this symbol, a [CIMVectorMarker](https://pro.arcgis.com/en/pro-app/sdk/api-reference/#topic6176.html) object is created as a new [CIMSymbolLayer](https://pro.arcgis.com/en/pro-app/sdk/api-reference/#topic5503.html).
        /// The circle and square markers created by [ContructMarker](https://pro.arcgis.com/en/pro-app/sdk/api-reference/#topic12350.html) method is then assigned to the [MarkerGraphics](https://pro.arcgis.com/en/pro-app/sdk/api-reference/#topic6188.html) property of the CIMVectorMarker. 
        /// When using this method, the CIMVectorMarker's [Frame](https://pro.arcgis.com/en/pro-app/sdk/api-reference/#topic6187.html) property needs to be set to the [CIMMarker](https://pro.arcgis.com/en/pro-app/sdk/api-reference/#topic3264.html) object's Frame. 
        /// Similarly, the CIMVectorMarker's [Size](https://pro.arcgis.com/en/pro-app/sdk/api-reference/#topic3284.html) property needs to be set to the CIMMarker object's size.
        /// </summary>
        /// <remarks>
        /// This line symbol comprises three symbol layers listed below: 
        /// 1. A solid stroke that has dashes.
        /// 1. A circle marker.
        /// 1. A square marker.
        /// ![LineSymbolTwoMarkers](http://Esri.github.io/arcgis-pro-sdk/images/Symbology/line-dash-two-markers.png)
        /// </remarks>
        /// <returns></returns>

        internal static Task<CIMLineSymbol> CreateLineDashTwoMarkers2Async()
        {
            return QueuedTask.Run<CIMLineSymbol>(() =>
            {
                //default line symbol that will get modified.
                var dash2MarkersLine = new CIMLineSymbol();
                //circle marker to be used in our line symbol as a layer
                var circleMarker = SymbolFactory.Instance.ConstructMarker(ColorFactory.Instance.BlackRGB, 5, SimpleMarkerStyle.Circle) as CIMVectorMarker;
                //circle marker to be used in our line symbol as a layer
                var squareMarker = SymbolFactory.Instance.ConstructMarker(ColorFactory.Instance.BlueRGB, 5, SimpleMarkerStyle.Square) as CIMVectorMarker;
                //Create the array of layers that make the new line symbol
                CIMSymbolLayer[] mySymbolLyrs =
                {
                    new CIMSolidStroke() //dash line
                    {
                        Color = ColorFactory.Instance.BlackRGB,
                        Enable = true,
                        ColorLocked = true,
                        CapStyle = LineCapStyle.Round,
                        JoinStyle = LineJoinStyle.Round,
                        LineStyle3D = Simple3DLineStyle.Strip,
                        MiterLimit = 10,
                        Width = 1,
                        CloseCaps3D = false,
                        Effects = new CIMGeometricEffect[]
                        {
                            new CIMGeometricEffectDashes()
                            {
                                CustomEndingOffset = 0,
                                DashTemplate = new double[] {20, 10, 20, 10},
                                LineDashEnding = LineDashEnding.HalfPattern,
                                OffsetAlongLine = 0,
                                ControlPointEnding = LineDashEnding.NoConstraint
                            },
                            new CIMGeometricEffectOffset()
                            {
                                Method = GeometricEffectOffsetMethod.Bevelled,
                                Offset = 0,
                                Option = GeometricEffectOffsetOption.Fast
                            }
                        }
                    },
                    new CIMVectorMarker() //circle marker
                    {
                        MarkerGraphics = circleMarker.MarkerGraphics,
                        Frame = circleMarker.Frame, //need to match the CIMVector marker's frame to the circleMarker's frame.
                        Size = circleMarker.Size,    //need to match the CIMVector marker's size to the circleMarker's size.                    
                       MarkerPlacement = new CIMMarkerPlacementAlongLineSameSize()
                       {
                           AngleToLine = true,
                           Offset = 0,
                           Endings = PlacementEndings.Custom,
                           OffsetAlongLine = 15,
                           PlacementTemplate = new double[] {60},
                       }
                       
                    },
                    new CIMVectorMarker() //square marker
                    {
                       MarkerGraphics = squareMarker.MarkerGraphics,
                       Frame = squareMarker.Frame, //need to match the CIMVector marker's frame to the squareMarker frame.
                       Size = squareMarker.Size, //need to match the CIMVector marker's size to the squareMarker size.
                       MarkerPlacement = new CIMMarkerPlacementAlongLineSameSize()
                       {
                           AngleToLine = true,
                           Endings = PlacementEndings.Custom,
                           OffsetAlongLine = 45,
                           PlacementTemplate = new double[] {60},
                       }                       
                    }
                };
                dash2MarkersLine.SymbolLayers = mySymbolLyrs;
                return dash2MarkersLine;
            });

        }
        #endregion

    }
}
