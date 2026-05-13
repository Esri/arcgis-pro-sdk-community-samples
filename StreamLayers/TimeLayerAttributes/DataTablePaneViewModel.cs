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
using ActiproSoftware.Windows.Extensions;
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
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLayerAttributes
{
  internal class DataTablePaneViewModel : DockPane
  {
    #region Private Properties
    private const string _dockPaneID = "TimeLayerAttributes_DataTablePane";
    private static readonly object _lockObject = new();

    private static readonly DateTime StartTime = new DateTime(2013, 12, 1);
    private static readonly DateTime EndTime = new DateTime(2013, 12, 2);
    private static readonly TimeDelta timeDelta = new TimeDelta(1, TimeUnit.Days);
    private static readonly string TimeLayerName = "Crimes";

    private static readonly List<TimeRange> eventTimeRanges = [];

    // properties
    private string _heading = "Time layer attributes";
    private FeatureLayer _featureLayer;
    private string _timeField;
    private string _featureLayerName;
    private IEnumerable<dynamic> _theRowEnumeration = [];
    private List<string> _columnNames = [];
    private ObservableCollection<System.Windows.Controls.DataGridColumn> _ColumnCollection = [];
    #endregion Private Properties

    protected DataTablePaneViewModel()
    {
      //System.Windows.Data.BindingOperations.EnableCollectionSynchronization(TheRowEnumeration, _lockObject);
      MapViewTimeChangedEvent.Subscribe(OnMapViewTimeChanged);
    }

    #region Event Handler

    private object _isBusyLock = new object();

    private void OnMapViewTimeChanged(MapViewTimeChangedEventArgs args)
    {
      TimeRange lastTimeRange = null;
      lock (_isBusyLock)
      {
        eventTimeRanges.Add(args.CurrentTime);
        lastTimeRange = eventTimeRanges.Last();
      }
      // process the last time range
      QueuedTask.Run(() =>
      {
        try
        {
          if (_featureLayer == null) return;
          List<dynamic> resultRows = [];
          // Get the features in the time range
          using var rowCursor = _featureLayer.Search(null, lastTimeRange);
          while (rowCursor.MoveNext())
          {
            using var row = rowCursor.Current;
            // Copy data
            var rowValues = new Dictionary<string, object>();
            foreach (var columnName in _columnNames)
            {
              rowValues.Add(columnName, row[columnName]);
            }
            var dyn = ConvertToDynamic.ToDynamic(rowValues);
            resultRows.Add(dyn);
          }
          RunOnUIThread(() =>
          {
            // update the UI
            TheRowEnumeration = resultRows;
            // update the heading
            Heading = $"Time layer attributes ({lastTimeRange.Start} - {lastTimeRange.End})";
          });
        }
        catch (Exception ex)
        {
          System.Diagnostics.Debug.WriteLine($"Error in OnMapViewTimeChanged: {ex.Message}");
        }
      });
    }

    #endregion Event Handler

    #region Commands

    public System.Windows.Input.ICommand GetReady
    {
      get
      {
        return new RelayCommand(async () =>
        {
          try
          {
            eventTimeRanges.Clear();
            // Find the time aware layer in the map
            var result = await QueuedTask.Run<(FeatureLayer featLayer, TimeParameters timeParams)>(() =>
            {
              var layers = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>();
              if (layers == null) return (null, null);
              // Determine if that layer has a time enabled field
              foreach (var layer in layers)
              {
                if (layer.IsTimeSupported())
                {
                  if (layer.Name != TimeLayerName) continue;
                  System.Diagnostics.Debug.WriteLine($"This layer is time enabled");
                  var timeParameters = layer.GetTime();
                  return (layer, timeParameters);
                }
              }
              return (null, null);
            });
            if (result.featLayer == null)
            {
              System.Diagnostics.Debug.WriteLine($"No time enabled layer found");
              FeatureLayerName = "No time enabled layer";
              TimeField = "N/A";
              return;
            }
            // update on the UI thread
            FeatureLayerName = result.featLayer.Name;
            _featureLayer = result.featLayer;
            TimeField = result.timeParams.StartTimeFieldName;

            // Create Column Headers
            var colNames = await GetColumnsFromFeatureLayerAsync(_featureLayer);
            _columnNames = colNames.colNames;
            _ = RunOnUIThread(() =>
            {
              var rowValues = new Dictionary<string, object>();
              var columns = new ObservableCollection<System.Windows.Controls.DataGridTextColumn>();
              foreach (var name in _columnNames)
              {
                var column = new System.Windows.Controls.DataGridTextColumn
                {
                  Header = colNames.aliasNames[name].ToString(),
                  Binding = new System.Windows.Data.Binding(name.ToString())
                };
                columns.Add(column);
                rowValues.Add(name, "Test");
              }
              ColumnCollection.Clear();
              ColumnCollection.AddRange (columns);
            });
            MapView.Active.Time.Start = StartTime;
            MapView.Active.Time.End = EndTime;
            MapView.Active.Time.Offset(timeDelta);
          }
          catch (Exception ex)
          {
            System.Diagnostics.Debug.WriteLine($"Error in GetReady: {ex.Message}");
          }
        }, () => true);
      }
    }

    #endregion Commands

    #region Properties

    /// <summary>
    /// The subject feature layer of the DockPane
    /// </summary>
    public FeatureLayer FeatureLayer
    {
      get => _featureLayer;
      set => SetProperty(ref _featureLayer, value);
    }

    /// <summary>
    /// The name of the subject feature layer
    /// </summary>
    public string FeatureLayerName
    {
      get => _featureLayerName;
      set => SetProperty(ref _featureLayerName, value);
    }

    /// <summary>
    /// The name of the time field in the subject feature layer
    /// </summary>
    public string TimeField
    {
      get => _timeField;
      set => SetProperty(ref _timeField, value);
    }

    /// <summary>
    /// TheRowEnumeration property bound to DataGrid's ItemsSource
    /// this is an enumerable of dynamic objects (each object is a row)
    /// </summary>
    public IEnumerable<dynamic> TheRowEnumeration
    {
      get => ((_ColumnCollection.Count == 0)
        ? [] 
        : _theRowEnumeration);
      set => SetProperty(ref _theRowEnumeration, value);
    }

    /// <summary>
    /// TheColumnNames property bound to DataGrid's Columns
    /// because the columns are bound using DataGridColumnsBehavior the datatype 
    /// has to be DataGridColumn
    /// </summary>
    public ObservableCollection<System.Windows.Controls.DataGridColumn> ColumnCollection
    {
      get => _ColumnCollection;
      set => SetProperty(ref _ColumnCollection, value);
    }

    /// <summary>
    /// Text shown near the top of the DockPane.
    /// </summary>
    public string Heading
    {
      get => _heading;
      set => SetProperty(ref _heading, value);
    }

    #endregion Properties

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

    #region Helper functions

    private async Task<(List<string> colNames, Dictionary<string,string> aliasNames)> 
      GetColumnsFromFeatureLayerAsync(FeatureLayer featureLayer)
    {
      return await QueuedTask.Run(() =>
      {
        Dictionary<string, string> aliasNames = [];
        var tableDefinition = _featureLayer.GetTable().GetDefinition();
        var colNames = new List<string>();
        foreach (var field in tableDefinition.GetFields())
        {
          if (field.FieldType == FieldType.Geometry) continue;
          if (field.FieldType == FieldType.Blob) continue;
          if (field.FieldType == FieldType.Raster) continue;
          if (field.FieldType == FieldType.GlobalID) continue;
          if (field.FieldType == FieldType.OID) continue;
          colNames.Add(field.Name);
          aliasNames.Add(field.Name, field.AliasName);
        }
        return (colNames, aliasNames);
      });
    }

    /// <summary>
    /// Utility function to enable an action to run on the UI thread (if not already)
    /// </summary>
    /// <param name="action">the action to execute</param>
    /// <returns></returns>
    internal static Task RunOnUIThread(Action action)
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
    /// Determines if the application is currently on the UI thread
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

    #endregion Helper functions
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class DataTablePane_ShowButton : Button
  {
    protected override void OnClick()
    {
      DataTablePaneViewModel.Show();
    }
  }
}
