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

using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace TimeNavigation
{
  internal class TimeBkmNavViewModel : CustomControl
  {
    private RelayCommand _prevBkmCmd;
    private RelayCommand _playBkmCmd;
    private RelayCommand _nextBkmCmd;

    public TimeBkmNavViewModel()
    {
      _prevBkmCmd = new RelayCommand(() => GoToBkm(false), () => CanGoToBkm());
      _playBkmCmd = new RelayCommand(() => PlayBkm(), () => CanGoToBkm());
      _nextBkmCmd = new RelayCommand(() => GoToBkm(true), () => CanGoToBkm());   

      ActiveMapViewChangedEvent.Subscribe(OnActiveViewChanged);
    }

    ~TimeBkmNavViewModel()
    {
      ActiveMapViewChangedEvent.Unsubscribe(OnActiveViewChanged);
    }

    private static double _playbackspeed = TimeModule.Settings.PlaybackSpeed;
    public double PlaybackSpeed
    {
      get { return _playbackspeed; }
      set
      {
        SetProperty(ref _playbackspeed, value, () => PlaybackSpeed);
        TimeModule.Settings.PlaybackSpeed = _playbackspeed;
      }
    }

    private bool _runEnabled;
    public bool RunEnabled 
    { 
      get {return _runEnabled;}
      set { SetProperty(ref _runEnabled, value, () => RunEnabled); } 
    }

    public ICommand PrevBkmCmd
    {
      get { return _prevBkmCmd; }
    }

    public ICommand PlayBkmCmd
    {
      get { return _playBkmCmd; }
    }

    public ICommand NextBkmCmd
    {
      get { return _nextBkmCmd; }
    }

    private void PlayBkm()
    {
      RunEnabled = !RunEnabled;
      if (RunEnabled)
        Task.Run(async () =>
        {
          while (RunEnabled)
          {
            if (!await GoToBkm(true))
            {
              RunEnabled = false;
              return;
            }
          }
        });
    }

    private Task<bool> GoToBkm(bool next)
    {
      var mapView = MapView.Active;
      if (mapView == null)
        return Task.FromResult(false);

      var bookmark = (next) ? TimeModule.GetNextBookmark() : TimeModule.GetPreviousBookmark();

      if (bookmark != null)
        return mapView.PanToAsync(TimeModule.CurrentBookmark, TimeSpan.FromMilliseconds((int)(Math.Abs(TimeModule.Settings.PlaybackSpeed))));
      return Task.FromResult(false);
    }

    private bool CanGoToBkm() { return (TimeModule.Bookmarks.Count > 1); }

    private void OnActiveViewChanged(ActiveMapViewChangedEventArgs args)
    {
      RunEnabled = false;
    }
  }
}
