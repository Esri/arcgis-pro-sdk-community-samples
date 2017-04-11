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
using System.Windows;

namespace DockPaneBookmarkAdvanced
{
  /// <summary>
  /// View model for the bookmarks dockpane.  
  /// </summary>
  internal class BookmarkViewModel : DockPane
  {
    private const string DockPaneId = "DockPaneBookmarkAdvanced_Bookmark";
    private const string MenuId = "DockPaneBookmarkAdvanced_Bookmark_Menu";

    /// <summary>
    /// used to lock collections for use by multiple threads
    /// </summary>
    private readonly object _lockCollection = new object();

    private readonly ObservableCollection<Bookmark> _bookmarks = new ObservableCollection<Bookmark>();
    // Read only alias for map bookmarks.
    private readonly ReadOnlyObservableCollection<Bookmark> _readOnlyBookmarks;

    private readonly ObservableCollection<Map> _maps = new ObservableCollection<Map>();
    // Read only alias for project bookmarks.
    private readonly ReadOnlyObservableCollection<Map> _readOnlyMaps;


    /// <summary>
    /// constructor.  
    /// </summary>
    protected BookmarkViewModel()
    {
      // set up the command to retrieve the maps
      _retrieveMapsCommand = new RelayCommand(() => RetrieveMaps(), () => true);

      _delBookmarkCommand = new RelayCommand(() => DeleteBookmark(), () => true);

      _readOnlyBookmarks = new ReadOnlyObservableCollection<Bookmark>(_bookmarks);
      _readOnlyMaps = new ReadOnlyObservableCollection<Map>(_maps);

      Utils.RunOnUiThread(() =>
      {
        BindingOperations.EnableCollectionSynchronization(_readOnlyMaps, _lockCollection);
        BindingOperations.EnableCollectionSynchronization(_readOnlyBookmarks, _lockCollection);
      });
      ProjectItemsChangedEvent.Subscribe(OnProjectCollectionChanged, false);
      CheckBookmarks(_selectedMap);
      IsShowCircularAnimation = Visibility.Collapsed;
    }

       

    #region Overrides

    /// <summary>
    /// Override to implement custom initialization code for this dockpane
    /// </summary>
    /// <returns></returns>
    protected override async Task InitializeAsync()
    {
        IsNoBookmarkExists = Visibility.Collapsed;
                
    }
    #endregion

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
    IsShowCircularAnimation = Visibility.Visible;
    // new project item was added
    switch (args.Action)
      {
        case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
          {
            var foundItem = _maps.FirstOrDefault(m => m.URI == mapItem.Path);
            // one cannot be found; so add it to our list
            if (foundItem == null)
            {
              _maps.Add(mapItem.GetMap());
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
            if (_maps.Contains(map))
            {
              _maps.Remove(map);
            }
          }
          break;
      }
      InitMaps();
      IsShowCircularAnimation = Visibility.Hidden;
    }

    #endregion

    #region Commands

    /// <summary>
    /// Command for retrieving commands.  Bind to this property in the view.
    /// </summary>
    private readonly ICommand _retrieveMapsCommand;
    public ICommand RetrieveMapsCommand => _retrieveMapsCommand;

    /// <summary>
    /// Method for retrieving map items in the project.
    /// </summary>
    private void RetrieveMaps()
    {

      // create / clear the collection
      _maps.Clear();
      if (Project.Current != null)
      {
        // GetMap needs to be on the MCT
        QueuedTask.Run(() =>
            {
                // get the map project items and add to my collection
                foreach (var item in Project.Current.GetItems<MapProjectItem>())
              {
                _maps.Add(item.GetMap());
              }               
            });
      }       
    }
        #endregion

    #region Properties
    private Visibility _isCircularAnimation;
        
    public Visibility IsShowCircularAnimation
        {
            get
            {
                return _isCircularAnimation;
            }

            set
            {
                SetProperty(ref _isCircularAnimation, value, () => IsShowCircularAnimation);
            }
    }

    private Visibility _isNoBookmarkExists = Visibility.Collapsed;
    public Visibility IsNoBookmarkExists
    {
            get
            {
                return _isNoBookmarkExists;
            }

            set
            {
                SetProperty(ref _isNoBookmarkExists, value, () => IsNoBookmarkExists);
            }
        }


    /// <summary>
    /// collection of bookmarks.  Bind to this property in the view.
    /// </summary>
    public ReadOnlyObservableCollection<Bookmark> Bookmarks => _readOnlyBookmarks;

