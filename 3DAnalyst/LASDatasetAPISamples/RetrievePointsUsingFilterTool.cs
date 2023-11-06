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
using ArcGIS.Core.Data.Analyst3D;
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LASDatasetAPISamples
{
  internal class RetrievePointsUsingFilterTool : MapTool
  {
    private FeatureLayer _pointsLayer;
    private LasDatasetLayer _lasLayer;
    private LASDatasetDockpaneViewModel _vm;

    public RetrievePointsUsingFilterTool()
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Rectangle;
      SketchOutputMode = SketchOutputMode.Map;
      _vm = FrameworkApplication.DockPaneManager.Find("LASDatasetAPISamples_LASDatasetDockpane") as LASDatasetDockpaneViewModel;

      if (_vm != null)
        _lasLayer = _vm.SelectedLASLayer;
    }

    protected override Task OnToolActivateAsync(bool active)
    {
      _pointsLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(s => s.Name == "PointsFromLASLayer");
      if (_pointsLayer == null)
        MessageBox.Show("PointsFromLASLayer layer not found");
      if (_lasLayer == null)
        MessageBox.Show("LASDataset layer not found");
      
      return base.OnToolActivateAsync(active);
    }

    protected async override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      LasPointFilter lasPointFilter = new LasPointFilter();
      lasPointFilter.FilterGeometry = geometry as Polygon;
      lasPointFilter.Returns = _vm.GetLasReturnTypes();
      lasPointFilter.ClassCodes = _vm.GetLasClassCodes();
      lasPointFilter.KeyPoints = _vm.IsKeyPointChecked;
      lasPointFilter.OverlapPoints = _vm.IsOverlapPointsChecked;
      lasPointFilter.SyntheticPoints = _vm.IsSyntheticPointsChecked;
      lasPointFilter.WithheldPoints = _vm.IsWithheldPointsChecked;
      await QueuedTask.Run(() => {
        if (_lasLayer != null)
        {
          var cursor = _lasLayer.SearchPoints(lasPointFilter);
          Dictionary<string, object> keyValuePairs = new Dictionary<string, object>();
          var editOperation = new EditOperation();
          editOperation.Name = "Create Points from LAS Dataset";
          int i = 0;
          while (cursor.MoveNext())
          {
            i++;
            using (var lasPoint = cursor.Current)
            {
              keyValuePairs["SHAPE"] = lasPoint.ToMapPoint();
              keyValuePairs["PointID"] = lasPoint.PointID;
              keyValuePairs["ClassCode"] = lasPoint.ClassCode.ToString();
              keyValuePairs["ReturnNumber"] = lasPoint.ReturnNumber.ToString();
              keyValuePairs["Intensity"] = lasPoint.Intensity.ToString();
              keyValuePairs["NumberOfReturns"] = lasPoint.NumberOfReturns.ToString();
              if (lasPoint.IsKeyPoint)
                keyValuePairs["KeyPoint"] = "1";
              else
                keyValuePairs["KeyPoint"] = "0";
              if (lasPoint.IsOverlapPoint)
                keyValuePairs["OverlapPoint"] = "1";
              else
                keyValuePairs["OverlapPoint"] = "0";
              if (lasPoint.IsSyntheticPoint)
                keyValuePairs["SyntheticPoint"] = "1";
              else
                keyValuePairs["SyntheticPoint"] = "0";
              if (lasPoint.IsWithheld)
                keyValuePairs["WithheldPoint"] = "1";
              else
                keyValuePairs["WithheldPoint"] = "0";
              editOperation.Create(_pointsLayer, keyValuePairs);
            }
          }
          System.Diagnostics.Debug.WriteLine($"No of Points found: {i}");
          if (!editOperation.IsEmpty)
            editOperation.Execute();
        }
      });
      return true;
    }
  }
}
