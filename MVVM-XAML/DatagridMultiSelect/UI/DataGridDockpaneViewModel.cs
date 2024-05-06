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
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace DatagridMultiSelect.UI
{
  internal class DataGridDockpaneViewModel : DockPane
  {
    private const string _dockPaneID = "DatagridMultiSelect_UI_DataGridDockpane";
    private DataGrid _dataGrid = null;

    protected DataGridDockpaneViewModel() 
    {
      BindingOperations.EnableCollectionSynchronization(
        this.FeatureData, Module1.Current.LockFeatureData);
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

    /// <summary>
    /// Text shown near the top of the DockPane.
    /// </summary>
    private string _heading = "Multiselect-Datagrid";
    public string Heading
    {
      get => _heading;
      set => SetProperty(ref _heading, value);
    }

    private static readonly object _lock = new object();
    private List<string> _sel_features = new List<string>();

    //Mody, From:https://stackoverflow.com/questions/22868445/select-multiple-items-from-a-datagrid-in-an-mvvm-wpf-project
    //DataGrid does not support the MVVM pattern very well.
    //I need to get the selected items from it but we cannot
    //do it via Binding - hence my view passes a datagrid
    //to my view model here. Look in DataGridDockpane.xaml.cs
    internal void SetDataGrid(DataGrid dataGrid)
    {
      _dataGrid = dataGrid;
      _dataGrid.SelectionChanged += (o, e) =>
      {
        var grid = o as DataGrid;
        var selected = grid.SelectedItems;
        lock (_lock)
          _sel_features.Clear();
        if (selected != null)
        {
          lock(_lock)
          {
            var sel_fd = selected.OfType<FeatureData>().ToList();
            foreach (var fd in sel_fd)
              _sel_features.Add(fd.ToString());
            _sel_features.Insert(0, $"{sel_fd.Count} rows selected");
          }
        }
        else
        {
          lock (_lock)
            _sel_features.Insert(0, "0 rows selected");
        }
        this.NotifyPropertyChanged("SelectionResults");
      };
    }

    public ObservableCollection<FeatureData> FeatureData => Module1.Current.FeatureData;

    public string SelectionResults
    {
      get
      {
        string contents = "";
        lock (_lock)
        {
          contents = string.Join("\r\n", _sel_features.ToArray());
        }
        return contents;
      }
    }

    private ICommand _cmd_sel_all_rows = null;

    public ICommand SelectAllCmd
    {
      get
      {
        if (_cmd_sel_all_rows == null)
        {
          _cmd_sel_all_rows = new RelayCommand(() =>
          {
            _dataGrid.SelectAll();
          });
        }
        return _cmd_sel_all_rows;
      }
    }

    private ICommand _cmd_clear_sel_all_rows = null;
    public ICommand ClearSelectCmd
    {
      get
      {
        if (_cmd_clear_sel_all_rows == null)
        {
          _cmd_clear_sel_all_rows = new RelayCommand(() =>
          {
            _dataGrid?.UnselectAll();
          });
        }
        return _cmd_clear_sel_all_rows;
      }
    }
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class DataGridDockpane_ShowButton : ArcGIS.Desktop.Framework.Contracts.Button
  {
    protected override void OnClick()
    {
      DataGridDockpaneViewModel.Show();
    }
  }
}
