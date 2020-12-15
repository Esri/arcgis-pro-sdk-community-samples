using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
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
using ArcGIS.Desktop.Internal.Mapping.Locate;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Controls;
using ArcGIS.Desktop.Mapping.Events;

namespace SymbolSearcherControl
{
  internal class SymbolSearcherDockpaneViewModel : DockPane
  {
    private const string _dockPaneID = "SymbolSearcherControl_SymbolSearcherDockpane";
    private Dictionary<StyleItemType, SymbolPatchType> _patchTypes = null;

    private static readonly object _StyleProjectItemsLock = new object();
    private static readonly object _layersInMapLock = new object();

    private ObservableCollection<StyleItemType> _searchTypes = new ObservableCollection<StyleItemType>();
    private StyleItemType _selectedSearchType;
    private SymbolSearcherSearchFilter _selectedFilterStyle;
    private bool _searchPauseSearching = true;

    private ObservableCollection<MapMember> _layersInMap = new ObservableCollection<MapMember>();
    private MapMember _selectedLayer;
    private StyleItemType _searchFilterType;

    private ObservableCollection<SymbolSearcherSearchFilter> _filterStyleItems = new ObservableCollection<SymbolSearcherSearchFilter>();
    private SymbolSearcherSearchFilter _searchFilterStyle;

    private StyleItem _selectedPickerStyleItem;

    private int _selectedFilterStyleItemIdx = 0;
    private List<string> _statusLines = new List<string>();
    private string _status;

    protected SymbolSearcherDockpaneViewModel()
    {
      System.Diagnostics.Debug.WriteLine($@"==== SymbolPicker constructor start");
      //Subscribe to ProjectItemsChangedEvent to update ProjecyStyles
      ProjectItemsChangedEvent.Subscribe(OnProjectItemsChanged);
      ProjectClosedEvent.Subscribe(OnProjectClosed);
      //Subscribe to ActiveMapViewChangedEvent in order to get the layers in the map
      ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);
      _ = GetLayersInMapAsync();      
      _patchTypes = new Dictionary<StyleItemType, SymbolPatchType>
            {
                { StyleItemType.LineSymbol, SymbolPatchType.ZigzagLine },
                { StyleItemType.PolygonSymbol, SymbolPatchType.BoundaryPoly }
            };
      BindingOperations.EnableCollectionSynchronization(_filterStyleItems, _StyleProjectItemsLock);
      BindingOperations.EnableCollectionSynchronization(_layersInMap, _layersInMapLock);

      // if current project is null the OnProjectItemsChanged event will take care of FilterStyles
      if (Project.Current != null)
      {
        // setting up the search filter StyleItem list for the combo happens in OnProjectItemsChanged unless the project is already open
        FilterStyles.Add(new SymbolSearcherSearchFilter(true));
        FilterStyles.Add(new SymbolSearcherSearchFilter());
        foreach (var pi in Project.Current.GetItems<StyleProjectItem>())
        {
          FilterStyles.Add(new SymbolSearcherSearchFilter(false, pi));
        }
        if (SelectedFilterStyle == null && FilterStyles.Count > 0)
        {
          // select the default search type
          SelectedFilterStyle = FilterStyles[1];
        }
        SearchPauseSearching = false;
        System.Diagnostics.Debug.WriteLine($@"==== SearchPauseSearching false");
      }
      System.Diagnostics.Debug.WriteLine($@"==== SymbolPicker constructor end");

      // set the start value to line type
      SelectedSearchType = SearchTypes[3];
    }

    #region Test Prep properties

    public ObservableCollection<StyleItemType> SearchTypes
    {
      get
      {
        if (_searchTypes.Count > 0) return _searchTypes;
        foreach (StyleItemType sit in Enum.GetValues(typeof(StyleItemType)))
        {
          if (sit == 0)
            continue;
          _searchTypes.Add(sit);
        }
        SelectedSearchType = _searchTypes.FirstOrDefault();
        return _searchTypes;
      }
    }

    public StyleItemType SelectedSearchType
    {
      get { return _selectedSearchType; }
      set
      {
        SetProperty(ref _selectedSearchType, value, () => SelectedSearchType);
        AddStatus($@"SelectedStyleItemType: {_selectedSearchType}");
        System.Diagnostics.Debug.WriteLine($@"==== SelectedSearchType {_selectedSearchType}");
        SearchFilterType = _selectedSearchType;
      }
    }

    /// <summary>
    /// List of project styles from which to select the desired project style
    /// </summary>
    public ObservableCollection<SymbolSearcherSearchFilter> FilterStyles
    {
      get
      {
        return _filterStyleItems;
      }
      set
      {
        SetProperty(ref _filterStyleItems, value, () => FilterStyles);
      }
    }

