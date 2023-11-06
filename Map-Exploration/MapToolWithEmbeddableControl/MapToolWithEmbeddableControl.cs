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
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapToolWithEmbeddableControl
{
  internal class MapToolWithEmbeddableControl : MapTool
  {
    private IDisposable _currentGraphic;

    public MapToolWithEmbeddableControl()
    {
      IsSketchTool = false;
      SketchType = SketchGeometryType.Rectangle;
      SketchOutputMode = SketchOutputMode.Map;
      this.OverlayControlCanResize = false;
      this.OverlayControlID = "MapToolWithEmbeddableControl_EmbeddableControl";
      Module1.MapToolWithEmbeddableControl = this;
    }

    private static CIMSymbolReference _point2DSymbolRef = null;


    /// <summary>
    /// Get point symbol, must call from MCT
    /// </summary>
    /// <returns></returns>
    internal static CIMSymbolReference GetPointSymbolRef()
    {
      if (_point2DSymbolRef != null) return _point2DSymbolRef;
      CIMPointSymbol pntSym = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.RedRGB, 10, SimpleMarkerStyle.Circle);
      _point2DSymbolRef = pntSym.MakeSymbolReference();
      return _point2DSymbolRef;
    }

    internal void ShowPoint (Geometry point)
    {
      QueuedTask.Run(() =>
      {
        if (_currentGraphic != null)
        {
          // clear the previous point
          _currentGraphic.Dispose();
          _currentGraphic = null;
        }
        _currentGraphic = AddOverlay(point, GetPointSymbolRef());
      });
    }

    protected override Task OnToolActivateAsync(bool active)
    {
      
      return base.OnToolActivateAsync(active);
    }

    protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      return base.OnSketchCompleteAsync(geometry);
    }
  }
}
