/*

   Copyright 2023 Esri

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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableFromCsv
{
  internal class CreateTableFromCsv : Button
  {
    protected override async void OnClick()
    {
      var tbl = await CSVtoTable(@"C:\Data\SimplePointPlugin\SimplePointData\Meteorites_UK.csv");
      MessageBox.Show(tbl == null ? "Failed to create table from CSV" : tbl.Name);
    }

    public async Task<StandaloneTable> CSVtoTable(string csvPath)
    {
      StandaloneTable tbl = null;
      try
      {
        var container = MapView.Active.Map;
        await QueuedTask.Run(() =>
        {
          string tblName = System.IO.Path.GetFileName(csvPath);
          tbl = StandaloneTableFactory.Instance.CreateStandaloneTable(new Uri(csvPath, UriKind.Absolute), container);
        });
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.ToString());
        return null;
      }
      return tbl;

    }
  }
}
