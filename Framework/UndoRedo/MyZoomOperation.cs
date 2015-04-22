//   Copyright 2015 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System.Threading.Tasks;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;

namespace PreRel_UndoRedo
{
  internal class MyZoomOperation : Operation
  {
    private static readonly double MinimumScale = 100.0;
    private static readonly double MinimumElevation = 10;
    private static readonly double MaximumScale = 1000000000.0;
    private static readonly double MaximumElevation = MaximumScale;


    private bool _zoomIn;
    private Camera _origCamera;

    public MyZoomOperation(bool zoomIn)
    {
      _zoomIn = zoomIn;
    }

    public override string Name
    {
        get { 
          if (_zoomIn)
            return "Fixed Zoom In";
          else
            return "Fixed Zoom Out"; 
        }
    }

    public override string Category
    {
        get { return PreRel_UndoRedo.Category; }
    }

    protected override async Task DoAsync()
    {
      MapView activeMapView = MapView.Active;
      if (activeMapView == null)
        return;

      Camera camera = await activeMapView.GetCameraAsync();
      _origCamera = await activeMapView.GetCameraAsync();

      double factor = (_zoomIn) ? 0.75 : 1.25;

      if (activeMapView.ViewMode == ViewMode.Map)
      {
        double scale = camera.Scale * factor;
        if (_zoomIn)
          camera.Scale = scale > MinimumScale ? scale : MinimumScale;
        else
          camera.Scale = scale < MaximumScale ? scale : MaximumScale;
      }
      else
      {
        double z = camera.Z * factor;
        if (_zoomIn)
          camera.Z = z > MinimumElevation ? z : MinimumElevation;
        else
          camera.Z = z < MaximumScale ? z : MaximumScale;
      }

      await activeMapView.ZoomToAsync(camera);
    }

    protected override Task RedoAsync()
    {
      return DoAsync();
    }

    protected override async Task UndoAsync()
    {
      MapView activeMapView = MapView.Active;
      if (activeMapView == null)
        return;

      await activeMapView.ZoomToAsync(_origCamera);
    }
  }
}
