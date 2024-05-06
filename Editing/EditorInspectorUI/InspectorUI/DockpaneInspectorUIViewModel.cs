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
using ArcGIS.Desktop.Mapping.Events;
using EditorInspectorUI.InspectorUIProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Button = ArcGIS.Desktop.Framework.Contracts.Button;

namespace EditorInspectorUI.InspectorUI
{
  internal class DockpaneInspectorUIViewModel : DockPane
  {
    private const string _dockPaneID = "EditorInspectorUI_InspectorUI_DockpaneInspectorUI";
    private Inspector _featureInspector = null;
    private MyProvider _provider = null;
    private static DockpaneInspectorUIViewModel _dockpaneVM = null;
    //private bool _inspectorInitialized = false;
    protected DockpaneInspectorUIViewModel() {       
      //Listen to the MapSelectionChanged event
      MapSelectionChangedEvent.Subscribe(OnMapSelectionChanged);
      _dockpaneVM = this;
    }

    private async void OnMapSelectionChanged(MapSelectionChangedEventArgs args)
    {
      #region Initialize
      if (MapView.Active?.Map == null) return;     
      var map = MapView.Active.Map;
      //This is used to populate the tree view in the dock pane
      var selectionPermitRecordsDictionary = new Dictionary<MapMember, List<PermitRecord>>();
      #endregion
      //Get the building permits layer
      var buildingPermitsLayer = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().
                FirstOrDefault(n => n.Name == "NYC building permits");
      await QueuedTask.Run(() => {
        //get the selected features
        var selectionInMap = map.GetSelection();

        if (buildingPermitsLayer == null) return;
        if (selectionInMap.ToDictionary().ContainsKey(buildingPermitsLayer))
        {
          //get the selected features oid list
          var selectionDictionary = new Dictionary<MapMember, List<long>>();
          selectionDictionary.Add(buildingPermitsLayer, selectionInMap.ToDictionary()[buildingPermitsLayer]);
          //load the first feature into the inspector
          var firstOid = selectionDictionary[buildingPermitsLayer][0];
          AttributeInspector.LoadAsync(buildingPermitsLayer, firstOid);
          // set up a dictionary to "PermitRecord" objects for each selected feature
          var permitRecordList = new List<PermitRecord>();
          #region Create PermitRecords from the selection
          var queryFilter = new QueryFilter
          {
            ObjectIDs = selectionInMap.ToDictionary()[buildingPermitsLayer].ToArray()
          };
          var cursor = buildingPermitsLayer.Search(queryFilter);

          while (cursor.MoveNext())
          {
            var feature = cursor.Current as Feature;
            if (feature != null)
            {
              var oid = feature.GetObjectID();
              var jobNo = feature["Job_Number"].ToString();
              var jobType = feature["Job_Type"].ToString();
              var address = feature["Address"].ToString();
              var permitRecord = new PermitRecord(oid, jobNo, address, jobType);
              permitRecordList.Add(permitRecord);
            }
          }
          #endregion
          selectionPermitRecordsDictionary.Add(buildingPermitsLayer as MapMember, permitRecordList);

        }
      });
      // assign the dictionary to the view model - notifies the tree view to update
      SelectedMapFeatures = selectionPermitRecordsDictionary;
    }

