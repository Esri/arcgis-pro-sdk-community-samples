//Copyright 2019 Esri

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
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using LayersPane.Extensions;

namespace LayersPane
{
    internal class LayersPaneViewModel : ViewStatePane, INotifyPropertyChanged
    {
        #region Private Properties
        public const string ViewPaneID = "LayersPane_LayersPane";
        public const string ViewDefaultPath = "LayersPaneViewModel_Pane_View_Path";
        private string _path = null;
        private bool _isLoading = false;
        private string _status = "";
        private Layer _selectedLayer;
        private DataTable _dataTable;
        private IReadOnlyList<Layer> _allMapLayers;
        #endregion Properties

        #region CTor

        /// <summary>
        /// Default construction - Consume the passed in CIMView. Call the base constructor to wire up the CIMView.
        /// </summary>
        /// <param name="view"></param>
        public LayersPaneViewModel(CIMView view)
            : base(view)
        {
            _path = view.ViewXML;

            //register 
            LayersPaneUtils.PaneCreated(this);

            //get the active map
            MapView activeView = MapView.Active;
            //get all the layers in the active map
            _allMapLayers = activeView.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().ToList();
            //set the selected layer to be the first one from the list
            if (_allMapLayers.Count > 0)
                _selectedLayer = _allMapLayers[0];

            //set up the command for the query
            QueryRowsCommand = new RelayCommand(new Action<object>(async (qry) => await QueryRows(qry)), CanQueryRows);
        }

        #endregion CTor

        #region Public Properties

        public IReadOnlyList<Layer> AllMapLayers
        {
            get { return _allMapLayers; }
        }
        
        public Layer SelectedLayer
        {
            get { return _selectedLayer; }
            set
            {
                _selectedLayer = value;
                RaisePropertyChanged();
            }
        }

        public DataTable FeatureData
        {
            get { return _dataTable; }
            set
            {
                _dataTable = value;
                RaisePropertyChanged();
            }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                SetProperty(ref _isLoading, value, () => IsLoading);
                RaisePropertyChanged();
            }
        }

        public string Status
        {
            get { return _status; }
            set
            {
                SetProperty(ref _status, value, () => Status);
                RaisePropertyChanged();
            }
        }

        public string Path
        {
            get { return _path == null ? ViewDefaultPath : _path; }
        }

        public ICommand QueryRowsCommand { get; private set; }

        #endregion Public Properties

        #region Private Helpers

        private bool CanQueryRows()
        {
            return _selectedLayer != null;
        }

        /// <summary>
        /// Execute a query against the selected layer's feature class and 
        /// display the resulting datatable
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private async Task QueryRows(object query)
        {
            var where = string.Empty;
            if (query != null) where = query.ToString();
            IsLoading = true;
            var rowCount = 0;
            // get the features using the query
            if (_selectedLayer != null)
            {
                await QueuedTask.Run(() =>
                {
                    var basicFl = _selectedLayer as BasicFeatureLayer;
                    if (basicFl != null)
                    {
                        Table layerTable = basicFl.GetTable();

                        var dt = new DataTable();
                        var queryFilter = new ArcGIS.Core.Data.QueryFilter
                        {
                            WhereClause = where
                        };
                        RowCursor cursor;
                        // Use try catch to catch invalid SQL statements in queryFilter
                        try
                        {
                            cursor = layerTable.Search(queryFilter);
                        }
                        catch (GeodatabaseGeneralException gdbEx)
                        {
                            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Error searching data. " + gdbEx.Message,
                                "Search Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            return;
                        }
                        if (cursor.MoveNext())
                        {
                            var maxcols = cursor.Current.GetFields().Count() > 6
                                ? 6
                                : cursor.Current.GetFields().Count();
                            for (var c = 0; c < maxcols; c++)
                            {
                                Type colType = typeof(string);
                                var format = string.Empty;
                                var fldDefinition = cursor.Current.GetFields()[c];
                                switch (fldDefinition.FieldType)
                                {
                                    case FieldType.Blob:
                                        format = "Blob";
                                        break;
                                    case FieldType.Raster:
                                        format = "Raster";
                                        break;
                                    case FieldType.Geometry:
                                        format = "Geom";
                                        break;
                                    case FieldType.Date:
                                        colType = typeof(DateTime);
                                        format = @"mm/dd/yyyy";
                                        break;
                                    case FieldType.Double:
                                        format = "0,0.0##";
                                        break;
                                    case FieldType.Integer:
                                    case FieldType.OID:
                                    case FieldType.Single:
                                    case FieldType.SmallInteger:
                                        format = "0,0";
                                        break;
                                    case FieldType.GlobalID:
                                    case FieldType.GUID:
                                    case FieldType.String:
                                    case FieldType.XML:
                                    default:
                                        break;
                                }
                                var col = new DataColumn(fldDefinition.Name, colType)
                                {
                                    Caption = fldDefinition.AliasName
                                };
                                dt.Columns.Add(col);
                            }
                            do
                            {
                                var row = dt.NewRow();
                                rowCount++;
                                for (var colIdx = 0; colIdx < maxcols; colIdx++)
                                {
                                    row[colIdx] = cursor.Current[colIdx];
                                }
                                dt.Rows.Add(row);
                            } while (cursor.MoveNext());
                        }
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                            (Action)(() => UpdateDataTableOnUI(dt)));
                    }
                });
            }
            Status = string.Format("{0} rows loaded", rowCount);
            IsLoading = false;
        }

        private string GetName(ArcGIS.Core.Data.Field field)
        {
            return string.IsNullOrEmpty(field.AliasName) ? field.Name : field.AliasName;
        }

        private DataTable _dtDataTable;
        /// <summary>
        /// called on UI thread
        /// </summary>
        internal void UpdateDataTableOnUI(DataTable dt)
        {
            _dtDataTable = dt.Copy();
            FeatureData = _dtDataTable;
        }

        /// <summary>
        /// Must be overridden in child classes - persist the state of the view to the CIM.
        /// </summary>
        public override CIMView ViewState
        {
            get
            {
                var view = CreatePane();
                view.InstanceID = (int)InstanceID;//from Framework.Pane
                                                  //view.InstanceIDSpecified = true;
                return view;
            }
        }

        internal static CIMView CreatePane(string path = ViewDefaultPath)
        {
            var view = new CIMGenericView();
            view.ViewXML = path;
            view.ViewType = ViewPaneID;
            return view;
        }

        /// <summary>
        /// Create a new instance of the pane.
        /// </summary>
        internal static LayersPaneViewModel Create()
        {
            var view = new CIMGenericView();
            view.ViewType = ViewPaneID;
            return FrameworkApplication.Panes.Create(ViewPaneID, new object[] { view }) as LayersPaneViewModel;
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        
        #endregion Private Helpers

        #region Pane Overrides

        /// <summary>
        /// Called when the pane is initialized.
        /// </summary>
        protected async override Task InitializeAsync()
        {
            await base.InitializeAsync();
        }

        /// <summary>
        /// Called when the pane is uninitialized.
        /// </summary>
        protected async override Task UninitializeAsync()
        {
            await base.UninitializeAsync();
        }

        #endregion Pane Overrides
    }

    /// <summary>
    /// Button implementation to create a new instance of the pane and activate it.
    /// </summary>
    internal class LayersPane_OpenButton : Button
    {
        protected override void OnClick()
        {
            //LayersPaneViewModel.Create();
            LayersPaneUtils.OpenPaneView(LayersPaneViewModel.ViewPaneID);
        }
    }


}
