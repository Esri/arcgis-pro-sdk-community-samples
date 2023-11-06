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
using ArcGIS.Core.Data.Analyst3D;
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

namespace TINApiSamples.SearchMethods
{
  internal class SearchTriangles : MapTool
  {
    private TinLayer _tinLayer;
    private GraphicsLayer _graphicsLayer;

    public SearchTriangles()
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Rectangle;
      SketchOutputMode = SketchOutputMode.Map;
    }

    protected override Task OnToolActivateAsync(bool active)
    {
      _tinLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<TinLayer>().FirstOrDefault();
      _graphicsLayer = MapView.Active.Map.TargetGraphicsLayer;
      return base.OnToolActivateAsync(active);
    }

    protected async override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      var searchDataExtent = geometry as Polygon;
      await QueuedTask.Run(() =>
      {
        if (_tinLayer == null) return;
        if (_graphicsLayer == null) return;
        if (!Module1.IsView2D()) return;       

        List<Polygon> triangles = new List<Polygon>();
        TinTriangleFilter tinTriangleFilter = new TinTriangleFilter();
        tinTriangleFilter.FilterEnvelope = searchDataExtent.Extent; //custom extent defined by a map tool.
        tinTriangleFilter.FilterType = Module1.Current._tinFilterType;
        tinTriangleFilter.DataElementsOnly = Module1.Current._dataElementsOnlyValue;
        //Get the triangles defined by the TinTriangleFilter
        //If no filter is set, all triangles will be retrieved
        using (var tinTriangleCursor = _tinLayer.SearchTriangles(tinTriangleFilter))
        {
          while (tinTriangleCursor.MoveNext())
          {
            using (TinTriangle tinTriangleRecord = tinTriangleCursor.Current)
            {
              var triangleGeometry = PolygonBuilderEx.CreatePolygon(tinTriangleRecord.ToPolygon(), MapView.Active.Map.SpatialReference);
              triangles.Add(triangleGeometry);
              //Do something with triangle
              var tinTrianglePoints = tinTriangleRecord.ToPolygon().Points;
            }
          }
        }
          
        var polygonSymbol = SymbolFactory.Instance.ConstructPolygonSymbol(ColorFactory.Instance.BlackRGB, SimpleFillStyle.Null);
        polygonSymbol.UseRealWorldSymbolSizes = true;
        foreach (var triangle in triangles)
        {
          _graphicsLayer.AddElement(triangle, polygonSymbol, "TINTriangle");
        }
      });
      return true;
    }
  }
}
