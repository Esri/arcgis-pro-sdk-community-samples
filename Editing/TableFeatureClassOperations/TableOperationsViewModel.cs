/*

   Copyright 2023 Esri

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
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Editing.Controls;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using static ArcGIS.Core.Data.NetworkDiagrams.ReshapeEdgesDiagramLayoutParameters;

namespace TableFeatureClassOperations
{
  internal class TableOperationsViewModel : DockPane
  {
    private const string _dockPaneID = "TableFeatureClassOperations_TableOperations";

    private ObservableCollection<StandaloneTable> _tables = new();
    private Uri _logTableUri = null;
    private static readonly object _theLock = new();

    protected TableOperationsViewModel()
    {
      BindingOperations.EnableCollectionSynchronization(_tables, _theLock);      
      
      //subscribe to events to populate snap layer list when the map changes, layers added/removed
      ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);
      
      StandaloneTablesAddedEvent.Subscribe(OnStandaloneTablesAddRem);
      StandaloneTablesRemovedEvent.Subscribe(OnStandaloneTablesAddRem);
           
      PopulateTableList();
    }

    protected override async Task<Task> InitializeAsync()
    {
      try
      {
        if (MapView.Active != null)
          ValidateLogTableState();
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Error creating log table");
      }
      return base.InitializeAsync();
    }

    private void ValidateLogTableState()
    {
      _ = QueuedTask.Run(() =>
      {
        // check the active map view to see if the log table exists
        var logStandaloneTable = MapView.Active.Map.GetStandaloneTablesAsFlattenedList().FirstOrDefault(t => t.Name == Module1.LogTableName);
        if (logStandaloneTable != null) return;

        // Create the log table
        Module1.CreateLogTable();
        // Get the log table
        var logTable = Module1.GetLogTable();
        // add the log table to the map
        StandaloneTableFactory.Instance.CreateStandaloneTable(new StandaloneTableCreationParams(logTable), MapView.Active.Map);
        //var ds = logTable.GetDatastore();
        //var fullPath = GetFullPath(Module1.LogTableName, ds);
        //_logTableUri = new Uri(fullPath);
      });
    }

    /// <summary>
    /// Call from MCT.
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="datastore"></param>
    /// <returns></returns>
    public static string GetFullPath (string tableName, Datastore datastore)
    {
      try
      {
        var workspaceNameDef = datastore.GetConnectionString();
        var workspaceName = workspaceNameDef.Split('=')[1];
        var fullSpec = System.IO.Path.Combine(workspaceName, tableName);
        return fullSpec;
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.ToString());
        return string.Empty;
      }
    }

    private void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs obj)
    {
      if (obj.IncomingView == null)
        return;
      //populate the table list in the incoming active map
      PopulateTableList();
      ValidateLogTableState();
    }

    private void OnStandaloneTablesAddRem(StandaloneTableEventArgs eventArgs)
    {
      //regenerate snaplist when layers are added or removed
      //run on UI Thread to sync layersadded event (which runs on background)
      System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() => { PopulateTableList(); }));
    }

    internal void PopulateTableList()
    {
      if (MapView.Active == null)
        return;

      Tables.Clear();
      var lstTables = MapView.Active.Map.GetStandaloneTablesAsFlattenedList();
      Tables.AddRange(new ObservableCollection<StandaloneTable> (lstTables));
    }

    #region Properties

    /// <summary>
    /// Gets the tables for the given GDB
    /// </summary>
    public ObservableCollection<StandaloneTable> Tables
    {
      get
      {
        return _tables;
      }
    }

    private TableControlContent _tableContent;
    public TableControlContent TableContent
    {
      get { return _tableContent; }
      set { SetProperty(ref _tableContent, value); }
    }

    private StandaloneTable _selectedTable;
    public StandaloneTable SelectedTable
    {
      get { return _selectedTable; }
      set
      {
        SetProperty(ref _selectedTable, value);
        if (_selectedTable == null)
          return;
        if (TableControlContentFactory.IsMapMemberSupported(_selectedTable))
        {
          // create the content
          var tableContent = TableControlContentFactory.Create(_selectedTable);

          // assign it
          if (tableContent != null)
          {
            this.TableContent = tableContent;
          }
        }
      }
    }

    public ICommand CmdShowOperationLog
    {
      get
      {
        return new RelayCommand(() =>
        {
          try
          {
            var logStandaloneTable = MapView.Active.Map.GetStandaloneTablesAsFlattenedList().FirstOrDefault(t => t.Name == Module1.LogTableName);
            if (logStandaloneTable == null)
            {
              MessageBox.Show("Log standalone table not found");
              return;
            }
            var vm = new ProWindowChangeLogViewModel();
            var changeLog = new ProWindowChangeLog
            {
              Owner = FrameworkApplication.Current.MainWindow,
              DataContext = vm
            };
            vm.TableContent = TableControlContentFactory.Create(logStandaloneTable);
            changeLog.Closed += (o, e) =>
            {
              // in case special processing after dialog closed has be done
              System.Diagnostics.Debug.WriteLine("Pro Window Dialog closed");
            };
            var dlgResult = changeLog.ShowDialog();
          }
          catch (Exception ex)
          {
            MessageBox.Show(ex.ToString());
          }
        });
      }
    }

    public ICommand CmdFindMax
    {
      get
      {
        return new RelayCommand(async () =>
        {
          // use the selected feature layer to find the max value of the first numeric field
          try
          {
            double maxVal = 0.0;
            await QueuedTask.Run(() =>
            {
              // first find the int field
              bool Ignore = false;
              var numericField = GetFieldByFieldType(SelectedTable, FieldType.Integer, ref Ignore);

              // Create StatisticsDescriptions
              StatisticsDescription numMaxDesc = new(numericField, new List<StatisticsFunction>() { StatisticsFunction.Max });

              // Create TableStatisticsDescription
              TableStatisticsDescription tableStatisticsDescription = new(new List<StatisticsDescription>() { numMaxDesc });

              // Calculate Statistics
              IReadOnlyList<TableStatisticsResult> statisticsResults = SelectedTable.GetTable().CalculateStatistics(tableStatisticsDescription);

              // Code to process result
              foreach (TableStatisticsResult statisticsResult in statisticsResults)
              {
                maxVal = statisticsResult.StatisticsResults[0].Max;
              }
              MessageBox.Show($"Max {numericField.Name} Value: {maxVal}");
            });
          }
          catch (Exception ex)
          {
            MessageBox.Show(ex.ToString());
          }
        });
      }
    }

    /// <summary>
    /// Command to duplicate selected records:
    /// - the selected records are duplicated
    /// - the new records are marked with a time-stamp
    /// - the originally selected records have a string field updated with is a "Duplicated: " prefix
    /// - a log record is added to the log table for each duplicated record and new record
    /// </summary>
    public ICommand CmdDuplicate
    {
      get
      {
        return new RelayCommand(async () =>
        {
          try
          {
            var logStandaloneTable = MapView.Active.Map.GetStandaloneTablesAsFlattenedList().FirstOrDefault(t => t.Name == Module1.LogTableName);
            if (logStandaloneTable == null)
            {
              MessageBox.Show("Log standalone table not found");
              return;
            }
            if (SelectedTable == null)
            {
              MessageBox.Show("No table selected");
              return;
            }
            await QueuedTask.Run(() =>
            {
              // get the selected records that need to be duplicated
              var sourceSelection = SelectedTable.GetSelection();
              if (sourceSelection.GetCount() == 0)
              {
                MessageBox.Show($@"No records in '{SelectedTable.Name}' selected");
                return;
              }
              // Get the first string field in SelectedTable in order to update the text later
              bool Ignore = false;
              var stringField = GetFieldByFieldType(SelectedTable, FieldType.String, ref Ignore);
              // Get the first string field in SelectedTable in order to update the text later
              bool IsDateField = false;
              var dateField = GetFieldByFieldType(SelectedTable, FieldType.Date, ref IsDateField);
              // Create edit operation
              var duplicateOperation = new EditOperation
              {
                Name = "Duplicate records",
                SelectModifiedFeatures = false,
                SelectNewFeatures = true
              };
              // Copy all selected records
              duplicateOperation.Copy(SelectedTable, SelectedTable, sourceSelection.GetObjectIDs().ToList());
              // Execute the operation
              var editResult = duplicateOperation.Execute();
              if (editResult != true || duplicateOperation.IsSucceeded != true)
                throw new Exception($@"Duplicate Edit step failed: {duplicateOperation.ErrorMessage}");
              else
              {
                // now add records to the Log table and update the duplicated and new records
                // If edits are dependent on each other, then those operations must be chained (see chaining edit operations).
                var chainedEditOperation = duplicateOperation.CreateChainedOperation();
                // get the new records that were duplicated
                var newSelection = SelectedTable.GetSelection();
                if (newSelection.GetCount() == 0)
                {
                  MessageBox.Show($@"No new records in '{SelectedTable.Name}' were selected");
                  return;
                }
                // we use Inspector to perform these single record edits
                var inspector = new Inspector();
                // first update the selected records
                foreach (var selOid in sourceSelection.GetObjectIDs())
                {
                  inspector.Load(SelectedTable, new List<long>() { selOid });
                  inspector[stringField.Name] = $"Duplicated: {inspector[stringField.Name]}";
                  chainedEditOperation.Modify(inspector);
                  // add a log record for each selected record
                  Dictionary<string, object> logAttributes = new()
                  {
                    { "Changed", DateTime.Now },
                    { "Operation", "Duplicated" },
                    { "TableName", SelectedTable.Name },
                    { "ChangedOid", selOid }
                  };
                  chainedEditOperation.Create(logStandaloneTable, logAttributes);
                }
                if (chainedEditOperation.IsEmpty == false)
                {
                  editResult = chainedEditOperation.Execute();
                  if (editResult != true || chainedEditOperation.IsSucceeded != true)
                    throw new Exception($@"Update Duplicated step failed: {chainedEditOperation.ErrorMessage}");
                }
                // now mark all new records with a time-stamp
                var secondChainedEditOperation = chainedEditOperation.CreateChainedOperation();
                foreach (var newOid in newSelection.GetObjectIDs())
                {
                  inspector.Load(SelectedTable, new List<long>() { newOid });
                  inspector[dateField.Name] = IsDateField ? DateTime.Now : DateTime.Now.ToShortDateString();
                  secondChainedEditOperation.Modify(inspector);
                  // add a log record for each new record
                  Dictionary<string, object> logAttributes = new()
                  {
                    { "Changed", DateTime.Now },
                    { "Operation", "New" },
                    { "TableName", SelectedTable.Name },
                    { "ChangedOid", newOid }
                  };
                  secondChainedEditOperation.Create(logStandaloneTable, logAttributes);
                }
                if (secondChainedEditOperation.IsEmpty == false)
                {
                  editResult = secondChainedEditOperation.Execute();
                  if (editResult != true || secondChainedEditOperation.IsSucceeded != true)
                    throw new Exception($@"Update New step failed: {secondChainedEditOperation.ErrorMessage}");
                }
                // finally select the sourceSelection and the newSelection
                SelectedTable.Select(new QueryFilter() { ObjectIDs = sourceSelection.GetObjectIDs() }, SelectionCombinationMethod.New);
                SelectedTable.Select(new QueryFilter() { ObjectIDs = newSelection.GetObjectIDs() }, SelectionCombinationMethod.Add);
              }
            });
          }
          catch (Exception ex)
          {
            MessageBox.Show(ex.ToString());
          }
        });
      }
    }

    public ImageSource CmdShowOperationLogImg
    {
      get
      {
        var imageSource = System.Windows.Application.Current.Resources["GnssLogging32"] as ImageSource;
        return imageSource;
      }
    }

    public ImageSource CmdDuplicateImg
    {
      get
      {
        var imageSource = System.Windows.Application.Current.Resources["DuplicateSection32"] as ImageSource;
        return imageSource;
      }
    }

    public ImageSource CmdFindMaxImg
    {
      get
      {
        var imageSource = System.Windows.Application.Current.Resources["Maximum32"] as ImageSource;
        return imageSource;
      }
    }

    #endregion Properties

    #region Helpers

    /// <summary>
    /// returns the first field of the specified type in the specified map member
    /// </summary>
    /// <param name="mapMember"></param>
    /// <param name="fieldType"></param>
    /// <param name="IsDateField"
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private static Field GetFieldByFieldType (MapMember mapMember, FieldType fieldType, ref bool IsDateField)
    {
      Field field = null;
      IsDateField = true;
      var isDate = field == null && fieldType == FieldType.Date;
      if (mapMember is FeatureLayer featureLayer)
      {
        var featureClass = featureLayer.GetFeatureClass();
        field = featureClass.GetDefinition().GetFields().Where(f => f.FieldType == fieldType).FirstOrDefault();
        if (field == null && isDate)
        {
          IsDateField = false;
          field = featureClass.GetDefinition().GetFields().Where(f => f.FieldType == FieldType.String && f.Name.Contains ("date", StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
        }
      }
      else if (mapMember is StandaloneTable standaloneTable)
      {
        var table = standaloneTable.GetTable();
        field = table.GetDefinition().GetFields().Where(f => f.FieldType == fieldType).FirstOrDefault();
        if (field == null && isDate)
        {
          IsDateField = false;
          field = table.GetDefinition().GetFields().Where(f => f.FieldType == FieldType.String && f.Name.Contains("date", StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
        }
      }
      if (field == null)
        throw new Exception($@"No {fieldType} field found in {mapMember.Name}");
      return field;
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

    #endregion


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
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class TableOperations_ShowButton : Button
  {
    protected override void OnClick()
    {
      TableOperationsViewModel.Show();
    }
  }
}
