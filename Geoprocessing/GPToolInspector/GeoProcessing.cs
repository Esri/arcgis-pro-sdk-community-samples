/*

   Copyright 2025 Esri

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
using ArcGIS.Core.Events;
using ArcGIS.Desktop.Core.Events;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Events;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.GeoProcessing;
using ArcGIS.Desktop.Internal.Core.Events;
using ArcGIS.Desktop.Internal.Core;
using ArcGIS.Desktop.Internal.Framework.Controls;
using ArcGIS.Desktop.Internal.GeoProcessing;
using ArcGIS.Desktop.Internal.Mapping.Locate;
using ArcGIS.Desktop.Internal.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GPToolInspector
{
  using Properties = ArcGIS.Desktop.Internal.GeoProcessing.Properties;
  internal partial class Commands { } //forward declaration

  /// <summary>
  /// Geoprocessing module
  /// </summary>
  public sealed class GeoprocessingModule : ArcGIS.Desktop.Framework.Contracts.Module, IGeoprocessing2, IDisposable, IGPToolDialogHelper, IGPSettingsInternal
  {
    static GeoprocessingModule _module;
    static GPServiceSync _sync;

    #region Module Public API
    /// <summary>
    /// singleton instance
    /// </summary>
    static public GeoprocessingModule Current
    {
      get
      {
        if (_module == null)
          _module = FrameworkApplication.FindModule("esri_geoprocessing_module") as GeoprocessingModule;
        System.Diagnostics.Debug.Assert(_module != null);
        return _module._disposed ? null : _module;
      }
    }
    #endregion


    #region IGeoprocessing implementation
    static string relative2project_tool(string toolPath)
    {
      try
      {
        if (!System.IO.Path.IsPathFullyQualified(toolPath))
        {
          var workspace_dir = Project.Current.HomeFolderPath;
          if (toolPath.StartsWith(System.IO.Path.GetFileName(Project.Current.HomeFolderPath), StringComparison.InvariantCultureIgnoreCase))
            workspace_dir = System.IO.Path.GetDirectoryName(workspace_dir);
          var tbx_path = System.IO.Path.GetFullPath(System.IO.Path.Combine(workspace_dir, System.IO.Path.GetDirectoryName(toolPath)));
          if (System.IO.File.Exists(tbx_path))
            toolPath = System.IO.Path.Combine(tbx_path, System.IO.Path.GetFileName(toolPath));
        }
      }
      catch { }
      return toolPath;
    }
    Task<IGPResult> IGeoprocessing.ExecuteTool(string tool, IEnumerable<string> args, IEnumerable<KeyValuePair<string, string>> env, CancellationToken? ct, GPToolExecuteEventHandler callback, GPExecuteToolFlags flags)
    {
      var tool_path = SysToolsUtil.shortcat2path(tool);
      ArcGIS.Desktop.Internal.Framework.UserExperienceImprovementLog.Log(
        Internal.Framework.UserExperienceEventType.Custom,
        "ESRI_GEOPROCESSING_EXECUTETOOL_SDK",
        $"{tool} FLAG={flags}");

      if (!System.IO.Path.IsPathFullyQualified(tool_path))
        tool_path = relative2project_tool(tool_path);
      return execute_helper.eval(tool_path, args, env, ct.HasValue ? ct.Value : CancellationToken.None, callback, (execute_helper.ExecuteFlags)flags);
    }
    Task<IGPResult> IGeoprocessing.ExecuteTool(string tool, IEnumerable<string> args, IEnumerable<KeyValuePair<string, string>> env, CancelableProgressor progressor, GPExecuteToolFlags flags)
    {
      var tool_path = SysToolsUtil.shortcat2path(tool);
      ArcGIS.Desktop.Internal.Framework.UserExperienceImprovementLog.Log(
        Internal.Framework.UserExperienceEventType.Custom,
        "ESRI_GEOPROCESSING_EXECUTETOOL_SDK",
        $"{tool} FLAG={flags}");

      if (!System.IO.Path.IsPathFullyQualified(tool_path))
        tool_path = relative2project_tool(tool_path);
      return execute_helper.eval_modal(tool_path, args, env, progressor, flags);
    }

    static Task<ToolInfo> query_toolinfo(string toolPath)
    {
      if (string.IsNullOrEmpty(toolPath))
        return Task.FromResult<ToolInfo>(null);

      return QueuedTask.Run<ToolInfo>(() =>
      {
        var ti = SearchTools.getToolInfo(toolPath);
        if (ti == null && SysToolsUtil.shortcat2path(toolPath) != toolPath)
        {
          var parts = toolPath.Split('_');
          if (parts.Length == 2)
            ti = SearchTools.getToolInfo($"{parts[1]}.{parts[0]}");
        }
        return ti;
      });
    }
    void IGeoprocessing.OpenToolDialog(string toolPath, IEnumerable<string> values, IEnumerable<KeyValuePair<string, string>> environments, bool newSubPane, GPToolExecuteEventHandler callback)
    {
      var pane = FrameworkApplication.DockPaneManager.Find("esri_geoprocessing_toolBoxes") as GPDocPaneViewModel;
      if (pane == null || string.IsNullOrEmpty(toolPath))
        return;
      ArcGIS.Desktop.Internal.Framework.UserExperienceImprovementLog.Log(
          Internal.Framework.UserExperienceEventType.Custom,
          "ESRI_GEOPROCESSING_TOOLDIALOG_OPEN_SDK"
      //$"{toolPath}"
          );
      openToolDialogLocal(toolPath, null, values?.ToArray(), environments?.ToArray(), newSubPane, callback);
    }

    async internal void openToolDialogLocal(string toolPath, string title, string[] param_list = null, KeyValuePair<string, string>[] env = null, bool bNewSubPanel = false, GPToolExecuteEventHandler callback = null, bool bShowEnviroments = true, GPExecuteToolFlags executeFlags = GPExecuteToolFlags.Default | GPExecuteToolFlags.InheritGPOptions | GPExecuteToolFlags.GPThread)
    {
      var ti = await query_toolinfo(string.IsNullOrEmpty(title) ? toolPath : null);
      if (ti == null)
        toolPath = relative2project_tool(toolPath);
      GPDocPaneViewModel.Progress?.clear();
      if (System.Windows.Application.Current.Dispatcher.CheckAccess())
        (FrameworkApplication.DockPaneManager.Find("esri_geoprocessing_toolBoxes") as GPDocPaneViewModel)?.ShowToolPage(bNewSubPanel, ti?.getExecutePath() ?? toolPath, ti?.Name ?? title, param_list, env, callback, bShowEnviroments);
      else
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        (FrameworkApplication.DockPaneManager.Find("esri_geoprocessing_toolBoxes") as GPDocPaneViewModel)?.ShowToolPage(bNewSubPanel, ti?.getExecutePath() ?? toolPath, ti?.Name ?? title, param_list, env, callback, bShowEnviroments));
    }

    async internal void openToolDialogLocal(ToolInfo toolInfo, string[] param_list = null, KeyValuePair<string, string>[] env = null, bool bNewSubPanel = false, GPToolExecuteEventHandler callback = null)
    {
      if (toolInfo?.IsValid == true && toolInfo?.executable == true)
      {
        string data_field = "";
        if (toolInfo is FavoritesToolInfoViewModel item)
        {
          data_field += $" KIND={item.Kind.ToString()}";
          if (!string.IsNullOrEmpty(item.payload))
            data_field += $" PAYLOAD='{item.payload}'";
        }
        ArcGIS.Desktop.Internal.Framework.UserExperienceImprovementLog.Log(
          Internal.Framework.UserExperienceEventType.Custom,
          "ESRI_GEOPROCESSING_TOOLDIALOG_OPEN",
          $"{toolInfo.getExecutePathShort()}{data_field}"
          );

        if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.RightShift))
          await openToolDialogAsyncLocal(toolInfo.getExecutePath(), param_list, env, callback, GPExecuteToolFlags.Default | GPExecuteToolFlags.InheritGPOptions, GPToolDialogFlags.FloatingAddApplyButton | GPToolDialogFlags.ShowEnvironment, toolInfo.Name);
        else
          openToolDialogLocal(toolInfo.getExecutePath(), toolInfo.Name, param_list, env, bNewSubPanel, callback);
      }
      else
        openToolDialogLocal("", "unknown", param_list, env, bNewSubPanel, callback);
    }

    Task<IGPResult> IGeoprocessing2.OpenToolDialogAsync(string toolPath, IEnumerable<string> values, IEnumerable<KeyValuePair<string, string>> environments, GPToolExecuteEventHandler callback, GPExecuteToolFlags executeFlags, GPToolDialogFlags dialogFlags, string dialogTitle)
    {
      return openToolDialogAsyncLocal(toolPath, values, environments, callback, executeFlags, dialogFlags, dialogTitle);
    }

    IReadOnlyList<string> IGeoprocessing2.query_recent_args(string toolname, int parameter_index, object parameter_value)
    {
      try
      {
        var history = GetHistoryContainer(false);
        if (history is null)
          return null;
        var items = history.Where(it => it.HistoryData.ToolInfo.getExecutePathShort() == toolname);
        if (parameter_value is null || parameter_index < 0)
          return items.LastOrDefault()?.HistoryData?.ParamList.Where(it => !it.isDerived).Select(it => it.value).ToList();//.GPResult.Parameters.Where(it => it.Item4).Select(it => it.Item3).ToList();

        var value_str = parameter_value.ToString();
        if (parameter_value is Mapping.Layer m)
        {
          Func<Mapping.Layer, string> fn_full_name = null;
          fn_full_name = (Mapping.Layer lyr) =>
          {
            if (fn_full_name is not null && lyr?.Parent is Mapping.Layer l2)
              return $"{fn_full_name(l2)}\\{lyr.Name}";
            return lyr?.Name;
          };
          try
          {
            value_str = fn_full_name(m);
          }
          catch { }
        }

        var gpresult = items.LastOrDefault(it => it.HistoryData?.GPResult.Parameters.ElementAtOrDefault(parameter_index)?.Item3 == value_str)?.HistoryData?.ParamList;
        if (gpresult is null)
          return null;

        var ret = gpresult.Where(it => !it.isDerived).Select(it => it.value).ToList();
        //to support duplicate layers, replace the layer name with the CIM path
        if (parameter_value is Mapping.MapMember)
          ret[parameter_index] = Geoprocessing.MakeValueString(parameter_value);
        return ret;
      }
      catch { return null; }
    }

    private Task<IGPResult> openToolDialogAsyncLocal(string toolPath, IEnumerable<string> values, IEnumerable<KeyValuePair<string, string>> environments, GPToolExecuteEventHandler callback, GPExecuteToolFlags executeFlags, GPToolDialogFlags dialogFlags, string dialogTitle)
    {
      //var parent = ArcGIS.Desktop.Framework.FrameworkApplication.Current.MainWindow;
      return query_toolinfo(string.IsNullOrEmpty(dialogTitle) ? toolPath : null).ContinueWith(r =>
      {
        bool bShowEnviroments = (dialogFlags & GPToolDialogFlags.ShowEnvironment) == GPToolDialogFlags.ShowEnvironment;
        var ti = r.Result;
        var title = ti == null ? toolPath : ti.Name;
        var tool_path = ti == null ? toolPath : ti.getExecutePath();
        if ((dialogFlags & GPToolDialogFlags.GPPane) == GPToolDialogFlags.GPPane)
        {
          bool newSubPane = (dialogFlags & GPToolDialogFlags.GPPaneNewSubPane) == GPToolDialogFlags.GPPaneNewSubPane;
          //_external_notify
          TaskCompletionSource<IGPResult> resultPromise = new TaskCompletionSource<IGPResult>();
          bool is_running = false;
          GPToolExecuteEventHandler dlg_event = (n, o) =>
          {
            switch (n)
            {
              case "OnOpen":
                var tdvm = o as ToolDialogPageViewModel;
                if (tdvm != null)
                  tdvm.ExecuteFlags = (execute_helper.ExecuteFlags)executeFlags;
                break;
              case "OnClose":
                if (is_running == false)
                  resultPromise.TrySetResult(null);
                break;
              case "OnBeginExecute":
                is_running = true;
                break;
              case "OnEndExecute":
                resultPromise.TrySetResult(o as IGPResult);
                break;
            }
          };
          dlg_event += callback;
          openToolDialogLocal(tool_path, title, values?.ToArray(), environments?.ToArray(), newSubPane, dlg_event, bShowEnviroments);
          return resultPromise.Task;
        }
        else //floating dialog
        {
          var tdFloatVM = new ToolDialogFloatingViewModel(executeFlags, bShowEnviroments, (dialogFlags & GPToolDialogFlags.FloatingAddApplyButton) == GPToolDialogFlags.FloatingAddApplyButton);
          System.Windows.Application.Current.Dispatcher.Invoke(() =>
          {
            tdFloatVM.ShowDialog(toolPath, string.IsNullOrEmpty(dialogTitle) ? title : dialogTitle, values, environments, callback);
          });
          return tdFloatVM.ResultPromise.Task;
        }
      }).Unwrap();
    }

    void IGeoprocessing2.OpenNotebook(string path)
    {
      ArcGIS.Desktop.GeoProcessing.Commands.OnOpenNotebook(path);
    }

    void IGeoprocessing2.RenameNotebook(string oldPath, string newPath)
    {
      ArcGIS.Desktop.GeoProcessing.Commands.RenameNotebook(oldPath, newPath);
    }

    void IGeoprocessing2.SaveNotebook(string path)
    {
      ArcGIS.Desktop.GeoProcessing.Commands.SaveNotebook(path);
    }

    void IGeoprocessing.ShowMessageBox(IEnumerable<IGPMessage> messages, string content_header, GPMessageBoxStyle style, string window_title, string icon_source)
    {
      (this as IGeoprocessing2).ShowMessageBox(messages, content_header, style, window_title, icon_source, null);
    }
    void IGeoprocessing2.ShowMessageBox(IEnumerable<IGPMessage> messages, string content_header, GPMessageBoxStyle style, string window_title, string icon_source, Framework.Contracts.ViewModelBase parentViewModel)
    {
      if (!System.Windows.Application.Current.Dispatcher.CheckAccess())
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
          ArcGIS.Desktop.Internal.Framework.DialogManager.ShowDialog(GPMessageBoxViewModel.Create(messages, content_header, style, window_title, icon_source), parentViewModel));
      else
        ArcGIS.Desktop.Internal.Framework.DialogManager.ShowDialog(GPMessageBoxViewModel.Create(messages, content_header, style, window_title, icon_source), parentViewModel);
    }

    string IGeoprocessing.MakeValueString(object arg, out object oref)
    {
      return ObjectToValueConverter.ToString(arg, out oref);
    }
    #endregion


    #region IGPSettingsInternal

    bool IGPSettingsInternal.OverwriteExistingDatasets => GetOptions().OverWriteOutput;

    bool IGPSettingsInternal.RemoveOverwrittenLayers => GetOptions().RemoveOverwrittenLayers;

    bool IGPSettingsInternal.AddOutputDatasetsToOpenMap => GetOptions().AddOutputsToMap;

    bool IGPSettingsInternal.AddToTopOfMapContents => GetOptions().AddToTopOfMapContents;

    bool IGPSettingsInternal.DisplayDisabledParameters => GetOptions().DisplayDisabledParams;

    bool IGPSettingsInternal.EnableUndoOn => GetOptions().EnableUndo;

    bool IGPSettingsInternal.DisplayShortedDataPaths => GetOptions().ShortenedDataPath;

    bool IGPSettingsInternal.AnalyzeScriptsAndModels => GetOptions().AnalysisForPro;

    bool IGPSettingsInternal.AutoOpenMessagesWindow => GetOptions().AutoOpenDetails;

    bool IGPSettingsInternal.WriteGPOperationsToHistory => GetOptions().AddHistoryItem;

    bool IGPSettingsInternal.WriteGPOperationsToLog => GetOptions().LogOperations;

    bool IGPSettingsInternal.WriteGPOperationsToDataset => GetOptions().LogLineage;

    void IGPSettingsInternal.SetOption(string optionName, bool optionValue)
    {
      //must be on the QueuedTask
      //ArcGIS.Core.Internal.Interop.ThrowOnWrongThread();
      if (GetOptions().SaveOptionToProfile(optionName, optionValue))
        PushSettings(optionName);
    }

    #endregion

    #region Construction
    List<object[]> _subscribers;
    static readonly WeakReference<object> _bClosed = new WeakReference<object>(null); //not null if framework really closing
    static internal bool isAppClosing()
    {
      if (_bClosed.TryGetTarget(out _))
        return true;
      return false;
    }
    static internal WeakReference<GPDocPaneViewModel> _gp_pane = new WeakReference<GPDocPaneViewModel>(null);
    private GeoprocessingModule()
    {
      _module = this;
    }
    static Task OnApplicationClosing(System.ComponentModel.CancelEventArgs e)
    {
      _bClosed.SetTarget(e);
      return Task.CompletedTask;
    }
    static void OnGPToolExecuteEvent(GPToolExecuteEventArgs args)
    {
      if (!string.IsNullOrEmpty(args.ID) && args.GPResult is not null)
      {
        static void fn(object o)
        {
          if (o is GPExecuteToolEventArgs arg)
          {
            try
            {
              GPExecuteToolEvent.Publish(arg);
            }
            catch { }
          }
        }
        //generate public API event
        System.Threading.ThreadPool.QueueUserWorkItem(fn, args);
      }
    }

    private bool _isSingleItemSelected;
    private bool _isSelectedItemInvalid;
    private Item _selectedItem;
    private Item _catalogViewItem;

    private void ProcessSelection(ProjectWindowSelectedItemsChangedEventArgs eventArgs)
    {
      ProcessSelection(eventArgs.IProjectWindow);
    }

    private void ProcessSelection(Core.IProjectWindow projectWindow)
    {
      if (projectWindow == null)
        return;

      System.Threading.ThreadPool.QueueUserWorkItem(_ =>
      {
        if (_disposed)
          return;

        _isSingleItemSelected = false;
        _selectedItem = null;
        _catalogViewItem = null;
        try
        {
          if (projectWindow?.SelectionCount == 1)
          {
            _isSingleItemSelected = true;
            _selectedItem = projectWindow.SelectedItems?.FirstOrDefault();
            _isSelectedItemInvalid = _selectedItem?.IsInvalid == true;
          }
          // If we're in the ProjectPane, see if there's a current item (a parent)
          else if (projectWindow?.SelectionCount == 0 && projectWindow is Core.IProjectView pv)
            _catalogViewItem = pv.CurrentItem;
        }
        catch { }
      });
    }

    internal static bool IsSingleItemSelected() { return _module?._isSingleItemSelected != false; }
    internal static Item CatalogViewItem { get { return _module?._catalogViewItem; } }
    internal static Item SelectedItem { get { return _module?._selectedItem; } }
    internal static bool SelectedItemInvalid { get { return _module?._isSelectedItemInvalid != false; } }

    //async static private void OnPortalUpdate(PortalAvailableStateEventArgs arg)
    static void OnActivePortalChangeEvent(ActivePortalChangeEventArgs arg)
    {
      updatePortal(arg.SignOn);
    }
    static void OnAGOLSignOnEvent(AGOLSignOnEventArgs arg)
    {
      updatePortal(arg.SignOn);
    }

    async static private void updatePortal(ArcGIS.Desktop.Internal.Core.SignOn signOnModel)
    {
      if (_module?._disposed == true)
        return;
      ValidateToolDialogEvent.Publish();
      if (signOnModel == null)
        return;

      bool isOn;
      //signOnModel = (FrameworkApplication.FindModule("esri_core_module") as IInternalCoreModule).GetSignOn();

      isOn = await signOnModel.GetIsSignedInAsync("", false);

      if (isOn)
      {
        ToolboxUtil.reload_portal();
        PortalGalleryCtrl.reload_if_needed();

        if (ToolboxUtil.IsPortalEnabled())
          FrameworkApplication.State.Activate("esri_geoprocessing_ShowPortal");
        else
          FrameworkApplication.State.Deactivate("esri_geoprocessing_ShowPortal");
      }
      else
        FrameworkApplication.State.Deactivate("esri_geoprocessing_ShowPortal");

      if (_gp_pane.TryGetTarget(out GPDocPaneViewModel pane))
        pane.update_portal();
    }

    private void OnMapMemberPropertyChanged(MapMemberPropertiesChangedEventArgs obj)
    {
      if (obj == null || obj.EventHints == null || !obj.EventHints.Contains(MapMemberEventHint.Name))
        return;

      ValidateToolDialogEvent.Publish();
    }

    #endregion

    #region Overrides
    /// <exclude></exclude>
    protected override bool Initialize()
    {
      if (_subscribers == null)
      {
        _subscribers = new List<object[]>(){
          new object[]{ ProjectOpenedEvent.Subscribe(OnProjectOpened), new Action<SubscriptionToken>((o) => ProjectOpenedEvent.Unsubscribe(o))},
          new object[]{ ProjectClosingEvent.Subscribe(OnProjectClosing), new Action<SubscriptionToken>((o) => ProjectClosingEvent.Unsubscribe(o))},
          new object[]{ ProjectClosedEvent.Subscribe(OnProjectClosed), new Action<SubscriptionToken>((o) => ProjectClosedEvent.Unsubscribe(o))},
          new object[]{ ProjectSavingEvent.Subscribe(OnProjectSaving), new Action<SubscriptionToken>((o) => ProjectSavingEvent.Unsubscribe(o))},
          new object[]{ ActiveMapViewChangedEvent.Subscribe(OnMapChanged), new Action<SubscriptionToken>((o) => ActiveMapViewChangedEvent.Unsubscribe(o))},
          new object[]{ ActivePaneChangedEvent.Subscribe(OnActivePaneChanged), new Action<SubscriptionToken>((o) => ActivePaneChangedEvent.Unsubscribe(o))},
          new object[]{ ArcGISMapClosingEvents.Subscribe(OnMapClosing), new Action<SubscriptionToken>((o) => ArcGISMapClosingEvents.Unsubscribe(o))},
          new object[]{ MapMemberPropertiesChangedEvent.Subscribe(OnMapMemberPropertyChanged), new Action<SubscriptionToken>((o) => MapMemberPropertiesChangedEvent.Unsubscribe(o))},
          new object[]{ GPThreadBusyEvent.Subscribe((o)=> _gpthreadbusy = o.bBusy), new Action<SubscriptionToken>((o) => GPThreadBusyEvent.Unsubscribe(o))},
          new object[]{ ActivePortalChangeEvent.Subscribe(OnActivePortalChangeEvent), new Action<SubscriptionToken>((o) => ActivePortalChangeEvent.Unsubscribe(o))},
          new object[]{ AGOLSignOnEvent.Subscribe(OnAGOLSignOnEvent), new Action<SubscriptionToken>((o) => AGOLSignOnEvent.Unsubscribe(o))},
          new object[]{ ProjectWindowSelectedItemsChangedEvent.Subscribe(ProcessSelection), new Action<SubscriptionToken>((o) => ProjectWindowSelectedItemsChangedEvent.Unsubscribe(o))},
          new object[]{ ApplicationClosingEvent.Subscribe(OnApplicationClosing), new Action<SubscriptionToken>((o) => ApplicationClosingEvent.Unsubscribe(o)) },
          new object[]{ GPToolExecuteEvent.Subscribe(OnGPToolExecuteEvent), new Action<SubscriptionToken>((o) => GPToolExecuteEvent.Unsubscribe(o)) }
        };
      }

      var signOnModel = InternalCoreModule.GetSignOn();
      updatePortal(signOnModel);

      //if project already open 
      System.Diagnostics.Debug.WriteLine("GeoprocessingModule.Initialize()");
      if (Project.Current != null)
        OnProjectOpened(new ProjectEventArgs(Project.Current));

      var dataInteropInstalled = System.IO.Path.Combine(SysToolsUtil.InstallPath, "bin", "fme.dll");
      if (System.IO.File.Exists(dataInteropInstalled))
      {
        FrameworkApplication.State.Activate("esri_geoprocessing_DI_installed");
      }
      else
        FrameworkApplication.State.Deactivate("esri_geoprocessing_DI_installed");

      Internal.GeoProcessing.R.support.check_once();

      // Process selection manually one time since module is delay loaded
      ProcessSelection(Core.Project.GetActiveCatalogWindow());

      return base.Initialize();
    }

    /// <exclude></exclude>
    protected override void Uninitialize()
    {
      System.Diagnostics.Debug.WriteLine("GeoprocessingModule.Uninitialize()");
      base.Uninitialize();
      Dispose(false);
    }

    #endregion

    /// <exclude></exclude>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }
    bool _disposed;
    private void Dispose(bool disposing)
    {
      if (!_disposed)
      {
        _disposed = true;
        if (_subscribers != null)
        {
          foreach (var it in _subscribers)
            //(it[0] as Type).InvokeMember("Unsubscribe", BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, new object[] { it[1] });
            (it[1] as Action<SubscriptionToken>)?.Invoke(it[0] as SubscriptionToken);
          _subscribers.Clear();
          _subscribers = null;
        }
        //remove leftovers
        _web_controls.ForEach(it =>
        {
          if (it.TryGetTarget(out IDisposable obj))
            try { obj.Dispose(); } catch { }
        });
        _web_controls = null;
        _GPOptions = null;
        _MBOptions = null;
        if (_sync != null)
          _sync.disconnect();
        _sync = null;
        _srv = null;
        //_module = null;
      }
    }

    static Commands _DAML_commands;
    internal static Commands Commands
    {
      get
      {
        if (_DAML_commands == null)
          _DAML_commands = new Commands();
        return _DAML_commands;
      }
    }

    static Cad_Commands _DAML_cadcommands;
    internal static Cad_Commands CadCommands
    {
      get
      {
        if (_DAML_cadcommands == null)
          _DAML_cadcommands = new Cad_Commands();
        return _DAML_cadcommands;
      }
    }

    static Bim_Commands _DAML_bimcommands;
    internal static Bim_Commands BimCommands => _DAML_bimcommands ?? (_DAML_bimcommands = new Bim_Commands());

    #region UtilityMethods CAD
    internal static string GetFullQualifiedLayerPath(Mapping.Layer layer)
    {
      if (layer == null)
        return string.Empty;

      string layerPath = layer.Name;
      var currentLayerContainer = layer.Parent;

      while (currentLayerContainer != null)
      {
        Mapping.GroupLayer currentContainer = currentLayerContainer as Mapping.GroupLayer;
        Mapping.CompositeLayer currentComposite = currentLayerContainer as Mapping.CompositeLayer;
        if (currentContainer != null)
        {
          layerPath = currentContainer.Name + "\\" + layerPath;
          currentLayerContainer = currentContainer.Parent;
        }
        else if (currentComposite != null)
        {
          layerPath = currentComposite.Name + "\\" + layerPath;
          currentLayerContainer = currentComposite.Parent;
        }
        else
        {
          break;
        }
      }
      return layerPath;
    }

    internal static string GetTOCSelectedFeatureLayerURI()
    {
      var selectedLayer = MappingInternal.ActiveTOC.MostRecentlySelectedLayer;
      if (selectedLayer is Mapping.FeatureLayer featureLayer)
        return featureLayer.URI;

      if (selectedLayer is BuildingLayer || selectedLayer is BuildingDisciplineLayer)
        return GetFirstFeatureLayerInBuilding(selectedLayer as Mapping.CompositeLayer)?.URI ?? string.Empty;

      return string.Empty;
    }

    internal static string GetSelectedBuildingLayerPath()
    {
      var selectedLayer = MappingInternal.ActiveTOC.MostRecentlySelectedLayer;
      while (selectedLayer != null && !(selectedLayer is BuildingLayer))
      {
        var parent = selectedLayer.Parent as Mapping.Layer;
        selectedLayer = parent;
      }

      return GetFullQualifiedLayerPath(selectedLayer);
    }

    private static Mapping.FeatureLayer GetFirstFeatureLayerInBuilding(Mapping.CompositeLayer bldgLayer)
    {
      // always use the exterior shell if it is present
      IReadOnlyList<Mapping.Layer> foundLayers = bldgLayer.FindLayers("ExteriorShell", true);
      if (foundLayers.Count >= 1)
      {
        Mapping.FeatureLayer fLayer = foundLayers[0] as Mapping.FeatureLayer;
        return fLayer;
      }

      // 
      foreach (Mapping.Layer subLayer in bldgLayer.GetLayersAsFlattenedList())
      {
        if (subLayer is Mapping.FeatureLayer)
        {
          var featureLayer = subLayer as Mapping.FeatureLayer;
          return featureLayer;
        }
      }
      return null;
    }

    #endregion

    volatile bool _gpthreadbusy;
    internal static bool IsGPThreadBusy() { return Current?._gpthreadbusy == true; }

    static bool _isSemanticSearchAvailable;
    static internal bool IsSemanticSearchAvailable
    {
      get => _isSemanticSearchAvailable;
      set
      {
        if (_isSemanticSearchAvailable != value)
        {
          _isSemanticSearchAvailable = value;
          const string pane_id = "esri_geoprocessing_toolBoxes";
          if (FrameworkApplication.DockPaneManager.IsDockPaneCreated(pane_id) &&
              FrameworkApplication.DockPaneManager.Find(pane_id) is GPDocPaneViewModel pane)
            pane.UpdateSemanticSearchStatus();
        }
      }
    }

    #region private members and methods
    private static GPOptions _GPOptions = null;
    private static MBOptions _MBOptions = null;
    private static List<IETLToolEditor> _etlToolEditors = new List<IETLToolEditor>();

    static internal void SetETLEditor(IETLToolEditor editor)
    {
      _etlToolEditors.Add(editor);
    }
    static IGPService _srv;
    static internal IGPService getIGPService()
    {
      if (_module?._disposed == true)
        return null;
      if (_srv == null)
      {
        _srv = ServiceManager.Find<IGPService>();
        if (_sync == null)
        {
          _sync = new GPServiceSync();
          _sync.connect(_srv);
        }
      }
      PushSettings();
      PushMapandSceneSettings();
      PushMBSettings();
      return _srv;
    }

    static internal void PushMBSettings()
    {
      if (_module?._disposed == true)
        return;
      _srv?.SetOption("AddDisplayWithinModelBuilderGroupLayer",
                      ArcGIS.Desktop.Internal.GeoProcessing.Properties.Settings.Default.AddDisplayWithinModelBuilderGroupLayer ? "true" : "false");
      //var opt = GeoprocessingModule.GetMBOptions();
      //if (opt.IsSynchronizeWithEngine)
      //{
      //  opt.IsSynchronizeWithEngine = false;
      //  //push the current settings
      //  await QueuedTask.Run(() =>
      //  {
      //    try
      //    {
      //      if (_module?._disposed == true)
      //        return;
      //      var srv = _srv;
      //      if (srv == null) return;
      //      srv.SetOption("AddDisplayWithinModelBuilderGroupLayer", opt.AddDisplayWithinModelBuilderGroupLayer ? "true" : "false");
      //    }
      //    catch (COMException)
      //    {
      //    }
      //  });
      //}
    }

    async static internal void PushSettings(string only_propName = "")
    {
      if (_module?._disposed == true)
        return;

      var opt = GetOptions();

      if (opt.IsSynchronizeWithEngine)
      {
        opt.IsSynchronizeWithEngine = false;
        //push the current settings
        await QueuedTask.Run(() =>
        {
          try
          {
            if (_module?._disposed == true)
              return;
            var srv = _srv;
            if (srv == null) return;
            foreach (var propinfo in typeof(GPOptions).GetProperties().Where(it => it.IsDefined(typeof(GPOptions.IsSynchronizeWithEngineAttribute), false)))
            {
              System.Diagnostics.Debug.Assert(propinfo.PropertyType == typeof(bool));
              if (propinfo.GetValue(opt) is bool b)
              {
                var attr = propinfo.GetCustomAttribute(typeof(GPOptions.IsSynchronizeWithEngineAttribute)) as GPOptions.IsSynchronizeWithEngineAttribute;
                try
                {
                  if (!string.IsNullOrEmpty(only_propName) && only_propName != attr.propName && only_propName != propinfo.Name)
                    continue;
                  System.Diagnostics.Debug.WriteLine($"GP push option:{attr.propName} = {b}");
                  srv.SetOption(attr.propName, b ? "true" : "false");
                  if (!string.IsNullOrEmpty(only_propName))
                    return;
                }
                catch (COMException) { }
              }
            }
            string message_levels = string.Empty;
            if (opt.MessageLevel_ProjectionTransformation)
              message_levels += " 1";
            if (opt.MessageLevel_CommandSyntax)
              message_levels += " 2";
            if (opt.MessageLevel_Diagnostics)
              message_levels += " 3";
            System.Diagnostics.Debug.WriteLine($"GP push settings option:GPMessageLevels ={message_levels}");
            srv.SetOption("GPMessageLevels", message_levels.TrimStart());
          }
          catch
          {
          }
        });
      }
    }

    static internal void PushMapandSceneSettings()
    {
      _srv?.SetOption("CheckMapsWhenDeleting", ApplicationOptions.MappingOptions.CheckMapsWhenDeleting.ToLower());
      _srv?.SetOption("CheckMapsWhenRenaming", ApplicationOptions.MappingOptions.CheckMapsWhenRenaming.ToLower());
    }

    //return string matching pattern [a-z]+[a-z0-9]*
    private static string make_alias(string basename)
    {
      //allow only English characters a-z | 0-9 starting from letter
      var alias = new String(basename.ToCharArray().
        SkipWhile(it => { var ch = Char.ToLower(it); return !(ch >= 'a' && ch <= 'z'); }).
        Where(it => { var ch = Char.ToLower(it); return (ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= 'z'); }).
        ToArray());
      return string.IsNullOrEmpty(alias) ? "newtoolbox" : alias;
    }

    //return full path to new toolbox, empty string when failed
    static internal Task<string> CreateToolboxAsync(string fullpath, bool add_to_project, bool try_correct_name)
    {
      if (string.IsNullOrEmpty(fullpath))
        return Task.FromResult(string.Empty);
      return QueuedTask.Run(() =>
      {
        try
        {
          var service = getIGPService();
          string parent_path;
          string basename;
          string ext;
          ext = System.IO.Path.GetExtension(fullpath);
          basename = System.IO.Path.GetFileNameWithoutExtension(System.IO.Path.GetFileName(fullpath));
          parent_path = System.IO.Path.GetDirectoryName(fullpath);
          int count = 0;
          var name = basename;
          while (true)
          {
            try
            {
              var alias = make_alias(System.IO.Path.GetFileNameWithoutExtension(name));
              fullpath = service.CreateToolbox(fullpath, alias);
              if (string.IsNullOrEmpty(fullpath))
                return string.Empty;
              break;
            }
            catch (Exception e)
            {
              var hr = (System.UInt32)e.HResult;
              if ((hr == 0x80041543 || hr == 0x80040353 || hr == 0x80070057) && try_correct_name)
              {
                if (count > 1024) try_correct_name = false; //stop infinit loop
                name = string.IsNullOrEmpty(ext) ? $"{basename}{++count}" : $"{basename} ({++count}){ext}";
                fullpath = System.IO.Path.Combine(parent_path, name);
              }
              else
                return string.Empty;
            }
          }

          if (Project.Current != null)
          {
            if (add_to_project)
            {
              ArcGIS.Desktop.Internal.Core.Operations.ProjectOperationHelpers.AddProjectItem(GeoprocessingContainer.ContainerKey, fullpath, name, "");
              GPToolProjectItemsChangedEvent.Publish();
              ArcGIS.Desktop.Core.Project.Current.SetDirty();
            }
            //var parent = System.IO.Path.GetDirectoryName(fullpath);
            //((IInternalProject)Project.Current).RefreshProjectItems(parent);
          }
          return fullpath;
        }
        catch
        {
          return string.Empty;
        }
      });
    }
    //    async static void Test_EBK() moved to \ArcGIS\ArcGISDesktop\ArcGISCore\ArcGIS.Desktop.Core\Geoprocessing\GeoprocessingModuleBase.cs

    ///<exclude></exclude>
    public static GPOptions GetOptions()
    {
      if (_GPOptions == null)
      {
        _GPOptions = new GPOptions();
        try
        {
          _GPOptions.LoadFromProfile();
        }
        catch
        {
          _GPOptions = new GPOptions();
        }
      }
      return _GPOptions;
    }

    static internal MBOptions GetMBOptions()
    {
      _MBOptions ??= new MBOptions();
      try
      {
        _MBOptions.LoadFromProfile();
      }
      catch (Exception)
      {
        _MBOptions = new MBOptions();
      }
      return _MBOptions;
    }

    static internal Task RemoveToolboxAsync(GeoprocessingProjectItem[] items)
    {
      return QueuedTask.Run(() =>
      {
        foreach (var it in items)
        {
          try
          {
            if (it.IsDefault) continue;
            ArcGIS.Desktop.Internal.Core.Operations.ProjectOperationHelpers.RemoveProjectItem("GP", it.Path);
            getIGPService().RemoveToolboxFromProject(it.Path);
          }
          catch { }
        }
        GPToolProjectItemsChangedEvent.Publish();
      });
    }

    static internal GeoprocessingContainer GetProjectContainer(bool create = true)
    {
      if (Project.Current == null)
        return null;

      if (create)
        return (Project.Current as IInternalProject).GetProjectItemContainer(GeoprocessingContainer.ContainerKey) as GeoprocessingContainer;

      if ((Project.Current as IInternalProject).ExistsProjectItemContainer(GeoprocessingContainer.ContainerKey))
        return (Project.Current as IInternalProject).GetProjectItemContainer(GeoprocessingContainer.ContainerKey) as GeoprocessingContainer;
      return null;
    }

    static internal HistoryContainer GetHistoryContainer(bool create = true)
    {
      if (create && Project.Current != null)
        return (Project.Current as IInternalProject).GetProjectItemContainer(HistoryContainer.ContainerKey) as HistoryContainer;

      if (Project.Current != null && (Project.Current as IInternalProject).ExistsProjectItemContainer(HistoryContainer.ContainerKey))
        return (Project.Current as IInternalProject).GetProjectItemContainer(HistoryContainer.ContainerKey) as HistoryContainer;

      return null;
      //return GetProjectContainerT(_historyContainer);
    }

    static internal Internal.Core.IGeoprocessing2 GeoprocessingInternal => GeoprocessingModule.Current;

    static internal IInternalMappingModule MappingInternal => FrameworkApplication.FindModule(ArcGIS.Desktop.Internal.Mapping.Constants.ModuleID) as IInternalMappingModule;

    internal static IInternalCoreModule InternalCoreModule => FrameworkApplication.FindModule("esri_core_module") as IInternalCoreModule;

    //static internal IInternalArcGISRasterModule RasterInternal => FrameworkApplication.FindModule("esri_datasourcesraster") as IInternalArcGISRasterModule;

    internal static ISpatialAnalystService _saService;
    internal static ISpatialAnalystService SAService
    {
      get
      {
        if (_saService == null)
          _saService = ArcGIS.Desktop.Internal.Framework.Utilities.ServiceManager.Find<ISpatialAnalystService>();
        return _saService;
      }
    }

    internal static IAGOBrowseService _agoBrowseService;
    internal static IAGOBrowseService AGOBrowseService
    {
      get
      {
        if (_agoBrowseService == null)
          _agoBrowseService = ArcGIS.Desktop.Internal.Framework.Utilities.ServiceManager.Find<IAGOBrowseService>();
        return _agoBrowseService;
      }
    }

    #endregion

    #region Project Events
    async static private void ProjectDefault_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e?.PropertyName == "DefaultGeodatabasePath")
      {
        await QueuedTask.Run(Environments.resetProjectSettings);
      }
    }

    private async void OnProjectOpened(ProjectEventArgs e)
    {
      e.Project.PropertyChanged += ProjectDefault_PropertyChanged;
      string map_path = (MappingInternal.ActiveMapView != null && MappingInternal.ActiveMapView.Map != null) ? MappingInternal.ActiveMapView.MapURI : string.Empty;
      await QueuedTask.Run(() =>
      {
        if (_disposed) return;
        try
        {
          var gp = getIGPService();
          gp.SetProject(e.Project.GetID());
          gp.SetActiveMapFile(e.Project.URI);
          if (!string.IsNullOrEmpty(map_path))
            gp.SetActiveMap(map_path);
          _activeMap = map_path;
          if (SysToolsUtil._productCode == -1)
            SysToolsUtil._productCode = int.Parse(gp.GetOption("productCode"));
        }
        catch { System.Diagnostics.Debug.Assert(false); }
      });
      string string_xml = await e.Project.GetInternal().GetModuleSettingsAsync(this.ID);
      //await Task.Factory.StartNew(() =>
      {
        XDocument save_doc = null;
        if (!string.IsNullOrEmpty(string_xml))
          save_doc = System.Xml.Linq.XDocument.Parse(string_xml);

        ProjectGallery.FromXML(save_doc);
        if (FavoritesGalleryViewModel.created)
          ProjectGallery.Items.Count(); //force ribbon to pull new items
        ProjectFavorites.FromXML(save_doc);
        if (System.IO.File.Exists(UserFavorites.userFavoritesPath))
          UserFavorites.FromXML(XDocument.Parse(System.IO.File.ReadAllText(UserFavorites.userFavoritesPath)));

        Environments.SetFromXML(save_doc, GeoprocessingModule.getIGPService() as IGPEnvironmentService);

        if (MappingInternal.ActiveMapView != null && MappingInternal.ActiveMapView.Map != null)
          ((INotifyCollectionChanged)MappingInternal.ActiveMapView.Map.Layers).CollectionChanged += OnMapLayersCollectionChanged;

        toolHistoryDataExt.onload_messages(e.Project);
        GPToolProjectItemsChangedEvent.suspended = false;
        GPHistoryChangedEvent.suspended = false;
        GPToolProjectItemsChangedEvent.Publish();
        GPHistoryChangedEvent.Publish();
        ArcGIS.Desktop.Internal.Framework.FrameworkApplication.QueueIdleAction((Action)(() => execute_helper.pickup_remote_jobs()));
      }//);
    }

    private Task OnProjectSaving(ProjectEventArgs obj)
    {
      return GeoprocessingModule.Current?.moduleSave(obj.Project, obj.IsSavingBackup);
    }

    private static Task cleanup_PortalProjectHistory(IInternalGISProjectItem prj, HistoryContainer history)
    {
      if (prj is null || history is null)
        return Task.CompletedTask;
      return QueuedTask.Run(() =>
      {
        try
        {
          foreach (var hitem in history.Select(it => it.HistoryData))
            hitem.portal_exclude(prj);
        }
        catch { }
      });
    }

    private async Task moduleSave(Project prj, bool isBackup)
    {
      if (Project.Current == null)
        return;

      try
      {
        if (!isBackup)
        {
          var history = GeoprocessingModule.GetHistoryContainer(false);
          if (prj.IsPortalProject && history != null)
          {
            await cleanup_PortalProjectHistory(prj, history);
          }
          if (prj.ReadOnly == false && history != null)
          {
            toolHistoryDataExt.onsave_messages(history);
          }
        }
      }
      catch { }

      try
      {
        await prj.GetInternal().RemoveModuleSettingsAsync(this.ID);
      }
      catch
      {
        return;
      }

      var root = new System.Xml.Linq.XElement("gp_project_root", new XAttribute("ver", 1));
      root.Add(ProjectGallery.ToXML());
      root.Add(ProjectFavorites.ToXML());
      root.Add(ArcGIS.Desktop.Internal.GeoProcessing.Environments.ToXML());
      await prj.GetInternal().PutModuleSettingsAsync(this.ID, root.ToString()).
        ContinueWith(_ => getIGPService().SetActiveMapFile(prj.URI));
    }
    static private Task OnProjectClosing(ProjectClosingEventArgs e)
    {
      if (IsGPThreadBusy())
      {
        var names = execute_helper.GetRunningNames();
        if (names.Count > 0)
        {
          if (ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
              string.Format(Internal.GeoProcessing.LocalResources.GpResources.executeInterruptMessage, string.Join(",", names)),
              string.Format(Internal.GeoProcessing.LocalResources.GpResources.executeInterruptTitle, ArcGIS.Desktop.Internal.Framework.FrameworkApplication.AppShortName),
              System.Windows.MessageBoxButton.YesNo,
              System.Windows.MessageBoxImage.Question) != System.Windows.MessageBoxResult.Yes)
            e.Cancel = true;
          else
          {
            return Task.Run(() =>
            {
              execute_helper.InterruptLocalTasks();
            });
          }
        }
      }
      return Task.CompletedTask;
    }
    static private async void OnProjectClosed(ProjectEventArgs e)
    {
      try
      {
        execute_helper.DetachRemoteTasks();

        System.Diagnostics.Debug.WriteLine("*** GP:OnProjectClosed()");
        //FrameworkApplication.State.Deactivate("esri_geoprocessing_historyContainerState");
        //GPToolProjectItemsChangedEvent.Publish();
        //GPHistoryChangedEvent.Publish();
        GPToolProjectItemsChangedEvent.suspended = true;
        GPHistoryChangedEvent.suspended = true;
        if (isAppClosing())
          return;
        GPDocPaneViewModel pane;
        if (_gp_pane.TryGetTarget(out pane))
          pane.CloseTabs();

        e.Project.PropertyChanged -= ProjectDefault_PropertyChanged;
      }
      catch { }
      await QueuedTask.Run(() =>
      {
        try
        {
          getIGPService()?.SetProject(-1);
          Environments.prep_for_reset();
        }
        catch { };
      });
    }

    static int _hcMap = 0;
    private async void OnMapClosing(ArcGISMapClosingEventArgs obj)
    {
      if (isAppClosing())
        return;
      if (_hcMap == obj?.MapView?.GetHashCode())
      {
        await QueuedTask.Run(() =>
        {
          try
          {
            getIGPService()?.SetActiveMap(string.Empty);
          }
          catch { }
        }, Progressor.None);

        _hcMap = 0;
        _activeMap = string.Empty;
        ValidateToolDialogEvent.Publish();
      }
      try
      {
        ((INotifyCollectionChanged)obj.MapView.Map.Layers).CollectionChanged -= OnMapLayersCollectionChanged;
      }
      catch { }
    }

    internal string _activeMap;
    private async void OnMapChanged(ActiveMapViewChangedEventArgs obj)
    {
      if (isAppClosing())
        return;

      var inView = obj.IncomingView;

      if (inView != null)
      {
        var inHashCode = inView.GetHashCode();
        if (inHashCode != _hcMap)
        {
          var inMap = inView.Map;
          if (inMap == null)
            return;
          _hcMap = inHashCode;
          _activeMap = inMap.URI;

          await QueuedTask.Run(() =>
          {
            try
            {
              getIGPService()?.SetActiveMap(inMap.URI);
            }
            catch { }
          }, Progressor.None);

          ValidateToolDialogEvent.Publish();
          if (inMap != null)
            ((INotifyCollectionChanged)inMap.Layers).CollectionChanged += OnMapLayersCollectionChanged;
        }
      }
    }
    private void OnActivePaneChanged(PaneEventArgs args)
    {
      if (isAppClosing())
        return;

      ValidateToolDialogEvent.Publish();
    }
    static private void OnMapLayersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (isAppClosing())
        return;
      ValidateToolDialogEvent.Publish();
    }
    #endregion
    //track cef conrols, disposse on exit
    static internal List<WeakReference<IDisposable>> _web_controls = new List<WeakReference<IDisposable>>();
    #region tools statistics

    internal sealed class ToolInfoStats : ToolInfo
    {
      public ToolInfoStats(XElement e, DateTime t) : base(e) { LastRunTime = t; }
      public DateTime LastRunTime { get; private set; }
    }

    static internal async void LocateTool(string toolPath)
    {
      var toolInfo = await query_toolinfo(toolPath);
      if (toolInfo == null)
        return;

      if (SysToolsUtil.isSystem(toolInfo) || SysToolsUtil.isPortal(toolInfo))
      {
        if (FrameworkApplication.DockPaneManager.Find("esri_geoprocessing_toolBoxes") is GPDocPaneViewModel pane)
        {
          pane.Activate();
          pane.LocateTool(toolInfo);
        }
      }
    }

    static internal Task<List<ToolInfoStats>> GetToolStatsList(bool? most_used = null, int? ret_len = null)
    {
      try
      {
        if (GeoprocessingModule.GetHistoryContainer(false) is HistoryContainer history)//_historyContainer.TryGetTarget(out history))
        {
          var all = history.Reverse().Select(it => it.HistoryData).ToArray();
          if (most_used is null) most_used = !Properties.Settings.Default.StatsAsMostUsed;
          if (ret_len is null) ret_len = Properties.Settings.Default.StatsDisplayCount;
          return Task.Factory.StartNew<List<ToolInfoStats>>(() => get_stats_list(all, most_used.Value, ret_len.Value),
            System.Threading.CancellationToken.None, TaskCreationOptions.AttachedToParent, QueuedTask.UIScheduler);
        }
      }
      catch { }
      return Task.FromResult(new List<ToolInfoStats>());
    }

    static List<ToolInfoStats> get_stats_list(IEnumerable<toolHistoryData> all, bool bAsRecent, int n)
    {
      if (_module?._disposed == true)
        return null;
      var favorites = GeoprocessingModule.ProjectFavorites;
      var list = all.Where(it => it.ToolInfo.IsValid).GroupBy(it => it.ToolInfo.getExecutePathShort()).
          Select((it) =>
          {
            var data = it.First();
            //if (!favorites.isIn(data.ToolInfo))
            return new ToolInfoStats(data.ToolInfo.Node, data.timestamp);
            //return null;
          });
      //System.Diagnostics.Debug.WriteLine("*** get_stats_list()");
      return list.Where(it => it != null).Take(n).ToList();
    }
    #endregion

    #region Favorites and Gallery

    static private Favorites _favorites;
    static private UserFavorites _userfavorites;
    static internal Favorites ProjectFavorites
    {
      get
      {
        if (_favorites == null)
          _favorites = new Favorites();
        return _favorites;
      }
    }
    static internal UserFavorites UserFavorites
    {
      get
      {
        if (_userfavorites == null)
          _userfavorites = new UserFavorites();
        return _userfavorites;
      }
    }

    private static GalleryItems _galleryItems;

    static internal GalleryItems ProjectGallery
    {
      get
      {
        if (_galleryItems == null)
          _galleryItems = new GalleryItems();
        return _galleryItems;
      }
    }

    #endregion


    #region IInternalToolDialog_Helper
    Task<object> IGPToolDialogHelper.CreateToolDialogControlAsync(string path, string[] param_list, KeyValuePair<string, string>[] envs)
    {
      return ToolDialog_Helper.CreateToolDialogControlAsync(path, param_list, envs);
    }

    Task<IGPResult> IGPToolDialogHelper.Run(object control, CancellationToken cancel, GPToolExecuteEventHandler notifyHandler, GPExecuteToolFlags flags)
    {
      return ToolDialog_Helper.Run(control, cancel, notifyHandler, flags);
    }

    Task<IGPResult> IGPToolDialogHelper.Run(object control, object progress_control, GPExecuteToolFlags flags)
    {
      return ToolDialog_Helper.Run(control, progress_control, flags);
    }

    bool IGPToolDialogHelper.IsValid(object control)
    {
      return ToolDialog_Helper.IsValid(control);
    }

    object IGPToolDialogHelper.ProgressControl => ToolDialog_Helper.ProgressControl;
    #endregion

    /// <exclude></exclude>
    public readonly record struct DatatypeInfo(string name, string label, string description);

    static Lazy<IList<DatatypeInfo>> _datatypes = new Lazy<IList<DatatypeInfo>>(() =>
    {
      try
      {
        var datatype_dir = System.IO.Path.Combine(SysToolsUtil.gpHelpPath, "DataTypes");
        var items = System.IO.Directory.EnumerateFiles(datatype_dir, "*.xml").Select(it =>
        {
          try
          {
            var root = XDocument.Load(it).Root;
            string name = root.Attribute("Name").Value;
            string label = root.Element("DisplayName").Value;
            string description = root.Element("Description").Value;
            return new DatatypeInfo(name, label, description);
          }
          catch { return new DatatypeInfo(string.Empty, string.Empty, string.Empty); }
        }).Where(it => it.name != string.Empty);
        return items.OrderBy(it => it.label).ToList();
      }
      catch { return new List<DatatypeInfo>(); }
    });

    static internal IList<DatatypeInfo> QueryDatatypes() => _datatypes.Value;
    static internal Task<IList<DatatypeInfo>> QueryDatatypesAsync() => _datatypes.IsValueCreated ? Task.FromResult(_datatypes.Value) : Task.Run(() => _datatypes.Value);
  }

  class GPServiceSync : IGPServiceSync
  {
    WeakReference<IGPService> _service;
    internal void connect(IGPService service)
    {
      _service = new WeakReference<IGPService>(service);
      if (service is IDesktopServiceRegisterSync srv)
      {
        srv.Advise(this);
        if (Internal.Framework.GeoprocessingSettings.BlockNonSystemPythonToolbox?.GetValue() is string block_pyt)
          service.SetOption("BlockNonSystemPythonToolbox", block_pyt);
      }
    }
    internal void disconnect()
    {
      IGPService service;
      if (_service != null && _service.TryGetTarget(out service))
      {
        (service as IDesktopServiceRegisterSync).Unadvise();
        _service = null;
      }
    }
    #region IGPServiceSync Members
    void IGPServiceSync.OnNotify(string xml_event)
    {
      XElement? xml = null;
      try
      {
        xml = XDocument.Parse(xml_event).Root;
      }
      catch { return; }

      switch (xml?.Attribute("type").Value)
      {
        case "toolbox":
          System.Windows.Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ContextIdle, new Action(() =>
          {
            try
            {
              toolbox_notiffications(xml.Attribute("name").Value, xml);
            }
            catch { }
          })).Task.ConfigureAwait(false);
          break;
        case "user_ask_access_pyt":
          {
            var path = xml.Attribute("path").Value;
            string ask = string.Format(Internal.GeoProcessing.LocalResources.GpResources.pyt_access_question, path);
            if (System.Windows.Application.Current.Dispatcher.InvokeAsync(() => Internal.Framework.DialogManager.ShowMessageBox(ask, "", System.Windows.MessageBoxButton.YesNo, image: System.Windows.MessageBoxImage.Warning) == System.Windows.MessageBoxResult.No).Result)
            {
              //COR_E_UNAUTHORIZEDACCESS
              Marshal.ThrowExceptionForHR(unchecked((int)0x80070005));
            }
          }
          break;
        default:
          break;
      }
    }

    static void toolbox_notiffications(string kind, XElement root)
    {
      try
      {
        switch (kind)
        {
          case "OnToolAdded":
          case "OnToolDeleted":
            //var path = root.Attribute("path").Value;
            //await Project.Current.GetInternal().RefreshProjectItemsAsync(path);
            break;
          case "OnToolStored":
            //var tool_name = root.Element("param").Value;
            ValidateToolDialogEvent.Publish();
            break;
        }
      }
      catch { }
    }
    #endregion
  }

  // Used for showing GP tools in the application's customization dialog
  internal sealed class GeoprocessingToolsComponent : ICustomizationComponent
  {
    public Task<IEnumerable<ICustomizeCommand>> GetCommandsList(string searchText)
    {
      // Execute the search
      return QueuedTask.Run(async () => {
        // to return only system tools, the Lucene indexer makes a call with *
        if (searchText == "*")
        {
          await embeddings.initialize_embeddings();
          return SearchTools.get_cache(false).Values.Select(it =>
          {
            return new GeoprocessingCustomizationCommand(it.get_tool(), it.embeddings) as ICustomizeCommand;
          });
        }
        else //search by Options - Customize the Quick Access Toolbar (All Geoprocessing tools)
        {
          if (string.IsNullOrEmpty(searchText))
            searchText = "*";
          return SearchTools.Search(searchText, () => SearchTools.get_cache(true).Values).Select(it => new GeoprocessingCustomizationCommand(it as ToolInfo, null) as ICustomizeCommand);
        }
      });
    }

    public bool HasCustomSearch => true;
    //public string PrimaryCategory => "GP";
    public override int GetHashCode() { return embeddings.get_hcode(ToolboxUtil.gen_cache_filename(0), 1234); }
  }

  internal sealed class GeoprocessingCustomizationCommand : ArcGIS.Desktop.Internal.Framework.ICommandSearchTerm2 //ICustomizeCommand
  {
    readonly string _id;//, _caption, _smallImagePath, _largeImagePath, _description, _disabledTooltip, _helpContextID, _toolboxName, _asmPath, _className;
    readonly string _smallImagePath;
    readonly string _largeImagePath;
    float[][] _embeddings;
    int _helpID;
    ToolInfo _info;
    XElement _args;
    //static readonly string _asmPath = typeof(GeoprocessingModule).Assembly.Location;
    static readonly string _className = "esri_geoprocessing_module:Commands.ExecuteGPToolButton";
    static void get_tool_icon(in ToolInfo info, out string small, out string large)
    {
      large = info.ImageSource;
      //adjust path to read image from zip by ArcGIS.Desktop.Internal.Framework.Utilities.ReadPictureFile()
      if (large?.IndexOf(@".atbx\") is int pos && pos > 0)
      {
        large = $"{large.Substring(0, pos + 5)}|{large.Substring(pos + 6)}";
        //conservative approach
        if (SysToolsUtil.isSystem(info) == false && SysToolsUtil.isPortal(info) == false)
        {
          small = large;
          return;
        }
      }
      switch (info.ToolType)
      {
        case "model":
          small = "GeoprocessingModel16";
          if (string.IsNullOrEmpty(large))
            large = "GeoprocessingModel32";
          break;
        case "tool_script":
        case "script":
        case "pythonscript":
          small = "GeoprocessingScript16";
          if (string.IsNullOrEmpty(large))
            large = "GeoprocessingScript32";
          break;
        default:
          small = "GeoprocessingTool16";
          if (string.IsNullOrEmpty(large))
            large = "GeoprocessingTool32";
          break;
      }
    }
    public GeoprocessingCustomizationCommand(ToolInfo info, float[][] embeddings)
    {
      _info = info;
      _id = $"esri_geoprocessing_{_info.toolboxalias}_{_info.toolName}";
      get_tool_icon(info, out _smallImagePath, out _largeImagePath);
      _helpID = SysToolsUtil.query_system_tool_helpID(_info.getExecutePathShort());
      _embeddings = embeddings;
    }
    public string Caption => _info.Name;
    public string Tooltip => _info.Description;
    public string HelpContextID => _helpID > 0 ? _helpID.ToString() : string.Empty;
    public string ID => _id;
    public string SmallImagePath => _smallImagePath;
    public string LargeImagePath => _largeImagePath;
    public string DisabledTooltip => string.Empty;
    public XElement Args
    {
      get
      {
        if (_args == null)
          _args = new XElement("params", new XAttribute("toolPath", _info.IsSystem ? _info.getExecutePathShort() : _info.getExecutePath()), new XAttribute("name", _info.Name));
        return _args;
      }
    }
    public string ExtendedCaption => _info.ToolBoxName;
    public string AssemblyPath => string.Empty;//_asmPath;
    public string ClassName => _className;
    public string SearchTerms => string.Join(",", _info.keys);

    //ICommandSearchTerm2
    public string[] Category => ["GP", $"toolbox:{_info.toolboxalias}"];
    public float[][] Embeddings => _embeddings;
  }
}
