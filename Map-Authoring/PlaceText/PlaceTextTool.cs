/*

   Copyright 2018 Esri

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

namespace PlaceText
{
  internal class PlaceTextTool : MapTool
  {

    public PlaceTextTool()
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Point;
      SketchOutputMode = SketchOutputMode.Map;
    }

    protected override Task OnToolActivateAsync(bool active)
    {
      return base.OnToolActivateAsync(active);
    }

    protected override Task OnToolDeactivateAsync(bool hasMapViewChanged)
    {
      foreach(var graphic in Module1.Current.Graphics)
      {
        graphic.Dispose();
      }
      Module1.Current.Graphics.Clear();
      return Task.CompletedTask;
    }

    protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      return QueuedTask.Run(() =>
      {
        //MapView.Active.AddOverlay()
        var textGraphic = new CIMTextGraphic();
        textGraphic.Symbol = SymbolFactory.Instance.ConstructTextSymbol(
          ColorFactory.Instance.BlackRGB, 12, "Times New Roman", "Regular").MakeSymbolReference();

        //make the callout for the circle
        var callOut = new CIMPointSymbolCallout();
        callOut.PointSymbol = new CIMPointSymbol();
        //Circle outline - 40
        var circle_outline = SymbolFactory.Instance.ConstructMarker(40, "ESRI Default Marker") as CIMCharacterMarker;
        //Square - 41
        //Triangle - 42
        //Pentagon - 43
        //Hexagon - 44
        //Octagon - 45
        circle_outline.Size = 40;
        //eliminate the outline
        foreach (var layer in circle_outline.Symbol.SymbolLayers)
        {
          if (layer is CIMSolidStroke)
          {
            ((CIMSolidStroke)layer).Width = 0;
          }
        }

        //Circle fill - 33
        var circle_fill = SymbolFactory.Instance.ConstructMarker(33, "ESRI Default Marker") as CIMCharacterMarker;
        //Square - 34
        //Triangle - 35
        //Pentagon - 36
        //Hexagon - 37
        //Octagon - 38
        circle_fill.Size = 40;
        //eliminate the outline, make sure the fill is white
        foreach (var layer in circle_fill.Symbol.SymbolLayers)
        {
          if (layer is CIMSolidFill)
          {
            ((CIMSolidFill)layer).Color = ColorFactory.Instance.WhiteRGB;
          }
          else if (layer is CIMSolidStroke)
          {
            ((CIMSolidStroke)layer).Width = 0;
          }
        }

        var calloutLayers = new List<CIMSymbolLayer>();
        calloutLayers.Add(circle_outline);
        calloutLayers.Add(circle_fill);
        //set the layers on the callout
        callOut.PointSymbol.SymbolLayers = calloutLayers.ToArray();

        //set the callout on the text symbol
        var textSym = textGraphic.Symbol.Symbol as CIMTextSymbol;
        textSym.Callout = callOut;
        textSym.Height = 12;//adjust as needed

        //now set the text
        textGraphic.Text = "12 <SUP><UND>00</UND></SUP>";
        textGraphic.Shape = geometry;

        var graphic = MapView.Active.AddOverlay(textGraphic);
        Module1.Current.Graphics.Add(graphic);
        return true;
      });
    }
  }
}