    /// <summary>
    /// The selected StyleProjectItem that is used for the search
    /// </summary>
    public SymbolSearcherSearchFilter SelectedFilterStyle
    {
      get
      {
        return _selectedFilterStyle;
      }
      set
      {
        SetProperty(ref _selectedFilterStyle, value, () => SelectedFilterStyle);
        AddStatus($@"SelectedSymbolPickerSearchFilter: {_selectedFilterStyle?.Name}");
        System.Diagnostics.Debug.WriteLine($@"==== SelectedFilterStyle {_selectedFilterStyle?.Name}");
        SearchFilterStyle = _selectedFilterStyle;
      }
    }

    public ObservableCollection<MapMember> LayersInMap
    {
      get
      {
        return _layersInMap;
      }
      set
      {
        SetProperty(ref _layersInMap, value, () => LayersInMap);
      }
    }

    /// <summary>
    /// The selected layer
    /// </summary>
    public MapMember SelectedLayer
    {
      get { return _selectedLayer; }
      set
      {
        SetProperty(ref _selectedLayer, value, () => SelectedLayer);
        _ = GetStyleItemForLayerAsync(SelectedLayer);
      }
    }

    #endregion Test Prep properties

    #region SymbolControl properties

    public SymbolSearcherSearchOutputOptions SearchOutputOptions =>
      new SymbolSearcherSearchOutputOptions()
      {
        FitSizeForPointSymbol = true,
        DefaultPatchTypes = _patchTypes
      };

    public StyleItemType SearchFilterType
    {
      get { return _searchFilterType; }
      set
      {
        SetProperty(ref _searchFilterType, value, () => SearchFilterType);
      }
    }

    /// <summary>
    /// SearchPauseSearching is used to 'pause' search in case multiple searcher control properties are updated
    /// </summary>
    public bool SearchPauseSearching
    {
      internal get
      {
        return _searchPauseSearching;
      }
      set
      {
        SetProperty(ref _searchPauseSearching, value, () => SearchPauseSearching);

        System.Diagnostics.Debug.WriteLine($@"==== SearchPauseSearching setter: {_searchPauseSearching}");
      }
    }

    /// <summary>
    /// The selected StyleProjectItem that is used for the search
    /// </summary>
    public SymbolSearcherSearchFilter SearchFilterStyle
    {
      get
      {
        return _searchFilterStyle;
      }
      set
      {
        SetProperty(ref _searchFilterStyle, value, () => SearchFilterStyle);
      }
    }

    /// <summary>
    /// Selected Picker Style Item to be used
    /// </summary>
    public StyleItem SelectedPickerStyleItem
    {
      get
      {
        return _selectedPickerStyleItem;
      }
      set
      {
        SetProperty(ref _selectedPickerStyleItem, value, () => SelectedPickerStyleItem);
        AddStatus($@"SelectedPickerStyleItem: {_selectedPickerStyleItem?.Name}");
      }
    }

    #endregion SymbolControl properties

    #region Symbol Control Testing properties

    /// <summary>
    /// Pick the style from a style input file
    /// </summary>
    public ICommand PickStyleXFileCommand
    {
      get
      {
        return new RelayCommand(async () =>
        {
          //Create instance of BrowseProjectFilter using the id for Pro's StyleX filter
          BrowseProjectFilter bf = new BrowseProjectFilter("esri_browseDialogFilters_styleFiles");
          //Display the filter in an Open Item dialog
          OpenItemDialog styleXOpenDlg = new OpenItemDialog
          {
            Title = "Open StyleX files",
            InitialLocation = @"C:\Data",
            MultiSelect = false,
            BrowseFilter = bf
          };
          bool? ok = styleXOpenDlg.ShowDialog();
          if (!ok.HasValue || !ok.Value) return;
          // Full path for the new style file(.stylx) to be created
          string styleToCreate = styleXOpenDlg.Items.FirstOrDefault().Path;
          if (styleToCreate == null) return;
          // will add this to the current project 
          await QueuedTask.Run(() => StyleHelper.CreateStyle(Project.Current, styleToCreate));
          // var stylxProjectItem = Project.Current.GetItems<StyleProjectItem>().FirstOrDefault(i => i.Path == styleToCreate);
        }, true);
      }
    }

