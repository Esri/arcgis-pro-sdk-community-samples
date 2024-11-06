/*

   Copyright 2020 Esri

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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Events;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Controls;
using ArcGIS.Desktop.Mapping.Events;

namespace WorkingWithQueryDefinitionFilters
{
  public class DefineQueryDefinitionFiltersViewModel : DockPane
  {
    private const string _dockPaneID = "WorkingWithQueryDefinitionFilters_DefineQueryDefinitionFilters";
    private bool _subscribed;
    private object _lock = new object();

    protected DefineQueryDefinitionFiltersViewModel()
    {
      System.Windows.Data.BindingOperations.EnableCollectionSynchronization(_defintionQueryFilters, _lock);
      HasHelp = true;
    }
    #region overrides and event subscriptions
    /// <summary>
    /// Show the DockPane.
    /// </summary>
    internal static void Show()
    {
      DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
      if (pane == null)
        return;

      pane.Activate();

      var vm = pane as DefineQueryDefinitionFiltersViewModel;
      if (vm != null && MapView.Active != null)
      {
        vm.GetMapMembers(MapView.Active);
        vm.InitializeFilters();
      }
    }
    protected override void OnShow(bool isVisible)
    {
      if (isVisible)
      {
        Module1.Current.DefFilterVM = this;
        if (!_subscribed)
        {
          _subscribed = true;

          // connect to events
          ArcGIS.Desktop.Mapping.Events.ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);
          ArcGIS.Desktop.Mapping.Events.DrawCompleteEvent.Subscribe(OnDrawComplete);
          if (Module1.Current._layersAddedEventoken == null) //don't subscribe if already subscribed
            Module1.Current._layersAddedEventoken = ArcGIS.Desktop.Mapping.Events.LayersAddedEvent.Subscribe(OnLayersAdded);
          if (Module1.Current._tablesAddedEventoken == null) //don't subscribe if already subscribed
            Module1.Current._tablesAddedEventoken = ArcGIS.Desktop.Mapping.Events.StandaloneTablesAddedEvent.Subscribe(OnTablesAdded);
          if (Module1.Current._layersRemovedEventoken == null) //don't subscribe if already subscribed
            Module1.Current._layersRemovedEventoken = ArcGIS.Desktop.Mapping.Events.LayersRemovedEvent.Subscribe(OnLayersRemoved);
          if (Module1.Current._tablesRemovedEventoken == null) //don't subscribe if already subscribed
            Module1.Current._tablesRemovedEventoken = ArcGIS.Desktop.Mapping.Events.StandaloneTablesRemovedEvent.Subscribe(OnTablesRemoved);
          if (Module1.Current._mapMembePropChangedEventtoken == null) //don't subscribe if already subscribed
            Module1.Current._mapMembePropChangedEventtoken = ArcGIS.Desktop.Mapping.Events.MapMemberPropertiesChangedEvent.Subscribe(OnMapMemberPropertiesChanged);
        }
      }
      else
      {
        if (_subscribed)
        {
          _subscribed = false;

          // unsubscribe from events
          ArcGIS.Desktop.Mapping.Events.ActiveMapViewChangedEvent.Unsubscribe(OnActiveMapViewChanged);
          ArcGIS.Desktop.Mapping.Events.DrawCompleteEvent.Unsubscribe(OnDrawComplete);
        }
      }
      base.OnShow(isVisible);
    }

    private void OnTablesRemoved(StandaloneTableEventArgs obj)
    {
      //Update the list of layers shown in the drop down and get their filters
      GetMapMembers(MapView.Active);
      InitializeFilters();
    }

    private void OnTablesAdded(StandaloneTableEventArgs obj)
    {
      //Update the list of layers shown in the drop down and get their filters
      GetMapMembers(MapView.Active);
      InitializeFilters();
    }

    private void OnDrawComplete(MapViewEventArgs obj)
    {
      //TODO:If no feature layers, show empty dockpanes(DockpaneVisibility)
      GetMapMembers(MapView.Active);
      InitializeFilters();
    }

    private void OnLayersRemoved(LayerEventsArgs obj)
    {
      //Update the list of layers shown in the drop down and get their filters
      GetMapMembers(MapView.Active);
      InitializeFilters();
    }

    public void OnLayersAdded(LayerEventsArgs obj)
    {
      //Update the list of layers shown in the drop down and get their filters
      GetMapMembers(MapView.Active);
      InitializeFilters();
    }

    private void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs obj)
    {
      //Module1.Current.DefFilterVM = this; //Remove this.
      GetMapMembers(MapView.Active);
      InitializeFilters();
    }
    public void OnMapMemberPropertiesChanged(MapMemberPropertiesChangedEventArgs obj)
    {
      if (SelectedMapMember == null)
        return;
      if (obj.MapMembers.Contains(SelectedMapMember))
        InitializeFilters();
    }
    #endregion
    #region Binding properties
    /// <summary>
    /// Text shown near the top of the DockPane.
    /// </summary>
    private string _heading = "Definition Query Filters";
    public string Heading
    {
      get { return _heading; }
      set
      {
        SetProperty(ref _heading, value, () => Heading);
      }
    }

    private ObservableCollection<MapMember> _mapMembers = new ObservableCollection<MapMember>();

    public ObservableCollection<MapMember> MapMembers
    {
      get
      {
        return _mapMembers;
      }
    }

    private MapMember _selectedMapMember;
    public MapMember SelectedMapMember
    {
      get { return _selectedMapMember; }
      set
      {
        SetProperty(ref _selectedMapMember, value, () => SelectedMapMember);
        Module1.Current.ActiveFilterExists = false;
        //Get the Definition Filters for the layer
        InitializeFilters();
      }
    }

    private ObservableCollection<DefinitionFilterItem> _defintionQueryFilters = new ObservableCollection<DefinitionFilterItem>();
    public ObservableCollection<DefinitionFilterItem> DefinitionFilters
    {
      get { return _defintionQueryFilters; }

    }

    private DefinitionFilterItem _selectedDefinitionFilter;


    public DefinitionFilterItem SelectedDefinitionFilter
    {

      get { return _selectedDefinitionFilter; }
      set
      {
        SetProperty(ref _selectedDefinitionFilter, value, () => SelectedDefinitionFilter);
        //SaveChanges();
      }
    }

    private Visibility _dockpaneVisibility;
    public Visibility DockpaneVisibility
    {
      get { return _dockpaneVisibility; }
      set
      {
        SetProperty(ref _dockpaneVisibility, value, () => DockpaneVisibility);
      }
    }

    private Visibility _filtersListBoxVisibility;
    public Visibility FiltersListBoxVisibility
    {
      get { return _filtersListBoxVisibility; }
      set
      {
        SetProperty(ref _filtersListBoxVisibility, value, () => FiltersListBoxVisibility);
      }
    }

    #endregion
    #region Commands
    private ICommand _createNewFilterCommand;
    public ICommand CreateNewFilterCommand
    {
      get
      {
        _createNewFilterCommand = new RelayCommand(() => CreateDefinitionFilter());
        return _createNewFilterCommand;
      }
    }

    private void CreateDefinitionFilter()
    {
      //Get the Selected layer
      //Create QueryBuilderControlProperties   
      var queryBuilderControlProps = new QueryBuilderControlProperties
      {
        MapMember = SelectedMapMember,
        EditClauseMode = true,
        AutoValidate = true
      };
      //Show the Query builder Pro Window
      var querybuilderwindow = new QueryBuilderWindow(new DefinitionFilterItem(SelectedMapMember, null), queryBuilderControlProps);
      querybuilderwindow.Owner = FrameworkApplication.Current.MainWindow;
      querybuilderwindow.Closed += (o, e) => { querybuilderwindow = null; };
      querybuilderwindow.ShowDialog();
    }
    #endregion
    #region Private methods
    internal void InitializeFilters()
    {
      DefinitionFilters.Clear();
      Module1.Current.ActiveFilterExists = false; //reinitialize
      if (MapView.Active != null)
      {
        if (SelectedMapMember != null)
        {
          var filters = SelectedMapMember is StandaloneTable ? (SelectedMapMember as StandaloneTable).DefinitionQueries
              : (SelectedMapMember as BasicFeatureLayer).DefinitionQueries;
          if (filters.Count > 0)
          {
            foreach (var filter in filters)
            {
              lock (_lock)
              {
                _defintionQueryFilters.Add(new DefinitionFilterItem(SelectedMapMember, filter));
              }
            }
            SelectedDefinitionFilter = DefinitionFilters[0];
          }
        }
        if (MapMembers.Count > 0)
          FiltersListBoxVisibility = DefinitionFilters.Count == 0 ? Visibility.Hidden : Visibility.Visible;
      }
    }
    private void GetMapMembers(MapView mapView)
    {
      RunOnUiThread(() =>
      {
        MapMembers.Clear();
        if (mapView != null)
        {
          var tocLayers = mapView.Map.GetLayersAsFlattenedList().OfType<BasicFeatureLayer>().ToList();
          var tocTables = mapView.Map.StandaloneTables.ToList();
          if (tocLayers.Count > 0 || tocTables.Count > 0)
          {
            tocLayers.ForEach(x => _mapMembers.Add(x));
            tocTables.ForEach(t => _mapMembers.Add(t));
            var selectedMapMember = GetSelectedMapMemberInTOC();
            if (selectedMapMember != null) //mapmember has been selected
              SelectedMapMember = selectedMapMember; //Show that map member's filters
            else
              SelectedMapMember = MapMembers[0]; //default. Nothing selected in TOC
          }
          //If no feature layers, show empty dockpanes(DockpaneVisibility)
          DockpaneVisibility = MapMembers.Count == 0 ? Visibility.Hidden : Visibility.Visible;
        }
      });
    }

    private MapMember GetSelectedMapMemberInTOC()
    {
      if (MapView.Active == null) return null;
      var lyrSelectedInTOC = MapView.Active.GetSelectedLayers().OfType<FeatureLayer>().FirstOrDefault();
      var tableSelectedInTOC = MapView.Active.GetSelectedStandaloneTables().FirstOrDefault();
      if (lyrSelectedInTOC == null && tableSelectedInTOC == null) return null;
      var mapMemberSelectedInTOC = lyrSelectedInTOC == null ? tableSelectedInTOC as MapMember : lyrSelectedInTOC as MapMember;

      return mapMemberSelectedInTOC;

    }

    #endregion

    #region Threading Utilities

    /// <summary>
    /// utility function to enable an action to run on the UI thread (if not already)
    /// </summary>
    /// <param name="action">the action to execute</param>
    /// <returns></returns>
    internal static Task RunOnUiThread(Action action)
    {
      if (OnUIThread)
      {
        action();
        return Task.FromResult(0);
      }
      else
        return Task.Factory.StartNew(action, System.Threading.CancellationToken.None, TaskCreationOptions.None, QueuedTask.UIScheduler);
    }

    /// <summary>
    /// determines if the application is currently on the UI thread
    /// </summary>
    private static bool OnUIThread
    {
      get
      {
        if (FrameworkApplication.TestMode)
          return QueuedTask.OnWorker;
        else
          return System.Windows.Application.Current.Dispatcher.CheckAccess();
      }
    }

    #endregion

  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
  internal class DefineQueryDefinitionFilters_ShowButton : Button
  {
    protected override void OnClick()
    {
      DefineQueryDefinitionFiltersViewModel.Show();
    }
  }
}
