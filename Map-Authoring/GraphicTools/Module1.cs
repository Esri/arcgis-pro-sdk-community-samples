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
using System.Windows.Input;
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
using ArcGIS.Desktop.Mapping.Events;
using ArcGIS.Desktop.Layouts;

namespace GraphicTools
{
  /// <summary>
  /// This sample demonstrates tools that can be created around a graphic element creation workflow using map 'markups' for identifying and managing status of field inspection or survey locations.
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data(see under the 'Resources' section for downloading sample data). The sample data contains a dataset called GraphicTools.Make sure that the Sample data is unzipped under C:\Data and the folder "C:\Data\GraphicTools" is available.
  /// 1. Open the solution in Visual Studio.
  /// 1. Click the Build menu and select Build Solution.
  /// 1. Launch the debugger to open ArcGIS Pro. 
  /// 1. Open the project "GraphicTools.aprx" found in folder: "C:\data\GraphicTools. The project opens in a 2D map view, at the Washington, DC bookmark.  In the Contents pane, you can see the two layers, Graphics Markup and Overall SVI - Tracts, along with several layers included in the larger CDC Social Vulnerability Index 2018 - USA group layer dataset. 
  /// 1. Click on the Survey Site bookmark and notice that street level blocks are clearly visible.This area will be the center of the survey, and planning using the map graphics markup tools.Click on the Graphics Markup tab on the ribbon.  The add-in divides tools into four groups on the tab:
  ///    * Graphic Selection – Standard Pro tools for working with graphic selections.
  ///    * Markup Tools – Tools for creating and interacting with survey markups on the map.
  ///    * Text Values – Text strings that are combined with point and polygon markup graphics.
  ///    * Update Status – Tools for updating the status color of the selected markups.
  ///    * These tools, will be used for creating map graphic markups for a field survey.
  /// ![UI](Screenshots/Screen1.png)
  /// 1. In the Contents pane, select the graphics layer named Graphics Markup in the Map view.Notice that the Point + Text and Polygon + Text tools have become activated in the Markup Tools group on the tab.
  /// 1. Populate the edit boxes in the Text Values group.  The text in these two edit boxes will accompany the point or polygon markup tool you choose from the Markup Tools group.Your first polygon markup area will be "Area 1", so type this into the Text Value edit box.  
  /// 1. Populate the Risk Value edit box with an attribute value from the Overall SVI – Tracts layer, which is the percentage of adults in the tract that are over 65 years in age.Select the Insert Risk Value tool from the Markup Tools group.Move the cursor to the Tract 37 area in purple and click the mouse.  You should see the text "Age &gt; 65: 6.4%" in the Risk Value edit box.
  /// 1. You will now create a polygon markup with these text values.Zoom into the area seen in the screenshot below.Select the Polygon + Text tool and sketch a polygon in the area shown below.  You will see a polygon graphic in yellow with the text.  The yellow color shows that this is an area that is designated "To Review".
  /// ![UI](Screenshots/Screen2.png)
  /// &lt;br /&gt; Additional things you can try:
  ///    *	Using the Point + Text and Polygon + Text tools to create graphic markups on the map.
  ///    *	Use the Update Text button to bulk update the text of all selected text graphics.
  ///    *	Click on the Text Pane button in the Markup Tools group on the tab.The Graphic Text pane will open.  Add text and risk values as you did in the steps above, and use the buttons to create point and polygon text values.
  ///    *	Select one or more point and polygon markups, and use the Issue Found and Complete buttons in the Update Status group to update the status color of selected graphics.
  /// </remarks>
  internal class Module1 : Module
    {
        private static Module1 _this = null;
        public bool blnDockpaneOpenStatus;
        public GraphicsLayer SelectedGraphicsLayerTOC;
        private TextPaneViewModel _dockpane;
        public TextEditBox TextValueEditBox { get; set; }
        public QueryEditBox QueryValueEditBox { get; set; }

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current
        {
            get
            {
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("GraphicTools_Module"));
            }
        }

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            return true;
        }

        protected override bool Initialize()
        {
            ArcGIS.Desktop.Mapping.Events.TOCSelectionChangedEvent.Subscribe(OnTOCSelectionChanged);
            blnDockpaneOpenStatus = false;
            
            return base.Initialize();
        }

        private void OnTOCSelectionChanged(MapViewEventArgs obj)
        {
            var selectedGraphicLayers = obj.MapView.GetSelectedLayers().OfType<GraphicsLayer>();
            if (selectedGraphicLayers.Count() == 0)
            { //nothing selected. So clear the selected graphic layer.
                SelectedGraphicsLayerTOC = null;
                return;
            }
            SelectedGraphicsLayerTOC = selectedGraphicLayers.FirstOrDefault();
        }

        #endregion Overrides

        #region Business Logic

        public void RunPointTextTool()
        {
            var cmd = FrameworkApplication.GetPlugInWrapper("GraphicTools_Point_Text_Graphics") as ICommand;
            if (cmd.CanExecute(null))
                cmd.Execute(null);
        }

        public void RunPolygonTextTool()
        {
            var cmd = FrameworkApplication.GetPlugInWrapper("GraphicTools_Polygon_Text_Graphics") as ICommand;
            if (cmd.CanExecute(null))
                cmd.Execute(null);
        }

        public void RunApplyTextTool()
        {

            QueuedTask.Run(() =>
            {
                // Take the currently selected text and update it as needed
                // get the first graphics layer in the map's collection of graphics layers
                var graphicsLayer = MapView.Active.Map.GetLayersAsFlattenedList()
                  .OfType<ArcGIS.Desktop.Mapping.GraphicsLayer>().FirstOrDefault();
                if (graphicsLayer == null)
                    return;

                var selectedGraphicLayers = MapView.Active.GetSelectedLayers().OfType<GraphicsLayer>();
                if (selectedGraphicLayers.Count() == 0)
                { //nothing selected. So clear the selected graphic layer.
                    SelectedGraphicsLayerTOC = null;
                    MessageBox.Show("No graphic layer selected.", "Select layer");
                    return;
                }

                SelectedGraphicsLayerTOC = selectedGraphicLayers.FirstOrDefault();
                var selectedElements = graphicsLayer.GetSelectedElements().
                OfType<GraphicElement>().Where(elem => elem.GetGraphic() is CIMTextGraphic || elem.GetGraphic() is CIMParagraphTextGraphic);

                if (selectedElements.Count() == 0)
                { //nothing selected. So clear the selected graphic layer.
                    MessageBox.Show("No Text or Paragraph Text workflow graphics selected.", "Select graphics");
                    return;
                }

                // Get the editbox or the dockpane textbox value
                string txtBoxString = null;
                string queryTxtBoxString = null;
                if (Module1.Current.blnDockpaneOpenStatus == false)
                {
                    // Use values in the edit boxes
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

                foreach (var elem in selectedElements)
                {
                    // Get the CIMTextGraphic of the current text graphic and update it with contents of text box
                    if (elem.GetGraphic() is CIMTextGraphic)
                    {
                        var newCIMGraphic = elem.GetGraphic() as CIMTextGraphic;
                        newCIMGraphic.Text = txtBoxString;
                        elem.SetGraphic(newCIMGraphic);
                    }
                    // Get the CIMParagraphTextGraphic of the current text graphic and update it with contents of text box
                    if (elem.GetGraphic() is CIMParagraphTextGraphic)
                    {
                        var newCIMGraphic = elem.GetGraphic() as CIMParagraphTextGraphic;
                        newCIMGraphic.Text = txtBoxString;
                        elem.SetGraphic(newCIMGraphic);
                    }
                }

                SelectedGraphicsLayerTOC.ClearSelection();

            });
        }

        public void RunZoomToSelection()
        {
            // Zoom To Selection Tool
            QueuedTask.Run(() =>
            {
                // Take the currently selected text and update it as needed
                // get the first graphics layer in the map's collection of graphics layers
                var graphicsLayer = MapView.Active.Map.GetLayersAsFlattenedList()
              .OfType<ArcGIS.Desktop.Mapping.GraphicsLayer>().FirstOrDefault();
                if (graphicsLayer == null)  return;

                var selectedGraphicLayers = MapView.Active.GetSelectedLayers().OfType<GraphicsLayer>();
                if (selectedGraphicLayers.Count() == 0)
                { //nothing selected. So clear the selected graphic layer.
                    SelectedGraphicsLayerTOC = null;
                    MessageBox.Show("No graphic layer selected.", "Select layer");
                    return;
                }

                SelectedGraphicsLayerTOC = selectedGraphicLayers.FirstOrDefault();
                var selectedElements = graphicsLayer.GetSelectedElements().OfType<GraphicElement>(); 
                Envelope fullEnv = selectedElements.FirstOrDefault().GetBounds();

                foreach (var elem in selectedElements)
                {
                    Envelope elemEnv = elem.GetBounds(false);
                    fullEnv = fullEnv.Union(elemEnv);
                }

                // Zoom to extent of the full selection envelope
                MapView.Active.ZoomTo(fullEnv.Extent);         

            });
        }

        public void UpdateColor(string colorValue)
        {
            // Update Status Color for Selected Point and Polygon Graphics
            QueuedTask.Run(() =>
            {
                // Take the currently selected text and update it as needed
                // get the first graphics layer in the map's collection of graphics layers
                var graphicsLayer = MapView.Active.Map.GetLayersAsFlattenedList()
              .OfType<ArcGIS.Desktop.Mapping.GraphicsLayer>().FirstOrDefault();
                if (graphicsLayer == null) return;

                var selectedGraphicLayers = MapView.Active.GetSelectedLayers().OfType<GraphicsLayer>();
                if (selectedGraphicLayers.Count() == 0)
                { //nothing selected. So clear the selected graphic layer.
                    SelectedGraphicsLayerTOC = null;
                    MessageBox.Show("No graphic layer selected.", "Select layer");
                    return;
                }

                SelectedGraphicsLayerTOC = selectedGraphicLayers.FirstOrDefault();
                var selectedElements = graphicsLayer.GetSelectedElements().
                  OfType<GraphicElement>().Where(elem => elem.GetGraphic() is CIMPolygonGraphic || elem.GetGraphic() is CIMPointGraphic);

                if (selectedElements.Count() == 0)
                { //nothing selected. So clear the selected graphic layer.
                    MessageBox.Show("No point or polygon workflow graphics selected.", "Select graphics");
                    return;
                }

                CIMColor myColor = null;
                switch (colorValue)
                {
                    case "red":
                        myColor = CIMColor.CreateRGBColor(255, 0, 0, 40);
                        break;
                    case "yellow":
                        myColor = CIMColor.CreateRGBColor(255, 255, 0, 40);
                        break;
                    case "green":
                        myColor = CIMColor.CreateRGBColor(0, 255, 0, 40);
                        break;
                }

                foreach (var elem in selectedElements)
                {
                    // Get the CIM Graphic and update it
                    if (elem.GetGraphic() is CIMPolygonGraphic)
                    {
                        var newCIMGraphic = elem.GetGraphic() as CIMPolygonGraphic;
                        CIMPolygonSymbol polySymbol = SymbolFactory.Instance.ConstructPolygonSymbol(myColor);    //  (CIMColor.CreateRGBColor(CreateRGBColor(0, 255, 0, 50));
                        newCIMGraphic.Symbol = polySymbol.MakeSymbolReference();
                        elem.SetGraphic(newCIMGraphic);
                    }

                    if (elem.GetGraphic() is CIMPointGraphic)
                    {
                        var newCIMGraphic = elem.GetGraphic() as CIMPointGraphic;
                        CIMPointSymbol pointSymbol = SymbolFactory.Instance.ConstructPointSymbol(myColor);    //  (CIMColor.CreateRGBColor(CreateRGBColor(0, 255, 0, 50));
                        newCIMGraphic.Symbol = pointSymbol.MakeSymbolReference();
                        elem.SetGraphic(newCIMGraphic);
                    }
                }

            });

        }

        #endregion

    }
}
