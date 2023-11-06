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
using ActiproSoftware.Products.Logging;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Events;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Catalog.DistributedGeodatabase.ManageReplicas;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Linq;

namespace ProcessProjectFiles
{
  internal enum ProcessState
  {
    NextAprx,
    OpenProject,
    OpenedProject,
    CheckMapView,
    AddMapView,
    AddedMapView,
    SaveProject,
    Done,
    Idle
  }

  internal class ProcessProjectFilesViewModel : DockPane
  {
    private const string _dockPaneID = "ProcessProjectFiles_OpenProjectDockpane";
    private Queue<string> _aprxQueue = new();
    private DispatcherTimer _timer = new ();
    private ProcessState _state = ProcessState.Idle;
    private Project _projectLoaded = null;
    private MapView _mapViewLoaded = null;

    protected ProcessProjectFilesViewModel()
    {
      Module1.ProcessProjectFilesViewModel = this;
    }

    #region Properties

    public ICommand CmdBrowseFolder
    {
      get
      {
        return new RelayCommand(() =>
        {
          BrowseProjectFilter bf = new("esri_browseDialogFilters_folders");
          //Display the filter in Open Item dialog
          OpenItemDialog projectBasePath = new()
          {
            Title = "Select base folder to find .aprx file",
            InitialLocation = @"C:\Data",
            MultiSelect = false,
            BrowseFilter = bf
          };
          bool? ok = projectBasePath.ShowDialog();
          if (ok.Value == true)
          {
            BrowseFolder = projectBasePath.Items[0].Path;
          }
        });
      }
    }

    public ImageSource CmdBrowseFolderImg
    {
      get
      {
        var imageSource = System.Windows.Application.Current.Resources["FolderOpenState24"] as ImageSource;
        return imageSource;
      }
    }

    private string _BrowseFolder = @"c:\data";

    public string BrowseFolder
    {
      get => _BrowseFolder;
      set => SetProperty(ref _BrowseFolder, value);
    }

    public ICommand CmdStartProcessing
    {
      get
      {
        return new RelayCommand(() =>
        {
          try
          {            
            ProcessProjectFilesViewModel.ClearStatusMessage();
            ProcessProjectFilesViewModel.AddStatusMessage($@"Processing all .aprx files under this root folder: {BrowseFolder}");
            var aprxFiles = Directory.GetFiles(BrowseFolder, "*.aprx", SearchOption.AllDirectories);
            if (aprxFiles.Length < 1)
            {
              ProcessProjectFilesViewModel.ClearStatusMessage();
            }
            else
            {
              // queue all aprx files for background processing
              foreach (var aprxFile in aprxFiles)
              {
                _aprxQueue.Enqueue(aprxFile);
              }
              ProcessProjectFilesViewModel.AddStatusMessage($@"Starting processing of {_aprxQueue.Count} project files");
              // use UI time to process the queue
              _timer.Interval = TimeSpan.FromSeconds(1);
              _timer.Tick += TimerTick;
              _timer.Start();
              _state = ProcessState.NextAprx;
              _changedCount = 0;
            }
          }
          catch (Exception ex)
          {
            ProcessProjectFilesViewModel.AddStatusMessage(ex.Message);
          }
        });
      }
    }

    private void OnProjectOpened(ProjectEventArgs projEventArgs)
    {
      _projectLoaded = projEventArgs.Project;
      EventLog.WriteDebug($@" Event Proj Opened: {_projectLoaded.Name}...");
    }

    public ImageSource CmdStartProcessingImg
    {
      get
      {
        var imageSource = System.Windows.Application.Current.Resources["StartJob32"] as ImageSource;
        return imageSource;
      }
    }
    
    private bool _CmdStartProcessingEnabled = true;
    public bool CmdStartProcessingEnabled
    {
      get => _CmdStartProcessingEnabled;
      set => SetProperty(ref _CmdStartProcessingEnabled, value);
    }

    private string _status;
    public string Status
    {
      get => _status;
      set => SetProperty(ref _status, value);
    }

    #endregion

    #region Public Helpers

    public static void AddStatusMessage(string message)
    {
      if (Module1.ProcessProjectFilesViewModel == null) return;
      RunOnUiThread(() =>
      {
        Module1.ProcessProjectFilesViewModel.Status =
          $@"{Module1.ProcessProjectFilesViewModel.Status}{Environment.NewLine}{message}";
      });
    }

