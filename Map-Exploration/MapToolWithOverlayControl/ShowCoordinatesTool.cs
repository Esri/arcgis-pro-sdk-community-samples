//   Copyright 2017 Esri
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

namespace MapToolWithOverlayControl
{
  /// <summary>
  /// Class providing the behavior for the custom map tool.
  /// </summary>
  internal class ShowCoordinatesTool : MapTool
  {
    public ShowCoordinatesTool()
    {
      //Set the tools OverlayControlID to the DAML id of the embeddable control
      OverlayControlID = "MapToolWithOverlayControl_EmbeddableControl";
    }

    protected override void OnToolMouseDown(MapViewMouseButtonEventArgs e)
    {
      //On mouse down check if the mouse button pressed is the left mouse button. If it is handle the event.
      if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
        e.Handled = true;
    }

    /// <summary>
    /// Called when the OnToolMouseDown event is handled. Allows the opportunity to perform asynchronous operations corresponding to the event.
    /// </summary>
    protected override Task HandleMouseDownAsync(MapViewMouseButtonEventArgs e)
    {
      //Get the instance of the ViewModel
      var vm = OverlayEmbeddableControl as EmbeddedControlViewModel;
      if (vm == null)
        return Task.FromResult(0);

      //Get the map coordinates from the click point and set the property on the ViewModel.
      return QueuedTask.Run(() =>
      {
        var mapPoint = ActiveMapView.ClientToMap(e.ClientPoint);
        var coords = GeometryEngine.Instance.Project(mapPoint, SpatialReferences.WGS84) as MapPoint;
        if (coords == null) return;
        var sb = new StringBuilder();
        sb.AppendLine($"X: {coords.X:0.000}");
        sb.Append($"Y: {coords.Y:0.000}");
        if (coords.HasZ)
        {
          sb.AppendLine();
          sb.Append($"Z: {coords.Z:0.000}");
        }         
        vm.Text = sb.ToString();
      });
    }
  }
}
