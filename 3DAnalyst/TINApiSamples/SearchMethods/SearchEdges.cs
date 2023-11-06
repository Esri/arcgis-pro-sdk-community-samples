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
  internal class SearchEdges : MapTool
  {
    private TinLayer _tinLayer;
    private GraphicsLayer _graphicsLayer;

    public SearchEdges()
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

        List<Polyline> edges = new List<Polyline>();
        TinEdgeFilter tinEdgeFilter = new TinEdgeFilter();
        tinEdgeFilter.FilterEnvelope = searchDataExtent.Extent;
        tinEdgeFilter.FilterType = Module1.Current._tinFilterType;
        tinEdgeFilter.DataElementsOnly = Module1.Current._dataElementsOnlyValue;
        using (var tinEdgeCursor = _tinLayer.SearchEdges(tinEdgeFilter))
        {
          while (tinEdgeCursor.MoveNext())
          {
            using (TinEdge tinEdgeRecord = tinEdgeCursor.Current)
            {
              var edgeGeometry = PolylineBuilderEx.CreatePolyline(tinEdgeRecord.ToPolyline(), MapView.Active.Map.SpatialReference);

              edges.Add(edgeGeometry);
            }
          }
        }
          
        var lineSymbol = SymbolFactory.Instance.ConstructLineSymbol(ColorFactory.Instance.GreyRGB, 1);
        lineSymbol.UseRealWorldSymbolSizes = true;
        foreach (var edge in edges)
        {
          _graphicsLayer.AddElement(edge, lineSymbol, "TINEdge");
        }
      });
      return true;
    }
  }
}
