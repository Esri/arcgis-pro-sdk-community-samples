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
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using System.Windows.Input;
using ArcGIS.Desktop.Mapping.Events;

namespace CameraNavigation
{
  /// <summary>
  /// Camera Properties DockPane implementation. Provideds bindable Camera property and button commands to zoom to and pan to new camera position.
  /// </summary>
  internal class CameraPaneViewModel : DockPane
  {
    private const string _dockPaneID = "CameraNavigation_Camera_CameraPane";

    public CameraPaneViewModel() 
    {
      _zoomToCmd = new RelayCommand(() => MapView.Active.ZoomToAsync(Camera, TimeSpan.FromSeconds(1.5)), () => { return MapView.Active != null; });
      _panToCmd = new RelayCommand(() => MapView.Active.PanToAsync(Camera, TimeSpan.FromSeconds(1.5)), () => { return MapView.Active != null; });

      MapViewCameraChangedEvent.Subscribe(OnCameraChanged);
      ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);

      if (MapView.Active != null)
        Camera = MapView.Active.Camera;
    }

    ~CameraPaneViewModel()
    {
      _zoomToCmd.Disconnect();
      _panToCmd.Disconnect();

      ActiveMapViewChangedEvent.Unsubscribe(OnActiveMapViewChanged);
      MapViewCameraChangedEvent.Unsubscribe(OnCameraChanged);
    }

    /// <summary>
    /// Show the DockPane.
    /// </summary>
    internal static void Show()
    {
      DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
      if (pane == null)
        return;

      pane.Activate();
    }

    private Camera _camera;
    public Camera Camera
    {
      get { return _camera; }
      set
      {
        SetProperty(ref _camera, value, () => Camera);
      }
    }

    private RelayCommand _panToCmd;
    public ICommand PanToCmd
    {
      get { return _panToCmd; }
    }

    private RelayCommand _zoomToCmd;
    public ICommand ZoomToCmd
    {
      get { return _zoomToCmd; }
    }

    private void OnCameraChanged(MapViewCameraChangedEventArgs obj)
    {
      if (obj.MapView == MapView.Active)
        Camera = obj.CurrentCamera;
    }

    private void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs obj)
    {
      if (obj.IncomingView == null)
      {
        Camera = null;
        return;
      }
        
      Camera = obj.IncomingView.Camera;
    }
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
  internal class CameraPane_ShowButton : Button
  {
    protected override void OnClick()
    {
      CameraPaneViewModel.Show();
    }
  }

}