    /// <summary>
    /// Apply the select style item to the layer (only if a match is there)
    /// </summary>
    public ICommand ApplySymbologyCommand
    {
      get
      {
        return new RelayCommand(() =>
        {
          if (SelectedLayer is FeatureLayer)
          {
            var featureLayer = SelectedLayer as FeatureLayer;
            QueuedTask.Run(() =>
            {
              if (!(featureLayer.GetRenderer() is CIMSimpleRenderer renderer)) return;
              if (!(SelectedPickerStyleItem.GetObject() is CIMSymbol symbol)) return;
              // Set symbol's real world setting to be the same as that of the feature layer
              symbol.SetRealWorldUnits(featureLayer.UsesRealWorldSymbolSizes);
              renderer.Symbol = symbol.MakeSymbolReference();
              featureLayer.SetRenderer(renderer);
            });
          }
        }, IsSymbolCompatibleWithLayer);
      }
    }

    public ICommand CmdSetSelectedFilterStyleItem
    {
      get
      {
        return new RelayCommand(() => {
          // set selections to second elements
          if (FilterStyles.Count > 0)
          {
            if (FilterStyles.Count <= _selectedFilterStyleItemIdx) _selectedFilterStyleItemIdx = 0;
            SelectedFilterStyle = FilterStyles[_selectedFilterStyleItemIdx++];
          }
        }, true);
      }
    }

    public ICommand CmdTest
    {
      get
      {
        return new RelayCommand(() =>
       {
         SearchPauseSearching = !SearchPauseSearching;
       }, true);
      }
    }
    private void AddStatus (string addText)
    {
      if (_statusLines.Count > 5) _statusLines.RemoveAt(0);
      _statusLines.Add(addText);
      Status = string.Join(Environment.NewLine, _statusLines);
    }

    /// <summary>
    /// Status output
    /// </summary>
    public string Status
    {
      get { return _status; }
      set
      {
        SetProperty(ref _status, value, () => Status);
      }
    }

    #endregion Testing properties

    #region Helpers

    private bool IsSymbolCompatibleWithLayer()
    {
      var isCompatible = false;
      if (!(SelectedLayer is FeatureLayer featureLayer)) return false;
      if (SelectedPickerStyleItem == null) return false;
      switch (featureLayer.ShapeType)
      {
        case esriGeometryType.esriGeometryPoint:
          if (SelectedPickerStyleItem.ItemType == StyleItemType.PointSymbol)
            isCompatible = true;
          break;
        case esriGeometryType.esriGeometryMultipoint:
          if (SelectedPickerStyleItem.ItemType == StyleItemType.PointSymbol)
            isCompatible = true;
          break;
        case esriGeometryType.esriGeometryPolyline:
          if (SelectedPickerStyleItem.ItemType == StyleItemType.LineSymbol)
            isCompatible = true;
          break;
        case esriGeometryType.esriGeometryLine:
          if (SelectedPickerStyleItem.ItemType == StyleItemType.LineSymbol)
            isCompatible = true;
          break;
        case esriGeometryType.esriGeometryPolygon:
          if (SelectedPickerStyleItem.ItemType == StyleItemType.PolygonSymbol)
            isCompatible = true;
          break;
        default:
          isCompatible = false;
          break;
      }
      return isCompatible;
    }

    private void OnProjectClosed(ProjectEventArgs obj)
    {
      ActionOnGuiThread(() => {
        FilterStyles.Clear();
      });      
    }

    private void OnProjectItemsChanged(ProjectItemsChangedEventArgs obj)
    {
      if (!(obj.ProjectItem is StyleProjectItem pi)) return;
      var getStyleProjectItems = (Action)(() =>
      {
        if (FilterStyles.Count == 0)
        {
          FilterStyles.Add(new SymbolSearcherSearchFilter(true));
          FilterStyles.Add(new SymbolSearcherSearchFilter());
        }
        FilterStyles.Add(new SymbolSearcherSearchFilter(false, pi));
        if (SelectedFilterStyle == null && FilterStyles.Count > 0)
        {
          // select the default search type
          SelectedFilterStyle = FilterStyles[1];
          SearchPauseSearching = false;
          System.Diagnostics.Debug.WriteLine($@"==== SearchPauseSearching false OnProjectItemsChanged");
        }
      });
      ActionOnGuiThread(getStyleProjectItems);
    }

    private ObservableCollection<SymbolSearcherSearchFilter> GetSymbolPickerSearchFilters()
    {
      var SymbolSearcherSearchFilters = new ObservableCollection<SymbolSearcherSearchFilter>
      {
        new SymbolSearcherSearchFilter(true),
        new SymbolSearcherSearchFilter()
      };
      foreach (var styleProjItem in Project.Current.GetItems<StyleProjectItem>())
      {
        SymbolSearcherSearchFilters.Add(new SymbolSearcherSearchFilter(false, styleProjItem));
      }
      return SymbolSearcherSearchFilters;
    }

