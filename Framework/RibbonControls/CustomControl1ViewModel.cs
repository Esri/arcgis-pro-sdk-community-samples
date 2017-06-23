// Copyright 2017 Esri 
//
// 
//   Licensed under the Apache License, Version 2.0 (the "License"); 
//   you may not use this file except in compliance with the License. 
//   You may obtain a copy of the License at 
//
//       http://www.apache.org/licenses/LICENSE-2.0 
//
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


namespace RibbonControls
{
  internal class CustomControl1ViewModel : CustomControl
  {
    public CustomControl1ViewModel()
    {
      // subscribe to spinner changed
      SpinnerChangedEvent.Subscribe(OnSpinnerValueChanged);
    }

    double? _sliderValue = null;
    public double? SliderValue
    {
      get { return _sliderValue; }
      set
      {
        if (SetProperty(ref _sliderValue, value, () => SliderValue))
        {
          if (!_ignore)
            SliderChangedEvent.Publish(new ValueChangedEventArgs(value));
        }
      }
    }

    private bool _ignore;
    internal void OnSpinnerValueChanged(ValueChangedEventArgs args)
    {
      _ignore = true;
      // set the slider value to the spinner value
      SliderValue = args._value;
      _ignore = false;
    }
  }
}
