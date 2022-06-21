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
using ArcGIS.Desktop.Mapping;

namespace GraphicsLayers.GraphicCreationTools
{
  internal class TextCalloutGraphic : MapTool
  {
    private CIMTextSymbol _calloutTextSymbol = null;
    public TextCalloutGraphic()
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Point;
      SketchOutputMode = SketchOutputMode.Map;
    }

    protected override async Task OnToolActivateAsync(bool active)
    {
      _calloutTextSymbol = await CreateBalloonCalloutAsync();
      return;
    }

    protected override  Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      if (Module1.Current.SelectedGraphicsLayerTOC == null)
      {
        MessageBox.Show("Select a graphics layer in the TOC", "No graphics layer selected",
          System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Exclamation);
        return  Task.FromResult(true);
      }
      if (_calloutTextSymbol == null)
        return  Task.FromResult(true);     
      var textGraphic = new CIMTextGraphic
      {
        Symbol = _calloutTextSymbol.MakeSymbolReference(),
        Shape = geometry as MapPoint,
        Text = "Some Text"
      };

      return QueuedTask.Run(() =>
      {
        Module1.Current.SelectedGraphicsLayerTOC.AddElement(textGraphic);

        return true;
      });
    }

    private static Task<CIMTextSymbol> CreateBalloonCalloutAsync()
    {
      return QueuedTask.Run<CIMTextSymbol>(() =>
      {
              //create a text symbol
              var textSymbol = SymbolFactory.Instance.ConstructTextSymbol(ColorFactory.Instance.WhiteRGB, 11, "Corbel", "Regular");
              //A balloon callout
              var balloonCallout = new CIMBalloonCallout();
              //set the callout's style
              balloonCallout.BalloonStyle = BalloonCalloutStyle.RoundedRectangle;
              //Create a solid fill polygon symbol for the callout.
              var polySymbol = SymbolFactory.Instance.ConstructPolygonSymbol(ColorFactory.Instance.BlackRGB, SimpleFillStyle.Solid);
              //Set the callout's background to be the black polygon symbol
              balloonCallout.BackgroundSymbol = polySymbol;
              //margin inside the callout to place the text
              balloonCallout.Margin = new CIMTextMargin{
                Left = 5,
                Right = 5,
                Bottom = 5,
                Top = 5
              };
              //assign the callout to the text symbol's callout property
              textSymbol.Callout = balloonCallout;
              return textSymbol;
      });
    }
  }
}
