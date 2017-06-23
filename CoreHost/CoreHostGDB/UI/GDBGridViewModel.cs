//Copyright 2017 Esri

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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using ArcGIS.Core.Data;
using CoreHostGDB.Common;
using CoreHostGDB.ForRows;
using Microsoft.Win32;

namespace CoreHostGDB.UI {
    class GDBGridViewModel : INotifyPropertyChanged {
        private string _gdbPath = "";
        private ICommand _openGDB = null;
        private ICommand _browseGDB = null;
        private ICommand _readTable = null;
        private ObservableCollection<string>  _tables = new ObservableCollection<string>();
        private ObservableCollection<DynamicDataRow> _rows = new ObservableCollection<DynamicDataRow>();
        private List<ColumnData> _columns = new List<ColumnData>();
        private string _selectedTableName = "";

        private string _tableHasNoRows = "";

        private DispatcherTimer _timer = null;
        private double _progressValue = 1;
        private double _maxProgressValue = 100;
        private bool _executeQuery = false;

        private static readonly object _theLock = new object();

        public GDBGridViewModel() {
            Initialize();
        }

        private void Initialize() {
            BindingOperations.CollectionRegistering += BindingOperations_CollectionRegistering;

            //init timer
            _timer = new DispatcherTimer() {
                Interval = TimeSpan.FromMilliseconds(25d),
                IsEnabled = false
            };
            _timer.Tick += (o, e) => {
                //update the progress bar
                _progressValue += 1.0;
                if (_progressValue > _maxProgressValue)
                    _progressValue = 1.0;
                System.Windows.Application.Current.Dispatcher.Invoke(() => {
                    OnPropertyChanged("ProgressValue");
                });
            };

        }

