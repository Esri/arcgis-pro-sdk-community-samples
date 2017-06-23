/*

   Copyright 2017 Esri

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

namespace Renderer
{
    internal static class SimpleRenderers
    {
        #region Snippet Simple Renderer for a Polygon feature layer.
        /// <summary>
        /// Renders a Polygon feature layer using a single symbol.
        /// </summary>
        /// <remarks>
        /// ![Simple Renderer for Polygon features](http://Esri.github.io/arcgis-pro-sdk/images/Renderers/simple-polygon.png)
        /// </remarks>
        /// <param name="featureLayer"></param>
        /// <returns>
        /// </returns>
        internal async static Task SimpleRendererPolygon(FeatureLayer featureLayer)
        {            
            await QueuedTask.Run(() =>
            {
                //Creating a polygon with a red fill and blue outline.
                CIMStroke outline = SymbolFactory.Instance.ConstructStroke(
                     ColorFactory.Instance.BlueRGB, 2.0, SimpleLineStyle.Solid);
                CIMPolygonSymbol fillWithOutline = SymbolFactory.Instance.ConstructPolygonSymbol(
                     ColorFactory.Instance.CreateRGBColor(255, 190, 190), SimpleFillStyle.Solid, outline);
                //Get the layer's current renderer
                CIMSimpleRenderer renderer = featureLayer.GetRenderer() as CIMSimpleRenderer;

                //Update the symbol of the current simple renderer
                renderer.Symbol = fillWithOutline.MakeSymbolReference();

                //Update the feature layer renderer
                featureLayer.SetRenderer(renderer);
            });
        }
        #endregion

        #region Snippet Simple Renderer for a Point feature layer.
        /// <summary>
        /// Renders a Point feature layer using a single symbol.
        /// </summary>
        /// <remarks>
        /// ![Simple Renderer for Point features](http://Esri.github.io/arcgis-pro-sdk/images/Renderers/simple-point.png)
        /// </remarks>
        /// <param name="featureLayer"></param>
        /// <returns>
        /// </returns>
        internal async static Task SimpleRendererPoint(FeatureLayer featureLayer)
        {            
            await QueuedTask.Run(() =>
            {
                //Create a circle marker
                var pointSymbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.RedRGB, 8, SimpleMarkerStyle.Circle);

                //Get the layer's current renderer
                CIMSimpleRenderer renderer = featureLayer.GetRenderer() as CIMSimpleRenderer;

                //Update the symbol of the current simple renderer
                renderer.Symbol = pointSymbol.MakeSymbolReference();

                //Update the feature layer renderer
                featureLayer.SetRenderer(renderer);
            });
        }
        #endregion

        #region Snippet Simple Renderer for a Line feature layer.
        /// <summary>
        /// Renders a Line feature layer using a single symbol.
        /// </summary>
        /// <remarks>
        /// ![Simple Renderer for Line features](http://Esri.github.io/arcgis-pro-sdk/images/Renderers/simple-line.png)
        /// </remarks>
        /// <param name="featureLayer"></param>
        /// <returns>
        /// </returns>
        internal async static Task SimpleRendererLine(FeatureLayer featureLayer)
        {            
            await QueuedTask.Run(() =>
            {
                //Create a circle marker
                var lineSymbol = SymbolFactory.Instance.ConstructLineSymbol(ColorFactory.Instance.RedRGB, 2, SimpleLineStyle.DashDotDot);
                //();

                //Get the layer's current renderer
                CIMSimpleRenderer renderer = featureLayer.GetRenderer() as CIMSimpleRenderer;

                //Update the symbol of the current simple renderer
                renderer.Symbol = lineSymbol.MakeSymbolReference();

                //Update the feature layer renderer
                featureLayer.SetRenderer(renderer);
            });
        }
        #endregion
    }
}
