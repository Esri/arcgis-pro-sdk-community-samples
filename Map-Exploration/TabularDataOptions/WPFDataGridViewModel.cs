/*

   Copyright 2025 Esri

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
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace TabularDataOptions
{
  internal class WPFDataGridViewModel : DockPane
  {
    private const string _dockPaneID = "TabularDataOptions_WPFDataGrid";

    private MapMember _selectedMapMember;
    private DataTable _theDataTable;

    /// <summary>
    /// used to lock collections for use by multiple threads
    /// </summary>
    private readonly object _lockCollections = new();

    /// <summary>
    /// UI lists, read-only collections, and theDictionary
    /// </summary>
    private readonly ObservableCollection<MapMember> _mapMembers = [];
    /// <summary>
    /// Used to define the dynamic column headers
    /// </summary>
    private ObservableCollection<System.Windows.Controls.DataGridColumn> _ColumnCollection = [];

    protected WPFDataGridViewModel()
    {
      BindingOperations.EnableCollectionSynchronization(_mapMembers, _lockCollections);
      BindingOperations.EnableCollectionSynchronization(_ColumnCollection, _lockCollections);
      // subscribe to the map view changed event... that's when we update the list of feature layers
      ActiveMapViewChangedEvent.Subscribe((args) =>
      {
        if (args.IncomingView == null) return;
        GetMapMembers();
      });
      // in case we have a map already open
      GetMapMembers(true);
      // subscribe to the selection changed event ... that's when we refresh our features
      MapSelectionChangedEvent.Subscribe((args) => { });
    }

    #region Properties

    /// <summary>
    /// List of the current active map's mapmembers
    /// </summary>
    public ObservableCollection<MapMember> MapMembers
    {
      get => _mapMembers;
    }

    /// <summary>
    /// The selected map member
    /// </summary>
    public MapMember SelectedMapMember
    {
      get => _selectedMapMember;
      set
      {
        SetProperty(ref _selectedMapMember, value);
        ColumnCollection.Clear();
        QueuedTask.Run(() =>
        {
          if (_selectedMapMember == null)
            TheDataTable = null;
          else if (_selectedMapMember is FeatureLayer featureLayer)
          {
						// define a data table from the selected feature layer
						// WPF Demo: DataGrid bound with DataTable - fill TheDataTable property bound to DataGrid's ItemsSource
						TheDataTable = GetDataTable(featureLayer.GetTable());
            // define enumerator and column list from the selected feature layer
            var (EnumeratorRow, ColumnList) = GetEnumerator(featureLayer.GetTable());
            RunOnUiThread(() =>
            {
              ColumnCollection.AddRange(GetDataGridColumns(ColumnList));
							// WPF Demo: DataGrid bound with IEnumerable - fill TheRowEnumeration property bound to DataGrid's ItemsSource
							// Show the data in the DataGrid
							TheRowEnumeration = EnumeratorRow;
            });
          }
          else if (_selectedMapMember is StandaloneTable st)
          {
						// WPF Demo: DataGrid bound with DataTable - fill TheDataTable property bound to DataGrid's ItemsSource
						TheDataTable = GetDataTable(st.GetTable());
            // define enumerator and column list from the selected standalone table
            var (RowEnumerator, ColumnList) = GetEnumerator(st.GetTable());
            RunOnUiThread(() =>
            {              
              ColumnCollection.AddRange(GetDataGridColumns(ColumnList));
							// WPF Demo: DataGrid bound with IEnumerable - fill TheRowEnumeration property bound to DataGrid's ItemsSource
							// Show the data in the DataGrid
							TheRowEnumeration = RowEnumerator;
            });
          }
        });
      }
    }

		/// <summary>
		/// The DataTable created from the selected map member
		/// WPF Demo: DataGrid bound with DataTable - TheDataTable property bound to DataGrid's ItemsSource
		/// </summary>  
		public DataTable TheDataTable
    {
      get => _theDataTable;
      set => SetProperty(ref _theDataTable, value);
    }

		/// <summary>
		/// Enumerator of rows from the selected map member
		/// WPF Demo: DataGrid bound with IEnumerable - TheRowEnumeration property bound to DataGrid's ItemsSource
		/// </summary>
		public IEnumerable<dynamic> TheRowEnumeration
    {
      get => _theRowEnumeration;
      set => SetProperty(ref _theRowEnumeration, value);
    }
    
    /// <summary>
    /// The collection of columns for the DataGrid
    /// </summary>
    public ObservableCollection<System.Windows.Controls.DataGridColumn> ColumnCollection
    {
      get => _ColumnCollection;
      set => SetProperty(ref _ColumnCollection, value);
    }

    #endregion Properties

    #region Helper Methods

    /// <summary>
    /// This method is called to use the current active mapview and retrieve all 
    /// MapMembers that are part of the map in the current map view.
    /// </summary>
    private void GetMapMembers(bool startUp = false)
    {
      QueuedTask.Run(() =>
      {
        var map = MapView.Active?.Map;
        if (map == null)
        {
          // no active map ... use the first visible map instead
          var firstMapPane = ProApp.Panes.OfType<IMapPane>().FirstOrDefault();
          map = firstMapPane?.MapView?.Map;
        }
        if (map == null)
        {
          if (!startUp) ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Can't find a MapView");
          return;
        }
        MapMembers.Clear();
        MapMembers.AddRange(map.GetLayersAsFlattenedList().OfType<BasicFeatureLayer>());
        MapMembers.AddRange(map.GetStandaloneTablesAsFlattenedList().OfType<MapMember>());
      });
    }

    /// <summary>
    /// utility function to enable an action to run on the UI thread (if not already)
    /// </summary>
    /// <param name="action">the action to execute</param>
    /// <returns></returns>
    internal static void RunOnUiThread(Action action)
    {
      try
      {
        if (IsOnUiThread)
          action();
        else
          System.Windows.Application.Current.Dispatcher.BeginInvoke(action);
      }
      catch (Exception ex)
      {
        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($@"Error in RunOnUiThread: {ex.Message}");
      }
    }

    /// <summary>
    /// Determines whether the calling thread is the thread associated with this 
    /// System.Windows.Threading.Dispatcher, the UI thread.
    /// 
    /// If called from a View model test it always returns true.
    /// </summary>
    public static bool IsOnUiThread => ArcGIS.Desktop.Framework.FrameworkApplication.TestMode || System.Windows.Application.Current.Dispatcher.CheckAccess();

    #endregion Helper Methods

    #region Grid Helpers

    /// <summary>
    /// Used to create a DataTable (read/only) from an ArcGIS Pro table.
    /// </summary>
    /// <param name="table">table to convert into a DataTable</param>
    private DataTable GetDataTable(Table table)
    {
      // Get all selected features for selectedFeatureLayer
      // and populate a DataTable with data and column headers
      var listColumnNames = new List<KeyValuePair<string, string>>();
      var listValues = new List<List<string>>();
      using (var rowCursor = table.Search(null))
      {
        bool bDefineColumns = true;
        while (rowCursor.MoveNext())
        {
          using var anyRow = rowCursor.Current;
          if (bDefineColumns)
          {
            foreach (var fld in anyRow.GetFields().Where(fld => fld.FieldType != FieldType.Geometry))
            {
              listColumnNames.Add(new KeyValuePair<string, string>(fld.Name, fld.AliasName));
            }
          }
          bDefineColumns = false;
          var newRow = new List<string>();
          foreach (var fld in anyRow.GetFields().Where(fld => fld.FieldType != FieldType.Geometry))
          {
            newRow.Add((anyRow[fld.Name] == null) ? string.Empty : anyRow[fld.Name].ToString());
          }
          listValues.Add(newRow);
        }
      }
      var newTable = new DataTable();
      foreach (var col in listColumnNames)
      {
        newTable.Columns.Add(new DataColumn(col.Key, typeof(string)) { Caption = col.Value });
      }
      foreach (var row in listValues)
      {
        var newRow = newTable.NewRow();
        newRow.ItemArray = row.ToArray();
        newTable.Rows.Add(newRow);
      }
      return newTable;
    }

    /// <summary>
    /// Used to create a DataTable (read/only) from an ArcGIS Pro table.
    /// </summary>
    /// <param name="table">table to convert into a DataTable</param>
    private (IEnumerable<dynamic> RowEnumerator, List<(string ColumnName, string ColumnAliasName)> ColumnList)
      GetEnumerator(Table table)
    {
      List<dynamic> rowEnumerator = [];
      List<(string ColumnName, string ColumnAlias)> columnList = [];
      List<string> columnNames = [];

      using (var rowCursor = table.Search(null, false))
      {
        bool bDefineColumns = true;
        while (rowCursor.MoveNext())
        {
          using var anyRow = rowCursor.Current;
          if (bDefineColumns)
          {
            // Define columns
            foreach (var fld in anyRow.GetFields().Where(fld => fld.FieldType != FieldType.Geometry))
            {
              columnList.Add((fld.Name, fld.AliasName));
              columnNames.Add(fld.Name);
            }
          }
          bDefineColumns = false;
          // Copy data
          var rowValues = new Dictionary<string, object>();
          foreach (var columnName in columnNames)
          {
            rowValues.Add(columnName, anyRow[columnName]);
          }
          var dyn = ConvertToDynamic.ToDynamic(rowValues);
          rowEnumerator.Add(dyn);
        }
      }
      return (rowEnumerator, columnList);
    }

    private List<DataGridTextColumn> GetDataGridColumns(List<(string ColumnName, string ColumnAliasName)> ColumnList)
    {
      // update the column definitions on the UI thread only
      List<DataGridTextColumn> colList = [];
      foreach (var (ColumnName, ColumnAliasName) in ColumnList)
      {
        DataGridTextColumn textcol = new();
        //Create a Binding object to define the path to the DataGrid.ItemsSource property
        //The column inherits its DataContext from the DataGrid, so you don't set the source
        Binding b = new(ColumnName);
        //Set the theDictionary on the new column
        textcol.Binding = b;
        textcol.Header = ColumnAliasName;
        //Add the column to the list
        colList.Add(textcol);
      }
      return colList;
    }

    #endregion Grid Helpers

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

    /// <summary>
    /// Text shown near the top of the DockPane.
    /// </summary>
    private string _heading = "WPF DataGrid Sample";
    private IEnumerable<dynamic> _theRowEnumeration;

    public string Heading
    {
      get => _heading;
      set => SetProperty(ref _heading, value);
    }
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class WPFDataGrid_ShowButton : ArcGIS.Desktop.Framework.Contracts.Button
  {
    protected override void OnClick()
    {
      WPFDataGridViewModel.Show();
    }
  }
}
