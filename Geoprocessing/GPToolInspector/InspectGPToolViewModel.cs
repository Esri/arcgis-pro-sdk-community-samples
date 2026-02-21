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
using ArcGIS.Desktop.GeoProcessing;
using ArcGIS.Desktop.Internal.GeoProcessing;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using GPToolInspector.TreeHelpers;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using static GPToolInspector.TreeHelpers.TbxReader;

namespace GPToolInspector
{
  internal class InspectGPToolViewModel : DockPane
  {
    private const string _dockPaneID = "GPToolInspector_InspectGPTool";
    private ObservableCollection<GeoprocessingProjectItem> _GeoProcessingProjItems = [];
    private ObservableCollection<TbxItemBase> _SearchResults = [];
    private object _lock = new();

    protected InspectGPToolViewModel()
    {
      System.Windows.Data.BindingOperations.EnableCollectionSynchronization(_GeoProcessingProjItems, _lock);
      System.Windows.Data.BindingOperations.EnableCollectionSynchronization(_TbxItems, _lock);
      System.Windows.Data.BindingOperations.EnableCollectionSynchronization(_SearchResults, _lock);

      _TreeViewVisibility = Visibility.Visible;
      _ListBoxVisible = Visibility.Collapsed;

      InitializeTbxItems();
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

    #region Commands

    public ICommand CmdReload
    {
      get
      {
        return new RelayCommand(async () =>
        {
          TbxItems.Clear();
          GeoProcessingProjItems.Clear();
          //var gpItems = CoreModule.CurrentProject.Items.OfType<GeoprocessingProjectItem>();
          await QueuedTask.Run(() =>
          {
            InitializeTbxItems();
          });
        });
      }
    }
    
    public ICommand CmdStopSearch
    {
      get
      {
        return new RelayCommand(() =>
        {
          Searchtext = string.Empty;
        });
      }
    }

    #endregion Commands

    #region Properties

    public ObservableCollection<GeoprocessingProjectItem> GeoProcessingProjItems
    {
      get => _GeoProcessingProjItems;
      set => SetProperty(ref _GeoProcessingProjItems, value);
    }

    private string _Searchtext = "";

    public string Searchtext
    {
      get => _Searchtext;
      set
      {
        SetProperty(ref _Searchtext, value);
        var doSearch = !string.IsNullOrEmpty(_Searchtext);
        TreeViewVisibility = doSearch ? Visibility.Collapsed : Visibility.Visible;
        ListBoxVisible = doSearch ? Visibility.Visible : Visibility.Collapsed;
        if (doSearch)
        {
          // search the toolboxes
          var helperPath = MSIHelper.GetInstallDirAndVersion();
          if (!helperPath.HasValue) return;
          var systemToolBoxPath = Path.Combine(helperPath.Value.Folder, @"Resources\ArcToolbox\Toolboxes");
          SearchToolBoxes(systemToolBoxPath, string.Empty);
        }
      }
    }
    
    public ObservableCollection<TbxItemBase> SearchResults
    {
      get => _SearchResults;
      set
      {
        SetProperty(ref _SearchResults, value);
      }
    }

    public TbxItemBase SelectedSearchResult
    {
      get => _SelectedSearchResult;
      set
      {
        SetProperty(ref _SelectedSearchResult, value);
        if (_SelectedSearchResult is TbxItemTool theTool)
        {
          theTool.IsSelected = true;
        }
      }
    }

    /// <summary>
    /// Text shown near the top of the DockPane.
    /// </summary>
    private string _heading = "GeoProcessing Tool Inspector";
    public string Heading
    {
      get => _heading;
      set => SetProperty(ref _heading, value);
    }

    private ObservableCollection<TbxItem> _TbxItems = [];
    public ObservableCollection<TbxItem> TbxItems {
      get => _TbxItems;
      set => SetProperty(ref _TbxItems, value);
    }
    #endregion // Properties

    #region Visibility Properties

    private Visibility _ListBoxVisible;
    public Visibility ListBoxVisible
    {
      get { return _ListBoxVisible; }
      set
      {
        SetProperty(ref _ListBoxVisible, value);
      }
    }

    private Visibility _TreeViewVisibility;
    public Visibility TreeViewVisibility
    {
      get { return _TreeViewVisibility; }
      set
      {
        SetProperty(ref _TreeViewVisibility, value);
      }
    }

    #endregion // Visibility

    #region Image Properties

    public ImageSource ReloadImageSrc => Application.Current.Resources["GeoprocessingToolboxPython16"] as ImageSource;

    public ImageSource StopSearchImageSrc => System.Windows.Application.Current.Resources["back_btn14"] as ImageSource;

    public ImageSource ToolboxFolderImgSrc => Application.Current.Resources["ToolboxesFolder32"] as ImageSource;

    public ImageSource ToolboxImgSrc => Application.Current.Resources["GeoprocessingToolbox32"] as ImageSource;

    public ImageSource ToolImgSrc => Application.Current.Resources["GeoprocessingTool32"] as ImageSource;

    public ImageSource SriptImgSrc => Application.Current.Resources["GeoprocessingScript32"] as ImageSource;

    public ImageSource SearchBackgroundImgSource => Application.Current.Resources["Search16"] as ImageSource;

    #endregion // Image Properties

    #region Helpers

    private void InitializeTbxItems()
    {
      var helperPath = MSIHelper.GetInstallDirAndVersion();
      if (!helperPath.HasValue) return;
      var systemToolBoxPath = Path.Combine(helperPath.Value.Folder, @"Resources\ArcToolbox\Toolboxes");
      var systemTbxItemRoot = new TbxItem(null, systemToolBoxPath)
      {
        Title = "System Toolboxes"
      };
      TbxItems.Add(systemTbxItemRoot);
    }

    private List<(string Path, string Category, TbxItem TbxItem)> cachedTbxItems;
    private TbxItemBase _SelectedSearchResult;
    private List<string> _noDuplicateSearchResults;

    /// <summary>
    /// search the ToolBox(es)
    /// First LeveL: Folders in 'toolboxes' folder containing toolbox.content files
    /// Second Level: toolbox.content file containing toolsets, 
    ///               second level is only the '&lt;root&gt;' node and
    ///               all root level toolsets (name split on '\')
    /// Third Level: all second level toolsets (name after split on '\')
    /// Last Level: all tools in the toolsets
    /// </summary>
    private void SearchToolBoxes(string toolBoxPath, string category)
    {
      SearchResults.Clear();
      _noDuplicateSearchResults = [];
      _ = QueuedTask.Run(() =>
      {
        if (cachedTbxItems != null)
        {
          foreach (var cachedTbxItem in cachedTbxItems)
          {
            SearchToolSets(cachedTbxItem.Path, cachedTbxItem.Category, cachedTbxItem.TbxItem);
          }
        }
        else
        {
          cachedTbxItems = [];
          // First LeveL: Folders
          foreach (var path in Directory.EnumerateDirectories(toolBoxPath))
          {
            var (Alias, Title, ToolSets) = ReadToolBoxHeader(path);
            var tbxItem = new TbxItem(null, path)
            {
              ToolSets = ToolSets,
              Alias = Alias,
              Name = Title,
              Title = Title
            };
            SearchToolSets(path, tbxItem.Alias, tbxItem);
            cachedTbxItems.Add((path, tbxItem.Alias, tbxItem));
          }
        }
      });
    }

    private void SearchToolSets (string rootFolder, string category, TbxItem tbxItem)
    {
      var tools = new List<(TbxItem parent, string Name, string Displayname, string Path)>();
      foreach (var toolset in tbxItem.ToolSets)
      {
        tools.AddRange(toolset.Tools.Where
          (it => it.ToolDisplayname.Contains(Searchtext, StringComparison.CurrentCultureIgnoreCase))
          .Select(it => (tbxItem, it.ToolName, it.ToolDisplayname, Path.Combine(rootFolder, it.ToolRelativePath))));
      }
      if (tools.Count > 0)
      {
        foreach (var tool in tools)
        {
          if (!_noDuplicateSearchResults.Contains(tool.Path))
          {
            _noDuplicateSearchResults.Add(tool.Path);
            var itemTool = new TbxItemTool(tool.parent, category, tool.Path)
            {
              Title = tool.Displayname
            };
            SearchResults.Add(itemTool);
          }
        }
      }
    }

    #endregion Helpers

  }

  internal class ContextMenuButton1 : Button
  {
    protected override void OnClick()
    {
      // TODO: Replace with your desired logic
      ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("First Action triggered from context menu.", "GPToolInspector");
    }
  }
  internal class ContextMenuButton2 : Button
  {
    protected override void OnClick()
    {
      // TODO: Replace with your desired logic
      ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("First Action triggered from context menu.", "GPToolInspector");
    }
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class InspectGPTool_ShowButton : Button
  {
    protected override void OnClick()
    {
      InspectGPToolViewModel.Show();
    }
  }
}
