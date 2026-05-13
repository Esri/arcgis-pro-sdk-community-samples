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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Controls;
using ArcGIS.Desktop.Mapping.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QueryBuilderControl
{

  internal class DefinitionQueryDockPaneViewModel : DockPane
  {
    private const string _dockPaneID = "QueryBuilderControl_DefinitionQueryDockPane";

    public static DefinitionQueryDockPaneViewModel Current = null;
    protected DefinitionQueryDockPaneViewModel() {
    
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

      var vm = pane as DefinitionQueryDockPaneViewModel;
      if (vm != null && MapView.Active != null)
      {
        vm.BuildControlProperties(MapView.Active);
      }
    }

    private bool _subscribed = false;

    /// <summary>
    /// When visibility of dockpane changes, subscribe or unsubscribe from events.
    /// </summary>
    /// <param name="isVisible">is the dockpane visible?</param>
    protected override void OnShow(bool isVisible)
    {
      if (isVisible)
      {
        if (!_subscribed)
        {
          _subscribed = true;

          // connect to events
          ArcGIS.Desktop.Mapping.Events.TOCSelectionChangedEvent.Subscribe(OnSelectedLayersChanged);
          MapMemberPropertiesChangedEvent.Subscribe(OnMapMemberPropertiesChanged);
          ArcGIS.Desktop.Core.Events.ProjectClosingEvent.Subscribe(OnProjectClosing);
        }
      }
      else
      {
        if (_subscribed)
        {
          _subscribed = false;

          // unsubscribe from events
          ArcGIS.Desktop.Mapping.Events.TOCSelectionChangedEvent.Unsubscribe(OnSelectedLayersChanged);
          MapMemberPropertiesChangedEvent.Unsubscribe(OnMapMemberPropertiesChanged);
          ArcGIS.Desktop.Core.Events.ProjectClosingEvent.Unsubscribe(OnProjectClosing);
        }
      }
      base.OnShow(isVisible);
    }

    #region Properties

    /// <summary>
    /// Gets and sets the QueryBuilderControlProperties to bind to the QueryBuilderControl.
    /// </summary>
    private DefinitionQueryBuilderControlProperties _props = null;
    public DefinitionQueryBuilderControlProperties ConfigureControl
    {
      get { return _props; }
      set { 
        SetProperty(ref _props, value); 
      }
    }


    /// <summary>
    /// Gets and sets the name of currently selected mapMember.
    /// </summary>
    private string _mapMemberName;
    public string MapMemberName
    {
      get { return _mapMemberName; }
      set { SetProperty(ref _mapMemberName, value); }
    }
    private MapMember _mapMember;
    public MapMember MapMember
    {
      get { return _mapMember; }
      set { SetProperty(ref _mapMember, value); }
    }
    /// <summary>
    /// Gets the Apply command to write query definition to mapMember.
    /// </summary>
    private RelayCommand _applyCommand;
    public ICommand ApplyCommand
    {
      get
      {
        if (_applyCommand == null)
        {
          _applyCommand = new RelayCommand(() => ApplyChanges());
        }
        return _applyCommand;
      }
    }

    #endregion

    #region Events

    /// <summary>
    /// Event handler for ProjectClosing event.
    /// </summary>
    /// <param name="args">The ProjectClosing arguments.</param>
    /// <returns></returns>
    private Task OnProjectClosing(ArcGIS.Desktop.Core.Events.ProjectClosingEventArgs args)
    {
      // if already Canceled, ignore
      if (args.Cancel)
        return Task.CompletedTask;

      return Task.CompletedTask;
    }

    /// <summary>
    /// Event handler for TOCSelectionChangedEvent event
    /// </summary>
    /// <param name="args">The event arguments.</param>
    private void OnSelectedLayersChanged(MapViewEventArgs args)
    {
      // set up for the next selected mapMember
      BuildControlProperties(args.MapView);
    }
    private DelayedInvoker _invoker = new DelayedInvoker(50);
    private void OnMapMemberPropertiesChanged(MapMemberPropertiesChangedEventArgs args)
    {
      // check eventHint for ones we care about
      if (!args.EventHints.Contains(MapMemberEventHint.DefinitionQuery) && !args.EventHints.Contains(MapMemberEventHint.Any))
        return;

      //get all the mapMembers that have changed
      List<MapMember> changedMapMembers = args.MapMembers.ToList();
      //check if the list of mapMembers changed name is the same as the MapMemberName property
      var changedMapMember = changedMapMembers.FirstOrDefault(m => m.Name == MapMemberName);
      if (changedMapMember == null)
        return;
      _invoker.Invoke(() =>
      {
        //Populate the definition query control if the mapMember is the same
        // must run on UI thread
        Utilities.RunOnUIThread(() => BuildControlProperties(changedMapMember));
      });      
    }
    #endregion

    /// <summary>
    /// Build a QueryBuilderControlProperties for the specified mapView.  Finds the first BasicFeatureLayer or StandAloneTable highlighted in the TOC.
    /// </summary>
    /// <param name="mapView">a mapView.</param>
    private void BuildControlProperties(MapView mapView)
    {
      MapMember mapMember = null;

      if (mapView != null)
      {
        // only interested in basicFeatureLayers ... they are the ones with definition queries
        var selectedTOCLayers = mapView.GetSelectedLayers().OfType<BasicFeatureLayer>();
        var selectedTOCTables = mapView.GetSelectedStandaloneTables();

        // take layers over tables... but only take the first
        if (selectedTOCLayers.Count() > 0)
          mapMember = selectedTOCLayers.First();
        else if (selectedTOCTables.Count() > 0)
          mapMember = selectedTOCTables.First();
      }

      // build the control properties
      BuildControlProperties(mapMember);
    }

    /// <summary>
    /// Initialize a QueryBuilderControlProperties with the specified mapMember.  Use the current definition query of that mapMember (if it exists) to extend the
    /// initialization.
    /// </summary>
    /// <param name="mapMember">MapMember to initialize the QueryBuilderControlProperties. </param>
    private  void BuildControlProperties(MapMember mapMember)
    {
      try
      {
        DefinitionQuery activeDefinitionQuery = null;
        BasicFeatureLayer fLayer = mapMember as BasicFeatureLayer;
        StandaloneTable table = mapMember as StandaloneTable;
        List<DefinitionQuery> definitionQueries = null;
        if (fLayer != null)
        {
          definitionQueries = fLayer.DefinitionQueries.ToList();
          activeDefinitionQuery = fLayer.ActiveDefinitionQuery;
        }
        else if (table != null)
        {
          definitionQueries = table.DefinitionQueries.ToList();
          activeDefinitionQuery = table.ActiveDefinitionQuery;
        }

        // create it
        var props = new DefinitionQueryBuilderControlProperties()
        {
          MapMember = mapMember,
          DefinitionQueries = definitionQueries,
          ActiveDefinitionQuery = activeDefinitionQuery
        };
        //remove any existing definition queries from the control
        //var userControl = DefinitionQueryDockPaneView.Current;
        //userControl.DefinitionQueryBuilderControl.RemoveAllDefinitionQueries();

        // set the binding properties
        // ConfigureControl = null;
        if (this.ConfigureControl != null && ConfigureControl.DefinitionQueries != null)
          this.ConfigureControl.DefinitionQueries.Clear();
        this.ConfigureControl = props;
        MapMemberName = mapMember?.Name ?? "";
        MapMember = mapMember;
      }
      catch (Exception ex)
      {
        MessageBox.Show("Exception in BuildControlProperties: " + ex.Message, "DefinitionQueryDockPaneViewModel");
        //throw;
      }
    }


    /// <summary>
    /// Saves the current expression to the appropriate mapMember according to user response. 
    /// </summary>
    private async void ApplyChanges()
    {
      //access the usercontrol to get the properties
      var userControl = DefinitionQueryDockPaneView.Current; 
      var myControlsQueries = userControl.DefinitionQueryBuilderControl.DefinitionQueries;
      string myControlsActiveQuery = null;
      if (userControl.DefinitionQueryBuilderControl.ActiveDefinitionQuery != null)
        myControlsActiveQuery = userControl.DefinitionQueryBuilderControl.ActiveDefinitionQuery.Name;

      var fLayer = ConfigureControl.MapMember as BasicFeatureLayer;
      var table = ConfigureControl.MapMember as StandaloneTable;
      await QueuedTask.Run( () => {

        if (fLayer != null)
        {
          fLayer.RemoveAllDefinitionQueries();
          fLayer.InsertDefinitionQueries(myControlsQueries);
          fLayer.SetActiveDefinitionQuery(myControlsActiveQuery);
        }

        else if (table != null)
        {
          table.RemoveAllDefinitionQueries();
          table.InsertDefinitionQueries(myControlsQueries);
          table.SetActiveDefinitionQuery(myControlsActiveQuery);
        }
      });
    }
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class DefinitionQueryDockPane_ShowButton : Button
  {
    protected override void OnClick()
    {
      DefinitionQueryDockPaneViewModel.Show();
    }
  }
}
