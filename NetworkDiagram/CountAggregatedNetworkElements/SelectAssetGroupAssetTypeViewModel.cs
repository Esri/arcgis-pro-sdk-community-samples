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
using ArcGIS.Core.Data.NetworkDiagrams;
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Desktop.Core.Events;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using static CountAggregatedNetworkElements.CountAggregatedNetworkElementsModule;

namespace CountAggregatedNetworkElements
{
  internal class SelectAssetGroupAssetTypeViewModel : DockPane
  {
    private const string _dockPaneID = "CountAggregatedNetworkElements_SelectAssetGroupAssetType";
    private static DockPane _myPane;

    private ICommand _fillInfo;
    private ICommand _refreshCmd;
    private Dictionary<string, Dictionary<AssetGroup, List<AssetType>>> _possibleSources = [];
    private Dictionary<AssetGroup, List<AssetType>> _possibleGroups;
    private List<AssetType> _possibleTypes;
    private List<string> _junctionNames;
    private List<string> _groupNames;
    private List<string> _typeNames;

    private string _selectedJunctionClassName;
    private string _selectedGroupName;
    private string _selectedTypeName;

    private bool _isInitialized = false;
    private bool _isRunning = false;
    private bool _isProjectClosing = false;

    public ICommand FillInfo => _fillInfo;
    public ICommand RefreshCmd => _refreshCmd;

    protected SelectAssetGroupAssetTypeViewModel() { }

    /// <summary>
    /// Show the Count Aggregated Network Elements custom pane.
    /// </summary>
    internal static void Show()
    {
      if (_myPane == null)
      {
        _myPane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
        if (_myPane == null)
          return;
      }

      if (_myPane.IsVisible)
        _myPane.Hide();

      _myPane.Activate();

    }

    #region Override
    protected override async Task<Task> InitializeAsync()
    {
      MapClosedEvent.Subscribe(OnMapClosing);
      ProjectClosingEvent.Subscribe(OnProjectClosing);
      ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);
      ProjectOpenedEvent.Subscribe(OnProjectOpened);

      _fillInfo = new RelayCommand(() => FillInfoInternalAsync());
      _refreshCmd = new RelayCommand(() => RefreshCmdInternal());

      await ValidateMapAndSelection(MapView.Active?.Map);
      NotifyPropertyChanged(() => IsEnabled);
      NotifyPropertyChanged(() => Loading);
      return base.InitializeAsync();
    }

    private void RefreshCmdInternal()
    {
      CleanPane();
      CleanModule();
      Loading = false;
      ValidateMapAndSelection(MapView.Active?.Map);
      NotifyPropertyChanged(() => IsEnabled);
    }

    private void OnProjectOpened(ProjectEventArgs args)
    {
      CleanPane();
      CleanModule();
      FrameworkApplication.State.Activate(cIsUN);
      Loading = false;
      _isProjectClosing = false;
    }

    protected override Task UninitializeAsync()
    {
      MapClosedEvent.Unsubscribe(OnMapClosing);
      ProjectClosingEvent.Unsubscribe(OnProjectClosing);
      ActiveMapViewChangedEvent.Unsubscribe(OnActiveMapViewChanged);
      ProjectOpenedEvent.Unsubscribe(OnProjectOpened);

      _myPane = null;
      _fillInfo = null;
      return base.UninitializeAsync();
    }

