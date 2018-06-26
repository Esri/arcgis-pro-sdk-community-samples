/*

   Copyright 2018 Esri

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

//added references
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using System.Media;
using System.Windows.Threading;

namespace Puzzle_2_1
{
  internal class ScramblePieces : Button
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
      LayoutView layoutView = LayoutView.Active;

      //Prevent tool from executing based on some conditions
      if (layoutView == null)
      {
        System.Windows.MessageBox.Show("Can't find layout view, try clicking New Game Board");
        return;
      }

      if (layoutView.Layout.Name != "Game Board")
      {
        System.Windows.MessageBox.Show("Wrong layout view: should be Game Board");
        return;
      }

      await QueuedTask.Run(() =>
      {
        //Reference the layout for moving map frames and adding new layout elements
        Layout layout = layoutView.Layout;

        //Check to see if elements were already scrambled
        MapFrame mfTest = layout.FindElement("MF1") as MapFrame;
        if (mfTest != null)
        {
          System.Windows.MessageBox.Show("Game pieces already scrambled");
          return;
        }

        //Build 6 envelopes to represent the locations of puzzle pieces along the outer edges
        Coordinate2D env1_ll = new Coordinate2D(0.5, 0.5);
        Coordinate2D env1_ur = new Coordinate2D(3.5, 3.5);
        Envelope env1 = EnvelopeBuilder.CreateEnvelope(env1_ll, env1_ur);

        Coordinate2D env2_ll = new Coordinate2D(0.5, 4);
        Coordinate2D env2_ur = new Coordinate2D(3.5, 7);
        Envelope env2 = EnvelopeBuilder.CreateEnvelope(env2_ll, env2_ur);

        Coordinate2D env3_ll = new Coordinate2D(0.5, 7.5);
        Coordinate2D env3_ur = new Coordinate2D(3.5, 10.5);
        Envelope env3 = EnvelopeBuilder.CreateEnvelope(env3_ll, env3_ur);

        Coordinate2D env4_ll = new Coordinate2D(13.5, 0.5);
        Coordinate2D env4_ur = new Coordinate2D(16.5, 3.5);
        Envelope env4 = EnvelopeBuilder.CreateEnvelope(env4_ll, env4_ur);

        Coordinate2D env5_ll = new Coordinate2D(13.5, 4);
        Coordinate2D env5_ur = new Coordinate2D(16.5, 7);
        Envelope env5 = EnvelopeBuilder.CreateEnvelope(env5_ll, env5_ur);

        Coordinate2D env6_ll = new Coordinate2D(13.5, 7.5);
        Coordinate2D env6_ur = new Coordinate2D(16.5, 10.5);
        Envelope env6 = EnvelopeBuilder.CreateEnvelope(env6_ll, env6_ur);

        //Randomize the envelopes by assigning new envelope variables used for map frame creation
        //Also remove the assigned env before selecting the next random env
        List<Envelope> envList = new List<Envelope> { env1, env2, env3, env4, env5, env6 };

        Random r1 = new Random();
        int i1 = r1.Next(envList.Count);
        Envelope e1 = envList[i1];
        envList.Remove(e1);   

        Random r2 = new Random();
        int i2 = r2.Next(envList.Count);
        Envelope e2 = envList[i2];
        envList.Remove(e2);

        Random r3 = new Random();
        int i3 = r3.Next(envList.Count);
        Envelope e3 = envList[i3];
        envList.Remove(e3);

        Random r4 = new Random();
        int i4 = r4.Next(envList.Count);
        Envelope e4 = envList[i4];
        envList.Remove(e4);

        Random r5 = new Random();
        int i5 = r5.Next(envList.Count);
        Envelope e5 = envList[i5];
        envList.Remove(e5);

        Random r6 = new Random();
        int i6 = r6.Next(envList.Count);
        Envelope e6 = envList[i6];
        envList.Remove(e6);

        //Reference the active map view and gets its center location
                       //MapView map = MapView.Active;
        MapFrame mapFrame = layout.FindElement("Main MF") as MapFrame;
        Camera cam = mapFrame.Camera;
        double x = cam.X;
        double y = cam.Y;
        double scale = cam.Scale;
        double delta = scale * 1.5 / 12 / 3.28084;  //scale * 1/2 of a 3" MF / 12" per foot / 3.28084 feet per meter

        //Insert Map Frame 1 at random location
        MapFrame mf1Elm = LayoutElementFactory.Instance.CreateMapFrame(layout, e1, mapFrame.Map);
        mf1Elm.SetName("MF1");
        Camera mf1cam = mf1Elm.Camera;
        mf1cam.X = x - (delta * 2);
        mf1cam.Y = y - delta;
        mf1cam.Scale = scale;
        mf1Elm.SetCamera(mf1cam);

        //Insert Map Frame 2 at random location
        MapFrame mf2Elm = LayoutElementFactory.Instance.CreateMapFrame(layout, e2, mapFrame.Map);
        mf2Elm.SetName("MF2");  
        Camera mf2cam = mf2Elm.Camera;
        mf2cam.X = x;
        mf2cam.Y = y - delta;
        mf2cam.Scale = scale;
        mf2Elm.SetCamera(mf2cam);

        //Insert Map Frame 3 at random location
        MapFrame mf3Elm = LayoutElementFactory.Instance.CreateMapFrame(layout, e3, mapFrame.Map);
        mf3Elm.SetName("MF3");
        Camera mf3cam = mf3Elm.Camera;
        mf3cam.X = x + (delta * 2);
        mf3cam.Y = y - delta;
        mf3cam.Scale = scale;
        mf3Elm.SetCamera(mf3cam);

        //Insert Map Frame 4 at random location

        MapFrame mf4Elm = LayoutElementFactory.Instance.CreateMapFrame(layout, e4, mapFrame.Map);
        mf4Elm.SetName("MF4");
        Camera mf4cam = mf4Elm.Camera;
        mf4cam.X = x + (delta * 2);
        mf4cam.Y = y + delta;
        mf4cam.Scale = scale;
        mf4Elm.SetCamera(mf4cam);

        //Insert Map Frame 5 at random location
        MapFrame mf5Elm = LayoutElementFactory.Instance.CreateMapFrame(layout, e5, mapFrame.Map);
        mf5Elm.SetName("MF5");
        Camera mf5cam = mf5Elm.Camera;
        mf5cam.X = x;
        mf5cam.Y = y + delta;
        mf5cam.Scale = scale;
        mf5Elm.SetCamera(mf5cam);

        //Insert Map Frame 6 at random location
        MapFrame mf6Elm = LayoutElementFactory.Instance.CreateMapFrame(layout, e6, mapFrame.Map);
        mf6Elm.SetName("MF6");
        Camera mf6cam = mf6Elm.Camera;
        mf6cam.X = x - (delta * 2);
        mf6cam.Y = y + delta;
        mf6cam.Scale = scale;
        mf6Elm.SetCamera(mf6cam);

        //Create 6 polygon boxes that represent where the outer map frames need to be placed.
        Coordinate2D box1_ll = new Coordinate2D(4, 0.5);
        Coordinate2D box1_ur = new Coordinate2D(7, 3.5);
        Envelope box1_env = EnvelopeBuilder.CreateEnvelope(box1_ll, box1_ur);
        GraphicElement box1 = LayoutElementFactory.Instance.CreateRectangleGraphicElement(layout, box1_env);
        box1.SetName("Rectangle 1");        
        
        Coordinate2D box2_ll = new Coordinate2D(7, 0.5);
        Coordinate2D box2_ur = new Coordinate2D(10, 3.5);
        Envelope box2_env = EnvelopeBuilder.CreateEnvelope(box2_ll, box2_ur);
        GraphicElement box2 = LayoutElementFactory.Instance.CreateRectangleGraphicElement(layout, box2_env);
        box2.SetName("Rectangle 2");

        Coordinate2D box3_ll = new Coordinate2D(10, 0.5);
        Coordinate2D box3_ur = new Coordinate2D(13, 3.5);
        Envelope box3_env = EnvelopeBuilder.CreateEnvelope(box3_ll, box3_ur);
        GraphicElement box3 = LayoutElementFactory.Instance.CreateRectangleGraphicElement(layout, box3_env);
        box3.SetName("Rectangle 3");

        Coordinate2D box4_ll = new Coordinate2D(10, 3.5);
        Coordinate2D box4_ur = new Coordinate2D(13, 6.5);
        Envelope box4_env = EnvelopeBuilder.CreateEnvelope(box4_ll, box4_ur);
        GraphicElement box4 = LayoutElementFactory.Instance.CreateRectangleGraphicElement(layout, box4_env);
        box4.SetName("Rectangle 4");

        Coordinate2D box5_ll = new Coordinate2D(7, 3.5);
        Coordinate2D box5_ur = new Coordinate2D(10, 6.5);
        Envelope box5_env = EnvelopeBuilder.CreateEnvelope(box5_ll, box5_ur);
        GraphicElement box5 = LayoutElementFactory.Instance.CreateRectangleGraphicElement(layout, box5_env);
        box5.SetName("Rectangle 5");

        Coordinate2D box6_ll = new Coordinate2D(4, 3.5);
        Coordinate2D box6_ur = new Coordinate2D(7, 6.5);
        Envelope box6_env = EnvelopeBuilder.CreateEnvelope(box6_ll, box6_ur);
        GraphicElement box6 = LayoutElementFactory.Instance.CreateRectangleGraphicElement(layout, box6_env);
        box6.SetName("Rectangle 6");

        //Find MainMF and set invisible
        MapFrame mainMF = layout.FindElement("Main MF") as MapFrame;
        mainMF.SetVisible(false);

        //Update Instructions
        TextElement steps = layout.FindElement("Instructions") as TextElement;
        string text = "<bol>Instructions: </bol>\n\n  - Click the 'Play Game' command" as String;
        var stepsProp = steps.TextProperties;
        stepsProp.Text = text;
        steps.SetTextProperties(stepsProp);
      });

      layoutView.ClearElementSelection();
    }
  }
}
