// Copyright 2019 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.


using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;

namespace InspectorTool
{
  internal class AttributeControlViewModel : EmbeddableControl
  {
    private EmbeddableControl _inspectorViewModel = null;
    private UserControl _inspectorView = null;
    private Dictionary<MapMember, List<long>> _selection = null;
    private static AttributeControlViewModel _dockpaneVM;
    private Inspector _featureInspector = null;

    public AttributeControlViewModel(XElement options, bool canChangeOptions) : base(options, canChangeOptions)
    {
      _dockpaneVM = this;

      // create an inspector instance to support editing events
      _dockpaneVM.AttributeInspector = new Inspector();
      // create an embeddable control from the inspector class to display on the pane
      var icontrol = AttributeInspector.CreateEmbeddableControl();

      // get view and viewmodel from the inspector
      InspectorViewModel = icontrol.Item1;
      InspectorView = icontrol.Item2;

      // Listen for editing changes
      _dockpaneVM.AttributeInspector.PropertyChanged += InspectorViewModel_PropertyChanged;
    }

    private void InspectorViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      // Did something change?
      if (e.PropertyName == "IsDirty")
      {
        // IsDirty has been updated, update the buttons (UI) accordingly
        _dockpaneVM.NotifyPropertyChanged(nameof(IsApplyEnabled));
        _dockpaneVM.NotifyPropertyChanged(nameof(IsCancelEnabled));
      }
    }

    /// <summary>
    /// Property containing an instance for the inspector.
    /// </summary>
    internal Inspector AttributeInspector { get; private set; }

    /// <summary>
    /// Access to the view model of the inspector
    /// </summary>
    public EmbeddableControl InspectorViewModel
    {
      get { return _inspectorViewModel; }
      set
      {
        if (value != null)
        {
          _inspectorViewModel = value;
          _inspectorViewModel.OpenAsync();

        }
        else if (_inspectorViewModel != null)
        {
          _inspectorViewModel.CloseAsync();
          _inspectorViewModel = value;
        }
        NotifyPropertyChanged(() => InspectorViewModel);
      }
    }

    /// <summary>
    /// Dictionary holding the selected features in the map to populate the tree view for 
    /// layers and respective selected features.
    /// </summary>
    public Dictionary<MapMember, List<long>> SelectedMapFeatures
    {
      get => _selection;
      set => SetProperty(ref _selection, value);
    }

    /// <summary>
    /// Property for the inspector UI.
    /// </summary>
    public UserControl InspectorView
    {
      get { return _inspectorView; }
      set { SetProperty(ref _inspectorView, value); }
    }
    public bool IsApplyEnabled => AttributeInspector?.IsDirty ?? false;
    public bool IsCancelEnabled => AttributeInspector?.IsDirty ?? false;

    public ICommand CancelCommand
    {
      get => new RelayCommand(() => AttributeInspector?.Cancel());
    }

    public ICommand ApplyCommand
    {
      get => new RelayCommand(() =>
          {
            QueuedTask.Run(() =>
            {
              //Apply the attribute changes.
              //Writing them back to the database in an Edit Operation.
              AttributeInspector?.Apply();
            });
          });
		}
  }
}
