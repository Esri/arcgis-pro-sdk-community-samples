/*

   Copyright 2016 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

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
using System.Windows.Input;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Events;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.Windows.Data;
using ArcGIS.Desktop.Mapping.Events;

namespace IdentifyWindow
{
    internal class AttributeDockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "IdentifyWindow_AttributeDockpane";

        private FeatureLayer _selectedFeatureLayer;

        private ObservableCollection<FeatureLayer> _featureLayers = new ObservableCollection<FeatureLayer>();
        private readonly object _lockFeaturelayers = new object();

        private DataTable _selectedFeaturesDataTable = new DataTable();
        private DataRowView _selectedFeature = null;

        private readonly object _lockSelectedFeaturesDataTable = new object();

        private KeyValuePair<string, int>[] _chartResult;
        private readonly object _lockPieChart = new object();

        // hook ArcGIS Pro Button
        public ICommand SelectionTool { get; set; }
        public ICommand CloseCommand { get; set; }

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

            BindingOperations.EnableCollectionSynchronization(_featureLayers, _lockFeaturelayers);

            // subscribe to the map view changed event... that's when we update the list of feature layers
            ActiveMapViewChangedEvent.Subscribe(OnActiveViewChanged);
            ActivePaneChangedEvent.Subscribe(OnActivePaneChanged);

            // subscribe to the selection changed event ... that's when we refresh our features
            MapSelectionChangedEvent.Subscribe(OnMapSelectionChanged);

            // hook ArcGIS Pro Button
            var toolWrapper = FrameworkApplication.GetPlugInWrapper(DAML.Tool.esri_mapping_selectByRectangleTool);
            var toolCmd = toolWrapper as ICommand; // tool and command(Button) supports this
            if (toolCmd != null)
            {
                SelectionTool = new RelayCommand(param => toolCmd.Execute(null),
                                param => toolCmd.CanExecute(null));
            }
            var closeWrapper = FrameworkApplication.GetPlugInWrapper(DAML.Button.esri_core_exitApplicationButton);
            var closeCmd = closeWrapper as ICommand; // tool and command(Button) supports this
            if (closeCmd != null)
            {
                CloseCommand = new RelayCommand(param => closeCmd.Execute(null),
                                param => closeCmd.CanExecute(null));
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

        /// <summary>
        /// Called when the selection o
        /// </summary>
        /// <param name="args"></param>
        private void OnMapSelectionChanged(MapSelectionChangedEventArgs args)
        {
            if (SelectedFeatureLayer == null) return;
            GetSelectedFeatures(SelectedFeatureLayer);
            ComputePieChart();
            Zoom2Select();
        }

        private bool _mapViewNotInitialized = false;

        private void OnActivePaneChanged(PaneEventArgs args)
        {
            // get new feature layer list
            SelectedFeatureDataTable = null;
            ChartResult = new KeyValuePair<string, int>[0];
            FeatureLayers.Clear();
            var mapView = MapView.Active;
            if (mapView == null)
            {
                _mapViewNotInitialized = true;
                return;
            }
            GetFeatureLayers();
        }

        private void OnActiveViewChanged(ActiveMapViewChangedEventArgs args)
        {
            if (!_mapViewNotInitialized) return;
            _mapViewNotInitialized = false;
            ChartResult = new KeyValuePair<string, int>[0];
            FeatureLayers.Clear();
            GetFeatureLayers();
        }

        /// <summary>
        /// Zoom to selection
        /// </summary>
        private async void Zoom2Select()
        {
            var mapView = MapView.Active;
            if (mapView == null) return;
            await QueuedTask.Run(() =>
            {
                //select features that intersect the sketch geometry
                var selection =  mapView.Map.GetSelection()
                      .Where(kvp => kvp.Key is BasicFeatureLayer)
                      .ToDictionary(kvp => (BasicFeatureLayer)kvp.Key, kvp => kvp.Value);

                //zoom to selection
                MapView.Active.ZoomToAsync(selection.Select(kvp => kvp.Key), true);
            }).ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    var aggException = t.Exception.Flatten();
                    foreach (var exception in aggException.InnerExceptions)
                        System.Diagnostics.Debug.WriteLine(exception.Message);
                }
            });
        }

        /// <summary>
        /// List of the current active map's feature layers
        /// </summary>
        public ObservableCollection<FeatureLayer> FeatureLayers
        {
            get { return _featureLayers; }
            set
            {
                SetProperty(ref _featureLayers, value, () => FeatureLayers);
            }
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
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            pane.Activate();
        }

        private async void ComputePieChart()
        {
            var mapView = MapView.Active;
            if (mapView == null) return;
            await QueuedTask.Run(() =>
            {
                var pieChartResult = new List<KeyValuePair<string, int>>();
                foreach (var selection in mapView.Map.GetSelection()
                    .Where(kvp => kvp.Key is BasicFeatureLayer))
                {
                    pieChartResult.Add(new KeyValuePair<string, int>(selection.Key.Name, selection.Value.Count));
                }
                lock (_lockPieChart) _chartResult = pieChartResult.ToArray();
                NotifyPropertyChanged(() => ChartResult);
            }).ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    var aggException = t.Exception.Flatten();
                    foreach (var exception in aggException.InnerExceptions)
                        System.Diagnostics.Debug.WriteLine(exception.Message);
                }
            });
        }

        private async void GetFeatureLayers()
        {
            //Get the active map view.
            var mapView = MapView.Active;
            if (mapView == null) return;
            await QueuedTask.Run(() =>
            {
                var featureLayers = mapView.Map.Layers.OfType<FeatureLayer>();
                lock (_lockFeaturelayers)
                {
                    _featureLayers.Clear();
                    foreach (var featureLayer in featureLayers) _featureLayers.Add(featureLayer);
                }
            }).ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    var aggException = t.Exception.Flatten();
                    foreach (var exception in aggException.InnerExceptions)
                        System.Diagnostics.Debug.WriteLine(exception.Message);
                }
            });
            NotifyPropertyChanged(() => FeatureLayers);
            //var firstFeatureLayer = FeatureLayers.FirstOrDefault();
            //if (firstFeatureLayer != null) SelectedFeatureLayer = firstFeatureLayer;
        }

        private async void GetSelectedFeatures(FeatureLayer selectedFeatureLayer)
        {
            //Get the active map view.
            var mapView = MapView.Active;
            if (mapView == null || selectedFeatureLayer == null) return;
            await QueuedTask.Run(() =>
            {
                // Get all selected features for selectedFeatureLayer
                // and populate a datatable with data and column headers
                var resultTable = new DataTable();
                using (var rowCursor = selectedFeatureLayer.GetSelection().Search(null))
                {
                    bool bDefineColumns = true;
                    while (rowCursor.MoveNext())
                    {
                        var anyRow = rowCursor.Current;
                        if (bDefineColumns)
                        {
                            foreach (var fld in anyRow.GetFields().Where(fld => fld.FieldType != FieldType.Geometry))
                            {
                                resultTable.Columns.Add(new DataColumn(fld.Name, typeof(string)) { Caption = fld.AliasName });
                            }
                        }
                        var addRow = resultTable.NewRow();
                        foreach (var fld in anyRow.GetFields().Where(fld => fld.FieldType != FieldType.Geometry))
                        {
                            addRow[fld.Name] = (anyRow[fld.Name] == null) ? string.Empty : anyRow[fld.Name].ToString();
                        }
                        resultTable.Rows.Add(addRow);
                        bDefineColumns = false;
                    }
                }
                lock (_lockSelectedFeaturesDataTable) _selectedFeaturesDataTable = resultTable;
            }).ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    var aggException = t.Exception.Flatten();
                    foreach (var exception in aggException.InnerExceptions)
                        System.Diagnostics.Debug.WriteLine(exception.Message);
                }
            });
            NotifyPropertyChanged(() => SelectedFeatureDataTable);
        }

        private async void FlashFeaturesAsync(IReadOnlyDictionary<BasicFeatureLayer, List<long>> flashFeatures)
        {
            //Get the active map view.
            var mapView = MapView.Active;
            if (mapView == null)
                return;

            await QueuedTask.Run(() =>
            {
                //Flash the collection of features.
                mapView.FlashFeature(flashFeatures);
            });
        }

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
