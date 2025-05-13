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
  internal class AddMapPage : Button
  {
    protected override void OnClick()
    {
      var presentation = PresentationView.Active.Presentation;
      if (presentation == null)
      {
        MessageBox.Show("Presentation not found");
        return;
      }
      //Must be on QueuedTask
      QueuedTask.Run(() =>
      {
        // retrieve a map from the project based on the map name
        MapProjectItem mpi = Project.Current.GetItems<MapProjectItem>()
                                   .FirstOrDefault(m => m.Name.Equals("Map", StringComparison.CurrentCultureIgnoreCase));
        Map map = mpi.GetMap();
        //create a map page using map's default extent
        presentation.AddMapPage(map, -1);

        //create a page using map's bookmark
        Bookmark bookmark = map.GetBookmarks().FirstOrDefault(
                     b => b.Name == "Esri"); // get the bookmark based on the bookmark's name
        if (bookmark == null)
        {
          MessageBox.Show("Bookmark not found");
          return;
        }
        presentation.AddMapPage(bookmark, -1);

      });
    }
  }
}
