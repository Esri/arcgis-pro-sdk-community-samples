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
  internal class InteractWithMap : Button
  {
    protected override void OnClick()
    {
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
      PresentationPage activePage = activePresentationView.ActivePage;

      //check if the current page is a map page
      //Note: we are on the UI thread!
      if (activePage is MapPresentationPage)
      {
        activePresentationView.ActivateMapPageAsync();
      }

      //move to the QueuedTask to do something
      QueuedTask.Run(() => {
        // Get the active map page
        var mapPage = activePage as MapPresentationPage;
        if (mapPage == null)
        {
          MessageBox.Show("The active page is not a map page.");
          return;
        }

        //Reference map and layer
        MapProjectItem mp = Project.Current.FindItem(mapPage.MapURI) as MapProjectItem;
        Map map = mp.GetMap();
        if (map == null)
        {
          MessageBox.Show("Map not found in the active page.");
          return;
        }

        MessageBox.Show("You can now interact with the map inside the presentation page");
        // Zoom to the full extent of the map
        //var extent = map.GetDefaultExtent();
        //mapPage.SetCamera(extent);

        // Optionally, you can also zoom to a specific extent
        // var extent = new EnvelopeBuilder(map.Extent).Expand(1.5).ToGeometry();
        // map.ZoomTo(extent);
      });
    }
  }
}
