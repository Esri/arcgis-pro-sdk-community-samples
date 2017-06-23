//Copyright 2014 Esri

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
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using LivingAtlasOfTheWorld.Common;
using LivingAtlasOfTheWorld.Models;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace LivingAtlasOfTheWorld.UI {
    /// <summary>
    /// Represents the settings for the Browse Dialog
    /// </summary>
    class BrowseLayersViewModel : INotifyPropertyChanged  {
        private static readonly string _title = "Browse Esri Map Layers and Web Maps";

        private List<OnlineQuery> _browseQueries = new List<OnlineQuery>();
        private OnlineQuery _selectedOnlineQuery = null;
        private string _keywords = "";
        private ObservableCollection<OnlineResultItem> _results = new ObservableCollection<OnlineResultItem>();
        private ICommand _submitQueryCommand = null;

        private DispatcherTimer _timer = null;
        
        private double _progressValue = 1;
        private double _maxProgressValue = 100;
        
        private bool _executeQuery = false;
        private bool _isInitialized = false;
        private bool _addFailed = false;

        private static List<string> _resultOptions = new List<string>() {"Layer", "Web Map"};
        private string _selResultOption = "";

        public BrowseLayersViewModel() {
            BindingOperations.CollectionRegistering += BindingOperations_CollectionRegistering;
            Initialize();
        }

        void BindingOperations_CollectionRegistering(object sender, CollectionRegisteringEventArgs e) {
            if (e.Collection == Results)
                BindingOperations.EnableCollectionSynchronization(_results, new object());
        }

        private void Initialize() {
            if (_isInitialized)
                return;
            _isInitialized = true;
            _selResultOption = _resultOptions[0];
            OnlineUriFactory.CreateOnlineUris();
            foreach (var uri in OnlineUriFactory.OnlineUris) {
        //create a query
        OnlineQuery query = new OnlineQuery()
        {
          OnlineUri = uri
        };
        _browseQueries.Add(query);
            }

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
                FrameworkApplication.Current.Dispatcher.Invoke(() => OnPropertyChanged("ProgressValue"));
            };
            
        }
        /// <summary>
        /// Gets the title
        /// </summary>
        public string Title {
            get {
                return _title;
            }
        }

        /// <summary>
        /// Gets and sets the keywords for the current query
        /// </summary>
        public string Keywords {
            get {
                return _keywords;
            }
            set {
                _keywords = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Gets the list of supported named queries
        /// </summary>
        public IReadOnlyList<OnlineQuery> BrowseQueryList {
            get {
                if (_browseQueries.Count == 0)
                    Initialize();
                return _browseQueries;
            }
        }
        /// <summary>
        /// Gets and sets the selected Uri to be submitted
        /// </summary>
        public OnlineQuery SelectedOnlineQuery {
            get {
                return _selectedOnlineQuery;
            }
            set {
                _selectedOnlineQuery = value;
                OnPropertyChanged();
                SubmitQuery();
            }
        }

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

        /// <summary>
        /// Gets the list of results from the query
        /// </summary>
        public ObservableCollection<OnlineResultItem> Results {
            get {
                return _results;
            }
        }

        /// <summary>
        /// Gets the list of supported result options
        /// </summary>
        public static IReadOnlyList<string> ResultOptions {
            get {
                return _resultOptions;
            }
        }
        /// <summary>
        /// Gets and sets the current result option
        /// </summary>
        /// <remarks>Whether to return web maps or layers</remarks>
        public string SelectedResultOption {
            get {
                return _selResultOption;
            }
            set {
                _selResultOption = value;
                OnPropertyChanged();
                SubmitQuery();
            }
        }
        /// <summary>
        /// Gets the relevant link text to display on the thumbnails
        /// </summary>
        public string LinkText {
            get {
                string linkText = (SelectedResultOption == "Layer" ? "Add layer to map" : "Add web map");
                return linkText;
            }
        }
        /// <summary>
        /// Gets the add status
        /// </summary>
        public string AddStatus {
            get {
                return _addFailed ? "Sorry, item cannot be added" : "";
            }
        }

        #region Commands
        /// <summary>
        /// Gets a command to "manually" submit the query (assumes  keywords have been added) 
        /// </summary>
        public ICommand SubmitQueryCommand {
            get {
                if (_submitQueryCommand == null)
                    _submitQueryCommand = new RelayCommand((Action)SubmitQuery);
                return _submitQueryCommand;
            }
        }
        #endregion Commands

        internal void AddLayerToMap(string url) {
            //clear previous flag, if any
            _addFailed = false;
            OnPropertyChanged("AddStatus");

            string id = url;
            //get the query result
            var result = _results.FirstOrDefault(ri => ri.Id == id);
            if (result == null)
                throw new ApplicationException(string.Format("Debug: bad id {0}",id));
            if (result.Item == null)
                result.Item = ItemFactory.Instance.Create(id, ItemFactory.ItemType.PortalItem);
            // Syntax can get complicated here
            // Question - do you need to await the QueuedTask.Run? If so, you need a Func<Task>
            // and not an Action.
            //http://stackoverflow.com/questions/20395826/explicitly-use-a-functask-for-asynchronous-lambda-function-when-action-overloa
            //
            //As it currently stands, this QueuedTask.Run will return to the caller on the first await
            QueuedTask.Run(action: async () => {
                if (LayerFactory.Instance.CanCreateLayerFrom(result.Item))
                    LayerFactory.Instance.CreateLayer(result.Item, MapView.Active.Map);
                else if (MapFactory.Instance.CanCreateMapFrom(result.Item)) {
                    Map newMap = MapFactory.Instance.CreateMapFromItem(result.Item);
                    IMapPane newMapPane = await ProApp.Panes.CreateMapPaneAsync(newMap);
                }
                else {
                    _addFailed = true;//cannot be added
                    FrameworkApplication.Current.Dispatcher.Invoke(() => OnPropertyChanged("AddStatus"));
                }
            });
        }

        private async void SubmitQuery() {
            if (this.SelectedOnlineQuery == null)
                return;
            if (_executeQuery)
                return;
            _executeQuery = true;
            _addFailed = false;
            _results.Clear();
            _timer.Start();
            OnPropertyChanged("IsExecutingQuery");
            OnPropertyChanged("AddStatus");

            try {
                var submitQuery = new SubmitOnlineQuery();
                this.SelectedOnlineQuery.Keywords = this.Keywords;
                this.SelectedOnlineQuery.Content = SelectedResultOption;
                var response = await submitQuery.ExecWithEsriClientAsync(this.SelectedOnlineQuery, _results);
            }
            finally {
                _timer.Stop();
                _executeQuery = false;
                OnPropertyChanged("IsExecutingQuery");
                //OnPropertyChanged("Results");
            } 
        }
        
        public event PropertyChangedEventHandler PropertyChanged =  delegate {};

        protected void OnPropertyChanged([CallerMemberName] string name = "") {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
