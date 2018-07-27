//   Copyright 2018 Esri

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
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace IdentifyWithSketchTool
{
  /// <summary>
  ///  Select and Zoom MapTool
  /// </summary>
  
  class SelectAndZoom : MapTool
  {
    /// <summary>
    ///  Constructor
    /// </summary>
    public SelectAndZoom()
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Rectangle; // rectangle sketch tool
      SketchOutputMode = SketchOutputMode.Screen;
    }

    /// <summary>
    ///  On sketch completion select the intersecting features and zoom
    /// </summary>
    /// <param name="geometry"></param>
    /// <returns></returns>
    protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      return QueuedTask.Run(() => 
      {
        //select features that intersect the sketch geometry
        var selection = MapView.Active.SelectFeatures(geometry, SelectionCombinationMethod.New);

        //zoom to selection
        return MapView.Active.ZoomToAsync(selection.Select(kvp => kvp.Key), true, TimeSpan.FromSeconds(1.5), true);                                          
      });
    }
  }
}
