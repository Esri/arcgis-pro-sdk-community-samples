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
using System.Windows.Media.Imaging;

namespace ConstructionToolWithOptions
{
  //IEditingCreateToolControl
  internal class BufferedLineToolOptionsViewModel : ToolOptionsEmbeddableControl
  {
    public BufferedLineToolOptionsViewModel(XElement options, bool canChangeOptions) : base(options, canChangeOptions) { }


    internal const string BufferOptionName = nameof(Buffer);
    internal const double DefaultBuffer = 25.0;

    // binds in xaml
    private double _buffer;
    private double _originalBuffer;
    public double Buffer
    {
      get { return _buffer; }
      set
      {
        if (SetProperty(ref _buffer, value))
        {
          SetToolOption(BufferOptionName, value);
          IsDirty = _buffer != _originalBuffer;

          //** some scenario where IsValid = false especially in properties
          if (!HostIsActiveTemplatePane)
          {
            if (value > 30)
              IsValid = false;      // should deactivate the properties OK button
          }
        }
      }
    }
    public override bool IsAutoOpen(string toolID)
    {
      return true;
    }
    protected override Task LoadFromToolOptions()
    {
      double? buffer = GetToolOption<double?>(BufferOptionName, DefaultBuffer, null);
      _originalBuffer = buffer.GetValueOrDefault();
      if (buffer.HasValue)
        _buffer = buffer.Value;
      else
        _buffer = 0;

      return Task.CompletedTask;
    }
    public override Task OpenAsync()
    {
      return base.OpenAsync();
    }

    public override Task CloseAsync()
    {
      return base.CloseAsync();
    }

    public override void OnInitialize(IEnumerable<ToolOptions> optionsCollection, bool hostIsActiveTmplatePane)
    {
      base.OnInitialize(optionsCollection, hostIsActiveTmplatePane);
      //var options = this.ToolOptions;

      double val = double.NaN;
      double firstVal = double.NaN;
      foreach (var option in optionsCollection)
      {
        val = option.GetProperty(BufferOptionName, DefaultBuffer);

        if (double.IsNaN(firstVal))
          firstVal = val;
        else
        {
          if (firstVal != val)
            IsDifferentValue = true;
        }
      }
    }

    private bool _isDifferentValue;
    public bool IsDifferentValue
    {
      get => _isDifferentValue;
      internal set => SetProperty(ref _isDifferentValue, value);
    }

    private BitmapImage _img = null;
    public override ImageSource SelectorIcon
    {
      get
      {
        if (_img == null)
          _img = new BitmapImage(new Uri(
            "pack://application:,,,/ArcGIS.Desktop.Resources;component/Images/AddFilter16.png", UriKind.Absolute));
        return _img;
      }
    }

    public new bool HostIsActiveTemplatePane => base.HostIsActiveTemplatePane;
    public new bool HasMultipleTemplatesSelected => base.HasMultipleTemplatesSelected;
  }
}
