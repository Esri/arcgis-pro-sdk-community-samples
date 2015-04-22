//   Copyright 2015 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace MappingSampleAddIns.AddLayer
{
  /// <summary>
  /// Represents a button that appears on the ribbon that, when clicked,
  /// brings up a dialog to allow you add a layer using a path or a url
  /// </summary>
  /// <remarks>
  /// Few examples:
  /// FeatureClass inside a FileGeodatabase:        C:\Data\MyFileGDB.gdb\Census
  /// A shape file inside a folder:                 \\Machine\SharedFolder\Census.shp
  /// Raster Dataset inside a FileGeodatabase:      C:\Data\MyFileGDB.gdb\DEM
  /// An image file inside a folder:                \\Machine\SharedFolder\Imagery.tif
  /// A map service layer:                          http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Census_USA/MapServer
  /// A feature layer off a map or feature service: http://sampleserver6.arcgisonline.com/arcgis/rest/services/NapervilleShelters/FeatureServer/0
  /// A .lprx or .lpkx file:                        \\Machine\SharedFolder\Fires.lyrx 
  /// </remarks>
  internal class AddLayer : Button
  {
    /// <summary>
    /// Brings up a WPF dialog to allow you enter a path or url to a layer
    /// </summary>
    protected override void OnClick()
    {
      AddLayerDlg addlayerDlg = new AddLayerDlg()
          {
            Width = 650,
            Height = 175
          };

      addlayerDlg.ShowDialog();
    }
  }
}
