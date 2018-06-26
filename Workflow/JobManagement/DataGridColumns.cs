//   Copyright 2018 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace WorkflowSample
{
    /// <summary>
    /// This Class exists to help populate the open data grid with the query results
    /// allows binding to the columns property in a data grid
    /// </summary>
    internal class DataGridColumns
    {
        public static readonly DependencyProperty BindableColumnsProperty =
            DependencyProperty.RegisterAttached("BindableColumns",
                                                typeof(ReadOnlyObservableCollection<DataGridColumn>),
                                                typeof(DataGridColumns),
                                                new UIPropertyMetadata(null, BindableColumnsPropertyChanged));

        private static void BindableColumnsPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = source as DataGrid;
            if (dataGrid == null)
                return;

            dataGrid.Columns.Clear();
            var columns = e.NewValue as ReadOnlyObservableCollection<DataGridColumn>;
            if (columns == null)
                return;

            // Add columns
            foreach (var column in columns)
                dataGrid.Columns.Add(column);
        }

        public static void SetBindableColumns(DependencyObject element, ReadOnlyObservableCollection<DataGridColumn> value)
        {
            element.SetValue(BindableColumnsProperty, value);
        }

        public static ReadOnlyObservableCollection<DataGridColumn> GetBindableColumns(DependencyObject element)
        {
            return element.GetValue(BindableColumnsProperty) as ReadOnlyObservableCollection<DataGridColumn>;
        }

    }
}
