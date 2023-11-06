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
using ArcGIS.Desktop.Internal.Mapping;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TINApiSamples
{
  internal class GetTINExtents : Button
  {
    private TinLayer _tinLayer;
    private GraphicsLayer _graphicsLayer;

    protected async override void OnClick()
    {
      _tinLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<TinLayer>().FirstOrDefault();
      _graphicsLayer = MapView.Active.Map.TargetGraphicsLayer;
      await QueuedTask.Run(() => {
        if (_tinLayer == null) return;
        if (_graphicsLayer == null) return;
        if (!Module1.IsView2D()) return;
        var tinDataset = _tinLayer.GetTinDataset();
        var defn = tinDataset.GetDefinition();
        
        ////////Data Area////////////////////////
        var dataAreaStroke = SymbolFactory.Instance.ConstructStroke(ColorFactory.Instance.BlackRGB, 1, SimpleLineStyle.Solid);
        var polygonSymbolDataArea = SymbolFactory.Instance.ConstructPolygonSymbol(ColorFactory.Instance.BlackRGB, SimpleFillStyle.Null, dataAreaStroke);
        var dataAreaPoly = tinDataset.GetDataArea();
        _graphicsLayer.AddElement(dataAreaPoly, polygonSymbolDataArea, "TIN Data Area");

        /////GetExtent///////////////////////////
        var extentStroke = SymbolFactory.Instance.ConstructStroke(ColorFactory.Instance.BlackRGB, 1, SimpleLineStyle.Dot);
        var polygonSymbolExtent = SymbolFactory.Instance.ConstructPolygonSymbol(ColorFactory.Instance.BlackRGB, SimpleFillStyle.Null, extentStroke);
        var extent = tinDataset.GetExtent();
        _graphicsLayer.AddElement(extent, polygonSymbolExtent, "TIN Extent");

        /////GetFullExtent///////////////////////////
        var fullExtentStroke = SymbolFactory.Instance.ConstructStroke(ColorFactory.Instance.BlackRGB, 1, SimpleLineStyle.DashDotDot);
        var polygonSymbolFullExtent = SymbolFactory.Instance.ConstructPolygonSymbol(ColorFactory.Instance.BlackRGB, SimpleFillStyle.Null, fullExtentStroke);
        var fullExtent = tinDataset.GetFullExtent();
        _graphicsLayer.AddElement(fullExtent, polygonSymbolFullExtent, "TIN Full Extent");

        /////GetSuperNodeExtent///////////////////////////
        var superNodeExtentStroke = SymbolFactory.Instance.ConstructStroke(ColorFactory.Instance.BlackRGB, 1, SimpleLineStyle.DashDot);
        var polygonSymbolSuperNodeFullExtent = SymbolFactory.Instance.ConstructPolygonSymbol(ColorFactory.Instance.BlackRGB, SimpleFillStyle.Null, superNodeExtentStroke);
        var superNodeExtent = tinDataset.GetSuperNodeExtent();
        _graphicsLayer.AddElement(superNodeExtent, polygonSymbolSuperNodeFullExtent, "TIN Super Node Full Extent");

      });
    }
  }
}
