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

using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RibbonControls
{
  class Spinner1 : Spinner
  {
    public Spinner1()
    {
      // subscribe to slider changed event
      SliderChangedEvent.Subscribe(OnSliderValueChanged);
    }

    ~Spinner1()
    {
      SliderChangedEvent.Unsubscribe(OnSliderValueChanged);
    }

    private bool initialized = false;
    protected override void OnUpdate()
    {
      if (!initialized)
      {
        // perform Initialize

        initialized = true;
      }
    }

    protected override void OnValueChanged(double? value)
    {
      if (value == null)
        return;

      if (!value.HasValue)
        return;

      // do something with value.Value;

      // prevent recursion with events
      if (!_ignore)
        SpinnerChangedEvent.Publish(new ValueChangedEventArgs(value));

      base.OnValueChanged(value);
    }

    private bool _ignore = false;
    internal void OnSliderValueChanged(ValueChangedEventArgs args)
    {
      _ignore = true;
      // set spinner value to slider value
      this.Value = args._value;
      _ignore = false;
    }
  }
}
