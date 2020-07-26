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

namespace OverlayGroundSurface
{
  internal class ImportGeometries : Button
  {
    protected override void OnClick()
    {
      try
      {
        var bpf = new BrowseProjectFilter("esri_browseDialogFilters_json_file")
        {
          Name = "Select JSON file containing exported Geometries"
        };
        var openItemDialog = new OpenItemDialog { BrowseFilter = bpf };
        var result = openItemDialog.ShowDialog();
        if (result.Value == false || openItemDialog.Items.Count() == 0) return;
        var item = openItemDialog.Items.ToArray()[0];
        var filePath = item.Path;
        var json = System.IO.File.ReadAllText(filePath);
        var geometryBag = GeometryBagBuilder.FromJson(json);
        Module1.Geometries = geometryBag.Geometries.ToList();
        Module1.HasImportData = true;
        MessageBox.Show($@"Geometries loaded: {Module1.Geometries.Count()}");
      }
      catch (Exception ex)
      {
        MessageBox.Show($@"Import exception: {ex}");
      }
    }
  }
}