    private void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs obj)
    {
      CleanPane();
      if (_myPane == null || (!FrameworkApplication.State.Contains(cIsUN) && !_myPane.IsVisible))
      {
        return;
      }

      if (obj?.IncomingView?.Map == null)
      {
        _myPane.Hide();
        return;
      }

      if (obj.IncomingView.Map.MapType != MapType.NetworkDiagram)
      {
        _myPane.Hide();
        return;
      }

      ValidateMapAndSelection(obj.IncomingView.Map);
      NotifyPropertyChanged(() => IsEnabled);
      NotifyPropertyChanged(() => Loading);
    }

    private Task OnProjectClosing(ProjectClosingEventArgs arg)
    {
      _myPane?.Hide();

      _isProjectClosing = true;
      CleanPane();
      CleanModule();
      FrameworkApplication.State.Deactivate(cIsUN);
      return Task.CompletedTask;
    }

    private void OnMapClosing(MapClosedEventArgs obj)
    {
      _myPane?.Hide();

      CleanPane();
      CleanModule();
    }
    #endregion

    protected override void OnActivate(bool isActive)
    {
      NotifyPropertyChanged(() => JunctionClassNames);
      NotifyPropertyChanged(() => SelectedJunctionClassName);

      base.OnActivate(isActive);
    }

    public bool IsEnabled
    { get => _isInitialized && !_isRunning; }

    #region Junctions
    public List<string> JunctionClassNames
    {
      get => _junctionNames;
    }

    public string SelectedJunctionClassName
    {
      get => _selectedJunctionClassName;
      set
      {
        SetProperty(ref _selectedJunctionClassName, value, () => SelectedJunctionClassName);

        if (GlobalIsUn)
        {
          _groupNames = [];

          if (string.IsNullOrEmpty(value))
          {
            _possibleGroups = [];
          }
          else
            if (_possibleSources.TryGetValue(value, out _possibleGroups))
          {
            _groupNames.Add("");
            foreach (var group in _possibleGroups)
            {
              string name = group.Key.Name;
              if (!_groupNames.Contains(name))
              {
                _groupNames.Add(name);
              }
            }
          }

          if (_groupNames.Count > 1)
            _groupNames.Sort((a, b) => a.CompareTo(b));

          NotifyPropertyChanged(() => AssetGroupNames);
          NotifyPropertyChanged(() => IsAssetGroupEnabled);
          NotifyPropertyChanged(() => IsUN);

          SelectedAssetGroup = _groupNames.Count == 1 ? _groupNames[0] : "";
        }
      }
    }

    public bool IsUN
    { get => _isInitialized && GlobalIsUn; }
    #endregion

    #region AssetGroup
    public List<string> AssetGroupNames
    {
      get => _groupNames;
    }

    public bool IsAssetGroupEnabled
    {
      get => _isInitialized && _groupNames != null && _groupNames.Count > 1;
    }

    public string SelectedAssetGroup
    {
      get => _selectedGroupName;
      set
      {
        SetProperty(ref _selectedGroupName, value, () => SelectedAssetGroup);
        _typeNames = [];
        if (string.IsNullOrEmpty(value))
        {
          _possibleTypes = [];
        }
        else
        {
          AssetGroup group = _possibleGroups.FirstOrDefault(a => a.Key.Name == value).Key;
          if (_possibleGroups.TryGetValue(group, out _possibleTypes))
          {
            _typeNames.Add("");
            foreach (var type in _possibleTypes)
            {
              if (!_typeNames.Contains(type.Name))
                _typeNames.Add(type.Name);
            }
          }
        }

        if (_typeNames.Count != 0)
          _typeNames.Sort((a, b) => a.CompareTo(b));

        NotifyPropertyChanged(() => AssetTypeNames);
        NotifyPropertyChanged(() => IsAssetTypeEnabled);

        SelectedAssetType = _typeNames.Count == 1 ? _typeNames[0] : "";
      }
    }
    #endregion

    #region AssetType
    public List<string> AssetTypeNames
    {
      get => _typeNames;
    }

    public bool IsAssetTypeEnabled
    {
      get => _isInitialized && _typeNames != null && _typeNames.Count > 1;
    }

    public string SelectedAssetType
    {
      get => _selectedTypeName;
      set
      {
        SetProperty(ref _selectedTypeName, value, () => SelectedAssetType);
      }
    }
    #endregion

    internal void CleanPane()
    {
      _possibleGroups = [];
      _possibleTypes = [];

      _junctionNames = [];
      _groupNames = [];
      _typeNames = [];

      _selectedGroupName = "";
      _selectedTypeName = "";

      if (_isInitialized && _myPane != null && _myPane.IsVisible)
      {
        NotifyPropertyChanged(() => JunctionClassNames);
        NotifyPropertyChanged(() => SelectedJunctionClassName);
      }
      _isInitialized = false;
    }

    private Task ValidateMapAndSelection(Map SelectedMap)
    {
      return QueuedTask.Run(() =>
      {
        if (_isRunning || _isProjectClosing)
        {
          return;
        }
        try
        {
          Loading = true;
          NotifyPropertyChanged(() => IsEnabled);

          if (SelectedMap != null && SelectedMap.MapType == MapType.NetworkDiagram)
          {
            if (GlobalDiagram == null)
            {
              CleanModule();
              if (!IsDiagramUsable(MinUnVersion: GlobalMinUnVersion, MinTnVersion: GlobalMinTnVersion, GetAllElements: true, GetSelection: false, ReferenceMap: SelectedMap))
              {
                CleanModule();
                Loading = false;
                return;
              }
            }
            else
            {
              NetworkDiagram diagram = diagram = GetNetworkDiagram(SelectedMap);
              if (diagram == null)
              {
                CleanPane();
                CleanModule();
                Loading = false;
                return;
              }
              else if (diagram.Name == GlobalDiagram.Name)
              {
                FillPane();
                Loading = false;
                return;
              }
              else if (!IsDiagramUsable(MinUnVersion: GlobalMinUnVersion, MinTnVersion: GlobalMinTnVersion, GetAllElements: true, GetSelection: false, ReferenceMap: SelectedMap))
              {
                CleanModule();
                Loading = false;
                return;
              }
            }

            if (GlobalDiagram == null)
            {
              CleanModule();
              Loading = false;
              return;
            }

            UtilityNetworkDefinition definition = GlobalUtilityNetwork.GetDefinition();
            try
            {
              GlobalNetworkSources = definition.GetNetworkSources();
            }
            catch { }

            FillPane();
          }

          Loading = false;
          _isInitialized = true;
        }
        catch (Exception ex)
        {
          ShowException(ex);
        }
      });
    }

    private void FillPane()
    {
      _possibleGroups = [];
      _possibleTypes = [];
      _possibleSources = [];
      _groupNames = [];
      _typeNames = [];
      _junctionNames = [];
      _junctionNames.Add("");

      foreach (NetworkSource v in GlobalNetworkSources)
      {
        if (v.UsageType == SourceUsageType.SystemJunction || v.UsageType== SourceUsageType.SubnetLine)
          continue;

        if (v.UsageType == SourceUsageType.Association)
        {
          _junctionNames.Add(cAssociation);
          _possibleSources.Add(cAssociation, []);
          continue;
        }

        string name = GetInternalSourceNameFromRealName(v.Name);

        if (!string.IsNullOrEmpty(name))
        {
          _junctionNames.Add(name);

          _possibleGroups = [];
          _possibleSources.Add(name, _possibleGroups);

          IReadOnlyList<AssetGroup> groups = v.GetAssetGroups();
          foreach (AssetGroup group in groups)
          {
            _possibleGroups.Add(group, [.. group.GetAssetTypes()]);
          }
        }
      }

      _junctionNames.Sort((a, b) => a.CompareTo(b));
      NotifyPropertyChanged(() => JunctionClassNames);

      SelectedJunctionClassName = _junctionNames.Count == 1 ? _junctionNames[0] : "";

      NotifyPropertyChanged(() => IsEnabled);
    }

    public bool Loading
    {
      get => _isRunning;
      set
      {
        SetProperty(ref _isRunning, value, () => Loading);
        NotifyPropertyChanged(() => IsNotLoading);
      }
    }
    public bool IsNotLoading
    { get => !_isRunning; }


    /// <summary>
    /// Fill the Info field in the database
    /// </summary>
    /// <returns>Task</returns>
    private async Task FillInfoInternalAsync()
    {
      await QueuedTask.Run(() =>
      {
        Loading = true;
        CreateCustomGraph();

        int source = GlobalNetworkSources.FirstOrDefault(a => a.Name.Contains(_selectedJunctionClassName, StringComparison.OrdinalIgnoreCase)).ID;

        FillInfoFields(SearchSource: SelectedJunctionClassName, GroupName: _selectedGroupName, TypeName: _selectedTypeName);

        MapView.Active.Redraw(true);
        Loading= false;
      });
    }
  }

  /// <summary>
  /// Button implementation to open the Count Aggregated Network Elements custom pane.
  /// </summary>
	internal class SelectAssetGroupAssetType_ShowButton : Button
  {
    protected override void OnClick()
    {
      SelectAssetGroupAssetTypeViewModel.Show();
    }
  }
}
