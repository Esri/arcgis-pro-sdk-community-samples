//Copyright 2015 Esri

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
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
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
        public const string ViewPaneID = "LayersPane_LayersPane";
        public const string ViewDefaultPath = "LayersPaneViewModel_Pane_View_Path";
        private string _path = null;
        private bool _isLoading = false;
        private string _status = "";

        private IReadOnlyList<Layer> _allMapLayers;
        public IList<DynamicDataRow> ListOfRows { get; private set; }
        private Table _layerSource = null;
        private static readonly object _theLock = new object();
                
        public IReadOnlyList<Layer> AllMapLayers
        {
            get { return _allMapLayers; }
        }

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
                
            ListOfRows = new List<DynamicDataRow>();
            //Enable collection mods in the background
            BindingOperations.EnableCollectionSynchronization(ListOfRows, _theLock);
            //set up the command for the query
            QueryRowsCommand = new RelayCommand(new Action<object>(async (qry) => await QueryRows(qry)), CanQueryRows);
        }

        private Layer _selectedLayer;
        public Layer SelectedLayer
        {
            get { return _selectedLayer; }
            set
            {
                _selectedLayer = value;
                _layerSource = null;
                RaisePropertyChanged();
            }
        }

        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                SetProperty(ref _isLoading, value, () => IsLoading);
                RaisePropertyChanged();
            }
        }

        public string Status
        {
            get
            {
                return _status;
            }
            set
            {
                SetProperty(ref _status, value, () => Status);
                RaisePropertyChanged();
            }
        }

        public string Path
        {
            get
            {
                if (_path == null)
                    _path = ViewDefaultPath;
                return _path;
            }
        }

        public ICommand QueryRowsCommand { get; private set; }

        private bool CanQueryRows()
        {
            return _selectedLayer != null;
        }

        private async Task QueryRows(object query)
        {
            string where = "";
            if (query != null)
                where = query.ToString();
            IsLoading = true;
            lock (_theLock)
            {
                ListOfRows.Clear();
            }
            if (_layerSource == null)
                _layerSource = await _selectedLayer.getFeatureClass();
            if (_layerSource != null)
            {
                var data = new List<DynamicDataRow>();
                await QueuedTask.Run(() =>
                {
                    var queryFilter = new ArcGIS.Core.Data.QueryFilter
                    {
                        WhereClause = where
                    };
                    int maxcols = 6;

                    RowCursor cursor = null;

                    // Use try catch to catch invalid SQL statements in queryFilter
                    try
                    {
                        cursor = _layerSource.Search(queryFilter);
                    }
                    catch (GeodatabaseGeneralException gdbEx)
                    {
                        System.Windows.MessageBox.Show("Error searching data. " + gdbEx.Message, "Search Error", 
                                                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Exclamation);
                        return;
                    }

                    if (cursor.MoveNext())
                    {
                        ExtendListView.Columns = new List<ArcGIS.Core.Data.Field>();
                        maxcols = cursor.Current.GetFields().Count() > 6 ? 6 : cursor.Current.GetFields().Count();

                        for (int c = 0; c < maxcols; c++)
                        {
                            ExtendListView.Columns.Add(cursor.Current.GetFields()[c]);
                        }
                        do
                        {
                            var row = new DynamicDataRow();
                            for (int v = 0; v < maxcols; v++)
                            {
                                if (cursor.Current[v] != null)
                                { 
                                    row[GetName(cursor.GetFields()[v])] = cursor.Current[v].ToString();
                                }

                            }
                            data.Add(row);
                        } while (cursor.MoveNext());
                    }
                });

                lock (_theLock)
                {
                    ListOfRows = null;
                    ListOfRows = data;
                }
            }
            RaisePropertyChanged("ListOfRows");
            Status = string.Format("{0} rows loaded", ListOfRows.Count());
            IsLoading = false;
        }

        private string GetName(ArcGIS.Core.Data.Field field)
        {
            return string.IsNullOrEmpty(field.AliasName) ? field.Name : field.AliasName;
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
