//   Copyright 2019 Esri
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
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace PreRel_UndoRedo
{
  /// <summary>
  /// A sample undo/redo operation that illustrates zoom in and zoom out actions. 
  /// </summary>
  internal class MyZoomOperation : Operation
  {
    private bool _zoomIn;
    private Camera _origCamera;

    public MyZoomOperation(bool zoomIn)
    {
      _zoomIn = zoomIn;
    }

    /// <summary>
    /// Gets the name of the operation
    /// </summary>
    public override string Name
    {
      get { 
        if (_zoomIn)
          return "Fixed Zoom In";
        else
          return "Fixed Zoom Out"; 
      }
    }

    /// <summary>
    /// Gets the category of the operation
    /// </summary>
    public override string Category
    {
      get { return PreRel_UndoRedo.Category; }
    }

    /// <summary>
    /// Performs the operation
    /// </summary>
    /// <returns>A Task to the Do method</returns>
    protected override bool Do()
    {
      // get the active mapview
      MapView activeMapView = MapView.Active;
      if (activeMapView == null)
          return true;

      // note that MapView.ZoomInFixed and MapView.ZoomOutFixed must be called on the MCT
      QueuedTask.Run(() =>
      {
        // get the original camera position for undo
        _origCamera = activeMapView.Camera;

        // dp the zoom in/zoom out
        if (_zoomIn)
          activeMapView.ZoomInFixed();
        else
          activeMapView.ZoomOutFixed();
      });

      return true;
    }

    /// <summary>
    /// Performs the operation
    /// </summary>
    /// <returns>A Task to the DoAsync method</returns>
    protected override Task DoAsync()
    {
      Do();
      return Task.FromResult(0);
    }

    /// <summary>
    /// Repeats the operation
    /// </summary>
    /// <returns>A Task to the RedoAsync method</returns>
    protected override Task RedoAsync()
    {
      return DoAsync();
    }

    /// <summary>
    /// Undo the operation to reset the state
    /// </summary>
    /// <returns>A Task to the UndoAsync method</returns>
    protected override async Task UndoAsync()
    {
      // no need to wrap ZoomToAsync in QueuedTask.Run
      MapView activeMapView = MapView.Active;
      if (activeMapView == null)
        return;

      await activeMapView.ZoomToAsync(_origCamera);
    }
  }
}
