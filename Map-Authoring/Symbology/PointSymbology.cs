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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace Symbology
{
    internal static class MyPointSymbology
    {
        public static async Task<Dictionary<CIMSymbol, string>> GetAllPointSymbolsAsync()
        {
            var PtSymbols = new Dictionary<CIMSymbol, string>
            {
                {  await CreatePointSymbolAsync(), "Blue_Circle"},
                {  await CreateMarkerSymbolAsync(), "Marker"}
            };

            return PtSymbols;
        }
        // cref: ArcGIS.Core.CIM.CIMPointSymbol
        // cref: ArcGIS.Core.CIM.CIMVectorMarker
        // cref: ArcGIS.Desktop.Mapping.SymbolFactory.ConstructSolidFill(ArcGIS.Core.CIM.CIMColor)
        // cref: ArcGIS.Desktop.Mapping.SymbolFactory.ConstructPointSymbol(ArcGIS.Core.CIM.CIMColor,System.Double,ArcGIS.Desktop.Mapping.SimpleMarkerStyle)
        #region Snippet Custom fill and outline
        /// <summary>
        /// Creates a point symbol with custom fill and outline          
        /// ![PointSymbolMarker](http://Esri.github.io/arcgis-pro-sdk/images/Symbology/point-fill-outline.png)
        /// </summary>
        /// <returns></returns>
        internal static Task<CIMPointSymbol> CreatePointSymbolAsync()
        {
            return QueuedTask.Run<CIMPointSymbol>(() =>
            {
                 var circlePtSymbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.BlueRGB, 6, SimpleMarkerStyle.Circle);
                //Modifying this point symbol with the attributes we want.
                //getting the marker that is used to render the symbol
                var marker = circlePtSymbol.SymbolLayers[0] as CIMVectorMarker;
                //Getting the polygon symbol layers components in the marker
                var polySymbol = marker.MarkerGraphics[0].Symbol as CIMPolygonSymbol;
                //modifying the polygon's outline and width per requirements
                polySymbol.SymbolLayers[0] = SymbolFactory.Instance.ConstructStroke(ColorFactory.Instance.BlackRGB, 2, SimpleLineStyle.Solid); //This is the outline
                polySymbol.SymbolLayers[1] = SymbolFactory.Instance.ConstructSolidFill(ColorFactory.Instance.BlueRGB); //This is the fill
                return circlePtSymbol;              
            });           
            
        }
        #endregion
        // cref: ArcGIS.Core.CIM.CIMPointSymbol
        // cref: ArcGIS.Desktop.Mapping.SymbolFactory.ConstructMarker(System.Int32, System.String, System.String, System.Int32)
        // cref: ArcGIS.Desktop.Mapping.SymbolFactory.ConstructPointSymbol(ArcGIS.Core.CIM.CIMMarker)
        #region Snippet Point Symbol from a font
        /// <summary>
        /// Create a point symbol from a character in a font file
        /// ![PointSymbolFont](http://Esri.github.io/arcgis-pro-sdk/images/Symbology/point-marker.png)
        /// </summary>
        /// <returns></returns>
        internal static Task<CIMPointSymbol> CreateMarkerSymbolAsync()
        {           
            //Construct point symbol from marker
            return QueuedTask.Run<CIMPointSymbol>(() => {
                //creating the marker from the Font selected
                var cimMarker = SymbolFactory.Instance.ConstructMarker(47, "Wingdings 3", "Regular", 12);
                return SymbolFactory.Instance.ConstructPointSymbol(cimMarker);
            });

        }
        #endregion
    }
}
