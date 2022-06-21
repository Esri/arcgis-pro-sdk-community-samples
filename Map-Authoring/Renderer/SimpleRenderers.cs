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
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Dialogs;
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
        /// <returns>
        /// </returns>
        internal static Task SimpleRendererPolygon()
        {
            //Check feature layer name
            //Code works with the USDemographics feature layer available with the ArcGIS Pro SDK Sample data
            var featureLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(f => f.Name == "USDemographics");
            if (featureLayer == null)
            {
              MessageBox.Show("This renderer works with the USDemographics feature layer available with the ArcGIS Pro SDK Sample data", "Data missing");
              return Task.FromResult(0);
            }
            return QueuedTask.Run(() =>
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
        /// <returns>
        /// </returns>
        internal static Task SimpleRendererPoint()
        {
            //Check feature layer name
            //Code works with the USDemographics feature layer available with the ArcGIS Pro SDK Sample data
            var featureLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(f => f.ShapeType == esriGeometryType.esriGeometryPoint);
            if (featureLayer == null)
            {
              MessageBox.Show("This renderer works with a point feature layer", "Data missing");
              return Task.FromResult(0);
            }
            return QueuedTask.Run(() =>
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
        /// <returns>
        /// </returns>
        internal static Task SimpleRendererLine()
        {
            //Check feature layer name
            //Code works with the USDemographics feature layer available with the ArcGIS Pro SDK Sample data
            var featureLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(f => f.Name == "U.S. National Transportation Atlas Interstate Highways");
            if (featureLayer == null)
            {
              MessageBox.Show("This renderer works with the U.S. National Transportation Atlas Interstate Highways feature layer available with the ArcGIS Pro SDK Sample data", "Data missing");
              return Task.FromResult(0);
            }
            return QueuedTask.Run(() =>
            {
                //Create a circle marker
                var lineSymbol = SymbolFactory.Instance.ConstructLineSymbol(ColorFactory.Instance.RedRGB, 2, SimpleLineStyle.DashDotDot);

                //Get the layer's current renderer
                CIMSimpleRenderer renderer = featureLayer.GetRenderer() as CIMSimpleRenderer;

                //Update the symbol of the current simple renderer
                renderer.Symbol = lineSymbol.MakeSymbolReference();

                //Update the feature layer renderer
                featureLayer.SetRenderer(renderer);
            });
        }
    #endregion
    #region Snippet Simple Renderer for a Line feature layer using a style from a StyleProjectItem.
    /// <summary>
    /// Renders a Line feature layer using a style from a StyleProjectItem.
    /// </summary>
    /// <remarks>
    /// ![Simple Renderer Style item](http://Esri.github.io/arcgis-pro-sdk/images/Renderers/simple-line-style-item.png)
    /// </remarks>
    /// <returns></returns>
    internal static Task SimpleRendererLineFromStyeItem()
        {
          //Check feature layer name
          //Code works with the USDemographics feature layer available with the ArcGIS Pro SDK Sample data
          var featureLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(f => f.Name == "U.S. National Transportation Atlas Interstate Highways");
          if (featureLayer == null)
          {
            MessageBox.Show("This renderer works with the U.S. National Transportation Atlas Interstate Highways feature layer available with the ArcGIS Pro SDK Sample data", "Data missing");
            return Task.FromResult(0);
          }
          //Get all styles in the project
          var styleProjectItem2D = Project.Current.GetItems<StyleProjectItem>().FirstOrDefault(s => s.Name == "ArcGIS 2D");

          return QueuedTask.Run(() => {
            //Get a specific style in the project by name
            var arrowLineSymbol = styleProjectItem2D.SearchSymbols(StyleItemType.LineSymbol, "Arrow Line 2 (Mid)")[0];
            if (arrowLineSymbol == null) return;

            //Get the layer's current renderer
            var renderer = featureLayer?.GetRenderer() as CIMSimpleRenderer;

            //Update the symbol of the current simple renderer
            renderer.Symbol = arrowLineSymbol.Symbol.MakeSymbolReference();

            //Update the feature layer renderer
            featureLayer.SetRenderer(renderer);
          });
    }
    #endregion

  }
}
