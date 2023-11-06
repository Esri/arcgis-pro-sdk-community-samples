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
using ArcGIS.Core.Data.DDL;
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

namespace MemoryGeodatabase
{
  internal class AddRecords : Button
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
        var timer = new Stopwatch();
        timer.Start();
        await QueuedTask.Run(() =>
        {
          CreateRecordsFeatureClass(fileGdbPath, Module1.TestFcName, Module1.Cycles, camera);
        });
        timer.Stop();
        TimeSpan timeTaken = timer.Elapsed;
        if (Module1.MemoryGDBStatsViewModel != null)
          Module1.MemoryGDBStatsViewModel.FilePerformance = timeTaken.ToString(@"m\:ss\.fff");

        timer.Restart();
        await QueuedTask.Run(() =>
        {
          CreateRecordsFeatureClass(memoryCPs, Module1.TestFcName, Module1.Cycles, camera);
        }); 
        timer.Stop();
        timeTaken = timer.Elapsed;
        if (Module1.MemoryGDBStatsViewModel != null)
          Module1.MemoryGDBStatsViewModel.MemoryPerformance = timeTaken.ToString(@"m\:ss\.fff");

        var mCount = await QueuedTask.Run<long>(() =>
        {
          return Module1.GetRecordCountFeatureClass(memoryCPs, Module1.TestFcName);
        }); 
        var fCount = await QueuedTask.Run<long>(() =>
        {
          return Module1.GetRecordCountFeatureClass(fileGdbPath, Module1.TestFcName);
        });
        if (Module1.MemoryGDBStatsViewModel != null)
        {
          Module1.MemoryGDBStatsViewModel.MemoryCount = mCount.ToString();
          Module1.MemoryGDBStatsViewModel.FileCount = fCount.ToString();
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show($@"Exception in [{state}]: {ex}");
      }
    }

    #region Create records in FC

    private static void CreateRecordsFeatureClass(MemoryConnectionProperties memoryCPs,
      string fcName, uint cycles, Camera camera)
    {
      using var geoDb = new Geodatabase(memoryCPs);
      var fc = geoDb.OpenDataset<FeatureClass>(fcName);
      CreateRecordsFeatureClass(geoDb, fc, cycles, camera);
    }

    private static void CreateRecordsFeatureClass(FileGeodatabaseConnectionPath connectionPath,
      string fcName, uint cycles, Camera camera)
    {
      using var geoDb = new Geodatabase(connectionPath);
      var fc = geoDb.OpenDataset<FeatureClass>(fcName);
      CreateRecordsFeatureClass(geoDb, fc, cycles, camera);
    }

    private static void CreateRecordsFeatureClass(Geodatabase geoDb, 
        FeatureClass fc, uint cycles, Camera camera)
    {
      geoDb.ApplyEdits(() =>
      {
        for (var iRecord = 0; iRecord < Module1.Cycles; iRecord++)
        {
          var newRowBuffer = fc.CreateRowBuffer();
          newRowBuffer["Shape"] = MapPointBuilderEx.CreateMapPoint(camera.X+10*iRecord, 
                    camera.Y+10*iRecord, camera.SpatialReference);
          newRowBuffer["TheString"] = iRecord.ToString();
          newRowBuffer["TheInteger"] = iRecord;
          newRowBuffer["TheDouble"] = Convert.ToDouble(iRecord);
          newRowBuffer["TheDate"] = DateTime.Now;
          fc.CreateRow(newRowBuffer);
        }
      });
    }

    #endregion Create records in FC

  }
}
