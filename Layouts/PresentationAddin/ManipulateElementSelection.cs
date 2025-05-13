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

namespace PresentationsAddin
{
  internal class ManipulateElementSelection : Button
  {
    protected override void OnClick()
    {
      // reference current active presentation view
      var activeView = PresentationView.Active;
      if (activeView == null)
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
        //Select/unselect some elements...
        var elems = activePage.GetFlattenedElements();
        //select any element not a group element
        //activePage.SelectElements(elems.Where(e => !e.Name.StartsWith("Group")));
        //activePage.UnSelectElements(elems.Where(e => !e.Name.StartsWith("Group")));

        //Select/unselect all visible, graphic elements
        var ge_elems = elems.Where(ge => ge.IsVisible).ToList();
        activePage.SelectElements(ge_elems);
        activePage.UnSelectElements(ge_elems);

        //Select/unselect a specific element
        var na = activePage.FindElement("My Text Element");
        activePage.SelectElement(na);
        activePage.UnSelectElement(na);

        //Select everything
        activePage.SelectElements(elems);

        //enumerate the selected elements
        foreach (var sel_elem in activeView.GetSelectedElements())
        {
          //TODO
          MessageBox.Show($"Selected Element: {sel_elem.Name}, Type: {sel_elem.GetType().Name}");
        }
      });
    }
  }
}