//   Copyright 2019 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

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
using ArcGIS.Desktop.Framework.Dialogs;

namespace IdentifyWithSketchTool
{
  /// <summary>
  ///  Custom identify Tool
  /// </summary>
  
  class CustomIdentify : MapTool
  {
    /// <summary>
    ///  Constructor
    /// </summary>
    public CustomIdentify()
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Rectangle;
      SketchOutputMode = SketchOutputMode.Screen;
    }

    /// <summary>
    ///  On sketch completion find the intersecting features, flash features and show the number of features selected per layer
    /// </summary>
    /// <param name="geometry"></param>
    /// <returns></returns>
    protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
        return QueuedTask.Run(() =>
        {
          //Get all the features that intersect the sketch geometry and flash them in the view. 
          var features = MapView.Active.GetFeatures(geometry);

          //flash features
          MapView.Active.FlashFeature(features);

          //Show a message box reporting each layer the number of the features.
          MessageBox.Show(String.Join("\n", features.ToDictionary().Select(kvp => String.Format("{0}: {1}", kvp.Key.Name, kvp.Value.Count()))), "Identify Result");
            
          return true;
        });

      
    }
  }
}