        void BindingOperations_CollectionRegistering(object sender, CollectionRegisteringEventArgs e) {
            //register all the collections
            BindingOperations.EnableCollectionSynchronization(_tables, _theLock);
            BindingOperations.EnableCollectionSynchronization(_rows, _theLock);

            //unregister - we only need this event once
            BindingOperations.CollectionRegistering -= BindingOperations_CollectionRegistering;
        }
        /// <summary>
        /// Gets and sets the path to the GDB to be opened
        /// </summary>
        public string GDBPath {
            get {
                return _gdbPath;
            }
            set {
                _gdbPath = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the tables for the given GDB
        /// </summary>
        public ObservableCollection<string> Tables {
            get {
                return _tables;
            }
        }
        /// <summary>
        /// Gets the current set of rows for the selected table
        /// </summary>
        public ObservableCollection<DynamicDataRow> Rows {
            get {
                return _rows;
            }
        }
        /// <summary>
        /// Gets the column data (name, type, etc)
        /// </summary>
        public List<ColumnData> ColumnData {
            get {
                return _columns;
            }
        }
        /// <summary>
        /// Gets and sets the selected table name (for the opened GDB)
        /// </summary>
        public string SelectedTableName {
            get {
                return _selectedTableName;
            }
            set {
                _selectedTableName = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Gets and sets the string to display if the selected table has no rows
        /// </summary>
        public string TableHasNoRows {
            get {
                return _tableHasNoRows;
            }
            set {
                _tableHasNoRows = value;
                OnPropertyChanged();
            }
        }

        #region Progress

        /// <summary>
        /// Gets the value to set on the progress
        /// </summary>
        public double ProgressValue {
            get {
                return _progressValue;
            }
        }

        /// <summary>
        /// Gets the max value to set on the progress
        /// </summary>
        public double MaxProgressValue {
            get {
                return _maxProgressValue;
            }
        }
        /// <summary>
        /// Gets whether a query is executing or not
        /// </summary>
        public bool IsExecutingQuery {
            get {
                return _executeQuery;
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Gets the Browse for GDB command
        /// </summary>
        public ICommand BrowseForGDBCommand {
            get {
                if (_browseGDB == null)
                    _browseGDB = new RelayCommand(BrowseForGDB);
                return _browseGDB;
            }
        }

        /// <summary>
        /// Gets the Open GDB command
        /// </summary>
        public ICommand OpenGDBCommand {
            get {
                if (_openGDB == null)
                    _openGDB = new RelayCommand(OpenGDB);
                return _openGDB ;
            }
        }
        //_readTable
        /// <summary>
        /// Gets the rows from the selected table
        /// </summary>
        public ICommand ReadTableCommand {
            get {
                if (_readTable == null)
                    _readTable = new RelayCommand(ReadRows);
                return _readTable;
            }
        }

        private void BrowseForGDB() {
            if (_executeQuery)
                return;
            using (var dlg = new System.Windows.Forms.FolderBrowserDialog()) {
                dlg.Description = "Browse for a File Geodatabase Folder (.gdb)";
                dlg.SelectedPath = GDBPath;
                dlg.ShowNewFolderButton = false;
                System.Windows.Forms.DialogResult result = dlg.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK) {
                    GDBPath = dlg.SelectedPath;
                }
            }
        }

        private async void OpenGDB() {
            if (_executeQuery)
                return;
            if (_gdbPath.IsEmpty())
                return;
            _executeQuery = true;
            _timer.Start();
            OnPropertyChanged("IsExecutingQuery");
            _tables.Clear();

            if (_rows.Count > 0) {
                _rows.Clear();
                _rows = new ObservableCollection<DynamicDataRow>();
                ExtendListView.Columns = null;
                OnPropertyChanged("Rows");
            }

            TableHasNoRows = "";
            SelectedTableName = "";

            try {
                await TaskUtils.StartSTATask<int>(() => {
                    using (Geodatabase gdb = new Geodatabase(
                        new FileGeodatabaseConnectionPath(new Uri(_gdbPath, UriKind.Absolute)))) {

                        IReadOnlyList<Definition> fcList = gdb.GetDefinitions<FeatureClassDefinition>();
                        IReadOnlyList<Definition> tables = gdb.GetDefinitions<TableDefinition>();

                        //////Uncomment for just Feature class in Feature Datasets
                        ////IReadOnlyList<Definition> fdsList = gdb.GetDefinitions<FeatureDatasetDefinition>();
                        
                        lock (_theLock) {
                            //Feature class
                            foreach (var fcDef in fcList) {
                                _tables.Add(TableString(fcDef as TableDefinition));
                            }
                            //Tables
                            foreach (var def in tables) {
                                _tables.Add(TableString(def as TableDefinition));
                            }

                            //////Uncomment for just Feature class in Feature Datasets
                            ////foreach (var fdsDef in fdsList) {
                            ////    IReadOnlyList<Definition> tableDefsInDataset = gdb.GetRelatedDefinitions(fdsDef,
                            ////        DefinitionRelationshipType.DatasetInFeatureDataset);

                            ////    foreach (var def in tableDefsInDataset) {
                            ////        if (def.DatasetType == DatasetType.FeatureClass)
                            ////            _tables.Add(TableString(def as TableDefinition));
                            ////    }
                            ////}
    
                        }
                    }
                    return 0;
                });
            }
            finally {
                _timer.Stop();
                if (_tables.Count > 0)
                    SelectedTableName = _tables[0];
                else {
                    MessageBox.Show("No tables or feature datasets read", _gdbPath);
                }
                _executeQuery = false;
                OnPropertyChanged("IsExecutingQuery");
            }
            
        }

        private async void ReadRows() {
            if (_executeQuery)
                return;
            if (_gdbPath.IsEmpty())
                return;
            if (SelectedTableName.IsEmpty())
                return;

            _executeQuery = true;
            _columns.Clear();
            _timer.Start();
            OnPropertyChanged("IsExecutingQuery");

            string tableName = SelectedTableName.Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();

            var data = new ObservableCollection<DynamicDataRow>();

            try {
                //Note, we have to return something
                await TaskUtils.StartSTATask<int>(() => {
                    using (Geodatabase gdb = new Geodatabase(
                        new FileGeodatabaseConnectionPath(new Uri(_gdbPath, UriKind.Absolute))))
                    {
                        var table = gdb.OpenDataset<Table>(tableName);

                        RowCursor cursor = table.Search();
                        IReadOnlyList<Field> flds = cursor.GetFields();
                        foreach (var fld in flds) {
                            _columns.Add(new ColumnData() {
                                AliasName = fld.AliasName ?? fld.Name,
                                Name = fld.Name,
                                FieldType = fld.FieldType
                            });
                        }
                        
                        while (cursor.MoveNext()) {
                            var row = new DynamicDataRow();

                            for (int v = 0; v < flds.Count; v++) {
                                row[GetName(flds[v])] = GetValue(cursor.Current, v);
                            }
                            data.Add(row);
                        }
                    }
                    return 0;
                });
            }
            finally {
                ExtendListView.Columns = _columns;
                _timer.Stop();

                lock(_theLock) {
                    _rows.Clear();
                    _rows = null;
                    _rows = data;
                }
                if (_rows.Count > 0) {
                    TableHasNoRows = "";
                }
                else {
                    TableHasNoRows = "No rows returned";
                }
                _executeQuery = false;
                OnPropertyChanged("Rows");
                OnPropertyChanged("IsExecutingQuery");
            }
        }

        private string TableString(TableDefinition table) {
            string alias = table.GetAliasName();
            return string.Format("{0} ({1})", !alias.IsEmpty() ? alias : table.GetName(), table.GetName());
        }

        private string GetName(Field field) {
            return string.IsNullOrEmpty(field.AliasName) ? field.Name : field.AliasName;
        }

        private string GetValue(Row row, int index) {
            return row[index]?.ToString();
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void OnPropertyChanged([CallerMemberName] string propName = "") {
            PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }

    internal class ColumnData {
        public string AliasName { get; set; }
        public string Name { get; set; }
        public FieldType FieldType { get; set; }
    }
}
