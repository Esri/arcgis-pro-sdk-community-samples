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

using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using System;

namespace TimeNavigation
{
  internal class MapTimeViewModel : CustomControl
  {
    public MapTimeViewModel()
    {
      MapViewTimeChangedEvent.Subscribe(OnTimeChanged);
      ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);     
      var mapView = MapView.Active;
      if (mapView != null)
      {
        _startDate = mapView.Time.Start;
        _endDate = mapView.Time.End;
      }    
    }

    ~MapTimeViewModel()
    {
      MapViewTimeChangedEvent.Unsubscribe(OnTimeChanged);
      ActiveMapViewChangedEvent.Unsubscribe(OnActiveMapViewChanged);
    }

    private DateTime? _startDate;
    public DateTime? StartDate
    {
      get
      {
        if (_startDate.HasValue)
          return _startDate.Value;
        return null;
      }
      set
      {
        SetProperty(ref _startDate, value, () => StartDate);
        MapView.Active.Time.Start = _startDate;
      }
    }

    private DateTime? _endDate;
    public DateTime? EndDate
    {
      get
      {
        if (_endDate.HasValue)
          return _endDate.Value;
        return null;
      }
      set
      {
        SetProperty(ref _endDate, value, () => EndDate);
        MapView.Active.Time.End = _endDate;
      }
    }

    private void OnTimeChanged(MapViewTimeChangedEventArgs obj)
    {
      SetTimeProperties(obj.CurrentTime);
    }

    private void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs obj)
    {
      if (obj.IncomingView != null && obj.IncomingView.Time != null)
        SetTimeProperties(obj.IncomingView.Time);
    }

    private void SetTimeProperties(TimeRange Time)
    {
      SetProperty(ref _startDate, Time.Start, () => StartDate);
      SetProperty(ref _endDate, Time.End, () => EndDate);
    }
  }
}
