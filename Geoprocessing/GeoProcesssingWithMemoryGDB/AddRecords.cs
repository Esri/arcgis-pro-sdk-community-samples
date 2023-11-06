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

namespace GeoProcesssingWithMemoryGDB
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
      Module1.OpenStatsDockpane();
      var memoryCPs = new MemoryConnectionProperties("memory");
      var state = "Add records to Memory GDB";
      try
      {
        var timer = new Stopwatch();
        timer.Start();
        await QueuedTask.Run(() =>
        {
          CreateRecordsFeatureClass(memoryCPs, Module1.TestFcName, camera);
        }); 
        timer.Stop();
        var timeTaken = timer.Elapsed;
        if (Module1.MemoryGDBStatsViewModel != null)
          Module1.MemoryGDBStatsViewModel.MemoryPerformance = timeTaken.ToString(@"m\:ss\.fff");

        var mCount = await QueuedTask.Run<long>(() =>
        {
          return Module1.GetRecordCountFeatureClass(memoryCPs, Module1.TestFcName);
        });
        if (Module1.MemoryGDBStatsViewModel != null)
        {
          Module1.MemoryGDBStatsViewModel.MemoryCount = mCount.ToString();
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show($@"Exception in [{state}]: {ex}");
      }
    }

    #region Check if FC exists

    private static bool DoesFeatureClassExist(MemoryConnectionProperties memoryCPs,
            string fcName)
    {
      bool result = false;
      try
      {
        using var geoDb = new Geodatabase(memoryCPs);
        var fc = geoDb.OpenDataset<FeatureClass>(fcName);
        result = fc != null;
      }
      catch {
        result = false;
      }
      return result;
    }

    #endregion

    #region Create records in FC

    private static void CreateRecordsFeatureClass(MemoryConnectionProperties memoryCPs,
      string fcName, Camera camera)
    {
      using var geoDb = new Geodatabase(memoryCPs);
      var fc = geoDb.OpenDataset<FeatureClass>(fcName);
      CreateRecordsFeatureClass(geoDb, fc, camera);
    }

    private static void CreateRecordsFeatureClass(FileGeodatabaseConnectionPath connectionPath,
      string fcName, Camera camera)
    {
      using var geoDb = new Geodatabase(connectionPath);
      var fc = geoDb.OpenDataset<FeatureClass>(fcName);
      CreateRecordsFeatureClass(geoDb, fc, camera);
    }

    private static void CreateRecordsFeatureClass(Geodatabase geoDb, 
        FeatureClass fc, Camera camera)
    {
      geoDb.ApplyEdits(() =>
      {
        for (var iRecord = 0; iRecord < 20; iRecord++)
        {
          double radianMultiply = 0.1 * iRecord;
          var iGroup = iRecord % 4;
          var newRowBuffer = fc.CreateRowBuffer();
          newRowBuffer["Shape"] = GetEllipse(camera, radianMultiply);
          newRowBuffer["TheString"] = iGroup.ToString();
          newRowBuffer["TheInteger"] = iGroup;
          newRowBuffer["TheDouble"] = Convert.ToDouble(iGroup);
          newRowBuffer["TheDate"] = DateTime.Now;
          fc.CreateRow(newRowBuffer);
        }
      });
    }

    private static ArcGIS.Core.Geometry.Polygon GetEllipse (Camera camera, double radianMultiply)
    {
      var centerPnt = MapPointBuilderEx.CreateMapPoint(camera.X, camera.Y, camera.SpatialReference);
      var centerCoord = centerPnt.Coordinate2D;

      // Construct an ellipse centered at (1, 2) with rotationAngle = -pi/6,  
      // semiMajorAxis = scale*4, minorMajorRatio = 0.2, oriented clockwise.
      // Use a builderEx convenience method or use a builderEx constructor.

      // BuilderEx convenience methods don't need to run on the MCT.
      EllipticArcSegment ellipse = EllipticArcBuilderEx.CreateEllipse(centerCoord, 
          -1 * Math.PI * radianMultiply,
          camera.Scale/30.0, 0.2, ArcOrientation.ArcClockwise,
          camera.SpatialReference);
      var polyLine = PolylineBuilderEx.CreatePolyline(ellipse);
      return PolygonBuilderEx.CreatePolygon(polyLine);
    }

    #endregion Create records in FC

  }
}
