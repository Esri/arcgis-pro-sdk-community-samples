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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace CustomAnimation
{
  /// <summary>
  /// ViewModel for the timing options control.
  /// </summary>
  internal class TimingControlViewModel : CustomControl
  {
    private bool _isAtTime = !Animation.Settings.IsAfterTime;
    public bool IsAtTime
    {
      get { return _isAtTime; }
      set
      {
        if (SetProperty(ref _isAtTime, value, () => IsAtTime))
          Animation.Settings.IsAfterTime = !_isAtTime;
      }
    }

    private bool _isAfterTime = Animation.Settings.IsAfterTime;
    public bool IsAfterTime
    {
      get { return _isAfterTime; }
      set
      {
        if (SetProperty(ref _isAfterTime, value, () => IsAfterTime))
          Animation.Settings.IsAfterTime = _isAfterTime;
      }
    }

    private double _atTime = Animation.Settings.AtTime;
    public double AtTime
    {
      get { return _atTime; }
      set
      {
        if (SetProperty(ref _atTime, Math.Round(Math.Abs(value), 3), () => AtTime))
          Animation.Settings.AtTime = _atTime;
      }
    }

    private double _afterTime = Animation.Settings.AfterTime;
    public double AfterTime
    {
      get { return _afterTime; }
      set
      {
        if (SetProperty(ref _afterTime, Math.Round(Math.Abs(value), 3), () => AfterTime))
          Animation.Settings.AfterTime = _afterTime;
      }
    }

    private double _duration = Animation.Settings.Duration;
    public double Duration
    {
      get { return _duration; }
      set
      {
        if (SetProperty(ref _duration, Math.Round(Math.Abs(value), 3), () => Duration))
          Animation.Settings.Duration = _duration;
      }
    }
  }
}
