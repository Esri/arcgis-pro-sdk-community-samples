using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System.Linq;
using System.Threading.Tasks;

namespace GraphicTools
{
    internal class Point_Text_Graphics : MapTool
    {
        private CIMPointSymbol _pointSymbol = null;
        private CIMTextSymbol _textSymbol = null;
        private TextPaneViewModel _dockpane;

        public Point_Text_Graphics()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Point;
            SketchOutputMode = SketchOutputMode.Map;
        }


        protected override Task OnToolActivateAsync(bool active)
        {
            QueuedTask.Run(() =>
            {
                 _pointSymbol = SymbolFactory.Instance.ConstructPointSymbol(CIMColor.CreateRGBColor(255, 255, 0, 40), 10, SimpleMarkerStyle.Circle);
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

            if (_pointSymbol == null) return Task.FromResult(true);
            return QueuedTask.Run(() =>
            {
                var selectedElements = Module1.Current.SelectedGraphicsLayerTOC.GetSelectedElements().
                                            OfType<GraphicElement>();

                //If only one element is selected, is it of type Point?      
                if (selectedElements.Count() == 1)
                {
                    if (selectedElements.FirstOrDefault().GetGraphic() is CIMPointGraphic) //It is a Point
                    {
                        //So we use it
                        var polySymbol = selectedElements.FirstOrDefault().GetGraphic() as CIMPointGraphic;
                        _pointSymbol = polySymbol.Symbol.Symbol as CIMPointSymbol;
                    }
                }
                var cimGraphicElement = new CIMPointGraphic
                {
                    Location = geometry as MapPoint,
                    Symbol = _pointSymbol.MakeSymbolReference()
                };
                Module1.Current.SelectedGraphicsLayerTOC.AddElement(cimGraphicElement);

                //  Add a text label graphic next to the point graphic:
                if (selectedElements.Count() == 1)
                {
                    if (selectedElements.FirstOrDefault().GetGraphic() is CIMTextGraphic) //It is a Text
                    {
                        var textSymbol = selectedElements.FirstOrDefault().GetGraphic() as CIMTextGraphic;
                        _textSymbol = textSymbol.Symbol.Symbol as CIMTextSymbol;
                    }
                }

                // Get the ribbon or the dockpane text values
                string txtBoxString = null;
                string queryTxtBoxString = null;
                if (Module1.Current.blnDockpaneOpenStatus == false)
                {
                    // Use value in the edit box
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

                var textGraphic = new CIMTextGraphic
                {
                    Symbol = _textSymbol.MakeSymbolReference(),
                    Shape = geometry as MapPoint,
                    Text = txtBoxString
                };
                Module1.Current.SelectedGraphicsLayerTOC.AddElement(textGraphic);
                Module1.Current.SelectedGraphicsLayerTOC.ClearSelection();

                return true;
            });

        }
    }
}
