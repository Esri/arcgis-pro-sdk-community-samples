/*

   Copyright 2022 Esri

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

namespace GraphicElementSymbolPicker.PolygonTools
{
  internal class CloudTool : LayoutTool
  {
    public CloudTool()
    {
      SketchType = SketchGeometryType.Rectangle;
    }
    protected override Task OnToolActivateAsync(bool active)
    {
      return base.OnToolActivateAsync(active);
    }
    protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      if (ActiveElementContainer == null)
        Task.FromResult(true);

      QueuedTask.Run(() =>
      {
        var sketch = geometry as Polygon;
        var marker = SymbolFactory.Instance.ConstructMarker(
          CIMColor.CreateRGBColor(0, 0, 255), 20, SimpleMarkerStyle.Cloud) as CIMVectorMarker;

        var cloud = marker.MarkerGraphics[0].Geometry;
        var polySymbol = marker.MarkerGraphics[0].Symbol;

        //Add the cloud to the layout elem factory
        //which converts it to page or map (this is the _key_)
        var ge = ElementFactory.Instance.CreateGraphicElement(
          this.ActiveElementContainer, cloud, Module1.SelectedSymbol);

        //scale it to fill the extent of the sketch, then move
        ge.SetLockedAspectRatio(true);
        ge.SetHeight(sketch.Extent.Height);//scale
        ge.SetAnchor(Anchor.CenterPoint);
        ge.SetAnchorPoint(sketch.Extent.Center.Coordinate2D);//move

      });
      return base.OnSketchCompleteAsync(geometry);
    }
  }
}
