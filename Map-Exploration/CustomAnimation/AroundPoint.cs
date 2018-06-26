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
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework;

namespace CustomAnimation
{
  /// <summary>
  /// Tool used to click in the view and capture the point clicked to be used to construct keyframes around.
  /// </summary>
  internal class CenterAt : MapTool
  {
    private bool _pointClicked;

    /// <summary>
    /// Called when the mouse button is released in the view.
    /// </summary>
    protected override void OnToolMouseUp(MapViewMouseButtonEventArgs e)
    {
      if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
        e.Handled = true;
      base.OnToolMouseUp(e);
    }

    /// <summary>
    /// Asynchronous callback if the OnTooMouseUp event is handled.
    /// </summary>
    /// <param name="e"></param>
    protected override Task HandleMouseUpAsync(MapViewMouseButtonEventArgs e)
    {
      return QueuedTask.Run(() =>
      {
        var mapView = MapView.Active;
        if (mapView == null)
          return;

        //Get the point clicked, zoom to it and call the module method to construct keyframes around it.
        var mapPoint = mapView.ClientToMap(e.ClientPoint);
        mapView.LookAt(mapPoint, TimeSpan.Zero);
        Animation.Current.CreateKeyframesAroundPoint(mapPoint);
        _pointClicked = true;
      });
    }
    
    /// <summary>
    /// Called regularly by the framework.
    /// </summary>
    protected override void OnUpdate()
    {
      //If a point has been clicked by the tool we should deactivate the tool by setting the explore tool active.
      if (_pointClicked)
      {
        _pointClicked = false;
        FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");
      }
    }
  }

  /// <summary>
  /// Edit box which is used to set the total number of degrees to rotate around the point.
  /// </summary>
  internal class DegreesEdit : EditBox
  {
    public DegreesEdit()
    {
      this.Text = Animation.Settings.Degrees.ToString();
    }

    /// <summary>
    /// Called when text is committed to the edit box.
    /// </summary>
    protected override void OnEnter()
    {
      double degrees = -1;
      if (Double.TryParse(this.Text, out degrees))
        Animation.Settings.Degrees = degrees;
    }
  }
}
