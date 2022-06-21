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

namespace GraphicTools
{
    internal class Polygon_Text_Graphics : MapTool
    {

        private CIMPolygonSymbol _polygonSymbol = null;
        private CIMTextSymbol _textSymbol = null;
        private TextPaneViewModel _dockpane;

        public Polygon_Text_Graphics()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Polygon;
            SketchOutputMode = SketchOutputMode.Map;
        }

        protected override Task OnToolActivateAsync(bool active)
        {
            QueuedTask.Run(() =>
            {
                _polygonSymbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(255, 255, 0, 40));
                _textSymbol = SymbolFactory.Instance.ConstructTextSymbol(ColorFactory.Instance.BlackRGB, 9.5, "Corbel", "Bold");
            });

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

                if (selectedElements.Count() == 1)
                {
                    if (selectedElements.FirstOrDefault().GetGraphic() is CIMPolygonGraphic) 
                    {
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

                MapPoint pntPolygon = null;
                pntPolygon = GeometryEngine.Instance.LabelPoint(geometry);

                if (selectedElements.Count() == 1)
                {
                    if (selectedElements.FirstOrDefault().GetGraphic() is CIMTextGraphic) 
                    {
                        //So we use it
                        var textSymbol = selectedElements.FirstOrDefault().GetGraphic() as CIMTextGraphic;
                        _textSymbol = textSymbol.Symbol.Symbol as CIMTextSymbol;
                    }
                }

                // Get the ribbon or the dockpane text values
                string txtBoxString = null;
                string queryTxtBoxString = null;
                if (Module1.Current.blnDockpaneOpenStatus == false)
                {
                    // Use value in the edit boxes
                    txtBoxString = Module1.Current.TextValueEditBox.Text;
                    queryTxtBoxString = Module1.Current.QueryValueEditBox.Text;
                    if (txtBoxString == null || txtBoxString == "") txtBoxString = "    Default Text";
                    else txtBoxString = "   " + txtBoxString;
                    if (queryTxtBoxString != null && queryTxtBoxString != "")
                    {
                        txtBoxString = txtBoxString + "\r\n    " + queryTxtBoxString;
                    }
                }
                if (Module1.Current.blnDockpaneOpenStatus == true)
                {
                    _dockpane = FrameworkApplication.DockPaneManager.Find("GraphicTools_TextPane") as TextPaneViewModel;
                    txtBoxString = _dockpane.TxtBoxDoc;
                    queryTxtBoxString = _dockpane.QueryTxtBoxDoc;
                    if (txtBoxString == null || txtBoxString == "") txtBoxString = "    Default Text";
                    else txtBoxString = "   " + txtBoxString;
                    if (queryTxtBoxString != null && queryTxtBoxString != "")
                    {
                        txtBoxString = txtBoxString + "\r\n    " + queryTxtBoxString;
                    }
                }

                var textGraphic = new CIMParagraphTextGraphic
                {
                    Symbol = _textSymbol.MakeSymbolReference(),
                    Shape = geometry as Polygon,
                    Text = txtBoxString
                };

                Module1.Current.SelectedGraphicsLayerTOC.AddElement(textGraphic);
                Module1.Current.SelectedGraphicsLayerTOC.ClearSelection();

                return true;
            });

        }
    }
}
