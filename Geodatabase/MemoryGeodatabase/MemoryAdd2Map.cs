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
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.CIM;
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

namespace MemoryGeodatabase
{
  internal class MemoryAdd2Map : Button
  {
    protected override async void OnClick()
    {
      if (MapView.Active?.Map == null) return;

      var fileGdbPath = new FileGeodatabaseConnectionPath(new Uri(Project.Current.DefaultGeodatabasePath));
      var memoryCPs = new MemoryConnectionProperties("memory");
      var state = "Add Memory GDB to Map";
      try
      {
        await QueuedTask.Run(() =>
        {
          using var geoDb = new Geodatabase(memoryCPs);
          var fc = geoDb.OpenDataset<FeatureClass>(Module1.TestFcName);
          var flyrCreateParam = new FeatureLayerCreationParams(fc)
          {
            Name = "Memory GDB points",
          };
          var featureLayer = LayerFactory.Instance.CreateLayer<FeatureLayer>(
            flyrCreateParam, MapView.Active?.Map);
        });
        state = "Add File GDB to Map";
        await QueuedTask.Run(() =>
        {
          using var geoDb = new Geodatabase(fileGdbPath);
          var fc = geoDb.OpenDataset<FeatureClass>(Module1.TestFcName);
          var flyrCreateParam = new FeatureLayerCreationParams(fc)
          {
            Name = "File GDB points",
          };
          var featureLayer = LayerFactory.Instance.CreateLayer<FeatureLayer>(
            flyrCreateParam, MapView.Active?.Map);
        });
      }
      catch (Exception ex)
      {
        MessageBox.Show($@"Exception in [{state}]: {ex}");
      }
    }

  }
}
