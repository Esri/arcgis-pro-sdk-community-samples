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
  internal class ExportGeometries : Button
  {
    protected override void OnClick()
    {
      try
      {
        if (Module1.Geometries == null || Module1.Geometries.Count <= 0)
        {
          MessageBox.Show($@"You have to first render a geometry before you can export the Geometry");
          return;
        }
        var bpf = new BrowseProjectFilter("esri_browseDialogFilters_json_file")
        {
          Name = "Specify JSON file to export Geometries to"
        };
        var saveItemDialog = new SaveItemDialog { BrowseFilter = bpf };
        var result = saveItemDialog.ShowDialog();
        if (result.Value == false) return;
        var jsonPath = $@"{saveItemDialog.FilePath}.json";
        var folder = System.IO.Path.GetDirectoryName(jsonPath);
        if (!System.IO.Directory.Exists(folder)) System.IO.Directory.CreateDirectory(folder);
        var exists = System.IO.File.Exists(jsonPath);
        if (exists)
        {
          var isYes = MessageBox.Show($@"The export will write over the existing file {jsonPath}", "Override File", System.Windows.MessageBoxButton.YesNo);
          if (isYes != System.Windows.MessageBoxResult.Yes) return;
          System.IO.File.Delete(jsonPath);
        }
        GeometryBag bag = GeometryBagBuilder.CreateGeometryBag(Module1.Geometries, 
                                                                Module1.Geometries[0].SpatialReference);
        System.IO.File.WriteAllText(jsonPath, bag.ToJson());
        MessageBox.Show($@"Export saved to {jsonPath}");
      }
      catch (Exception ex)
      {
        MessageBox.Show($@"Export Exception: {ex}");
      }
    }
  }
}
