/*

   Copyright 2018 Esri

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
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;

namespace LayoutMapSeries.Helpers
{
  internal class MapSeriesHelper {

    /// <summary>
    /// In case you want to set it directly...
    /// </summary>
    public double MapToPageConversion = 1.0;

    /// <summary>
    /// Initialize to set up the MapToPage scale conversion factor
    /// </summary>
    /// <param name="layout"></param>
    /// <param name="mapSeriesLayer"></param>
    /// <returns></returns>
    public Task Initialize(Layout layout, FeatureLayer mapSeriesLayer) {
      return QueuedTask.Run(() => {
        var pageUnits = layout.GetPage().Units;
        var sr = mapSeriesLayer.GetSpatialReference();
        if (sr != null && sr.IsProjected)
          MapToPageConversion = ((LinearUnit)sr.Unit).MetersPerUnit / pageUnits.MetersPerUnit;
      });
    }

    /// <summary>
    /// Set the layout to the specified page id in the map series
    /// </summary>
    /// <param name="layout">layout where to change current page id</param>
    /// <param name="pageObjectId">object id field for MapSeries feature class</param>
    /// <returns></returns>
    public Task SetCurrentPageAsync(Layout layout, long pageObjectId, int Id) {
      return QueuedTask.Run(() => {
        
          // new version
          if (pageObjectId < 0 && Id < 0) return;

          var cimLayout = layout.GetDefinition();
          var mapSeries = cimLayout.MapSeries as CIMSpatialMapSeries;
          if (mapSeries.CurrentPageID == Id) return;
          mapSeries.CurrentPageID = Id;
          layout.SetDefinition(cimLayout);
        
      });
    }

    private double LongestPageSide(MapFrame mf) {
      double wd = mf.GetWidth();
      double ht = mf.GetHeight();
      return wd > ht ? wd : ht;
    }
  }
}
