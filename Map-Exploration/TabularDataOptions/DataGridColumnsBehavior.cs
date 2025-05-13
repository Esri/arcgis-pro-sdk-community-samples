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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace TabularDataOptions
{
  public class DataGridColumnsBehavior
  {
    public static readonly DependencyProperty BindableColumnsProperty =
        DependencyProperty.RegisterAttached("BindableColumns",
                                            typeof(ObservableCollection<DataGridColumn>),
                                            typeof(DataGridColumnsBehavior),
                                            new UIPropertyMetadata(null, BindableColumnsPropertyChanged));

    /// <summary>
    /// Collection to store collection change handlers - to be able to unsubscribe later.
    /// </summary>
    private static readonly Dictionary<DataGrid, NotifyCollectionChangedEventHandler> _handlers;

    static DataGridColumnsBehavior()
    {
      _handlers = new Dictionary<DataGrid, NotifyCollectionChangedEventHandler>();
    }

    private static void BindableColumnsPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
    {
      DataGrid dataGrid = source as DataGrid;

      if (e.OldValue is ObservableCollection<DataGridColumn> oldColumns)
      {
        // Remove all columns.
        dataGrid.Columns.Clear();

        // Unsubscribe from old collection.
        if (_handlers.TryGetValue(dataGrid, out NotifyCollectionChangedEventHandler h))
        {
          oldColumns.CollectionChanged -= h;
          _handlers.Remove(dataGrid);
        }
      }

      dataGrid.Columns.Clear();
      if (e.NewValue is ObservableCollection<DataGridColumn> newColumns)
      {
        // Add columns from this source.
        foreach (DataGridColumn column in newColumns)
          dataGrid.Columns.Add(column);

        // Subscribe to future changes.
        NotifyCollectionChangedEventHandler h = (_, ne) => OnCollectionChanged(ne, dataGrid);
        _handlers[dataGrid] = h;
        newColumns.CollectionChanged += h;
      }
    }

    static void OnCollectionChanged(NotifyCollectionChangedEventArgs ne, DataGrid dataGrid)
    {
      switch (ne.Action)
      {
        case NotifyCollectionChangedAction.Reset:
          dataGrid.Columns.Clear();
          if (ne.NewItems != null)
            foreach (DataGridColumn column in ne.NewItems)
              dataGrid.Columns.Add(column);
          break;
        case NotifyCollectionChangedAction.Add:
          if (ne.NewItems != null) 
            foreach (DataGridColumn column in ne.NewItems)
              dataGrid.Columns.Add(column);
          break;
        case NotifyCollectionChangedAction.Move:
          dataGrid.Columns.Move(ne.OldStartingIndex, ne.NewStartingIndex);
          break;
        case NotifyCollectionChangedAction.Remove:
          foreach (DataGridColumn column in ne.OldItems)
            dataGrid.Columns.Remove(column);
          break;
        case NotifyCollectionChangedAction.Replace:
          dataGrid.Columns[ne.NewStartingIndex] = ne.NewItems[0] as DataGridColumn;
          break;
      }
    }

    public static void SetBindableColumns(DependencyObject element, ObservableCollection<DataGridColumn> value)
    {
      element.SetValue(BindableColumnsProperty, value);
    }

    public static ObservableCollection<DataGridColumn> GetBindableColumns(DependencyObject element)
    {
      return (ObservableCollection<DataGridColumn>)element.GetValue(BindableColumnsProperty);
    }
  }
}
