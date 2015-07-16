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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Events;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace DockpaneSimple
{
    internal class BookmarkDockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "DockpaneSimple_BookmarkDockpane";

        /// <summary>
        /// used to lock collections for use by multiple threads
        /// </summary>
        private static readonly object _lockMapCollection = new object();

        #region Properties

        private ObservableCollection<Map> _listOfMaps = new ObservableCollection<Map>();
        /// <summary>
        /// Our List of Maps which is bound to our Dockpane XAML
        /// </summary>
        public IReadOnlyCollection<Map> ListOfMaps
        {
            get { return _listOfMaps; }
        }

        private ReadOnlyObservableCollection<Bookmark> _listOfBookmarks = null;
        /// <summary>
        /// Our List of Bookmark which is bound to our Dockpane XAML
        /// </summary>
        public ReadOnlyObservableCollection<Bookmark> ListOfBookmarks
        {
            get { return _listOfBookmarks; }
        }

        private Bookmark _selectedBookmark;
        /// <summary>
        /// This is where we store the selected Bookmark 
        /// </summary>
        public Bookmark SelectedBookmark
        {
            get { return _selectedBookmark; }
            set
            {
                SetProperty(ref _selectedBookmark, value, () => SelectedBookmark);

                // zoom to it
                MapView.Active.ZoomToAsync(_selectedBookmark);
            }
        }

        #endregion


        protected BookmarkDockpaneViewModel()
        {
            // set up the command to retrieve the maps
            _retrieveMapsCommand = new RelayCommand(() => RetrieveMaps(), () => true);
        }

        #region Overrides

        /// <summary>
        /// Override to implement custom initialization code for this dockpane
        /// </summary>
        /// <returns></returns>
        protected override Task InitializeAsync()
        {
            // TODO Step 2 - make sure that AllMaps can be updated from work threads as well as the UI thread
            BindingOperations.EnableCollectionSynchronization(_listOfMaps, _lockMapCollection);
            // TODO Step 2 - subscribe to the ProjectItemsChangedEvent
            ProjectItemsChangedEvent.Subscribe(OnProjectCollectionChanged, false);

            return base.InitializeAsync();
        }
        #endregion

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
        private string _heading = "My Maps and Bookmarks";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }

        #region Subscribed Events

        // TODO Step 2 - add OnProjectCollectionChanged method
        /// <summary>
        /// Subscribe to Project Items Changed events which is getting called each
        /// time the project items change which happens when a new map is added or removed in ArcGIS Pro
        /// </summary>
        /// <param name="args">ProjectItemsChangedEventArgs</param>
        private void OnProjectCollectionChanged(ProjectItemsChangedEventArgs args)
        {
            if (args == null)
                return;
            var mapItem = args.ProjectItem as MapProjectItem;
            if (mapItem == null)
                return;

            // new project item was added
            switch (args.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    {
                        lock (_lockMapCollection)
                        {
                            var foundItem = _listOfMaps.FirstOrDefault(m => m.URI == mapItem.Path);
                            // one cannot be found; so add it to our list
                            if (foundItem == null)
                            {
                                _listOfMaps.Add(mapItem.GetMap());
                            }
                        }
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    {
                        Map map = mapItem.GetMap();
                        // if this is the selected map, resest
                        if (SelectedMap == map)
                            SelectedMap = null;

                        // remove from the collection
                        lock (_lockMapCollection)
                        {
                            if (_listOfMaps.Contains(map))
                            {
                                _listOfMaps.Remove(map);
                            }
                        }
                    }
                    break;
            }
        }

        #endregion

        private ICommand _retrieveMapsCommand;
        /// <summary>
        /// Implement a 'RelayCommand' to retrieve all maps from the current project
        /// </summary>
        public ICommand RetrieveMapsCommand
        {
            get { return _retrieveMapsCommand; }
        }

        /// <summary>
        /// Method for retrieving map items in the project.
        /// </summary>
        private void RetrieveMaps()
        {
            // reset
            _selectedMap = null;
            _selectedBookmark = null;
            _listOfBookmarks = null;

            lock (_lockMapCollection)
            {
                // create / clear the collection
                if (_listOfMaps == null)
                    _listOfMaps = new ObservableCollection<Map>();
                else
                    _listOfMaps.Clear();

                if (Project.Current != null)
                {
                    // GetMap needs to be on the MCT
                    QueuedTask.Run(() =>
                    {
                        // get the map project items and add to my collection
                        foreach (MapProjectItem item in Project.Current.GetItems<MapProjectItem>())
                        {
                            _listOfMaps.Add(item.GetMap());
                        }
                    });
                }
            }

            // ensure the view re-binds to both the maps and bookmarks
            NotifyPropertyChanged(() => ListOfMaps);
            NotifyPropertyChanged(() => ListOfBookmarks);
            NotifyPropertyChanged(() => SelectedBookmark);
            NotifyPropertyChanged(() => SelectedMap);
        }

        private Map _selectedMap;
        /// <summary>
        /// This is where we store the selected map 
        /// </summary>
        public Map SelectedMap
        {
            get { return _selectedMap; }
            set
            {
                // make sure we're on the UI thread
                Utils.RunOnUIThread(() =>
                {
                    SetProperty(ref _selectedMap, value, () => SelectedMap);
                    if (_selectedMap == null)
                    {
                        _selectedBookmark = null;
                        _listOfBookmarks = null;
                        NotifyPropertyChanged(() => ListOfBookmarks);
                        NotifyPropertyChanged(() => SelectedBookmark);
                        return;
                    }

                    // open /activate the map
                    Utils.OpenAndActivateMap(_selectedMap.URI);
                });

                // no need to await
                UpdateBookmarks();                
            }
        }

        private async Task UpdateBookmarks()
        {
            // get the bookmarks.  GetBookmarks needs to be on MCT but want to refresh members and properties on UI thread
            _listOfBookmarks = await QueuedTask.Run(() => _selectedMap.GetBookmarks());
            _selectedBookmark = null;
            NotifyPropertyChanged(() => ListOfBookmarks);
            NotifyPropertyChanged(() => SelectedBookmark);
        }      

    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class BookmarkDockpane_ShowButton : Button
    {
        protected override void OnClick()
        {
            BookmarkDockpaneViewModel.Show();
        }
    }
}