    /// <summary>
    /// Collection of map items.  Bind to this property in the view. 
    /// </summary>
    public ReadOnlyObservableCollection<Map> AllMaps => _readOnlyMaps;

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
            _bookmarks.Clear();            
            Utils.RunOnUiThread(async () =>
                {
                    SetProperty(ref _selectedMap, value, () => SelectedMap);
                    CheckBookmarks(_selectedMap);
                    if (_selectedMap == null)
                    {
                        _selectedBmk = null;                       
                        return;
                    }
                // open /activate the map
                Utils.OpenAndActivateMap(_selectedMap.URI);

                // no need to await
                await UpdateBookmarks(SelectedMap);
                });                
        }
    }

     private string _bookmarksViewType = "Gallery";
      public string BookmarksViewType
        {
          get
          {
              return _bookmarksViewType;
          }
          set { SetProperty(ref _bookmarksViewType, value, () => BookmarksViewType); } }

    private async Task UpdateBookmarks(Map selectedMap)
    {
      _bookmarks.Clear();
      // get the bookmarks.  GetBookmarks needs to be on MCT but want to refresh members and properties on UI thread
      await QueuedTask.Run(() =>
      {
          foreach (var bmk in selectedMap.GetBookmarks())
        {
          _bookmarks.Add(bmk);
        }
      });
    }

    private async Task CheckBookmarks(Map selectedMap)
        {
            if (selectedMap == null)
            {
                IsNoBookmarkExists = Visibility.Visible;
                return;
            }
            await QueuedTask.Run(() =>
            {
                if (_selectedMap.GetBookmarks().Count == 0)
                {
                    IsNoBookmarkExists = Visibility.Visible;
                    return;
                }
                else
                    IsNoBookmarkExists = Visibility.Collapsed;
            });
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
        ZoomToBookmark();
      }
    }

        private string _SearchText;
        public string SearchText
        {
            get { return _SearchText; }
            set
            {

                SetProperty(ref _SearchText, value, () => SearchText);
                foreach (var bmk in Bookmarks)
                {
                    if (bmk.Name.StartsWith(_SearchText, StringComparison.CurrentCultureIgnoreCase))
                    {
                        SelectedBookmark = bmk;
                    }
                }
                
            }
        }
        public string DelBookmarkToolTip => "Delete this bookmark";

    /// <summary>
    /// Command for adding a new bookmark. Bind to this property in the view
    /// </summary>
    private ICommand _newBookmarkCommand;
    public ICommand NewBookmarkCommand
    {
      get
      {
        if (_newBookmarkCommand == null)
        {
          _newBookmarkCommand = FrameworkApplication.GetPlugInWrapper("esri_mapping_createBookmark") as ICommand;
        }
        return _newBookmarkCommand;
      }
    }

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
    private async void DeleteBookmark()
    {
      if (SelectedBookmark == null)
        return;

      if (SelectedMap == null)
        return;

      // clear the bookmarks
      _bookmarks.Clear();
      // find the map
      var mapItem = Project.Current.Items.FirstOrDefault(i => i.Path == SelectedBookmark.MapURI) as MapProjectItem;

      await QueuedTask.Run(() =>
      {
        var map = mapItem?.GetMap();
        if (map == null)
          return;

              // remove the bookmark
              map.RemoveBookmark(SelectedBookmark);
      });
      // no need to await
      await UpdateBookmarks(SelectedMap);
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

    /// <summary>
    /// Show the DockPane.
    /// </summary>
    internal static void Show()
    {
      DockPane pane = FrameworkApplication.DockPaneManager.Find(DockPaneId);
      if (pane == null)
        return;

      pane.Activate();
    }
  
    internal static void InitMaps()
    {
        Thread.Sleep(6000);
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
      get { return "Change view"; }
    }

    /// <summary>
    /// Menu shown when burger button is clicked.
    /// </summary>
    public System.Windows.Controls.ContextMenu BurgerButtonPopupMenu
    {
      get { return FrameworkApplication.CreateContextMenu(MenuId); }
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
  internal class BookmarkOutline_MenuButton : Button
  {
    protected override void OnClick()
    {
        var vm = FrameworkApplication.DockPaneManager.Find("DockPaneBookmarkAdvanced_Bookmark") as BookmarkViewModel;
        vm.BookmarksViewType = "List";
    }
  }

    /// <summary>
    /// Button implementation for the button on the menu of the burger button.
    /// </summary>
    internal class BookmarkGallery_MenuButton : Button
    {
        protected override void OnClick()
        {
            var vm = FrameworkApplication.DockPaneManager.Find("DockPaneBookmarkAdvanced_Bookmark") as BookmarkViewModel;
            vm.BookmarksViewType = "Gallery";
        }
    }
}
