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
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Layouts.Events;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.Geometry;
using System.Media;
using ArcGIS.Desktop.Mapping.Events;

namespace MapPuzzle
{
  /// <summary>
  /// This sample shows how to create and manipulate layout elements.
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.  
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open. 
  /// 1. Open any project file with a map and activate the 'Puzzle' tab.
  /// 1. Click Create Game Board – this should build a new active layout where a number of layout elements a created automatically.
  /// 1. Note: from here on, all the directions should appear in the layout:
  /// 1. Select the Map Frame on the newly created layout, right-click and choose Activate.
  /// ![UI](Screenshots/Screen2.png)  
  /// 1. Zoom in and navigate to a desired location.
  /// 1. Close the activation (via the X in the upper right corner of the layout tab).
  /// ![UI](Screenshots/Screen3.png)  
  /// 1. Active the 'Puzzle' tab again.
  /// 1. Click Scramble Pieces – this will create 6 smaller map frames placed in random order around the main map frame.
  /// 1. Click Play Game – select a smaller map frame and then select the empty rectangle where it belongs.  Continue in this order until all 6 pieces are placed.
  /// ![UI](Screenshots/Screen4.png)  
  /// 1. Click New Game and then Create Game Board.
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    public static Module1 Current
    {
      get
      {
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("MapPuzzleModule"));
      }
    }

    public static string AddinAssemblyLocation()
    {
      var asm = System.Reflection.Assembly.GetExecutingAssembly();
      return System.IO.Path.GetDirectoryName(
                        Uri.UnescapeDataString(
                                new Uri(asm.Location).LocalPath));
    }

    #region Overrides
    protected override bool Initialize()
    {
      IPlugInWrapper Create_Button = FrameworkApplication.GetPlugInWrapper("MapPuzzleCreateGameBoard");

      IPlugInWrapper Scramble_Button = FrameworkApplication.GetPlugInWrapper("MapPuzzleScramblePieces");
      //Scramble_Button.Enabled = false
      IPlugInWrapper Play_Button = FrameworkApplication.GetPlugInWrapper("MapPuzzlePlayGame");
      IPlugInWrapper New_Button = FrameworkApplication.GetPlugInWrapper("MapPuzzleNewGame");

      base.Initialize();
      ArcGIS.Desktop.Layouts.Events.ElementEvent.Subscribe(LayoutSelectionCallBack);
      return true;

    }

    async public void LayoutSelectionCallBack(ElementEventArgs args)
    {
      if (args.Hint != ElementEventHint.SelectionChanged) return;
      if (Globals.selEvents)
      {
        LayoutView layoutView = LayoutView.Active;
        Layout layout = layoutView.Layout;

        TextElement txtElm = layout.FindElement("Instructions") as TextElement;
        TextProperties txtProp = txtElm.TextProperties;

        TextElement statusText = layout.FindElement("Status") as TextElement;
        TextProperties statusProp = statusText.TextProperties;

        if (!(args.Container is Layout theLayoutView)) return;
        var selElm = theLayoutView.GetSelectedElements().ToList().FirstOrDefault();  // could also use layoutView.GetSelectedElements().FirstOrDefault();        

        if (Globals.elmType == "MF" && selElm is MapFrame)  //Select appropriate Map Frame
        {
          txtProp.Text = "<bol>Instructions:</bol> \n\n  - Select the rectangle where the map frame should be placed.";
          await QueuedTask.Run(() => txtElm.SetTextProperties(txtProp));
          Globals.elmType = "REC";
          Globals.mf_Name = selElm.Name;
          return;
        }

        if (Globals.elmType == "MF" && selElm is GraphicElement)
        {
          System.Windows.MessageBox.Show("Hey Bonehead: will you please follow instuctions, you selected the wrong thing!");
          return;
        }

        else if (Globals.elmType == "REC" && selElm is GraphicElement)  //Select appropriate Rectangle
        {
          txtProp.Text = "<bol>Instructions: </bol> \n\n  - Select another map frame.";
          await QueuedTask.Run(() => txtElm.SetTextProperties(txtProp));
          Globals.elmType = "MF";
          Globals.i_guesses = Globals.i_guesses + 1;
          if (selElm.Name == "Rectangle 1" && Globals.mf_Name == "MF1")
          {
            MoveMF(selElm.Name);
          }
          else if (selElm.Name == "Rectangle 2" && Globals.mf_Name == "MF2")
          {
            MoveMF(selElm.Name);
          }
          else if (selElm.Name == "Rectangle 3" && Globals.mf_Name == "MF3")
          {
            MoveMF(selElm.Name);
          }
          else if (selElm.Name == "Rectangle 4" && Globals.mf_Name == "MF4")
          {
            MoveMF(selElm.Name);
          }
          else if (selElm.Name == "Rectangle 5" && Globals.mf_Name == "MF5")
          {
            MoveMF(selElm.Name);
          }
          else if (selElm.Name == "Rectangle 6" && Globals.mf_Name == "MF6")
          {
            MoveMF(selElm.Name);
          }
          else
          {
            statusProp.Text = "<bol><clr red='255'> WRONG!!! </clr></bol> <_bol> Try again. You are not very 'spatial'</_bol>";
            await QueuedTask.Run(() => statusText.SetTextProperties(statusProp));
          }
          return;
        }
        if (Globals.elmType == "REC" && selElm is MapFrame)
        {
          System.Windows.MessageBox.Show("Hey Bonehead: will you please follow instuctions, you selected the wrong thing!");
          return;
        }
      }
    }

    async public void MoveMF(string elmName)
    {
      Globals.i_correct = Globals.i_correct + 1;

      LayoutView layoutView = LayoutView.Active;
      Layout layout = layoutView.Layout;
      if (elmName == "Rectangle 1")
      {
        MapFrame mf1 = layout.FindElement("MF1") as MapFrame;
        await QueuedTask.Run(() => mf1.SetX(4));
        await QueuedTask.Run(() => mf1.SetY(0.5));
      }
      if (elmName == "Rectangle 2")
      {
        MapFrame mf2 = layout.FindElement("MF2") as MapFrame;
        await QueuedTask.Run(() => mf2.SetX(7));
        await QueuedTask.Run(() => mf2.SetY(0.5));
      }
      if (elmName == "Rectangle 3")
      {
        MapFrame mf3 = layout.FindElement("MF3") as MapFrame;
        await QueuedTask.Run(() => mf3.SetX(10));
        await QueuedTask.Run(() => mf3.SetY(0.5));
      }
      if (elmName == "Rectangle 4")
      {
        MapFrame mf4 = layout.FindElement("MF4") as MapFrame;
        await QueuedTask.Run(() => mf4.SetX(10));
        await QueuedTask.Run(() => mf4.SetY(3.5));
      }
      if (elmName == "Rectangle 5")
      {
        MapFrame mf5 = layout.FindElement("MF5") as MapFrame;
        await QueuedTask.Run(() => mf5.SetX(7));
        await QueuedTask.Run(() => mf5.SetY(3.5));
      }
      if (elmName == "Rectangle 6")
      {
        MapFrame mf6 = layout.FindElement("MF6") as MapFrame;
        await QueuedTask.Run(() => mf6.SetX(4));
        await QueuedTask.Run(() => mf6.SetY(3.5));
      }

      TextElement statusText = layout.FindElement("Status") as TextElement;
      TextProperties statusProp = statusText.TextProperties;
      if (Globals.i_correct == 1)
      {
        statusProp.Text = "Nice job!  You got " + Globals.i_correct.ToString() + " correct out of " + Globals.i_guesses + " attempt.";
      }
      else
      {
        statusProp.Text = "Nice job!  You got " + Globals.i_correct.ToString() + " correct out of " + Globals.i_guesses + " attempts.";
      }
      await QueuedTask.Run(() => statusText.SetTextProperties(statusProp));

      if (Globals.i_correct == 6)  //YOU WIN
      {
        statusProp.Text = "GAME OVER!  You got " + Globals.i_correct.ToString() + " correct out of " + Globals.i_guesses + " attempts.";
        await QueuedTask.Run(() => statusText.SetTextProperties(statusProp));

        //Turn off rectangles
        GraphicElement rec1 = layout.FindElement("Rectangle 1") as GraphicElement;
        await QueuedTask.Run(() => rec1.SetVisible(false));
        GraphicElement rec2 = layout.FindElement("Rectangle 2") as GraphicElement;
        await QueuedTask.Run(() => rec2.SetVisible(false));
        GraphicElement rec3 = layout.FindElement("Rectangle 3") as GraphicElement;
        await QueuedTask.Run(() => rec3.SetVisible(false));
        GraphicElement rec4 = layout.FindElement("Rectangle 4") as GraphicElement;
        await QueuedTask.Run(() => rec4.SetVisible(false));
        GraphicElement rec5 = layout.FindElement("Rectangle 5") as GraphicElement;
        await QueuedTask.Run(() => rec5.SetVisible(false));
        GraphicElement rec6 = layout.FindElement("Rectangle 6") as GraphicElement;
        await QueuedTask.Run(() => rec6.SetVisible(false));

        //Toggle MFs
        MapFrame mf1 = layout.FindElement("MF1") as MapFrame;
        await QueuedTask.Run(() => mf1.SetVisible(false));
        MapFrame mf2 = layout.FindElement("MF2") as MapFrame;
        await QueuedTask.Run(() => mf2.SetVisible(false));
        MapFrame mf3 = layout.FindElement("MF3") as MapFrame;
        await QueuedTask.Run(() => mf3.SetVisible(false));
        MapFrame mf4 = layout.FindElement("MF4") as MapFrame;
        await QueuedTask.Run(() => mf4.SetVisible(false));
        MapFrame mf5 = layout.FindElement("MF5") as MapFrame;
        await QueuedTask.Run(() => mf5.SetVisible(false));
        MapFrame mf6 = layout.FindElement("MF6") as MapFrame;
        await QueuedTask.Run(() => mf6.SetVisible(false));
        MapFrame mainMF = layout.FindElement("Main MF") as MapFrame;
        await QueuedTask.Run(() => mainMF.SetVisible(true));

        //Update title
        TextElement titleText = layout.FindElement("Title") as TextElement;
        TextProperties titleProp = titleText.TextProperties;
        titleProp.Text = "Not any more!";
        await QueuedTask.Run(() => titleText.SetTextProperties(titleProp));

        //New Game         
        TextElement instrText = layout.FindElement("Instructions") as TextElement;
        TextProperties instrProp = instrText.TextProperties;
        instrProp.Text = "<bol>Instructions: </bol> " +
                          "\n\n\n\n\n\n\n\n\n - Click the 'New Game' command if you want to play again.";
        await QueuedTask.Run(() => instrText.SetTextProperties(instrProp));

        //Zoomto finished puzzle area
        Coordinate2D ll = new Coordinate2D(3, 0);
        Coordinate2D ur = new Coordinate2D(14, 7.5);

        await QueuedTask.Run(() =>
        {
          Envelope env = EnvelopeBuilderEx.CreateEnvelope(ll, ur);
          layoutView.ZoomTo(env);
        });


        //Turn off selection changed events
        Globals.selEvents = false;

      }
    }
    /// <summary>
    /// Called by Framework when ArcGIS Pro is closing
    /// </summary>
    /// <returns>False to prevent Pro from closing, otherwise True</returns>
    protected override bool CanUnload()
    {
      //TODO - add your business logic
      //return false to ~cancel~ Application close
      return true;
    }

    #endregion Overrides

  }
}