    /// <summary>
    /// Show the DockPane.
    /// </summary>
    internal async static void Show()
    {
      #region Initialize the DockPane
      DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
      if (pane == null)
        return;
      if (MapView.Active?.Map == null) return;
      var map = MapView.Active.Map;
      var selectionPermitRecordsDictionary = new Dictionary<MapMember, List<PermitRecord>>();
      _dockpaneVM.SelectedMapFeatures.Clear();
      #endregion
      bool forceReload = true;      
      if (_dockpaneVM._featureInspector == null || forceReload == true)
      {
        // the standard way of creating an inspector
        //TODO: Comment this line to create an inspector using the provider
        _dockpaneVM._featureInspector = new Inspector();
        #region Inspector UI with Provider
        //TODO: Uncomment this block to create an inspector using the provider
        ////******an alternative way to create an inspector *****
        ////allowing us to customize the grid view

        ////create the provider
        //_dockpaneVM._provider = new MyProvider();
        //if (_dockpaneVM._featureInspector == null)
        //  return;
        ////create the inspector from the provider
        //_dockpaneVM._featureInspector = _dockpaneVM._provider.Create();
        #endregion
        // create the embeddable control from the inspector (to display on the pane)
        var icontrol = _dockpaneVM._featureInspector.CreateEmbeddableControl();

        // get view and viewmodel from the inspector
        _dockpaneVM.InspectorViewModel = icontrol.Item1;
        _dockpaneVM.InspectorView = icontrol.Item2;
      }
      #region Subscribe to Inspector PropertyChanged event
      _dockpaneVM._featureInspector.PropertyChanged += _featureInspector_PropertyChanged;
      #endregion

      #region Get selected features and populate the inspector and tree view
      //Get the building permits layer
      var buildingPermitsLayer = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().
                FirstOrDefault(n => n.Name == "NYC building permits");
      await QueuedTask.Run(() => {
        //get the selected features
        var selectionInMap = map.GetSelection();

        if (buildingPermitsLayer == null) return;
        if (selectionInMap.ToDictionary().ContainsKey(buildingPermitsLayer))
        {
          //get the selected features oid list
          var selectionDictionary = new Dictionary<MapMember, List<long>>();
          selectionDictionary.Add(buildingPermitsLayer, selectionInMap.ToDictionary()[buildingPermitsLayer]);
          //load the first feature into the inspector
          var firstOid = selectionDictionary[buildingPermitsLayer][0];
          _dockpaneVM.AttributeInspector.LoadAsync(buildingPermitsLayer, firstOid);
          // set up a dictionary to hold "PermitRecord" objects for each selected feature
          //This is used to populate the tree view in the dock pane     
          var permitRecordList = new List<PermitRecord>();
          #region Create PermitRecords from the selection
          var queryFilter = new QueryFilter
          {
            ObjectIDs = selectionInMap.ToDictionary()[buildingPermitsLayer].ToArray()
          };
          var cursor = buildingPermitsLayer.Search(queryFilter);

          while (cursor.MoveNext())
          {
            var feature = cursor.Current as Feature;
            if (feature != null)
            {
              var oid = feature.GetObjectID();
              var jobNo = feature["Job_Number"].ToString();
              var jobType = feature["Job_Type"].ToString();
              var address = feature["Address"].ToString();
              var permitRecord = new PermitRecord(oid, jobNo, address, jobType);
              permitRecordList.Add(permitRecord);
            }
          }
          #endregion
          selectionPermitRecordsDictionary.Add(buildingPermitsLayer as MapMember, permitRecordList);
        }
      });
      #endregion
      // assign the dictionary to the view model - notifies the tree view to update
      _dockpaneVM.SelectedMapFeatures = selectionPermitRecordsDictionary;
      pane.Activate();
    }

    #region Binding properties: Business logic

    // Binding property: Property containing an instance for the inspector.
    public Inspector AttributeInspector => _featureInspector;
    // Binding property: Property containing the inspector viewmodel.
    public EmbeddableControl InspectorViewModel
    {
      get => _inspectorViewModel;
      set
      {
        if (value != null)
        {
          _inspectorViewModel = value;
          //Occurs when the control is hosted - this is where the control is opened
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
    // Binding property: Property containing the inspector view.
    public UserControl InspectorView
    {
      get => _inspectorView;
      set => SetProperty(ref _inspectorView, value, () => InspectorView);
    }
    // Binding property: Property containing the treeview itemsource
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

    private static void _featureInspector_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "IsDirty")
      {
        _dockpaneVM.NotifyPropertyChanged(nameof(IsApplyEnabled));
        _dockpaneVM.NotifyPropertyChanged(nameof(IsCancelEnabled));
      }
    }
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class DockpaneInspectorUI_ShowButton : Button
  {
    protected override void OnClick()
    {
      DockpaneInspectorUIViewModel.Show();
    }
  }
}
