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
  internal class SelectVisibleElements : ArcGIS.Desktop.Framework.Contracts.Button
  {
    protected override void OnClick()
    {
      // reference current active page
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
      PresentationPage activePage = PresentationView.Active.ActivePage;
      if (activePage == null)
      {
        MessageBox.Show("No active page found.");
        return;
      }

      //Must be on QueuedTask
      QueuedTask.Run(() =>
      {
        // Find specific elements by name
        var ge_rect = activePage.FindElement("Rectangle") as GraphicElement;
        var elements = new List<string>();
        elements.Add("Text");
        elements.Add("Polygon");
        //Get elements retaining hierarchy
        var top_level_elems = activePage.GetElements();

        //Flatten hierarchy
        var all_elems = activePage.GetFlattenedElements();

        //Use LINQ with any of the collections
        //Retrieve just those elements that are Visible
        var some_elems = all_elems.Where(ge => ge.IsVisible).ToList();
        MessageBox.Show($"{some_elems.Count} elements are visible");
      });
    }
  }
}
