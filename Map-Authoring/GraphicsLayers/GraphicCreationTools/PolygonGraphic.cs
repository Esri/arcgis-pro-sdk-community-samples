/*

   Copyright 2020 Esri

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
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;

namespace GraphicsLayers.GraphicCreationTools
{
  internal class PolygonGraphic : MapTool
  {
    private CIMPolygonSymbol _polygonSymbol = null;
    public PolygonGraphic()
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Polygon;
      SketchOutputMode = SketchOutputMode.Map;
    }

    protected override Task OnToolActivateAsync(bool active)
    {
      _polygonSymbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(255, 255, 0));
      return base.OnToolActivateAsync(active);
    }

    protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      if (Module1.Current.SelectedGraphicsLayerTOC == null)
      {
        MessageBox.Show("Select a graphics layer in the TOC", "No graphics layer selected",
          System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Exclamation);
        return Task.FromResult(true);
      }
      if (_polygonSymbol == null) return Task.FromResult(true);
      return QueuedTask.Run(() =>
      {
        var selectedElements = Module1.Current.SelectedGraphicsLayerTOC.GetSelectedElements().
                                    OfType<GraphicElement>();

        //If only one element is selected, is it of type Polygon?      
        if (selectedElements.Count() == 1)
        {
          if (selectedElements.FirstOrDefault().GetGraphic() is CIMPolygonGraphic) //It is a polygon
          {
            //So we use it
            var polySymbol = selectedElements.FirstOrDefault().GetGraphic() as CIMPolygonGraphic;
            _polygonSymbol = polySymbol.Symbol.Symbol as CIMPolygonSymbol;
          }
        }
        var cimGraphicElement = new CIMPolygonGraphic
        {
          Polygon = geometry as Polygon,
          Symbol = _polygonSymbol.MakeSymbolReference()
        };
        Module1.Current.SelectedGraphicsLayerTOC.AddElement(cimGraphicElement);
        return true;
      });
    }
  }
}
