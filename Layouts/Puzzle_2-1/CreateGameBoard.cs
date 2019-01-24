/*

   Copyright 2019 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

//Added references
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.Media;

namespace Puzzle_2_1
{
  internal class CreateGameBoard : Button
  {

    public static string AddinAssemblyLocation()
    {
      var asm = System.Reflection.Assembly.GetExecutingAssembly();
      return System.IO.Path.GetDirectoryName(
                        Uri.UnescapeDataString(
                                new Uri(asm.CodeBase).LocalPath));
    }
    async protected override void OnClick()
    {

      //Check to see if the Game Board layout already exists
      LayoutProjectItem layoutItem = Project.Current.GetItems<LayoutProjectItem>().FirstOrDefault(item => item.Name.Equals("Game Board"));
      if (layoutItem != null)
      {
        Layout lyt = await QueuedTask.Run(() => layoutItem.GetLayout());
        
        //Next check to see if a layout view is already open that referencs the Game Board layout
        foreach (var pane in ProApp.Panes)
        {
          var lytPane = pane as ILayoutPane;
          if (lytPane == null)  //if not a layout view, continue to the next pane
            continue;
          if (lytPane.LayoutView.Layout == lyt) //if there is a match, activate the view
          {
            (lytPane as Pane).Activate();
            System.Windows.MessageBox.Show("Activating existing pane");
            return;
          }
        }

        //If panes don't exist, then open a new pane
        await ProApp.Panes.CreateLayoutPaneAsync(lyt);
        System.Windows.MessageBox.Show("Opening already existing layout");
        return;
      }

      //The layout does not exist so create a new one
      Layout layout = await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run<Layout>(() =>
      {
        //*** CREATE A NEW LAYOUT ***
        
        //Set up a page
        CIMPage newPage = new CIMPage();
        //required properties
        newPage.Width = 17;
        newPage.Height = 11;
        newPage.Units = LinearUnit.Inches;

        //optional rulers
        newPage.ShowRulers = true;
        newPage.SmallestRulerDivision = 0.5;

        layout = LayoutFactory.Instance.CreateLayout(newPage);
        layout.SetName("Game Board");

        //*** INSERT MAP FRAME ***

        // create a new map with an ArcGIS Online basemap
        Map map = MapFactory.Instance.CreateMap("World Map", MapType.Map, MapViewingMode.Map, Basemap.NationalGeographic);

        //Build map frame geometry
        Coordinate2D ll = new Coordinate2D(4, 0.5);
        Coordinate2D ur = new Coordinate2D(13, 6.5);
        Envelope env = EnvelopeBuilder.CreateEnvelope(ll, ur);

        //Create map frame and add to layout
        MapFrame mfElm = LayoutElementFactory.Instance.CreateMapFrame(layout, env, map);
        mfElm.SetName("Main MF");

        //Set the camera
        Camera camera = mfElm.Camera;
        camera.X = 3365;
        camera.Y = 5314468;
        camera.Scale = 175000000;
        mfElm.SetCamera(camera);

        //*** INSERT TEXT ELEMENTS ***

        //Title text
        Coordinate2D titleTxt_ll = new Coordinate2D(6.5, 10);
        CIMTextSymbol arial36bold = SymbolFactory.Instance.ConstructTextSymbol(ColorFactory.Instance.BlueRGB, 36, "Arial", "Bold");
        GraphicElement titleTxtElm = LayoutElementFactory.Instance.CreatePointTextGraphicElement(layout, titleTxt_ll, "Feeling Puzzled?", arial36bold);
        titleTxtElm.SetName("Title");

        //Instuctions text
        Coordinate2D recTxt_ll = new Coordinate2D(4.25, 6.5);
        Coordinate2D recTxt_ur = new Coordinate2D(13, 9.75);
        Envelope recEnv = EnvelopeBuilder.CreateEnvelope(recTxt_ll, recTxt_ur);
        CIMTextSymbol arial18 = SymbolFactory.Instance.ConstructTextSymbol(ColorFactory.Instance.GreyRGB, 20, "Arial", "Regular");
        string text = "<bol>Instructions:</bol> " +
                             "\n\n  - Activate the map frame below and pan / zoom to desired extent" +
                             "\n\n  - Close map frame activation" +
                             "\n\n  - Click the 'Scramble Pieces' command" as String;
        GraphicElement recTxtElm = LayoutElementFactory.Instance.CreateRectangleParagraphGraphicElement(layout, recEnv, text, arial18);
        recTxtElm.SetName("Instructions");

        //Service layer credits
        Coordinate2D slcTxt_ll = new Coordinate2D(0.5, 0.2);
        Coordinate2D slcTxt_ur = new Coordinate2D(16.5, 0.4);
        Envelope slcEnv = EnvelopeBuilder.CreateEnvelope(slcTxt_ll, slcTxt_ur);
        CIMTextSymbol arial8reg = SymbolFactory.Instance.ConstructTextSymbol(ColorFactory.Instance.BlackRGB, 8, "Arial", "Regular");
        String slcText = "<dyn type='layout' name='Game Board' property='serviceLayerCredits'/>";
        GraphicElement slcTxtElm = LayoutElementFactory.Instance.CreateRectangleParagraphGraphicElement(layout, slcEnv, slcText, arial8reg);
        slcTxtElm.SetName("SLC");

        //Status and results text (blank to start)
        Coordinate2D statusTxt_ll = new Coordinate2D(4.5, 7);
        CIMTextSymbol arial24bold = SymbolFactory.Instance.ConstructTextSymbol(ColorFactory.Instance.BlackRGB, 24, "Arial", "Bold");
        GraphicElement statusTxtElm = LayoutElementFactory.Instance.CreatePointTextGraphicElement(layout, statusTxt_ll, "", arial24bold);
        statusTxtElm.SetName("Status");
        return layout;  
      });

      //*** OPEN LAYOUT VIEW (must be in the GUI thread) ***
      var layoutPane = await ProApp.Panes.CreateLayoutPaneAsync(layout);
      var sel = layoutPane.LayoutView.GetSelectedElements();
      if (sel.Count > 0)                                     
      {
        layoutPane.LayoutView.ClearElementSelection();        
      }
    }
  }
}
