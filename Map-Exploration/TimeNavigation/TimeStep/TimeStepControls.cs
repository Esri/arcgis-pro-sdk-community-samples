//   Copyright 2019 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;

namespace TimeNavigation
{
  internal class TimeStepValue : ComboBox
  {
    bool initialized = false;

    public TimeStepValue()
    {
      for (int i = 1; i < 6; i++)
      {
        Add(new ComboBoxItem(i.ToString()));
      }
    }

    protected override void OnUpdate()
    {
      if (!initialized)
      {
        Text = TimeModule.Settings.StepValue.ToString();
        initialized = true;
      }        
    }

    protected override void OnTextChange(string text)
    {
      double value;
      if (Double.TryParse(text, out value))
        TimeModule.Settings.StepValue = value;
    }
  }

  internal class TimeStepUnit : ComboBox
  {
    public TimeStepUnit()
    {
      var units = Enum.GetValues(typeof(TimeUnit));
      foreach (var unit in units)
      {
        Add(new ComboBoxItem(unit.ToString()));
      }

      SelectedIndex = TimeModule.Settings.StepUnit;
    }

    protected override void OnSelectionChange(ComboBoxItem item)
    {
      TimeModule.Settings.StepUnit = SelectedIndex;
    }
  }

  internal class NextStepButton : Button
  {
    protected override void OnClick()
    {
      var mapView = MapView.Active;
      if (mapView == null)
        return;

      var timeDelta = new TimeDelta(TimeModule.Settings.StepValue, (TimeUnit)TimeModule.Settings.StepUnit);
      mapView.Time = mapView.Time.Offset(timeDelta);
    }
  }

  internal class PreviousStepButton: Button
  {
    protected override void OnClick()
    {
      var mapView = MapView.Active;
      if (mapView == null)
        return;

      var timeDelta = new TimeDelta(TimeModule.Settings.StepValue * -1, (TimeUnit)TimeModule.Settings.StepUnit);
      mapView.Time = mapView.Time.Offset(timeDelta);
    }
  }
}
