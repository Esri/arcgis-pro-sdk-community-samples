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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping.Controls;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Mapping.Events;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.Windows.Input;

namespace QueryBuilderControl
{

  internal class DefinitionQueryDockPaneViewModel : DockPane
  {
    private const string _dockPaneID = "QueryBuilderControl_DefinitionQueryDockPane";


    private string _origExpression;

    protected DefinitionQueryDockPaneViewModel() { }

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
        vm.ClearControlProperties();
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
          ArcGIS.Desktop.Core.Events.ProjectClosingEvent.Unsubscribe(OnProjectClosing);
        }
      }
      base.OnShow(isVisible);
    }

    #region Properties

    /// <summary>
    /// Gets and sets the QueryBuilderControlProperties to bind to the QueryBuilderControl.
    /// </summary>
    private QueryBuilderControlProperties _props = null;
    public QueryBuilderControlProperties ControlProperties
    {
      get { return _props; }
      set { SetProperty(ref _props, value); }
    }

    /// <summary>
    /// Gets and sets the query expression in the QueryBuilderControl.
    /// </summary>
    private string _expression = string.Empty;
    public string Expression
    {
      get { return _expression; }
      set { _expression = value; }     // doesn't bind in xaml so no need to worry about NotifyPropertyChanged
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

    /// <summary>
    /// Gets the Apply command to write query definition to mapMember.
    /// </summary>
    private RelayCommand _applyCommand;
    public ICommand ApplyCommand
    {
      get
      {
        if (_applyCommand == null)
          _applyCommand = new RelayCommand(() => SaveChanges(), CanSaveChanges);

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

      // save current changes
      SaveChanges();

      // reset the control
      ClearControlProperties();

      return Task.CompletedTask;
    }

    /// <summary>
    /// Event handler for TOCSelectionChangedEvent event
    /// </summary>
    /// <param name="args">The event arguments.</param>
    private void OnSelectedLayersChanged(MapViewEventArgs args)
    {
      // save current changes
      SaveChanges();

      // set up for the next selected mapMember
      BuildControlProperties(args.MapView);
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
    private void BuildControlProperties(MapMember mapMember)
    {
      // find the current definition query for the mapMember
      string expression = "";
      BasicFeatureLayer fLayer = mapMember as BasicFeatureLayer;
      StandaloneTable table = mapMember as StandaloneTable;
      if (fLayer != null)
        expression = fLayer.DefinitionQuery;
      else if (table != null)
        expression = table.DefinitionQuery;

      // create it
      var props = new QueryBuilderControlProperties()
      {
        MapMember = mapMember,
        Expression = expression,
      };
      // set the binding properties
      this.ControlProperties = props;
      MapMemberName = mapMember?.Name ?? "";

      // keep track of the original expression
      _origExpression = expression;
    }

    /// <summary>
    /// Use a null mapMember to reset the QueryBuilderControlProperties.
    /// </summary>
    private void ClearControlProperties()
    {
      // reset the control
      MapMember mapMember = null;
      BuildControlProperties(mapMember);
    }

    /// <summary>
    /// Has the current expression been altered?  
    /// </summary>
    /// <returns>true if the current expression has been altered. False otherwise.</returns>
    private bool CanSaveChanges()
    {
      string newExpression = Expression ?? "";

      return (string.Compare(_origExpression, newExpression) != 0);
    }

    /// <summary>
    /// Saves the current expression to the appropriate mapMember according to user response. 
    /// </summary>
    private void SaveChanges()
    {
      // get the new expression
      string newExpression = Expression ?? "";

      // is it different?
      if (string.Compare(_origExpression, newExpression) != 0)
      {
        if (ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Expression has changed. Do you wish to save it?", "Definition Query", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
        {
          // update internal var
          _origExpression = newExpression;

          var fLayer = ControlProperties.MapMember as BasicFeatureLayer;
          var table = ControlProperties.MapMember as StandaloneTable;

          // update mapMember definition query
          QueuedTask.Run(() =>
          {
            if (fLayer != null)
              fLayer.SetDefinitionQuery(newExpression);
            else if (table != null)
              table.SetDefinitionQuery(newExpression);
          });
        }
      }
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
