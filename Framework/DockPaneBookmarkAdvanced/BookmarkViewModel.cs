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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Events;
using ArcGIS.Desktop.Mapping;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ArcGIS.Desktop.Mapping.Events;
using System;
using System.Threading;

namespace DockPaneBookmarkAdvanced
{
    /// <summary>
    /// View model for the bookmarks dockpane.  
    /// </summary>
    internal class BookmarkViewModel : DockPane
    {
        private const string _dockPaneID = "DockPaneBookmarkAdvanced_Bookmark";
        private const string _menuID = "DockPaneBookmarkAdvanced_Bookmark_Menu";

        /// <summary>
        /// used to lock collections for use by multiple threads
        /// </summary>
        private static readonly object _lockMapCollection = new object();

        /// <summary>
        /// constructor.  
        /// </summary>
        protected BookmarkViewModel()
        {
            // set up the command to retrieve the maps
            _retrieveMapsCommand = new RelayCommand(() => RetrieveMaps(), () => true);

            // TODO Step 3 - add delete bookmark command
            _delBookmarkCommand = new RelayCommand(() => DeleteBookmark(), () => true);
        }

        #region Overrides

        /// <summary>
        /// Override to implement custom initialization code for this dockpane
        /// </summary>
        /// <returns></returns>
        protected override Task InitializeAsync()
        {
            // TODO Step 2 - make sure that AllMaps can be updated from work threads as well as the UI thread
            BindingOperations.EnableCollectionSynchronization(_allMaps, _lockMapCollection);
            // TODO Step 2 - subscribe to the ProjectItemsChangedEvent
            ProjectItemsChangedEvent.Subscribe(OnProjectCollectionChanged, false);

            return base.InitializeAsync();
        }
        #endregion

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
                        var foundItem = _allMaps.FirstOrDefault(m => m.URI == mapItem.Path);
                        // one cannot be found; so add it to our list
                        if (foundItem == null)
                        {
                           _allMaps.Add(mapItem.GetMap());
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
                          if (_allMaps.Contains(map))
                          {
                            _allMaps.Remove(map);
                          }
                        }
                    }
                    break;
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command for retrieving commands.  Bind to this property in the view.
        /// </summary>
        private ICommand _retrieveMapsCommand;
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
          _selectedBmk = null;
          _bmks = null;

          lock (_lockMapCollection)
          {
            // create / clear the collection
            if (_allMaps == null)
              _allMaps = new ObservableCollection<Map>();
            else
              _allMaps.Clear();

            if (Project.Current != null)
            {
              // GetMap needs to be on the MCT
              QueuedTask.Run(() =>
                {
                  // get the map project items and add to my collection
                  foreach (MapProjectItem item in Project.Current.GetItems<MapProjectItem>())
                  {
                    _allMaps.Add(item.GetMap());
                  }
                });
            }
          }

          // ensure the view re-binds to both the maps and bookmarks
          NotifyPropertyChanged(() => AllMaps);
          NotifyPropertyChanged(() => Bookmarks);
          NotifyPropertyChanged(() => SelectedBookmark);
          NotifyPropertyChanged(() => SelectedMap);
        }

        #endregion

        #region Properties

        /// <summary>
        /// collection of bookmarks.  Bind to this property in the view.
        /// </summary>
        private ReadOnlyObservableCollection<Bookmark> _bmks;
        public ReadOnlyObservableCollection<Bookmark> Bookmarks
        {
          get { return _bmks; }
        }

        /// <summary>
        /// Collection of map items.  Bind to this property in the view. 
        /// </summary>
        private ObservableCollection<Map> _allMaps = new ObservableCollection<Map>();
        public IReadOnlyCollection<Map> AllMaps
        {
            get {
              lock (_lockMapCollection)
              {
                return _allMaps; 
              }
            }
        }

        /// <summary>
        /// Holds the selected map from the combobox.  When setting the value, ensure that the map is open and active before retrieving the bookmarks
        /// </summary>
        private Map _selectedMap = null;
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
                    _selectedBmk = null;
                    _bmks = null;
                    NotifyPropertyChanged(() => Bookmarks);
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
          _bmks = await QueuedTask.Run(() => _selectedMap.GetBookmarks());
          _selectedBmk = null;
          NotifyPropertyChanged(() => Bookmarks);
          NotifyPropertyChanged(() => SelectedBookmark);
        }

        /// <summary>
        /// Holds the selected bookmark from the listview. 
        /// </summary>
        private Bookmark _selectedBmk;
        public Bookmark SelectedBookmark
        {
          get { return _selectedBmk; }
          set
          {
            SetProperty(ref _selectedBmk, value, () => SelectedBookmark);

            // TODO Step 3 - update the DelBookmarkToolTip property when the selected bookmark changes

            ZoomToBookmark();
          }
        }

        // TODO Step 3 - Add DelBookmarkToolTip property
        public string DelBookmarkToolTip
        { get { return "Delete this bookmark"; } }

        // TODO Step 4 - Add new bookmark command property and member variable

        /// <summary>
        /// Command for adding a new bookmark. Bind to this property in the view
        /// </summary>
        private ICommand _newBookmarkCommand;
        public ICommand NewBookmarkCommand
        {
            get
            {
                if (_newBookmarkCommand == null)
                    _newBookmarkCommand = Utils.GetICommand("esri_mapping_createBookmark");
                return _newBookmarkCommand;
            }
        }


        // TODO Step 3 - Add DelBookmarkCommand property, member variable and action function

        /// <summary>
        /// command for deleting a bookmark.  Bind to this property in the view
        /// </summary>
        private ICommand _delBookmarkCommand;
        public ICommand DelBookmarkCommand
        {
            get { return _delBookmarkCommand; }
        }

        /// <summary>
        /// method for deleting a bookmark
        /// </summary>
        private void DeleteBookmark()
        {
          if (SelectedBookmark == null)
            return;

          if (SelectedMap == null)
            return;

          // find the map
          var mapItem = Project.Current.Items.FirstOrDefault(i => i.Path == SelectedBookmark.MapURI) as MapProjectItem;

          QueuedTask.Run(() =>
          {
            Map map = mapItem.GetMap();
            if (map == null)
              return;

            // remove the bookmark
            map.RemoveBookmark(SelectedBookmark);
          });

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
          MapView.Active.ZoomToAsync(SelectedBookmark);
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
        private string _heading = "Bookmarks";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }

        #region Burger Button

        /// <summary>
        /// Tooltip shown when hovering over the burger button.
        /// </summary>
        public string BurgerButtonTooltip
        {
            get { return "Options"; }
        }

        /// <summary>
        /// Menu shown when burger button is clicked.
        /// </summary>
        public System.Windows.Controls.ContextMenu BurgerButtonMenu
        {
            get { return FrameworkApplication.CreateContextMenu(_menuID); }
        }
        #endregion
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class Bookmark_ShowButton : Button
    {
        protected override void OnClick()
        {
            BookmarkViewModel.Show();
        }
    }

    /// <summary>
    /// Button implementation for the button on the menu of the burger button.
    /// </summary>
    internal class Bookmark_MenuButton : Button
    {
        protected override void OnClick()
        {
        }
    }
}
