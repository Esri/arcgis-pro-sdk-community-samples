//   Copyright 2019 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using ArcGIS.Desktop.Mapping.Events;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace TimeNavigation
{
	/// <summary>
	/// This sample provides a new tab and controls that allow you to set the time in the map view, step through time, and navigate between time enabled bookmarks in the map.
	/// </summary>
	/// <remarks>
	/// 1. Download the Community Sample data(see under the 'Resources' section for downloading sample data).  The sample data contains required data for this sample add-in.  Make sure that the Sample data is unzipped in c:\data and "C:\Data\Interacting with Maps" is available.
	/// 1. In Visual Studio click the Build menu.Then select Build Solution.  
	/// 1. Launch the debugger to open ArcGIS Pro.
	/// 1. Open the "C:\Data\Interacting with Maps\Interacting with Maps.aprx" project which contains the required data needed for this sample.
	/// 1. Open the Portland Crimes map.
  /// 1. Click on the new Navigation tab on the ribbon.  
	/// ![UI](screenshots/UICommands.png)  
	/// 1. Within this tab there are 3 groups that provide functionality to navigate through time.
	/// 1. The Map Time group provides two date picker controls to set the start and end time in the map.
	/// 1. The Time Step group provides two combo boxes to set the time step interval. The previous and next button can be used to offset the map time forward or back by the specified time step interval.
	/// 1. The Bookmarks group provides a gallery of time enabled bookmarks for the map. Clicking a bookmark in the gallery will zoom the map to that location and time. 
	/// It also provides play, previous and next buttons that can be used to navigate between the time enabled bookmarks. 
	/// These commands are only enabled when there are at least 2 bookmarks in the map. Finally it provides a slider that can be used to set how quickly to move between bookmarks during playback.
	/// </remarks>
	internal class TimeModule : Module
  {
    private static TimeModule _this = null;
    private static Settings _settings = Settings.Default;
    private static ReadOnlyObservableCollection<Bookmark> _bookmarks;

    public TimeModule()
    {
      _bookmarks = new ReadOnlyObservableCollection<Bookmark>(new ObservableCollection<Bookmark>());
      ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);     
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
      LoadBookmarks();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    }

    ~TimeModule()
    {
      _settings.Save();
      ActiveMapViewChangedEvent.Unsubscribe(OnActiveMapViewChanged);
    }

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static TimeModule Current
    {
      get
      {
        return _this ?? (_this = (TimeModule)FrameworkApplication.FindModule("TimeNavigation_Module"));
      }
    }

    public static Settings Settings
    {
      get { return _settings; }
    }

    #region Bookmarks

    public static List<Bookmark> Bookmarks
    {
      get { return new List<Bookmark>(_bookmarks.Where(b => b.HasTimeExtent)); }
    }

    public static Bookmark CurrentBookmark { get; set; }

    public static Bookmark GetNextBookmark()
    {
      var bookmarks = TimeModule.Bookmarks;

      int i = 0;
      if (TimeModule.CurrentBookmark != null)
      {
        i = bookmarks.IndexOf(TimeModule.CurrentBookmark) + 1;
        if (i >= bookmarks.Count)
          i = 0;
      }
      TimeModule.CurrentBookmark = bookmarks.ElementAt(i);
      return TimeModule.CurrentBookmark;
    }

    public static Bookmark GetPreviousBookmark()
    {
      var bookmarks = TimeModule.Bookmarks;

      int i = bookmarks.Count - 1;
      if (TimeModule.CurrentBookmark != null)
      {
        i = bookmarks.IndexOf(TimeModule.CurrentBookmark) - 1;
        if (i < 0)
          i = bookmarks.Count - 1;
      }
      TimeModule.CurrentBookmark = bookmarks.ElementAt(i);
      return TimeModule.CurrentBookmark;
    }

    private async Task LoadBookmarks()
    {
      if (MapView.Active != null)
        _bookmarks = await QueuedTask.Run(() => MapView.Active.Map.GetBookmarks());
      else
        _bookmarks = new ReadOnlyObservableCollection<Bookmark>(new ObservableCollection<Bookmark>());
    }

    #endregion

    #region Event Handlers

    private void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs obj)
    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
      LoadBookmarks();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    }

    #endregion
  }
}
