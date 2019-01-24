/*

   Copyright 2019 Esri

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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Windows.Input;
using System.Collections.ObjectModel;
using ArcGIS.Desktop.Mapping;
using System.Windows.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace ReusingProCommands
{
  internal class ReuseDockPaneViewModel : DockPane
  {
    private const string _dockPaneID = "ReusingProCommands_ReuseDockPane";
    /// <summary>
    /// used to lock collections for use by multiple threads
    /// </summary>
    private readonly object _lockCollection = new object();
    // Read only alias for map bookmarks.
    private readonly ReadOnlyObservableCollection<Bookmark> _readOnlyBookmarks;
    private readonly ObservableCollection<Bookmark> _bookmarks = new ObservableCollection<Bookmark>();

    private Bookmark _selectedBmk;

    protected ReuseDockPaneViewModel()
    {
      _readOnlyBookmarks = new ReadOnlyObservableCollection<Bookmark>(_bookmarks);
      BindingOperations.EnableCollectionSynchronization(_readOnlyBookmarks,
                        _lockCollection);
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

    #region Buttons
    public ICommand RefreshBookMarkCommand
    {
      get
      {
        return new RelayCommand(() =>
       {
         var map = MapView.Active?.Map;
         if (map == null) return;
         // no need to await
         UpdateBookmarksAsync(map);
       }, true);
      }
    }
    
    private async Task UpdateBookmarksAsync(Map selectedMap)
    {
      lock (_lockCollection)
      {
        _bookmarks.Clear();
      }
      // get the bookmarks.  GetBookmarks needs to be on MCT but want to refresh members and properties on UI thread
      await QueuedTask.Run(() =>
      {
        foreach (var bmk in selectedMap.GetBookmarks())
        {
          lock (_lockCollection)  _bookmarks.Add(bmk);
        }
      });
    }
    
    public ICommand CreateBookMarkCommand
    {
      get
      {
        var commandId = DAML.Button.esri_mapping_createBookmark;
        // get the ICommand interface from the ArcGIS Pro Button
        // using command's plug-in wrapper
        var iCommand = FrameworkApplication.GetPlugInWrapper(commandId) as ICommand;
        return new RelayCommand(() =>
                  {
                    iCommand.Execute(null);
                    var map = MapView.Active?.Map;
                    if (map == null) return;
                    // no need to await
                    UpdateBookmarksAsync(map);
                  },
                  () => iCommand.CanExecute(null));
      }
    }
    public ICommand ExitProCommand
    {
      get
      {
        var commandId = DAML.Button.esri_core_exitApplicationButton;
        // get the ICommand interface from the ArcGIS Pro Button
        // using command's plug-in wrapper
        var iCommand = FrameworkApplication.GetPlugInWrapper(commandId) as ICommand;
        return new RelayCommand(() => iCommand.Execute(null),
                                () => iCommand.CanExecute(null));
      }
    }
    #endregion // Buttons

    #region Bookmarks

    /// <summary>
    /// Holds the selected bookmark from the listview. 
    /// </summary>
    public Bookmark SelectedBookmark
    {
      get { return _selectedBmk; }
      set
      {
        SetProperty(ref _selectedBmk, value, () => SelectedBookmark);
        // zoom to it
        if (MapView.Active != null && SelectedBookmark != null) MapView.Active.ZoomToAsync(SelectedBookmark);
      }
    }

    /// <summary>
    /// collection of bookmarks.  Bind to this property in the view.
    /// </summary>
    public ReadOnlyObservableCollection<Bookmark> Bookmarks => _readOnlyBookmarks;

    #endregion Properties

    /// <summary>
    /// Text shown near the top of the DockPane.
    /// </summary>
    private string _heading = "Reusing ArcGIS Pro Commands";
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
  internal class ReuseDockPane_ShowButton : Button
  {
    protected override void OnClick()
    {
      ReuseDockPaneViewModel.Show();
    }
  }
}
