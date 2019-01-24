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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace CustomAnimation
{
  /// <summary>
  /// ViewModel for the path options custom control.
  /// </summary>
  internal class PathOptionsControlViewModel : CustomControl
  {
    private double _heightAbove = Animation.Settings.HeightAbove;
    public double HeightAbove
    {
      get { return _heightAbove; }
      set
      {
        if (SetProperty(ref _heightAbove, Math.Round(value, 2), () => HeightAbove))
          Animation.Settings.HeightAbove = _heightAbove;
      }
    }

    private double _keyEvery = Animation.Settings.KeyEvery;
    public double KeyEvery
    {
      get { return _keyEvery; }
      set
      {
        if (SetProperty(ref _keyEvery, Math.Round(Math.Abs(value), 2), () => KeyEvery))
          Animation.Settings.KeyEvery = _keyEvery;
      }
    }

    private bool _verticesOnly = Animation.Settings.VerticesOnly;
    public bool VerticesOnly
    {
      get { return _verticesOnly; }
      set
      {
        if (SetProperty(ref _verticesOnly, value, () => VerticesOnly))
          Animation.Settings.VerticesOnly = _verticesOnly;
      }
    }

    private double _pitch = Animation.Settings.Pitch;
    public double Pitch
    {
      get { return _pitch; }
      set
      {
        if (SetProperty(ref _pitch, Math.Round(value, 2), () => Pitch))
          Animation.Settings.Pitch = _pitch;
      }
    }

    private bool _useLinePitch = Animation.Settings.UseLinePitch;
    public bool UseLinePitch
    {
      get { return _useLinePitch; }
      set
      {
        if (SetProperty(ref _useLinePitch, value, () => UseLinePitch))
          Animation.Settings.UseLinePitch = _useLinePitch;
      }
    }
  }
}
