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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace GeoProcesssingEventsWithUI
{
  internal class GPFeedbackViewModel : DockPane
  {
    private const string _dockPaneID = "GeoProcesssingEventsWithUI_GPFeedback";

    protected GPFeedbackViewModel() 
    {
      Module1.GPFeedbackViewModel = this;
    }

    /// <summary>
    /// Show the DockPane.
    /// </summary>
    internal static void Show()
    {
      DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
      if (pane == null) return;
      pane.Activate();
    }

    #region Public accessible methods

    public static void ProgressBarValue (double newProgressValue)
    {
      if (Module1.GPFeedbackViewModel == null) return;
      RunOnUIThread(() =>
      {
        Module1.GPFeedbackViewModel.ProgressValue = newProgressValue;
      });
    }

    public static void AddGPStatusMessage(string message)
    {
      if (Module1.GPFeedbackViewModel == null) return;
      RunOnUIThread(() =>
      {
        Module1.GPFeedbackViewModel.GPStatus =
          $@"{Module1.GPFeedbackViewModel.GPStatus}{Environment.NewLine}{message}";
      });
    }

    public static void ClearGPStatusMessage()
    {
      if (Module1.GPFeedbackViewModel == null) return;
      RunOnUIThread(() =>
      {
        Module1.GPFeedbackViewModel.GPStatus = string.Empty;
      });
    }

    #endregion Public accessible methods

    #region Public properties

    private string _GPStatus;
    private double _progressValue;
    private System.Windows.Visibility _progressVisibility = System.Windows.Visibility.Collapsed;
    private string _progressTest;

    public string GPStatus
    {
      get => _GPStatus;
      set => SetProperty(ref _GPStatus, value);
    }

    public double ProgressValue
    {
      get => _progressValue;
      set
      {
        SetProperty(ref _progressValue, value);
        ProgressVisibility = _progressValue > 0 ? System.Windows.Visibility.Visible
            : System.Windows.Visibility.Collapsed;
        ProgressText = _progressValue > 0 ? $"{_progressValue} %" : string.Empty;
      }
    }

    public double ProgressValueMax
    {
      get => 100;
    }

    public System.Windows.Visibility ProgressVisibility
    {
      get => _progressVisibility;
      set => SetProperty(ref _progressVisibility, value);
    }

    public string ProgressText
    {
      get => _progressTest;
      set => SetProperty(ref _progressTest, value);
    }

    #endregion Public properties

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
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class GPFeedback_ShowButton : Button
  {
    protected override void OnClick()
    {
      GPFeedbackViewModel.Show();
    }
  }
}
