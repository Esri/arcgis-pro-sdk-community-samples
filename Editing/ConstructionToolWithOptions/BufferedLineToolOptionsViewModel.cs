// Copyright 2019 Esri 
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
using ArcGIS.Desktop.Framework.Controls;
using System.Xml.Linq;
using ArcGIS.Desktop.Editing;
using System.Windows.Media;

namespace ConstructionToolWithOptions
{
  //IEditingCreateToolControl
  internal class BufferedLineToolOptionsViewModel : ToolOptionsEmbeddableControl, IEditingCreateToolControl
  {
    public BufferedLineToolOptionsViewModel(XElement options, bool canChangeOptions) : base(options, canChangeOptions) { }


    internal const string BufferOptionName = nameof(Buffer);
    internal const double DefaultBuffer = 25.0;

    // binds in xaml
    private double _buffer;
    public double Buffer
    {
      get { return _buffer; }
      set
      {
        if (SetProperty(ref _buffer, value))
          SetToolOption(BufferOptionName, value);
      }
    }

    protected override bool ShowThisControl
    {
      get
      {
        //// show in ActiveTemplate pane
        //if (IsActiveTemplate)
        //  return true;

        //// dont show in TemplateProperties
        //return false;

        // show in both ActiveTemplate and TemplateProperties panes
        return true;
      }
    }

    public override bool InitializeForActiveTemplate(ToolOptions toolOptions)
    {
      base.InitializeForActiveTemplate(toolOptions);
      return true;    // true <==> do show me in ActiveTemplate;   false <==> don't show me
    }

    public override bool InitializeForTemplateProperties(ToolOptions toolOptions)
    {
      base.InitializeForTemplateProperties(toolOptions);
      return false;
    }

    protected override Task LoadFromToolOptions()
    {
      double? buffer = GetToolOption<double?>(BufferOptionName, DefaultBuffer, null);
      _buffer = buffer.Value;

      return Task.CompletedTask;
    }
  }
}
