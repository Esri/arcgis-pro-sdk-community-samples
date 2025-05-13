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
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ArcGIS.Desktop.Mapping.Events;

namespace ConstructionToolWithOptions
{
  // IEditingCreateToolControl 
  internal class UIActivatorViewModel : ToolOptionsEmbeddableControl
  {

    internal List<string> ProCommandsIds = new List<string> {
              "esri_mapping_selectByRectangleTool",
              "esri_mapping_selectByPolygonTool",
              "esri_mapping_selectByLassoTool",
              "esri_mapping_selectByCircleTool",
              "esri_mapping_selectByLineTool",
              "esri_mapping_selectByTraceTool"
            };

    public ObservableCollection<EditingCommand> MySelectionToolCommands { get; set; }

    private EditingCommand _selectedTool;
    public EditingCommand SelectedTool
    {
      get => _selectedTool;
      set => SetProperty(ref _selectedTool, value);
    }
    public UIActivatorViewModel(XElement options, bool canChangeOptions) : base(options, canChangeOptions)
    {
      //Create a collection of the selection tools.
      MySelectionToolCommands = [];
      foreach (var command in ProCommandsIds)
      {
        var plugin = FrameworkApplication.GetPlugInWrapper(command);
        var e = new EditingCommand(plugin, command);
        MySelectionToolCommands.Add(e);
      }
      //Selected tool
      SelectedTool = MySelectionToolCommands[0];
      //Subscribe to the selection changed event.
      ArcGIS.Desktop.Mapping.Events.MapSelectionChangedEvent.Subscribe(OnMapSelectionChanged);
    }
    //When map selection changes, if the selection happened because of our selection tool, activate our custom tool.
    private void OnMapSelectionChanged(MapSelectionChangedEventArgs args)
    {
      if (Module1._customTool)
      {
        FrameworkApplication.SetCurrentToolAsync("activator_polygon");
        Module1._customTool = false;
      }
    }

    protected override Task LoadFromToolOptions()
    {
      return Task.CompletedTask;
    }
  }
}
