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
using System.Windows;

namespace CopyPaste
{
  internal class CreateLine : MapTool
  {
    private CIMLineSymbol _lineSymbol = null;

    public CreateLine()
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Line;
      SketchOutputMode = SketchOutputMode.Map;
    }

    protected override Task OnToolActivateAsync(bool active)
    {
      _lineSymbol = SymbolFactory.Instance.ConstructLineSymbol(CIMColor.CreateRGBColor(255, 0, 0), 4.0, SimpleLineStyle.Solid);
      return base.OnToolActivateAsync(active);
    }

    protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      var graphicsLayerName = @"My lines";
      var graphicsLayerCreationParams = new GraphicsLayerCreationParams { Name = graphicsLayerName, MapMemberPosition = MapMemberPosition.AutoArrange };
      var myGraphicLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<GraphicsLayer>().Where(e => e.Name == graphicsLayerName).FirstOrDefault();

      if (_lineSymbol == null) return Task.FromResult(true);
      return QueuedTask.Run(() =>
      {
        if (myGraphicLayer == null)
        {
          myGraphicLayer = LayerFactory.Instance.CreateLayer<GraphicsLayer>(graphicsLayerCreationParams, MapView.Active.Map);
          if (myGraphicLayer == null)
          {
            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($@"{graphicsLayerName} not found", $@"Unable to create the '{graphicsLayerName}' graphics layer",
              System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Exclamation);
            return true;
          }
        }
        // add the line to 'my route'
        var cimGraphicElement = new CIMLineGraphic
        {
          Line = geometry as Polyline,
          Symbol = _lineSymbol.MakeSymbolReference()
        };
        myGraphicLayer.RemoveElements(null);
        myGraphicLayer.AddElement(cimGraphicElement);
        myGraphicLayer.ClearSelection();
        // update status in module that copy is now possible
        Module1.Current.TheGeometry = geometry.Clone();
        Module1.Current.TheGraphicsLayer = myGraphicLayer;
        // only this item is selected
        return true;
      });
    }
  }
}
