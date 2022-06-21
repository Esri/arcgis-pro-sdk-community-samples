// Copyright 2019 Esri 
//
// 
//   Licensed under the Apache License, Version 2.0 (the "License"); 
//   you may not use this file except in compliance with the License. 
//   You may obtain a copy of the License at 
//
//       https://www.apache.org/licenses/LICENSE-2.0 
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
using ArcGIS.Desktop.Framework.Controls;
using System.Xml.Linq;
using ArcGIS.Desktop.Editing;
using System.Windows.Media;
using ArcGIS.Desktop.Editing.Templates;

namespace ConstructionToolWithOptions
{
  // IEditingCreateToolControl 
  internal class CircleToolOptionsViewModel : ToolOptionsEmbeddableControl
  {
    public CircleToolOptionsViewModel(XElement options, bool canChangeOptions) : base(options, canChangeOptions) { }


    internal const string RadiusOptionName = nameof(Radius);
    internal const double DefaultRadius = 25.0;

    // binds in xaml
    private double _radius;
    public double Radius
    {
      get { return _radius; }
      set
      {
        if (SetProperty(ref _radius, value))
          SetToolOption(nameof(Radius), value);
      }
    }   

    protected override Task LoadFromToolOptions()
    {
      // null is the value returned if
      //  1. looking at tool in template properties if multiple templates are selected
      //  2. AND each template has different tool option values
      double? radius = GetToolOption<double?>(nameof(Radius), DefaultRadius, null);
      if (!radius.HasValue)
        IsDifferentValue = true;
      else
      {
        IsDifferentValue = false;
        Radius = radius.Value;
      }

      return Task.CompletedTask;
    }

    private bool _isDifferentValue;
    public bool IsDifferentValue
    {
      get => _isDifferentValue;
      internal set => SetProperty(ref _isDifferentValue, value);
    }

    public string DifferentValuesText => "Different Values";

    internal void SetValidIfNotEditingDifferentValues(bool isValid)
    {
      IsValid = IsDifferentValue || isValid;
    }
  }
}
