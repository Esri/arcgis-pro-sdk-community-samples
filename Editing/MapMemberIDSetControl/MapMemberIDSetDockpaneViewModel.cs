/*

   Copyright 2026 Esri

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
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Events;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Mapping.CommonControls;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace MapMemberIDSetControl
{

  internal class MapMemberIDSetDockpaneViewModel : DockPane
  {
    private const string _dockPaneID = "MapMemberIDSetControl_MapMemberIDSetDockpane";

    Map _activeMap;

    private object _lock = new object();

    protected MapMemberIDSetDockpaneViewModel() 
    {
      System.Windows.Data.BindingOperations.EnableCollectionSynchronization(_layers, _lock);

      LayersAddedEvent.Subscribe(OnLayersAdded);
      LayersRemovedEvent.Subscribe(OnLayersRemoved);
      ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);
    }

    ~MapMemberIDSetDockpaneViewModel()
    {
      LayersAddedEvent.Unsubscribe(OnLayersAdded);
      LayersRemovedEvent.Unsubscribe(OnLayersRemoved);
      ActiveMapViewChangedEvent.Unsubscribe(OnActiveMapViewChanged);
    }

    /// <summary>
    /// Called when the dock pane is first initialized.
    /// </summary>
    protected override Task InitializeAsync()
    {
      if (MapView.Active == null)
        return Task.FromResult(0);

      _activeMap = MapView.Active.Map;
      UpdateForActiveMap();
      return Task.CompletedTask;
    }


    /// <summary>
    /// Show the DockPane.
    /// </summary>
    internal static void Show()
    {
      DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
      if (pane == null)
        return;

      pane.Activate();
    }

    #region Bindable Properties

    public Map Map
    {
      get => _activeMap;
      set => SetProperty(ref _activeMap, value);
    }

    private ObservableCollection<BasicFeatureLayer> _layers = new ObservableCollection<BasicFeatureLayer>();
    public ObservableCollection<BasicFeatureLayer> Layers => _layers;

    private BasicFeatureLayer _selectedLayer;
    public BasicFeatureLayer SelectedLayer
    {
      get => _selectedLayer; 
      set => SetProperty(ref _selectedLayer, value);
    }

    private string _whereClause = "";
    public string WhereClause
    {
      get => _whereClause; 
      set
      {
        SetProperty(ref _whereClause, value);
        IsValid = false;
        HasError = false;
      }
    }

    private bool _hasError = false;
    public bool HasError
    {
      get => _hasError; 
      set => SetProperty(ref _hasError, value);
    }

    private bool _isValid = false;
    public bool IsValid
    {
      get => _isValid; 
      set => SetProperty(ref _isValid, value);
    }

    private RelayCommand _verifyExpressionCmd;
    public ICommand VerifyExpressionCmd
    {
      get
      {
        if (_verifyExpressionCmd == null)
        {
          _verifyExpressionCmd = new RelayCommand(() => QueuedTask.Run(() => ValidateExpresion(true)), () => { return MapView.Active != null && SelectedLayer != null && WhereClause != ""; });
        }
        return _verifyExpressionCmd;
      }
    }

    private RelayCommand _clearExpressionCmd;
    public ICommand ClearExpressionCmd
    {
      get
      {
        if (_clearExpressionCmd == null)
        {
          _clearExpressionCmd = new RelayCommand(() => Clear(), () => { return WhereClause != ""; });
        }
        return _clearExpressionCmd;
      }
    }

    private MapMemberIDSet _mapMemberIDSet;
    public MapMemberIDSet MapMemberIDSet
    {
      get => _mapMemberIDSet;
      set => SetProperty(ref _mapMemberIDSet, value);
    }

    public ImageSource VerifyExpressionImage => System.Windows.Application.Current.Resources["GenericCheckMark16"] as ImageSource; 
    public ImageSource ClearExpressionImage => System.Windows.Application.Current.Resources["GenericEraser16"] as ImageSource;
    public ImageSource GenericErrorImage => System.Windows.Application.Current.Resources["GenericError16"] as ImageSource;
    public ImageSource GenericCheckMarkImage => System.Windows.Application.Current.Resources["GenericCheckMark16"] as ImageSource;

    #endregion


    #region Event Handling

    private void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs args)
    {
      Map = args.IncomingView?.Map;
      if (Map == null)
      {
        SelectedLayer = null;
        _layers.Clear();
      }
      else
        UpdateForActiveMap();
    }

    private void OnLayersRemoved(LayerEventsArgs args)
    {
      foreach (var layer in args.Layers)
      {
        if (layer.Map == Map)
        {
          if (Layers.Contains(layer))
            Layers.Remove((BasicFeatureLayer)layer);
        }
      }

      if (SelectedLayer == null)
      {
        SelectedLayer = Layers.FirstOrDefault();
      }
    }

    private void OnLayersAdded(LayerEventsArgs args)
    {
      foreach (var layer in args.Layers)
      {
        if (layer.Map == _activeMap && layer is BasicFeatureLayer)
        {
          lock (_lock)
          {
            Layers.Add((BasicFeatureLayer)layer);
          }
          if (SelectedLayer == null)
            SelectedLayer = (BasicFeatureLayer)layer;
        }
      }
    }
    #endregion

    private void Clear()
    {
      WhereClause = "";
      MapMemberIDSet = null;
    }

    private void UpdateForActiveMap(bool activeMapChanged = true)
    {
      if (activeMapChanged)
      {
        RefreshLayerCollection();

        if (SelectedLayer == null)
          SelectedLayer = Layers.FirstOrDefault();
      }
    }

    private void RefreshLayerCollection()
    {
      Layers.Clear();
      if (_activeMap == null)
        return;

      var layers = _activeMap.GetLayersAsFlattenedList().OfType<BasicFeatureLayer>();
      lock (_lock)
      {
        foreach (var layer in layers)
        {
          Layers.Add(layer);
        }
      }
    }

    internal bool ValidateExpresion(bool showValidationSuccessMsg)
    {
      // determine if the whereClause is valid by performing a search on the layer
      // and trapping for any exceptions
      try
      {
        var dict = new Dictionary<MapMember, List<long>>();
        var oids = new List<long>();
        var qf = new QueryFilter() { WhereClause = WhereClause };
        using (var cursor = SelectedLayer.Search(qf))
        {
          // iterate and collect the OIDs
          while (cursor.MoveNext())
          {
            var row = cursor.Current;
            oids.Add(row.GetObjectID());
          }
          dict.Add(SelectedLayer, oids);
        }

        // build a MapMemberIDSet from the query results
        MapMemberIDSet = MapMemberIDSet.FromDictionary(dict);
      }
      catch (Exception)
      {
        HasError = true;
        IsValid = false;
        MapMemberIDSet = null;
        return false;
      }

      if (showValidationSuccessMsg)
        IsValid = true;
      HasError = false;
      return true;
    }
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class MapMemberIDSetDockpane_ShowButton : Button
  {
    protected override void OnClick()
    {
      MapMemberIDSetDockpaneViewModel.Show();
    }
  }
}
