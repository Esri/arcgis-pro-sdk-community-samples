/*

   Copyright 2024 Esri

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
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using EditorInspectorUI.InspectorUIProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;


namespace EditorInspectorUI.InspectorUI
{
  internal class AttributeControlViewModel : EmbeddableControl
  {
    private Inspector _featureInspector = null;
    private MyProvider _provider = null;
    public AttributeControlViewModel(XElement options, bool canChangeOptions) : base(options, canChangeOptions) {

      // the standard way of creating an inspector
      //_featureInspector = new Inspector();
      #region Inspector UI with Provider
      //******an alternative way to create an inspector *****
      //allowing us to customize the grid view

      //create the provider
      _provider = new MyProvider();
      //create the inspector from the provider
      _featureInspector = _provider.Create();
      #endregion

      // create the embeddable control from the inspector (to display on the pane)
      var icontrol = _featureInspector.CreateEmbeddableControl();

      // get view and viewmodel from the inspector - supports MVVM
      InspectorViewModel = icontrol.Item1;
      InspectorView = icontrol.Item2;

      #region Subscribe to Inspector PropertyChanged event
      _featureInspector.PropertyChanged += _featureInspector_PropertyChanged;
      #endregion
    }
    #region Binding properties: Business logic

    // Property containing an instance for the inspector.
    public Inspector AttributeInspector => _featureInspector;
   
    public EmbeddableControl InspectorViewModel
    {
      get => _inspectorViewModel;
      set
      {
        if (value != null)
        {
          _inspectorViewModel = value;
          //Occurs when the control is hosted
          _inspectorViewModel.OpenAsync(); 
        }
        else if (_inspectorViewModel != null)
        {
          //Occurs when the control is closed
          _inspectorViewModel.CloseAsync();
          _inspectorViewModel = value;
        }
        SetProperty(ref _inspectorViewModel, value, () => InspectorViewModel); //check this?
      }
    }

    //The inspector UI (Binding to the Content presenter in the XAML)
    public UserControl InspectorView
    {
      get => _inspectorView;
      set => SetProperty(ref _inspectorView, value, () => InspectorView);
    }

    // Itemsource for the Treeview control
    public Dictionary<MapMember, List<PermitRecord>> SelectedMapFeatures
    {
      get => _selectedMapFeatures;
      set => SetProperty(ref _selectedMapFeatures, value, () => SelectedMapFeatures);
    }
  
    #endregion
    public bool IsApplyEnabled => AttributeInspector?.IsDirty ?? false;
    public bool IsCancelEnabled => AttributeInspector?.IsDirty ?? false;

    private ICommand _cancelCommand;
    public ICommand CancelCommand
    {
      get
      {
        if (_cancelCommand == null)
          _cancelCommand = new RelayCommand(OnCancel);
        return _cancelCommand;
      }
    }

    internal void OnCancel()
    {
      AttributeInspector?.Cancel();
    }

    private ICommand _applyCommand;
    public ICommand ApplyCommand
    {
      get
      {
        if (_applyCommand == null)
          _applyCommand = new RelayCommand(OnApply);
        return _applyCommand;
      }
    }

    internal void OnApply()
    {
      QueuedTask.Run(() =>
      {
        //Apply the attribute changes.
        //Writing them back to the database in an Edit Operation.
        AttributeInspector?.Apply();
      });
    }
    private UserControl _inspectorView = null;
    private EmbeddableControl _inspectorViewModel = null;
    private Dictionary<MapMember, List<PermitRecord>> _selectedMapFeatures = new Dictionary<MapMember, List<PermitRecord>>();

    private void _featureInspector_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "IsDirty")
      {
        NotifyPropertyChanged(nameof(IsApplyEnabled));
        NotifyPropertyChanged(nameof(IsCancelEnabled));
      }
    }
  }
}