    private void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs obj)
    {
      if (obj.IncomingView == null) return;
      ActionOnGuiThread(async () =>
      {
        await GetLayersInMapAsync();
      });
    }

    private async Task GetLayersInMapAsync()
    {
      if (MapView.Active?.Map == null) return;
      var lyrs = MapView.Active.Map.GetLayersAsFlattenedList();
      //Add GraphicsLayers and Feature Layers with Simple renderer to the collection
      lock (_layersInMapLock)
      {
        _layersInMap.Clear();
        foreach (var lyr in lyrs)
        {
          if (lyr is GraphicsLayer)
            _layersInMap.Add(lyr);
          if (lyr is FeatureLayer)
          {
            var featureLayer = lyr as FeatureLayer;
            QueuedTask.Run(() =>
            {
              if (featureLayer.GetRenderer() is CIMSimpleRenderer)
                _layersInMap.Add(lyr);
            });
          }
        }
      }
      if (LayersInMap.Count == 0) return;
      SelectedLayer = LayersInMap[0];
      await GetStyleItemForLayerAsync(SelectedLayer);
      
    }

    /// <summary>
    /// We have to ensure that GUI updates are only done from the GUI thread.
    /// </summary>
    public void ActionOnGuiThread(Action theAction)
    {
      if (System.Windows.Application.Current.Dispatcher.CheckAccess())
      {
        //We are on the GUI thread
        theAction();
      }
      else
      {
        //Using the dispatcher to perform this action on the GUI thread.
        ProApp.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, theAction);
      }
    }

    private async Task GetStyleItemForLayerAsync(MapMember mapMember)
    {
      StyleItemType styleItemType = StyleItemType.Color;
      if (mapMember is FeatureLayer)
      {
        var featureLayer = mapMember as FeatureLayer;
        var featureShapeType = featureLayer.ShapeType;
        switch (featureShapeType)
        {
          case esriGeometryType.esriGeometryPoint:
            styleItemType = StyleItemType.PointSymbol;
            break;
          case esriGeometryType.esriGeometryMultipoint:
            styleItemType = StyleItemType.PointSymbol;
            break;
          case esriGeometryType.esriGeometryLine:
            styleItemType = StyleItemType.LineSymbol;
            break;
          case esriGeometryType.esriGeometryPolyline:
            styleItemType = StyleItemType.LineSymbol;
            break;
          case esriGeometryType.esriGeometryPolygon:
            styleItemType = StyleItemType.PolygonSymbol;
            break;
          default:
            break;
        }
        SelectedSearchType = styleItemType;
      }
      if (mapMember is GraphicsLayer)
      {
        SelectedSearchType = await GetSelectedTypeFromGraphicsLayerAsync(mapMember as GraphicsLayer);
      }
    }

    private async Task<StyleItemType> GetSelectedTypeFromGraphicsLayerAsync(GraphicsLayer graphicsLayers)
    {
      StyleItemType styleItemType = StyleItemType.PointSymbol; //default
                                                               //Get all selected graphics
      List<Element> selectedElements = graphicsLayers.GetSelectedElements().ToList();
      if (selectedElements.Count == 0) return styleItemType;
      //Convert "Element" list to "GraphicElement" list.
      var selectedGraphicElements = selectedElements.ConvertAll(x => (GraphicElement)x);
      //Get the type of the first graphic to compare against
      var firstGraphicElement = selectedElements.FirstOrDefault() as GraphicElement;
      await QueuedTask.Run(() =>
      {
        var firstGraphicElementType = firstGraphicElement.GetGraphic().GetType();
              //iterate to check if elements match
              var differentGraphicTypes = selectedGraphicElements.FirstOrDefault(se => se.GetGraphic().GetType() != firstGraphicElementType);
        if (differentGraphicTypes == null)
              //Find out what that type is
              {
          var graphicElement = selectedElements.FirstOrDefault() as GraphicElement;
          if (graphicElement.GetGraphic() is CIMPointGraphic)
            styleItemType = StyleItemType.PointSymbol;
          if (graphicElement.GetGraphic() is CIMTextGraphic)
            styleItemType = StyleItemType.TextSymbol;
          if (graphicElement.GetGraphic() is CIMLineGraphic)
            styleItemType = StyleItemType.LineSymbol;
          if (graphicElement.GetGraphic() is CIMPolygonGraphic)
            styleItemType = StyleItemType.PolygonSymbol;
        }
      });
      return styleItemType;
    }

    #endregion Helpers

    #region Manage Dockpane

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
    private string _heading = "Pro Style Symbol Searcher/Picker";
    public string Heading
    {
      get { return _heading; }
      set
      {
        SetProperty(ref _heading, value, () => Heading);
      }
    }

    #endregion Manage Dockpane
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
  internal class SymbolSearcherDockpane_ShowButton : Button
  {
    protected override void OnClick()
    {
      SymbolSearcherDockpaneViewModel.Show();
    }
  }
}
