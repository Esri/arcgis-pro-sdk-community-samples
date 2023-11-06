/*

   Copyright 2023 Esri

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
using ArcGIS.Core.Events;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Geoprocessing;
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
using System.Windows.Input;

namespace GeoProcessingHistory
{
  internal class GPHistoryAPITestViewModel : DockPane
  {
    private const string _dockPaneID = "GeoProcessingHistory_GPHistoryAPITest";

    protected GPHistoryAPITestViewModel() 
    {
      Module1.GPHistoryAPITestVM = this;
    }

    #region Button Commands

    public ICommand CmdRunTest
    {
      get
      {
        return new RelayCommand(async () =>
        {
          SubscriptionToken eventToken = null;
          try
          {
            var map = MapView.Active?.Map;
            if (map == null)
            {
              MessageBox.Show("An Active map view is required with at least one feature class");
              return;
            }
            ClearEventsMessage();
            ClearStatusMessage();
            // setup for a Geoprocessing command that is then used
            // to test GP events and history
            var featureLayer = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault();
            var args = Geoprocessing.MakeValueArray(featureLayer);
            var env = Geoprocessing.MakeEnvironmentArray(overwriteoutput: true);
            GPExecuteToolFlags executeFlags = GPExecuteToolFlags.AddToHistory;
            // add gp event listener
            eventToken =
              ArcGIS.Desktop.Core.Events.GPExecuteToolEvent.Subscribe(e =>
            {
              string id = e.ID;           // Same as history ID
              if (e.IsStarting == false)  // Execute completed
                _ = e.GPResult.ReturnValue;
              AddEventsMessage($@"Started: {!e.IsStarting} result: {e.GPResult.ReturnValue}");
            });

            var t = await Geoprocessing.ExecuteToolAsync("management.GetCount", args, env, null, null, executeFlags);
            IEnumerable<IGPHistoryItem> GpHistoryItems = Project.Current.GetProjectItemContainer(Geoprocessing.HistoryContainerKey) as IEnumerable<IGPHistoryItem>;
            // order by time stamp descending
            string hitemID = "";
            string hitemToolPath = "";
            IGPResult hitemGPResult = null;
            DateTime hitemTimeStamp;
            foreach (var hitem in GpHistoryItems.OrderByDescending<IGPHistoryItem, DateTime>(gphistory => gphistory.TimeStamp))
            {
              // common IGPHistoryItem and Item properties
              hitemID = (hitem as Item).ID;
              hitemToolPath = hitem.ToolPath;
              var shortToolName = hitemToolPath;
              if (shortToolName.Contains('.'))
              {
                shortToolName = shortToolName.Substring(shortToolName.LastIndexOf('.'));
              }
              hitemGPResult = hitem.GPResult;
              hitemTimeStamp = hitem.TimeStamp;
              var okFailed = (hitemGPResult.IsFailed && hitemGPResult.IsCanceled) ? "failed" : "ok";
              AddStatusMessage($@"{shortToolName} {hitemTimeStamp} {okFailed}");
            }
          }
          catch (Exception ex)
          {
            MessageBox.Show(ex.Message);
          }
          finally
          {
            if (eventToken != null)
              ArcGIS.Desktop.Core.Events.GPExecuteToolEvent.Unsubscribe(eventToken);
          }
        });
      }
    }

    #endregion Button Commands

    #region Button Images loaded from Pro

    public System.Windows.Media.ImageSource ImageCmdRunTest
    {
      get { 
        return System.Windows.Application.Current.Resources["DataReviewerReviewerRunRules16"] as System.Windows.Media.ImageSource;
      }
    }

    #endregion

    #region Bound Properties

    private string _CommandLine;
    public string CommandLine
    {
      get => _CommandLine;
      set => SetProperty(ref _CommandLine, value);
    }

    private string _status;
    public string Status
    {
      get => _status;
      set => SetProperty(ref _status, value);
    }

    private string _events;
    public string Events
    {
      get => _events;
      set => SetProperty(ref _events, value);
    }

    private string _InLayer;
    public string InLayer
    {
      get => _InLayer;
      set => SetProperty(ref _InLayer, value);
    }

    private string _OutLayer;
    public string OutLayer
    {
      get => _OutLayer;
      set => SetProperty(ref _OutLayer, value);
    }
    #endregion

    #region Public Helpers

    public static void AddStatusMessage(string message)
    {
      if (Module1.GPHistoryAPITestVM == null) return;
      RunOnUIThread(() =>
      {
        Module1.GPHistoryAPITestVM.Status =
          $@"{Module1.GPHistoryAPITestVM.Status}{Environment.NewLine}{message}";
      });
    }

    public static void ClearStatusMessage()
    {
      if (Module1.GPHistoryAPITestVM == null) return;
      RunOnUIThread(() =>
      {
        Module1.GPHistoryAPITestVM.Status = string.Empty;
      });
    }

    public static void AddEventsMessage(string message)
    {
      if (Module1.GPHistoryAPITestVM == null) return;
      RunOnUIThread(() =>
      {
        Module1.GPHistoryAPITestVM.Events =
          $@"{Module1.GPHistoryAPITestVM.Events}{Environment.NewLine}{message}";
      });
    }

    public static void ClearEventsMessage()
    {
      if (Module1.GPHistoryAPITestVM == null) return;
      RunOnUIThread(() =>
      {
        Module1.GPHistoryAPITestVM.Events = string.Empty;
      });
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Utility function to enable an action to run on the UI thread (if not already)
    /// </summary>
    /// <param name="action">the action to execute</param>
    /// <returns></returns>
    internal static Task RunOnUIThread(Action action)
    {
      if (OnUIThread)
      {
        action();
        return Task.FromResult(0);
      }
      else
        return Task.Factory.StartNew(action, System.Threading.CancellationToken.None, TaskCreationOptions.None, QueuedTask.UIScheduler);
    }

    /// <summary>
    /// Determines if the application is currently on the UI thread
    /// </summary>
    private static bool OnUIThread
    {
      get
      {
        if (FrameworkApplication.TestMode)
          return QueuedTask.OnWorker;
        else
          return System.Windows.Application.Current.Dispatcher.CheckAccess();
      }
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
    private string _heading = "My DockPane";
    public string Heading
    {
      get => _heading;
      set => SetProperty(ref _heading, value);
    }
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class GPHistoryAPITest_ShowButton : Button
  {
    protected override void OnClick()
    {
      GPHistoryAPITestViewModel.Show();
    }
  }
}
