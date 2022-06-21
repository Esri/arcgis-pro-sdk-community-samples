/*

   Copyright 2019 Esri

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
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Events;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.Windows.Data;
using ArcGIS.Desktop.Mapping.Events;

namespace IdentifyWindow
{
    internal class AttributeDockpaneViewModel : DockPane
    {
        #region Private Properties

        private const string _dockPaneID = "IdentifyWindow_AttributeDockpane";
        
        private FeatureLayer _selectedFeatureLayer;

        /// <summary>
        /// used to lock collections for use by multiple threads
        /// </summary>
        private readonly object _lockCollections = new object();
        /// <summary>
        /// UI lists, read-only collections, and properties
        /// </summary>
        private readonly ObservableCollection<FeatureLayer> _featureLayers = new ObservableCollection<FeatureLayer>();
        private readonly ReadOnlyObservableCollection<FeatureLayer> _readOnlyFeatureLayers;

        private readonly object _lockSelectedFeaturesDataTable = new object();
        private DataTable _selectedFeaturesDataTable = new DataTable();
        private DataRowView _selectedFeature = null;
        
        private KeyValuePair<string, int>[] _chartResult;

        #endregion Private Properties

        #region CTor

        protected AttributeDockpaneViewModel()
        {
            // By default, WPF data bound collections must be modified on the thread where the bound WPF control was created. 
            // This limitation becomes a problem when you want to fill the collection from a worker thread to produce a nice experience. 
            // For example, a search result list should be gradually filled as more matches are found, without forcing the user to wait until the 
            // whole search is complete.  

            // To get around this limitation, WPF provides a static BindingOperations class that lets you establish an 
            // association between a lock and a collection (e.g., ObservableCollection\<T>). 
            // This association allows bound collections to be updated from threads outside the main GUI thread, 
            // in a coordinated manner without generating the usual exception.  

            _readOnlyFeatureLayers = new ReadOnlyObservableCollection<FeatureLayer>(_featureLayers);
            BindingOperations.EnableCollectionSynchronization(_readOnlyFeatureLayers, _lockCollections);

            // subscribe to the map view changed event... that's when we update the list of feature layers
            ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);

            // subscribe to the selection changed event ... that's when we refresh our features
            MapSelectionChangedEvent.Subscribe(OnMapSelectionChanged);
        }

        /// <summary>
        /// Called when the pane is first created to give it the opportunity to initialize itself asynchronously.
        /// </summary>
        /// <returns>
        /// A task that represents the work queued to execute in the ThreadPool.
        /// </returns>
        protected override Task InitializeAsync() {
            GetFeatureLayers();
            return base.InitializeAsync();
        }

        #endregion CTor

        #region Public Properties

        // hook existing ArcGIS Pro Button
        /// <summary>
        /// Command to allow selection of features on the current MapView
        /// </summary>
        public ICommand SelectionTool { get { return FrameworkApplication.GetPlugInWrapper("esri_mapping_selectByRectangleTool") as ICommand; } }

        /// <summary>
        /// Command to allow closing of Pro from DockPane
        /// </summary>
        public ICommand CloseCommand { get { return FrameworkApplication.GetPlugInWrapper("esri_core_exitApplicationButton") as ICommand; } }

        /// <summary>
        /// List of the current active map's feature layers
        /// </summary>
        public ReadOnlyObservableCollection<FeatureLayer> FeatureLayers
        {
            get { return _readOnlyFeatureLayers; }
        }

        /// <summary>
        /// The selected feature layer
        /// </summary>
        public FeatureLayer SelectedFeatureLayer
        {
            get { return _selectedFeatureLayer; }
            set
            {
                SetProperty(ref _selectedFeatureLayer, value, () => SelectedFeatureLayer);
                OnMapSelectionChanged(null);
            }
        }

        /// <summary>
        /// The selected data table (for tabular display)
        /// </summary>
        public DataTable SelectedFeatureDataTable
        {
            get { return _selectedFeaturesDataTable; }
            set
            {
                SetProperty(ref _selectedFeaturesDataTable, value, () => SelectedFeatureDataTable);
            }
        }

        /// <summary>
        /// Chart Result
        /// </summary>
        public KeyValuePair<string, int>[] ChartResult
        {
            get { return _chartResult; }
            set
            {
                SetProperty(ref _chartResult, value, () => ChartResult);
            }
        }

        /// <summary>
        /// One row of the selected feature grid was selected
        /// </summary>
        public DataRowView SelectedFeature
        {
            get
            {
                return _selectedFeature;
            }
            set
            {
                SetProperty(ref _selectedFeature, value, () => SelectedFeature);
                if (_selectedFeature == null || SelectedFeatureLayer == null) return;
                // Flash the Feature
                IReadOnlyDictionary<BasicFeatureLayer, List<long>> flashFeature = new Dictionary<BasicFeatureLayer, List<long>>()
                    {{SelectedFeatureLayer, new List<long>(){Convert.ToInt64(_selectedFeature.Row["ObjectId"])}}};
                FlashFeaturesAsync(flashFeature);
            }
        }

        #endregion Public Properties

        #region Event Handlers

        /// <summary>
        /// The active map view changed therefore we refresh the feature layer drop-down
        /// </summary>
        /// <param name="args"></param>
        private void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs args)
        {
            if (args.IncomingView == null) return;
            SelectedFeatureDataTable = null;
            ChartResult = null;
            GetFeatureLayers();
        }

        /// <summary>
        /// Called after the feature selection changed
        /// </summary>
        /// <param name="args"></param>
        private async void OnMapSelectionChanged(MapSelectionChangedEventArgs args)
        {
            if (SelectedFeatureLayer == null) return;
            await GetSelectedFeaturesAsync(SelectedFeatureLayer);
            NotifyPropertyChanged(() => SelectedFeatureDataTable);

            await ComputePieChartAsync();
            NotifyPropertyChanged(() => ChartResult);

            Zoom2Selection();
        }

        #endregion Event Handlers

        #region Helpers

        /// <summary>
        /// Zoom to selection
        /// </summary>
        private void Zoom2Selection()
        {
            var mapView = MapView.Active;
            if (mapView == null) return;
            QueuedTask.Run(() =>
            {
                //select features that intersect the sketch geometry
                var selection = mapView.Map.GetSelection().ToDictionary()
                      .Where(kvp => kvp.Key is BasicFeatureLayer)
                      .Select(kvp => (BasicFeatureLayer)kvp.Key);
                //zoom to selection
                mapView.ZoomTo(selection, true);
            });
        }

        /// <summary>
        /// This method is called to use the current active MapView and retrieve all 
        /// feature layers that are part of the map layers in the current map view.
        /// </summary>
        private Task ComputePieChartAsync()
        {
            var mapView = MapView.Active;
            if (mapView == null) return Task.FromResult(0);
            return QueuedTask.Run(() => {
                var pieChartResult = new List<KeyValuePair<string, int>>();
                foreach (var selection in mapView.Map.GetSelection().ToDictionary()
                    .Where(kvp => kvp.Key is BasicFeatureLayer)) {
                    pieChartResult.Add(new KeyValuePair<string, int>(selection.Key.Name, selection.Value.Count));
                }
                _chartResult = pieChartResult.ToArray();
            });
        }

        /// <summary>
        /// This method is called to use the current active mapview and retrieve all 
        /// feature layers that are part of the map layers in the current map view.
        /// </summary>
        private void GetFeatureLayers()
        {
            //Get the active map view.
            var mapView = MapView.Active;
            if (mapView == null) return;
            var featureLayers = mapView.Map.Layers.OfType<FeatureLayer>();
            lock (_lockCollections) {
                _featureLayers.Clear();
                foreach (var featureLayer in featureLayers) _featureLayers.Add(featureLayer);
            }
            NotifyPropertyChanged(() => FeatureLayers);
        }

        /// <summary>
        /// This method is called when the selection on the map view has changed.  Because we are only
        /// interested in the 'selected' feature layer from our feature layer drop down we pass the 
        /// 'selected' feature layer as a parameter.
        /// </summary>
        /// <param name="selectedFeatureLayer">'selected' feature layer that we need to display data in the grid view for</param>
        private Task GetSelectedFeaturesAsync(FeatureLayer selectedFeatureLayer)
        {
            //Get the active map view.
            var mapView = MapView.Active;
            if (mapView == null || selectedFeatureLayer == null)
                return Task.FromResult(0);
            return QueuedTask.Run(() =>
            {
                // Get all selected features for selectedFeatureLayer
                // and populate a DataTable with data and column headers
                var listColumnNames = new List<KeyValuePair<string, string>>();
                var listValues = new List<List<string>>();
                using (var rowCursor = selectedFeatureLayer.GetSelection().Search(null))
                {
                    bool bDefineColumns = true;
                    while (rowCursor.MoveNext())
                    {
                        using (var anyRow = rowCursor.Current)
                        {
                            if (bDefineColumns)
                            {
                                foreach (var fld in anyRow.GetFields().Where(fld => fld.FieldType != FieldType.Geometry))
                                {
                                    listColumnNames.Add(new KeyValuePair<string, string>(fld.Name, fld.AliasName));
                                }
                            }
                            var newRow = new List<string>();
                            foreach (var fld in anyRow.GetFields().Where(fld => fld.FieldType != FieldType.Geometry))
                            {
                                newRow.Add((anyRow[fld.Name] == null) ? string.Empty : anyRow[fld.Name].ToString());
                            }
                            listValues.Add(newRow);
                            bDefineColumns = false;
                        }
                    }
                }
                _selectedFeaturesDataTable = new DataTable();
                foreach (var col in listColumnNames) {
                    _selectedFeaturesDataTable.Columns.Add(new DataColumn(col.Key, typeof(string)) { Caption = col.Value });
                }
                foreach (var row in listValues) {
                    var newRow = _selectedFeaturesDataTable.NewRow();
                    newRow.ItemArray = row.ToArray();
                    _selectedFeaturesDataTable.Rows.Add(newRow);
                }
            });
        }

        /// <summary>
        /// Flash the selected features
        /// </summary>
        /// <param name="flashFeatures"></param>
        private async void FlashFeaturesAsync(IReadOnlyDictionary<BasicFeatureLayer, List<long>> flashFeatures)
        {
            //Get the active map view.
            var mapView = MapView.Active;
            if (mapView == null)
                return;
            var selectionDictionary = new Dictionary<MapMember, List<long>>();
            foreach (var item in flashFeatures)
            {
              selectionDictionary.Add(item.Key, item.Value);
            }
            await QueuedTask.Run(() =>
            {
                //Flash the collection of features.
                mapView.FlashFeature(SelectionSet.FromDictionary(selectionDictionary));
            });
        }

        #endregion Helpers

        #region Dock Pane management

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "Select the layer to View";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
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

        #endregion Dock Pane management
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class AttributeDockpane_ShowButton : Button
    {
        protected override void OnClick()
        {
            AttributeDockpaneViewModel.Show();
        }
    }
}
