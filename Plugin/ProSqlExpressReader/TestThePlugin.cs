/*

   Copyright 2020 Esri

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
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.PluginDatastore;
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

namespace ProSqlExpressReader
{
  internal class TestThePlugin : Button
  {
    protected override void OnClick()
    {
      if (MapView.Active?.Map == null)
      {
        MessageBox.Show("There is no active map");
        return;
      }
      try
      {
        //The browse filter is used in an OpenItemDialog.
        BrowseProjectFilter bf = new BrowseProjectFilter
        {
          //Name the filter
          Name = "SQL Express Connection File"
        };
        //Display the filter in an Open Item dialog
        OpenItemDialog aNewFilter = new OpenItemDialog
        {
          Title = "Open SQL Express Feature classes",
          InitialLocation = @"C:\Data\PluginData",
          MultiSelect = false,
          BrowseFilter = bf
        };
        bool? ok = aNewFilter.ShowDialog();
        if (!(ok.HasValue && ok.Value)) return;
        _ = AddToCurrentMap.AddProDataSubItemsAsync(aNewFilter.Items.OfType<ProDataSubItem>(), MapView.Active.Map);
      }
      catch (Exception ex)
      {
        MessageBox.Show($@"Exception: {ex.ToString()}");
      }
    }
  }
}
