//   Copyright 2020 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

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
using ArcGIS.Desktop.Mapping.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeforeSketchCompleted
{
  internal class BeforeSketchCompleted : Button
  {
    readonly string surfaceName = "Ground";
    ArcGIS.Core.Events.SubscriptionToken bscToken = null;
    
    protected override void OnClick()
    {
      //subscribe to BeforeSketchCompleted event once
      if (bscToken == null)
        bscToken = ArcGIS.Desktop.Mapping.Events.BeforeSketchCompletedEvent.Subscribe(OnBeforeSketchCompletedEvent);
    }

    private async Task OnBeforeSketchCompletedEvent(BeforeSketchCompletedEventArgs arg)
    {
      //check if surfacename is in the map
      var elSurface = arg.MapView.Map.GetElevationSurfaceLayers().Where(s => s.Name == surfaceName).FirstOrDefault();
      if (arg.MapView.Map.GetElevationSurfaceLayers().Count(s => s.Name == surfaceName) == 0)
      {
        MessageBox.Show("Surface: " + surfaceName + " is not in the map");
        return;
      }

      //set the sketch Z values from the specified elevation surface
      var ZResult = await arg.MapView.Map.GetZsFromSurfaceAsync(arg.Sketch, elSurface);
      if (ZResult.Status == SurfaceZsResultStatus.Ok)
        arg.SetSketchGeometry(ZResult.Geometry);
    }
  }
}
