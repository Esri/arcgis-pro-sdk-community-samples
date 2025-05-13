/*

   Copyright 2025 Esri

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
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Presentations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PresentationsAddin
{
  internal class CreateElements : ArcGIS.Desktop.Framework.Contracts.Button
  {
    protected override void OnClick()
    {
      // Reference a page
      // A presentation view must be active
      var activePresentationView = PresentationView.Active;
      if (activePresentationView == null)
      {
        MessageBox.Show("No active presentation view found.");
        return;
      }
      var presentation = PresentationView.Active.Presentation;
      if (presentation == null)
      {
        MessageBox.Show("Presentation not found");
        return;
      }
      var page = activePresentationView.Presentation.GetPage(0);

      //Must be on QueuedTask
      QueuedTask.Run(() =>
      {
        //create a picture element

        var imgPath = @"https://www.esri.com/content/dam/esrisites/en-us/home/" +
         "homepage-tile-podcast-business-resilience-climate-change.jpg";
        //@"C:\Data\PresentationsContent\TheScienceofWhere2018.gif";

        //Build a geometry to place the picture
        Coordinate2D ll = new Coordinate2D(3.5, 1);
        Coordinate2D ur = new Coordinate2D(6, 5);
        Envelope env = EnvelopeBuilderEx.CreateEnvelope(ll, ur);
        //create a picture element on the page
        var gElement = PresentationElementFactory.Instance.CreatePictureGraphicElement(page, env, imgPath);

        //create a text element

        //Set symbology, create and add element to a presentation page
        CIMTextSymbol sym = SymbolFactory.Instance.ConstructTextSymbol(
                      ColorFactory.Instance.RedRGB, 15, "Arial", "Regular");
        //use ElementInfo to set placement properties
        var elemInfo = new ElementInfo()
        {
          Anchor = Anchor.CenterPoint,
          Rotation = 45
        };
        string textString = "My text";
        var textPos = new Coordinate2D(5, 3).ToMapPoint();
        var tElement = PresentationElementFactory.Instance.CreateTextGraphicElement(page,
          TextType.PointText, textPos, sym, textString, "textElement", false, elemInfo);

        //create a group element with elements created above
        var elmList = new List<Element> { gElement, tElement };
        GroupElement grp1 = PresentationElementFactory.Instance.CreateGroupElement(page, elmList, "Group");
      });
    }
  }
}
