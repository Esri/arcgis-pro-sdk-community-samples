/*

   Copyright 2024 Esri

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
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RowEventTest
{
  internal class ShowEventsViewModel : DockPane
  {
    private const string _dockPaneID = "RowEventTest_ShowEventsView";

    private static readonly object _lock = new(); 
    private List<string> _entries = new();


    protected ShowEventsViewModel() {
      Module1.ShowEventsViewModel = this;
    }

    public string EventLog
    {
      get
      {
        string contents = "";
        lock (_lock)
        {
          contents = string.Join("\r\n", _entries.ToArray());
        }
        return contents;
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

    public void AddEntry(string entry)
    {
      lock (_lock)
      {
        _entries.Add($"{entry}");
      }      
    }

    public void RefreshEvents ()
    {
      NotifyPropertyChanged(nameof(EventLog));
    }


    public void ClearEntries()
    {
      lock (_lock)
      {
        _entries.Clear();
      }
      NotifyPropertyChanged(nameof(EventLog));
    }

    /// <summary>
    /// utility function to enable an action to run on the UI thread (if not already)
    /// </summary>
    /// <param name="action">the action to execute</param>
    /// <returns></returns>
    internal static void RunOnUiThread(Action action)
    {
      try
      {
        if (IsOnUiThread)
          action();
        else
          System.Windows.Application.Current.Dispatcher.BeginInvoke(action);
      }
      catch (Exception ex)
      {
        MessageBox.Show($@"Error in RunOnUiThread: {ex.Message}");
      }
    }

    /// <summary>
    /// Determines whether the calling thread is the thread associated with this 
    /// System.Windows.Threading.Dispatcher, the UI thread.
    /// 
    /// If called from a View model test it always returns true.
    /// </summary>
    public static bool IsOnUiThread => ArcGIS.Desktop.Framework.FrameworkApplication.TestMode || System.Windows.Application.Current.Dispatcher.CheckAccess();


    /// <summary>
    /// Text shown near the top of the DockPane.
    /// </summary>
    private string _heading = "Event Log";
    public string Heading
    {
      get => _heading;
      set => SetProperty(ref _heading, value);
    }
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class ShowEventsView_ShowButton : Button
  {
    protected override void OnClick()
    {
      ShowEventsViewModel.Show();
    }
  }
}
