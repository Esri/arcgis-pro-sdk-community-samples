//Copyright 2018 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
        #region Private Properties
        private const string DockPaneId = "DockpaneSimple_BookmarkDockpane";

        /// <summary>
        /// used to lock collections for use by multiple threads
        /// </summary>
        private readonly object _lockBookmarkCollections = new object();
        private readonly object _lockMapCollections = new object();


        /// <summary>
        /// UI lists, readonly collections, and properties
        /// </summary>
        private readonly ObservableCollection<Map> _listOfMaps = new ObservableCollection<Map>();
        private readonly ObservableCollection<Bookmark> _listOfBookmarks = new ObservableCollection<Bookmark>();

        private readonly ReadOnlyObservableCollection<Map> _readOnlyListOfMaps;
        private readonly ReadOnlyObservableCollection<Bookmark> _readOnlyListOfBookmarks;

        private Bookmark _selectedBookmark;
        private Map _selectedMap;

        private ICommand _retrieveMapsCommand;

        #endregion

        #region Public Properties

        /// <summary>
        /// Our List of Maps which is bound to our Dockpane XAML
        /// </summary>
        public ReadOnlyObservableCollection<Map> ListOfMaps => _readOnlyListOfMaps;

        /// <summary>
        /// Our List of Bookmark which is bound to our Dockpane XAML
        /// </summary>
        public ReadOnlyObservableCollection<Bookmark> ListOfBookmarks => _readOnlyListOfBookmarks;

        /// <summary>
        /// This is where we store the selected Bookmark 
        /// </summary>
        public Bookmark SelectedBookmark
        {
            get { return _selectedBookmark; }
            set
            {
                SetProperty(ref _selectedBookmark, value, () => SelectedBookmark);
                System.Diagnostics.Debug.WriteLine("RetrieveMaps add maps");
                if (_selectedBookmark != null)
                {
                    // GetMap needs to be on the MCT
                    QueuedTask.Run(() =>
                    {
                        // zoom to it
                        MapView.Active.ZoomTo(_selectedBookmark);
                    });
                }
                System.Diagnostics.Debug.WriteLine("Selected bookmark changed");
            }
        }

        /// <summary>
        /// This is where we store the selected map 
        /// </summary>
        public Map SelectedMap
        {
            get { return _selectedMap; }
            set
            {
                System.Diagnostics.Debug.WriteLine("selected map");
                // make sure we're on the UI thread
                Utils.RunOnUIThread(() =>
                {
                    SetProperty(ref _selectedMap, value, () => SelectedMap);
                    if (_selectedMap != null)
                    {
                        // open /activate the map
                        Utils.OpenAndActivateMap(_selectedMap.URI);
                    }
                });
                System.Diagnostics.Debug.WriteLine("selected map opened and activated map");
                // no need to await
                UpdateBookmarks(_selectedMap);
                System.Diagnostics.Debug.WriteLine("updated bookmarks");
            }
        }

        /// <summary>
        /// Implement a 'RelayCommand' to retrieve all maps from the current project
        /// </summary>
        public ICommand RetrieveMapsCommand => _retrieveMapsCommand;

        #endregion

        #region CTor

        protected BookmarkDockpaneViewModel()
        {
            // setup the lists and sync between background and UI
            _readOnlyListOfMaps = new ReadOnlyObservableCollection<Map>(_listOfMaps);
            _readOnlyListOfBookmarks = new ReadOnlyObservableCollection<Bookmark>(_listOfBookmarks);
            BindingOperations.EnableCollectionSynchronization(_readOnlyListOfMaps, _lockMapCollections);
            BindingOperations.EnableCollectionSynchronization(_readOnlyListOfBookmarks, _lockBookmarkCollections);

            // set up the command to retrieve the maps
            _retrieveMapsCommand = new RelayCommand(() => RetrieveMaps(), () => true);
        }

        #endregion
        
        #region Overrides

        /// <summary>
        /// Override to implement custom initialization code for this dockpane
        /// </summary>
        /// <returns></returns>
        protected override Task InitializeAsync()
        {
            ProjectItemsChangedEvent.Subscribe(OnProjectCollectionChanged, false);
            return base.InitializeAsync();
        }
        #endregion

        #region Zoom to Bookmark

        /// <summary>
        /// Zooms to the currently selected bookmark. 
        /// </summary>
        internal void ZoomToBookmark()
        {
            if (SelectedBookmark == null)
                return;

            // make sure the map is open
            Utils.OpenAndActivateMap(SelectedBookmark.MapURI);
            // zoom to it
            if (MapView.Active != null) MapView.Active.ZoomToAsync(SelectedBookmark);
        }
        #endregion

        #region Show dockpane 
        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            var pane = FrameworkApplication.DockPaneManager.Find(DockPaneId);
            pane?.Activate();
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

        #endregion Show dockpane 

        #region Subscribed Events

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
                        var foundItem = _listOfMaps.FirstOrDefault(m => m.URI == mapItem.Path);
                        // one cannot be found; so add it to our list
                        if (foundItem == null)
                        {
                            _listOfMaps.Add(mapItem.GetMap());
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
                        if (_listOfMaps.Contains(map))
                        {
                            _listOfMaps.Remove(map);
                        }
                    }
                    break;
            }
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Method for retrieving map items in the project.
        /// </summary>
        private async void RetrieveMaps()
        {
            System.Diagnostics.Debug.WriteLine("RetrieveMaps");
            // clear the collections
            _listOfMaps.Clear();
            System.Diagnostics.Debug.WriteLine("RetrieveMaps list of maps clear");
            if (Project.Current != null)
            {
                System.Diagnostics.Debug.WriteLine("RetrieveMaps add maps");
                // GetMap needs to be on the MCT
                await QueuedTask.Run(() =>
                {
                    // get the map project items and add to my collection
                    foreach (MapProjectItem item in Project.Current.GetItems<MapProjectItem>())
                    {
                        _listOfMaps.Add(item.GetMap());
                    }
                });
            }
            System.Diagnostics.Debug.WriteLine("RetrieveMaps added maps");
        }

        private async Task UpdateBookmarks(Map map)
        {
            // get the bookmarks.  GetBookmarks needs to be on MCT but want to refresh members and properties on UI thread

            System.Diagnostics.Debug.WriteLine("UpdateBookmarks");
            _listOfBookmarks.Clear();
            System.Diagnostics.Debug.WriteLine("UpdateBookmarks list cleared");
            if (map == null)
            {
                System.Diagnostics.Debug.WriteLine("RetrieveMaps no maps");
                return;
            }
            await QueuedTask.Run(() =>
            {
                foreach (var bookmark in map.GetBookmarks())
                {
                    _listOfBookmarks.Add(bookmark);
                }
            });
            System.Diagnostics.Debug.WriteLine("UpdateBookmarks new list done");
        }

        #endregion Private Helpers
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
