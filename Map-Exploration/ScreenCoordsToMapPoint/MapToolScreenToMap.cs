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
using ArcGIS.Desktop.Mapping;

namespace TestScreenToMap
{
  internal class MapToolScreenToMap : MapTool
  {
    private CIMPointSymbol _pointSymbol = null;
    private IDisposable _graphic = null;

    public MapToolScreenToMap()
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Point;
      SketchOutputMode = SketchOutputMode.Screen;
    }

    protected async override Task OnToolActivateAsync(bool active)
    {
      if (_pointSymbol == null) _pointSymbol = await CreatePointSymbolAsync();
    }

    protected async override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      if (geometry.SpatialReference == null)
      {
        // screen coordinates
        var screenPointAsMapPoint = geometry as MapPoint;
        if (screenPointAsMapPoint != null)
        {
          var pnt = new System.Windows.Point
          {
            X = screenPointAsMapPoint.X,
            Y = screenPointAsMapPoint.Y
          };
          var mapScreenPoint = await QueuedTask.Run<MapPoint>(
                 () => MapView.Active.ClientToMap(pnt));
          if (mapScreenPoint.IsEmpty)
          {
            System.Diagnostics.Debug.WriteLine (@"Screen Point is empty");
          }
          else
          {
            _graphic = await this.AddOverlayAsync(mapScreenPoint, _pointSymbol.MakeSymbolReference());
          }
        }
      }
      else
      {
        // map coordinates
        _graphic = await this.AddOverlayAsync(geometry, _pointSymbol.MakeSymbolReference());
      }
      return true;
    }

    protected override Task OnToolDeactivateAsync(bool hasMapViewChanged)
    {
      if (_graphic != null) _graphic.Dispose();//Clear out the old overlay
      _graphic = null;
      return base.OnToolDeactivateAsync(hasMapViewChanged);
    }

    internal static Task<CIMPointSymbol> CreatePointSymbolAsync()
    {
      return QueuedTask.Run(() => SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.RedRGB, 14, SimpleMarkerStyle.Circle));
    }

  }
}
