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
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using QuakeItem.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuakeItem
{

  internal class QuakeItemDialog : Button
  {
    protected override void OnClick()
    {
      var bf = new BrowseProjectFilter("Add_QuakeItem_To_Project");

      //Display the filter in an Open Item dialog
      OpenItemDialog op = new OpenItemDialog
      {
        Title = "Add Quake Items",
        InitialLocation = @"E:\Data\SDK\Test\3.0",
        MultiSelect = false,
        BrowseFilter = bf
      };

      bool? ok = op.ShowDialog();
      if (ok != null)
      {
        if (ok.Value)
        {
          var item = op.Items[0] as QuakeProjectItem;
          if (item != null)
          {
            QueuedTask.Run(() =>
            {
              Project.Current.AddItem(item);
            });
          }
        }
      }
    }
  }
  internal class QuakeEventItemDialog : Button
  {
    protected override void OnClick()
    {
      var bf = new BrowseProjectFilter();
      //This allows us to view the .quake custom item (the "container")
                                               //This allows the .quake item to be browsable to access the events inside
      bf.AddCanBeTypeId("acme_quake_event");
                                             
      bf.Name = "Quake Event Item";

      var openItemDialog = new OpenItemDialog
      {
        Title = "Add Quake Event to Map",
        InitialLocation = @"E:\Data\CustomItem\QuakeCustomItem",
        BrowseFilter = bf,
        MultiSelect = false
      };
      bool? ok = openItemDialog.ShowDialog();
      if (ok != null)
      {
        if (ok.Value)
        {
          var quake_event = openItemDialog.Items.First() as QuakeEventCustomItem;
          QueuedTask.Run(() =>
          {
            Module1.AddToGraphicsLayer(quake_event);
          });
          
        }
      }
    }
  }
}
