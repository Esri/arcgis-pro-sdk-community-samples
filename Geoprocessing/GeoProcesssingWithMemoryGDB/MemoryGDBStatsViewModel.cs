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

namespace GeoProcesssingWithMemoryGDB
{
  internal class MemoryGDBStatsViewModel : DockPane
  {
    private const string _dockPaneID = "GeoProcesssingWithMemoryGDB_MemoryGDBStats";

    private ObservableCollection<string> _featureClasses = new ObservableCollection<string>();
    private object _lock = new();

    protected MemoryGDBStatsViewModel() 
    {
      Module1.MemoryGDBStatsViewModel = this;
      BindingOperations.EnableCollectionSynchronization(_featureClasses, _lock);
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

    private string _MemoryStatus = "N/A";
    public string MemoryStatus
    {
      get => _MemoryStatus;
      set => SetProperty(ref _MemoryStatus, value);
    }

    private string _MemoryPerformance = "N/A";
    public string MemoryPerformance
    {
      get => _MemoryPerformance;
      set => SetProperty(ref _MemoryPerformance, value);
    }

    private string _MemoryCount = "N/A";
    public string MemoryCount
    {
      get => _MemoryCount;
      set => SetProperty(ref _MemoryCount, value);
    }

    public ICommand CmdRefreshMemoryGDB
    {
      get
      {
        return new RelayCommand(() =>
        {
          var memoryCPs = new MemoryConnectionProperties("memory");
          FeatureClasses.Clear();
          QueuedTask.Run(() =>
          {
            using var geoDb = new Geodatabase(memoryCPs);
            IReadOnlyList<Definition> fcList = geoDb.GetDefinitions<FeatureClassDefinition>();
            // Feature class
            foreach (var fcDef in fcList)
            {
              _featureClasses.Add(NameOrAlias(fcDef as FeatureClassDefinition));
            }
          });
        }, true);
      }
    }

    public System.Windows.Media.ImageSource ImgRefreshMemoryGDB
    {
      get { return System.Windows.Application.Current.Resources["GenericRefresh16"] as System.Windows.Media.ImageSource; }
    }

    public ObservableCollection<string> FeatureClasses
    {
      get
      {
        return _featureClasses;
      }
    }

    public static void AddGPStatusMessage(string message)
    {
      if (Module1.MemoryGDBStatsViewModel == null) return;
      RunOnUIThread(() =>
      {
        Module1.MemoryGDBStatsViewModel.GPStatus =
          $@"{Module1.MemoryGDBStatsViewModel.GPStatus}{Environment.NewLine}{message}";
      });
    }

    public static void ClearGPStatusMessage()
    {
      if (Module1.MemoryGDBStatsViewModel == null) return;
      RunOnUIThread(() =>
      {
        Module1.MemoryGDBStatsViewModel.GPStatus = string.Empty;
      });
    }

    public static void AddStatusMessage (string message)
    {
      if (Module1.MemoryGDBStatsViewModel == null) return;
      RunOnUIThread(() =>
      {
        Module1.MemoryGDBStatsViewModel.Status =
          $@"{Module1.MemoryGDBStatsViewModel.Status}{Environment.NewLine}{message}";
      });
    }

    public static void ClearStatusMessage ()
    {
      if (Module1.MemoryGDBStatsViewModel == null) return;
      RunOnUIThread(() =>
      {
        Module1.MemoryGDBStatsViewModel.Status = string.Empty;
      });
    }
    
    private string _GPStatus;
    public string GPStatus
    {
      get => _GPStatus;
      set => SetProperty(ref _GPStatus, value);
    }

    private string _status;
    public string Status
    {
      get => _status;
      set => SetProperty(ref _status, value);
    }

    /// <summary>
    /// Text shown near the top of the DockPane.
    /// </summary>
    private string _heading = "Memory GDB info";
    public string Heading
    {
      get => _heading;
      set => SetProperty(ref _heading, value);
    }

    #region Helpers

    private string NameOrAlias(FeatureClassDefinition fc)
    {
      var alias = fc.GetAliasName();
      return string.Format("{0} ({1})", !string.IsNullOrEmpty(alias) ? alias : fc.GetName(), fc.GetName());
    }

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
	internal class MemoryGDBStats_ShowButton : Button
  {
    protected override void OnClick()
    {
      MemoryGDBStatsViewModel.Show();
    }
  }
}
