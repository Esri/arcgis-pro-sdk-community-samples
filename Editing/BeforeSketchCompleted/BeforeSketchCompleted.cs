//   Copyright 2020 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

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

/// <summary>
/// This sample demonstrates the use of the BeforeSketchCompleted event to
/// modify the sketch geometry before the sketch is completed.
/// The example sets the Z values of the sketch to the specified elevation surface
/// regardless of the current Z environment and any existing Z values the sketch may have.
/// </summary>
/// <remarks>
///1. In Visual Studio click the Build menu. Then select Build Solution.
///2. Click Start button to open ArcGIS Pro.
///3. ArcGIS Pro will open. 
///4. Open a map containing Z aware editable data and an elevation surface called 'Ground'
///Alternatively create a new map with Z aware editable data (e.g. map notes) and add an elevation source.
///5. Open the add-in tab and click on the BeforeSketchCompleted button in the Sketch Events group
///6. On the edit tab, click on the Create button in the Features group to display the create features pane.
///7. Create a feature using a construction tool (the default polygon or line tool where applicable.
///8. Select and examine the Z values on the newly created feature via the attributes pane and switch to the 
///geometry tab. The Z values should reflect the surface Z values.
/// </remarks>
/// 
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
      if (arg.MapView.Map.ElevationSurfaces.Count(s => s.Name == surfaceName) == 0)
      {
        MessageBox.Show("Surface: " + surfaceName + " is not in the map");
        return;
      }

      //set the sketch Z values from the specified elevation surface
      var ZResult = await arg.MapView.Map.GetZsFromSurfaceAsync(arg.Sketch,surfaceName);
      if (ZResult.Status == SurfaceZsResultStatus.Ok)
        arg.SetSketchGeometry(ZResult.Geometry);
    }
  }
}
