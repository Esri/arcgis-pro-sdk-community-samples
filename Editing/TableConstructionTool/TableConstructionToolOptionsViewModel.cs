/*

   Copyright 2022 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       https://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Templates;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Linq;


namespace TableConstructionTool
{
  internal class TableConstructionToolOptionsViewModel : ToolOptionsEmbeddableControl
  {

    private FeatureLayer _selectedFeatureLayer = null;
    internal const string SelectedLayerOptionName = nameof(SelectedLayer);
    internal const string SelectedLayerFieldOptionName = nameof(SelectedField);
    internal const string SelectedTableFieldOptionName = nameof(SelectedTableField);
    internal static List<string> FieldsInCurrentTemplate = new List<string>();
    public TableConstructionToolOptionsViewModel(XElement options, bool canChangeOptions) : base(options, canChangeOptions) {}
    #region binding properties
    private ObservableCollection<string> _layersInMap = new ObservableCollection<string>();
    public ObservableCollection<string> LayersInMap
    {
      get { return _layersInMap; }
      set
      {
        SetProperty(ref _layersInMap, value, () => LayersInMap);
        if (LayersInMap.Count > 0)
          SelectedLayer = LayersInMap[0];
      }
    }
    private string _selectedLayer;
    public string SelectedLayer
    {
      get { return _selectedLayer; }
      set
      {
        if (SetProperty(ref _selectedLayer, value))
        {
          SetToolOption(SelectedLayerOptionName, value);
        }
        _selectedFeatureLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == SelectedLayer);
        _ = GetLayerFieldsAsync();
      }
    }

    private ObservableCollection<string> _layerFields = new ObservableCollection<string>();
    public ObservableCollection<string> LayerFields
    {
      get { return _layerFields; }
      set
      { 
        SetProperty(ref _layerFields, value, () => LayerFields); 
      }
    }
    private string _selectedField;
    public string SelectedField
    {
      get { return _selectedField; }
      set
      {
        if (SetProperty(ref _selectedField, value))
        {
          {
            SetToolOption(SelectedLayerFieldOptionName, value);
          }
        }
      }
    }

    private ObservableCollection<string> _tableFields = new ObservableCollection<string>();
    public ObservableCollection<string> TableFields
    {
      get { return _tableFields; }
      set
      {
        SetProperty(ref _tableFields, value, () => TableFields);
      }
    }

    private string _selectedTableField;
    public string SelectedTableField
    {
      get { return _selectedTableField; }
      set
      {
        if (SetProperty(ref _selectedTableField, value))
        {
          SetToolOption(SelectedTableFieldOptionName, value);
        }
      }
    }
    #endregion

    public override bool IsAutoOpen(string toolID)
    {
      return true;
    }
    public override void OnInitialize(IEnumerable<ToolOptions> optionsCollection, bool hostIsActiveTemplatePane)
    {
      base.OnInitialize(optionsCollection, hostIsActiveTemplatePane);
      if (MapView.Active == null) return;
      //Gather layers in the map
      LayersInMap.Clear();
      foreach (var layer in MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>())
      {
        LayersInMap.Add(layer.Name);
      }
      if (LayersInMap.Count > 0)
        SelectedLayer = LayersInMap[0];
      //Get the table fields
      //Gather fields in the table - to prepopulate
      _ = GetTableFieldsAsync();
    }
    protected override Task LoadFromToolOptions()
    {
      string layer = GetToolOption<string>(SelectedLayerOptionName, SelectedLayer, "");
      _selectedLayer = layer;

      string field = GetToolOption<string>(SelectedLayerFieldOptionName, SelectedField, "");
      _selectedField = field;

      string tableField = GetToolOption<string>(SelectedTableFieldOptionName, SelectedTableField, "");
      _selectedTableField = tableField;

      return Task.CompletedTask;
    }
    private async Task GetLayerFieldsAsync()
    {
      LayerFields.Clear();
      if (SelectedLayer == null)
        return;
      if (_selectedFeatureLayer == null) return;
      await QueuedTask.Run(() =>
      {        
        ObservableCollection<string> fields = new ObservableCollection<string>();
 
        foreach (FieldDescription fd in _selectedFeatureLayer.GetFieldDescriptions())
        {          
            string shapeField = _selectedFeatureLayer.GetFeatureClass().GetDefinition().GetShapeField();
            if (fd.Name == shapeField) continue; //filter out the shape field.        
            fields.Add(fd.Name);
        }
        SetProperty(ref _layerFields, fields, () => LayerFields);
        SetProperty(ref _selectedField, fields.FirstOrDefault(), () => SelectedField);
      });
    }
    private async Task GetTableFieldsAsync()
    {
      if (EditingTemplate.Current.MapMember == null) return;      
      var tableToEdit = EditingTemplate.Current.MapMember as StandaloneTable;
      if (tableToEdit == null) return;

      TableFields.Clear();
      await QueuedTask.Run(() =>
      {
        ObservableCollection<string> tableFields = new ObservableCollection<string>();

        foreach (FieldDescription fd in tableToEdit.GetFieldDescriptions())
        {      
          tableFields.Add(fd.Name);
        }
        SetProperty(ref _tableFields, tableFields, () => TableFields);
        SetProperty(ref _selectedTableField, tableFields.FirstOrDefault(), () => SelectedTableField);
      });
    }

  }
}
