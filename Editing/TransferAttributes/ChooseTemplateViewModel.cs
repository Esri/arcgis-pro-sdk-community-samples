/*

   Copyright 2019 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Xml.Linq;
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
using ArcGIS.Desktop.Mapping;


namespace TransferAttributes
{
  /// <summary>
  /// ViewModel for the ChoosesTemplate view.  Allows a user to choose a layer, template combination for use with the "Transfer Attributes from Template" tool.
  /// </summary>
  internal class ChooseTemplateViewModel : EmbeddableControl
  {
    public ChooseTemplateViewModel(XElement options, bool canChangeOptions) : base(options, canChangeOptions) { }

    private BasicFeatureLayer _selectedLayer;
    /// <summary>
    /// The selected layer
    /// </summary>
    public BasicFeatureLayer SelectedLayer
    {
      get => _selectedLayer;
      set
      {
        SetProperty(ref _selectedLayer, value);
        // now get the templates

        GetTemplatesFromLayer();
      }
    }

    private EditingTemplate _selectedTemplate;
    /// <summary>
    /// The selected template
    /// </summary>
    public EditingTemplate SelectedTemplate
    {
      get => _selectedTemplate;
      set
      {
        SetProperty(ref _selectedTemplate, value);
        if (_selectedTemplate != null)
          ClearMessage();
      }
    }

    private List<BasicFeatureLayer> _layers;
    /// <summary>
    /// The set of layers in the active map
    /// </summary>
    public List<BasicFeatureLayer> Layers { get => _layers; }

    private readonly ObservableCollection<EditingTemplate> _templates = new ObservableCollection<EditingTemplate>();
    private ReadOnlyObservableCollection<EditingTemplate> _readonlyTemplates;

    /// <summary>
    /// The set of templates existing on the SelectedLayer
    /// </summary>
    public ReadOnlyObservableCollection<EditingTemplate> Templates { get => _readonlyTemplates; }


    /// <summary>
    /// used to lock collections for use by multiple threads
    /// </summary>
    private readonly object _lockCollection = new object();

    public override Task OpenAsync()
    {
      var map = MapView.Active?.Map;
      if (map == null)
        return Task.CompletedTask;

      // create collection and enable synchronization so we can use it on both UI and background threads
      _readonlyTemplates = new ReadOnlyObservableCollection<EditingTemplate>(_templates);
      BindingOperations.EnableCollectionSynchronization(_readonlyTemplates, _lockCollection);

      // find all the layers in the map
      _layers = map.GetLayersAsFlattenedList().OfType<BasicFeatureLayer>().ToList();

      _selectedLayer = null;
      // assign the first one
      if (_layers?.Count > 0)
        SelectedLayer = _layers[0];

      return base.OpenAsync();
    }

    private Task GetTemplatesFromLayer()
    {
      // clear the list
      _templates.Clear();

      // if no layer, exit
      if (_selectedLayer == null)
        return Task.CompletedTask;

      return QueuedTask.Run(() =>
      {
        var templates = _selectedLayer.GetTemplates();
        foreach (var template in templates)
          _templates.Add(template);
      });
    }


    #region Message
    private string _message;
    public string Message
    {
      get => _message;
      set
      {
        SetProperty(ref _message, value);
        NotifyPropertyChanged(nameof(HasMessage));
      }
    }

    public bool HasMessage => !string.IsNullOrEmpty(_message);

    public void ShowMessage(string msg)
    {
      Message = msg;
    }

    public void ClearMessage()
    {
      Message = "";
    }
    #endregion

    #region Burger Button

    /// <summary>
    /// Tooltip shown when hovering over the burger button.
    /// </summary>
    public string BurgerButtonTooltip => "Options"; 

    /// <summary>
    /// Menu shown when burger button is clicked.
    /// </summary>
    public System.Windows.Controls.ContextMenu BurgerButtonMenu => FrameworkApplication.CreateContextMenu("TransferAttributes_ChooseTemplate_Menu"); 

    #endregion
  }
}
