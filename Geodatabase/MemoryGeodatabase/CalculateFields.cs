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
using System.Windows.Media.Media3D;
using Camera = ArcGIS.Desktop.Mapping.Camera;

namespace MemoryGeodatabase
{
  internal class CalculateFields : Button
  {
    protected override async void OnClick()
    {
      var camera = MapView.Active?.Camera;
      if (camera == null)
      {
        MessageBox.Show("Select an active map view");
        return;
      }
      var fileGdbPath = new FileGeodatabaseConnectionPath(new Uri(Project.Current.DefaultGeodatabasePath));
      var memoryCPs = new MemoryConnectionProperties("memory");
      var state = "Create File GDB";
      try
      {
        var mCount = await QueuedTask.Run<long>(() =>
        {
          return Module1.GetRecordCountFeatureClass(memoryCPs, Module1.TestFcName);
        });
        var fCount = await QueuedTask.Run<long>(() =>
        {
          return Module1.GetRecordCountFeatureClass(fileGdbPath, Module1.TestFcName);
        });
        var timer = new Stopwatch();
        timer.Start();
        await QueuedTask.Run(() =>
        {
          UpdateRecordsInFeatureClass(fileGdbPath, Module1.TestFcName, fCount, camera);
        });
        timer.Stop();
        TimeSpan timeTaken = timer.Elapsed;
        if (Module1.MemoryGDBStatsViewModel != null)
          Module1.MemoryGDBStatsViewModel.FilePerformance = timeTaken.ToString(@"m\:ss\.fff");

        timer.Restart();
        await QueuedTask.Run(() =>
        {
          UpdateRecordsInFeatureClass(memoryCPs, Module1.TestFcName, mCount, camera);
        });
        timer.Stop();
        timeTaken = timer.Elapsed;
        if (Module1.MemoryGDBStatsViewModel != null)
          Module1.MemoryGDBStatsViewModel.MemoryPerformance = timeTaken.ToString(@"m\:ss\.fff");
      }
      catch (Exception ex)
      {
        MessageBox.Show($@"Exception in [{state}]: {ex}");
      }
    }

    #region Update all records in FC

    private static void UpdateRecordsInFeatureClass(MemoryConnectionProperties memoryCPs,
      string fcName, long recordCount, Camera camera)
    {
      using var geoDb = new Geodatabase(memoryCPs);
      var fc = geoDb.OpenDataset<FeatureClass>(fcName);
      UpdateRecordsInFeatureClass(geoDb, fc, recordCount, camera);
    }

    private static void UpdateRecordsInFeatureClass(FileGeodatabaseConnectionPath connectionPath,
      string fcName, long recordCount, Camera camera)
    {
      using var geoDb = new Geodatabase(connectionPath);
      var fc = geoDb.OpenDataset<FeatureClass>(fcName);
      UpdateRecordsInFeatureClass(geoDb, fc, recordCount, camera);
    }

    private static void UpdateRecordsInFeatureClass(Geodatabase geoDb,
        FeatureClass fc, long recordCount, Camera camera)
    {
      var rnd = new Random();
      geoDb.ApplyEdits(() =>
      {
        using var featureCursor = fc.Search(null, false);
        while (featureCursor.MoveNext())
        {
          using var feature = featureCursor.Current;
          var iRecord = feature.GetObjectID();
          iRecord += rnd.Next(20);

          var xoffset = rnd.NextInt64(-recordCount, recordCount) * 10;
          var yoffset = rnd.NextInt64(-recordCount, recordCount) * 10;
          feature["Shape"] = MapPointBuilderEx.CreateMapPoint(camera.X + xoffset,
          camera.Y + yoffset, camera.SpatialReference);

          feature["TheString"] = iRecord.ToString();
          feature["TheInteger"] = iRecord;
          feature["TheDouble"] = Convert.ToDouble(iRecord);
          feature["TheDate"] = DateTime.Now;
          feature.Store();
        }
      });
    }

    #endregion Update all records in FC

  }
}
