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
using ArcGIS.Desktop.Mapping;

namespace OverlayExamples
{
  class AddOverlayWithSnappingTool : MapTool
  {

    private IDisposable _graphic = null;
    private CIMLineSymbol _lineSymbol = null;
    public AddOverlayWithSnappingTool()
    {
      IsSketchTool = true;
      UseSnapping = true;//set this to true to honor snapping in the sketch
      SketchType = SketchGeometryType.Line;
      SketchOutputMode = SketchOutputMode.Map;
    }

    protected override Task OnToolDeactivateAsync(bool hasMapViewChanged)
    {
      if (_graphic != null)
      {
        _graphic.Dispose();//Clear out the old overlay
        _graphic = null;
      }
      return base.OnToolDeactivateAsync(hasMapViewChanged);
    }

    /// <summary>
    /// Occurs when the tool is activated.
    /// </summary>
    /// <param name="hasMapViewChanged">A value indicating if the active <see cref="T:ArcGIS.Desktop.Mapping.MapView"/> has changed.</param>
    /// <returns>
    /// A Task that represents a tool activation event.
    /// </returns>
    protected async override Task OnToolActivateAsync(bool hasMapViewChanged)
    {
      if (_lineSymbol == null)
      {
        _lineSymbol = await Module1.CreateLineSymbolAsync();
      }
      this.SketchSymbol = _lineSymbol.MakeSymbolReference();
      //configure snapping
      await Module1.ConfigureSnappingAsync();
    }

    protected override void OnToolMouseDown(MapViewMouseButtonEventArgs e)
    {
      if (_graphic != null)
      {
        _graphic.Dispose();//Clear out the old overlay
        _graphic = null;
      }
      base.OnToolMouseDown(e);
    }
    protected async override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      _graphic = await this.AddOverlayAsync(geometry, _lineSymbol.MakeSymbolReference());
      return true;
    }
  }
}