    public static void ClearStatusMessage()
    {
      if (Module1.ProcessProjectFilesViewModel == null) return;
      RunOnUiThread(() =>
      {
        Module1.ProcessProjectFilesViewModel.Status = string.Empty;
      });
    }

    #endregion

    #region Helpers for Project processing

    private int _myTimeoutTicks = 0;
    private string _aprxFile = string.Empty;
    private bool _active = false;
    private int _changedCount = 0;

    private async void TimerTick(object sender, EventArgs e)
    {
      if (_active) return;
      try
      {
        _active = true;
        switch (_state)
        {
          case ProcessState.NextAprx:
            // check the queue for an item
            if (_aprxQueue.Count == 0)
            {
              // done
              _state = ProcessState.Done;
              break;
            }
            _aprxFile = _aprxQueue.Dequeue();
            _state = ProcessState.OpenProject;
            CmdStartProcessingEnabled = false;
            EventLog.WriteDebug($@"Processing now: {_aprxFile}");
            break;
          case ProcessState.OpenProject:
            ProcessProjectFilesViewModel.AddStatusMessage($@"Processing: {_aprxFile}...");
            // open the project ... should close the current project
            if (!Project.CanOpen(_aprxFile, out string docVersion))
            {
              ProcessProjectFilesViewModel.AddStatusMessage($@" Error: cannot open: {_aprxFile}");
              EventLog.WriteDebug($@" Error: cannot open: {_aprxFile}");
              throw new Exception($@"Unable to open: {_aprxFile}");
            }
            ProcessProjectFilesViewModel.AddStatusMessage($@" Opening: {Path.GetFileNameWithoutExtension(_aprxFile)}...");
            EventLog.WriteDebug($@" Opening: {Path.GetFileNameWithoutExtension(_aprxFile)}...");
            _projectLoaded = null;
            ProjectOpenedEvent.Subscribe(OnProjectOpened);
            await Project.OpenAsync(_aprxFile);
            EventLog.WriteDebug($@" Opened: {Path.GetFileNameWithoutExtension(_aprxFile)}...");
            _state = ProcessState.OpenedProject;
            break;
          case ProcessState.OpenedProject:
            if (Project.Current.Path == _aprxFile)
            {
              _projectLoaded = Project.Current;
              EventLog.WriteDebug($@" Project Path verified: {_projectLoaded.Name}...");
            }
            if (_projectLoaded != null)
            {
              ProcessProjectFilesViewModel.AddStatusMessage($@" Opened: {_projectLoaded.Name}...");
              EventLog.WriteDebug($@" Opened: {_projectLoaded.Name}...");
              ProjectOpenedEvent.Unsubscribe(OnProjectOpened);
              // wait for 30 seconds to see if something happens
              _myTimeoutTicks = 30;
              _state = ProcessState.CheckMapView;
              _mapViewLoaded = null;
              ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);
            }
            break;
          case ProcessState.CheckMapView:
            IEnumerable<IMapPane> mapPanes = FrameworkApplication.Panes.OfType<IMapPane>();
            if (_mapViewLoaded != null || mapPanes.Any())
            {
              ActiveMapViewChangedEvent.Unsubscribe(OnActiveMapViewChanged);
              ProcessProjectFilesViewModel.AddStatusMessage($@" ...Project already has open MapPanes");
              EventLog.WriteDebug($@" ...Project already has open MapPanes");
              _projectLoaded.SetDirty(false);
              _mapViewLoaded = null;
              // nothing to do go to next aprx
              _state = ProcessState.NextAprx;
              break;
            }
            if (_myTimeoutTicks-- <= 0)
            {
              // waited for 30 seconds
              // nothing happend hence if have to add a MapView
              ActiveMapViewChangedEvent.Unsubscribe(OnActiveMapViewChanged);
              ProcessProjectFilesViewModel.AddStatusMessage($@" No MapPanes opened, timeout expired...");
              EventLog.WriteDebug($@" No MapPanes opened, timeout expired...");
              _mapViewLoaded = null;
              _state = ProcessState.AddMapView;
            }
            break;
          case ProcessState.AddMapView:
            ProcessProjectFilesViewModel.AddStatusMessage($@" Create MapView...");
            EventLog.WriteDebug($@" Create MapView...");
            await QueuedTask.Run(() =>
            {
              // Finding the first map project item 
              MapProjectItem mpi = _projectLoaded.GetItems<MapProjectItem>().FirstOrDefault();
              if (mpi != null)
              {
                var map = mpi.GetMap();
                _mapViewLoaded = null;
                ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);
                // Opening the map in a mapview
                _ = ProApp.Panes.CreateMapPaneAsync(map);
                EventLog.WriteDebug($@" Started Create MapView...");
                _state = ProcessState.AddedMapView;
              }
              else
              {
                ProcessProjectFilesViewModel.AddStatusMessage($@" ...Project has no Map project items");
                EventLog.WriteDebug($@" ...Project has no Map project items");
                _projectLoaded.SetDirty(false);
                // nothing to do go to next aprx
                _state = ProcessState.NextAprx;
              }
            });
            break;
          case ProcessState.AddedMapView:
            if (_mapViewLoaded != null)
            {
              ActiveMapViewChangedEvent.Unsubscribe(OnActiveMapViewChanged);
              _mapViewLoaded = null;
              ProcessProjectFilesViewModel.AddStatusMessage($@" MapPane added...");
              EventLog.WriteDebug($@" MapPane added...");
              // nothing to do go to next aprx
              _state = ProcessState.SaveProject;
              break;
            }
            break;
          case ProcessState.SaveProject:
            ProcessProjectFilesViewModel.AddStatusMessage($@" Save changes...");
            EventLog.WriteDebug($@" Save changes...");
            await Project.Current.SaveAsync();
            Project.Current.SetDirty(false);
            _changedCount++;
            ProcessProjectFilesViewModel.AddStatusMessage($@" ...Project Saved");
            EventLog.WriteDebug($@" ...Project Saved");
            _state = ProcessState.NextAprx;
            break;
          case ProcessState.Done:
            EventLog.WriteInfo($@"*** Processing .aprx files complete for: {BrowseFolder}");
            EventLog.WriteInfo($@"*** {_changedCount} files were updated");
            ProcessProjectFilesViewModel.AddStatusMessage($@"*** Processing .aprx files complete for: {BrowseFolder}");
            ProcessProjectFilesViewModel.AddStatusMessage($@"*** {_changedCount} files were updated");
            _state = ProcessState.Idle;
            break;
          default: // idle state
            _aprxQueue.Clear();
            _timer.Stop();
            CmdStartProcessingEnabled = true;
            break;
        }
      }
      catch (Exception ex)
      {
        EventLog.WriteError(ex.Message);
        ProcessProjectFilesViewModel.AddStatusMessage(ex.Message);
        ProcessProjectFilesViewModel.AddStatusMessage("Stop Processing");
        _state = ProcessState.Idle;
      }
      finally
      {
        _active = false;
      }
    }
    private void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs mvChangedEventArgs)
    {
      if (mvChangedEventArgs.IncomingView != null)
      {
        _mapViewLoaded = mvChangedEventArgs.IncomingView;
        EventLog.WriteDebug($@" Event Mapview changed, incoming: {_mapViewLoaded.Map.Name}...");
      }
    }

    #endregion

    #region Threading Helpers

    /// <summary>
    /// Utility function to enable an action to run on the UI thread (if not already)
    /// </summary>
    /// <param name="action">the action to execute</param>
    /// <returns></returns>
    /// <summary>
    /// utility function to enable an action to run on the UI thread (if not already)
    /// </summary>
    /// <param name="action">the action to execute</param>
    /// <returns></returns>
    internal static void RunOnUiThread(Action action)
    {
      try
      {
        if (OnUIThread)
          action();
        else
          System.Windows.Application.Current.Dispatcher.BeginInvoke(action);
      }
      catch (Exception ex)
      {
        MessageBox.Show($@"Error in OpenAndActivateMap: {ex.Message}");
      }
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
	internal class OpenProcessProjectFiles_ShowButton : Button
  {
    protected override void OnClick()
    {
      ProcessProjectFilesViewModel.Show();
    }
  }
}
