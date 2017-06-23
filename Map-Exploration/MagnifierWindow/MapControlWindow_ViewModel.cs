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

using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Mapping.Controls;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Core;

namespace MagnifierWindow
{
    class MapControlWindow_ViewModel: PropertyChangedBase
    {
        private MapControlContent _mapContent = null;
        private Camera _camera = null;
        private string _currentGeoCoordinate = null;
        
        public string CurrentGeoCoordinate
        {
            get { return _currentGeoCoordinate; }
            set
            {
                SetProperty(ref _currentGeoCoordinate, value, () => CurrentGeoCoordinate);
            }
        }
        
        public MapControlContent MapContent
        {
            get { return _mapContent; }
            set
            {
                SetProperty(ref _mapContent, value, () => MapContent);
            }
        }

        public Camera CameraProperty
        {
            get { return _camera; }
            set
            {
                SetProperty(ref _camera, value, () => CameraProperty);
                
            }
        }

        public Task UpdateMapControlContent()
        {
          return QueuedTask.Run(() =>
            {
              var currentMap = MapView.Active.Map;
              _mapContent = MapControlContentFactory.Create(currentMap, MapView.Active.Extent, currentMap.DefaultViewingMode);
              NotifyPropertyChanged(() => MapContent);
            });
        }

        public async void UpdateMapControlCamera(System.Windows.Point p)
        {
            //Update map control camera
            if (MapView.Active == null || !MapControlWindow._mapControl.IsReady)
                return;

            MapPoint mapPoint = await QueuedTask.Run<MapPoint>(() =>
            {
                return MapView.Active.ClientToMap(p);
            });
            
            _camera = MapView.Active.Camera;
            _camera.X = mapPoint.X;
            _camera.Y = mapPoint.Y;
            _camera.Scale = _camera.Scale / 4;

            try
            {
                _currentGeoCoordinate = mapPoint.ToGeoCoordinateString(new ToGeoCoordinateParameter(GeoCoordinateType.DDM));
                NotifyPropertyChanged(() => CameraProperty);
                NotifyPropertyChanged(() => CurrentGeoCoordinate);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($@"Error in UpdateMapControlCamera: {ex.ToString()}");
            }
        }

    }
}
