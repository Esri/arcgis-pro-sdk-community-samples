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
using ArcGIS.Desktop.Internal.Framework.Controls;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace CallScriptFromNet
{
  internal class RunPythonWithFeedbackViewModel : DockPane
  {
    private const string _dockPaneID = "CallScriptFromNet_RunPythonWithFeedback";

    protected RunPythonWithFeedbackViewModel() {
      RunPyScriptModule.RunPythonWithFeedbackViewModel = this;
    }

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
      if (RunPyScriptModule.RunPythonWithFeedbackViewModel == null) return;
      RunOnUIThread(() =>
      {
        RunPyScriptModule.RunPythonWithFeedbackViewModel.Status =
          $@"{RunPyScriptModule.RunPythonWithFeedbackViewModel.Status}{Environment.NewLine}{message}";
      });
    }

    public static void ClearStatusMessage()
    {
      if (RunPyScriptModule.RunPythonWithFeedbackViewModel == null) return;
      RunOnUIThread(() =>
      {
        RunPyScriptModule.RunPythonWithFeedbackViewModel.Status = string.Empty;
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
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class RunPythonWithFeedback_ShowButton : Button
  {
    protected override void OnClick()
    {
      RunPythonWithFeedbackViewModel.Show();
    }
  }
}
