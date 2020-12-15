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
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Controls;
using ArcGIS.Desktop.Mapping.Events;

namespace SymbolControls
{
  internal class SymbolsViewModel : DockPane
  {
    private const string _dockPaneID = "SymbolControls_Symbols";
    private Dictionary<StyleItemType, SymbolPatchType> _patchTypes = null;
    private static readonly object _layersInMapLock = new object();
    private bool _isAllGraphicsSameType;
    protected SymbolsViewModel() {
      SearchPauseSearching = true;
      #region Events
      //Subscribe to ProjectItemsChangedEvent to update ProjecyStyles
      ProjectItemsChangedEvent.Subscribe(OnProjectItemsChanged);
      ProjectClosedEvent.Subscribe(OnProjectClosed);
      //Subscribe to ActiveMapViewChangedEvent in order to get the layers in the map
      ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);
      LayersAddedEvent.Subscribe(OnLayersAdded);
      LayersRemovedEvent.Subscribe(OnLayersemoved);
      //Subscribe to Graphic Elements selection changed event
      ElementSelectionChangedEvent.Subscribe(OnGraphicsElementSelectionChanged);
      #endregion
      BindingOperations.EnableCollectionSynchronization(_layersInMap, _layersInMapLock);
      _ = GetLayersInMapAsync();      
      _patchTypes = new Dictionary<StyleItemType, SymbolPatchType>
            {
                { StyleItemType.LineSymbol, SymbolPatchType.ZigzagLine },
                { StyleItemType.PolygonSymbol, SymbolPatchType.BoundaryPoly }
            };
      //Get the Styles in the project
      ProjectFilterStyles.Add(new SymbolSearcherSearchFilter(true));
      ProjectFilterStyles.Add(new SymbolSearcherSearchFilter());
      foreach (var pi in Project.Current.GetItems<StyleProjectItem>())
      {
        ProjectFilterStyles.Add(new SymbolSearcherSearchFilter(false, pi));
      }
      if (SelectedProjectFilterStyle == null && ProjectFilterStyles.Count > 0)
      {
        SelectedProjectFilterStyle = ProjectFilterStyles[0];
      }
      //Get the various StyleItemTypes in Pro.
      foreach (StyleItemType sit in Enum.GetValues(typeof(StyleItemType)))
      {
        if (sit == 0)
          continue;
        StyleItemTypeValues.Add(sit);
      }
      SelectedStyleItemType = StyleItemTypeValues.FirstOrDefault();
      SearchPauseSearching = false;
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
    #region Supporting properties for UI controls

    /// <summary>
    /// Text shown near the top of the DockPane.
    /// </summary>
    private string _heading = "View and Apply Symbols";
    public string Heading
    {
      get { return _heading; }
      set
      {
        SetProperty(ref _heading, value, () => Heading);
      }
    }
    //Collection of Style item types: Points, Line, Polygon, Text, etc
    private ObservableCollection<StyleItemType> _styleItemTypes = new ObservableCollection<StyleItemType>();
    public ObservableCollection<StyleItemType> StyleItemTypeValues
    {
      get
      {        
        return _styleItemTypes;
      }
    }

    private StyleItemType _selectedStyleItemType;
    public StyleItemType SelectedStyleItemType
    {
      get { return _selectedStyleItemType; }
      set
      {
        SetProperty(ref _selectedStyleItemType, value, () => SelectedStyleItemType);
        SearchFilterType = _selectedStyleItemType;
      }
    }
    private ObservableCollection<MapMember> _layersInMap = new ObservableCollection<MapMember>();

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
    private MapMember _selectedLayer;
    public MapMember SelectedLayer
    {

      get { return _selectedLayer; }
      set
      {
        SetProperty(ref _selectedLayer, value, () => SelectedLayer);
        _ = GetStyleItemForLayerAsync(SelectedLayer);
      }
    }

    //project styles, All Styles, specific StyleX added to projects....
    private ObservableCollection<SymbolSearcherSearchFilter> _projectFilterStyles = new ObservableCollection<SymbolSearcherSearchFilter>();
    public ObservableCollection<SymbolSearcherSearchFilter> ProjectFilterStyles
    {
      get { return _projectFilterStyles; }
      set
      {
        SetProperty(ref _projectFilterStyles, value, () => ProjectFilterStyles);
      }
    }

    private SymbolSearcherSearchFilter _selectedProjectFilterStyle;
    public SymbolSearcherSearchFilter SelectedProjectFilterStyle
    {
      get { return _selectedProjectFilterStyle; }
      set
      {
        SetProperty(ref _selectedProjectFilterStyle, value, () => SelectedProjectFilterStyle);
        SearchFilterStyle = _selectedProjectFilterStyle;
      }
    }
    #endregion
    #region Symbol Control Properties
    public SymbolSearcherSearchOutputOptions SearchOutputOptions =>
      new SymbolSearcherSearchOutputOptions()
      {
        FitSizeForPointSymbol = true,
        DefaultPatchTypes = _patchTypes
      };

    private StyleItemType _searchFilterType;
    /// <summary>
    /// Points, Line, Polygon, color, etc.
    /// </summary>
    public StyleItemType SearchFilterType
    {
      get { return _searchFilterType; }
      set
      {
        SetProperty(ref _searchFilterType, value, () => SearchFilterType);
      }
    }
    private SymbolSearcherSearchFilter _searchFilterStyle;
    /// <summary>
    /// Styles in the projects
    /// </summary>
    public SymbolSearcherSearchFilter SearchFilterStyle
    {
      get { return _searchFilterStyle; }
      set
      {
        SetProperty(ref _searchFilterStyle, value, () => SearchFilterStyle);
      }
    }
    private StyleItem _selectedPickerStyleItem;
    /// <summary>
    /// The selected style item in the Symbol control
    /// </summary>
    public StyleItem SelectedPickerStyleItem
    {
      get { return _selectedPickerStyleItem; }
      set { SetProperty(ref _selectedPickerStyleItem, value, () => SelectedPickerStyleItem); }
    }
    private bool _searchPauseSearching;
    /// <summary>
    /// SearchPauseSearching is used to 'pause' search in case multiple searcher control properties are updated
    /// </summary>
    public bool SearchPauseSearching
    {
      get
      {
        return _searchPauseSearching;
      }
      set
      {
        SetProperty(ref _searchPauseSearching, value, () => SearchPauseSearching);
      }
    }
    #endregion
    #region Helper Methods
    private async Task GetLayersInMapAsync()
    {
      System.Diagnostics.Debug.WriteLine($"MapView name: {MapView.Active?.Map.Name}");
      if (MapView.Active?.Map == null) return;
      var lyrs = MapView.Active.Map.GetLayersAsFlattenedList();
      //Add GraphicsLayers and Feature Layers with Simple renderer to the collection

      lock (_layersInMapLock)
        _layersInMap.Clear();
      foreach (var lyr in lyrs)
      {
        if (lyr is GraphicsLayer)
          lock (_layersInMapLock)
            _layersInMap.Add(lyr);
        if (lyr is FeatureLayer)
        {
          var featureLayer = lyr as FeatureLayer;
          await QueuedTask.Run(() =>
          {
            if (featureLayer.GetRenderer() is CIMSimpleRenderer)
              lock (_layersInMapLock)
                _layersInMap.Add(lyr);
          });
        }
      }
      if (LayersInMap.Count == 0) return;
      SelectedLayer = LayersInMap[0];
      //Get the "Style item Type" that is relevant to the select layer.
      //For example if the layer is Point feature class..
      await GetStyleItemForLayerAsync(SelectedLayer);
    }
    private async Task GetStyleItemForLayerAsync(MapMember mapMember)
    {
      StyleItemType styleItemType = StyleItemType.Color;
      //Selected layer is a FeatureLayer
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
        SelectedStyleItemType = styleItemType;
      }
      //Selected Layer is a GraphicsLayer
      if (mapMember is GraphicsLayer)
      {
        await GetStyleItemForGraphicsLayerAsync(mapMember as GraphicsLayer);
      }
    }
    private async Task GetStyleItemForGraphicsLayerAsync(GraphicsLayer graphicsLayer)
    {
      //Get all selected graphics
      List<Element> selectedElements = graphicsLayer.GetSelectedElements().ToList();
      if (selectedElements.Count == 0) return;
      if (!_isAllGraphicsSameType) return;
    
      //All graphics selected are the same now.
      //Convert "Element" list to "GraphicElement" list.
      var selectedGraphicElements = selectedElements.ConvertAll(x => (GraphicElement)x);
      //Get the type of the first graphic to compare against
      await QueuedTask.Run( () => { 
        var firstGraphicElement = selectedGraphicElements.FirstOrDefault().GetGraphic();
        if (firstGraphicElement is CIMPointGraphic)
          SelectedStyleItemType = StyleItemType.PointSymbol;
        if (firstGraphicElement is CIMTextGraphic)
          SelectedStyleItemType = StyleItemType.TextSymbol;
        if (firstGraphicElement is CIMLineGraphic)
          SelectedStyleItemType = StyleItemType.LineSymbol;
        if (firstGraphicElement is CIMPolygonGraphic)
          SelectedStyleItemType = StyleItemType.PolygonSymbol;
      }); 
    }   
    
    private bool IsSymbolCompatibleWithLayer()
    {
      var isCompatible = false;
      if (SelectedLayer is GraphicsLayer)
      {
        var gl = SelectedLayer as GraphicsLayer;
        //Get all selected graphics
        List<Element> selectedElements = gl.GetSelectedElements().ToList();
        if (selectedElements.Count == 0) return false;
        //Are they all the same type of graphic?
        return _isAllGraphicsSameType;
        ;
      }
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
    #endregion
    #region Commands
    /// <summary>
    /// Apply the select style item to the layer (only if a match is there)
    /// </summary>
    public ICommand CmdApplySymbology
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
          if (SelectedLayer is GraphicsLayer)
          {
            var gl = SelectedLayer as GraphicsLayer;
            QueuedTask.Run( () => {
              if (!(SelectedPickerStyleItem.GetObject() is CIMSymbol symbol)) return;
              //all elements should be of the same type.
              foreach (var e in gl.GetSelectedElements())
              {
                GraphicElement ge = e as GraphicElement;
                CIMGraphic graphic = ge.GetGraphic();
                graphic.Symbol = symbol.MakeSymbolReference();
                ge.SetGraphic(graphic);
              }
            });
          }
        }, IsSymbolCompatibleWithLayer);
      }
    }
    /// <summary>
    /// Pick the style from a style input file
    /// </summary>
    public ICommand CmdPickStyleXFile
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
    #endregion
    #region Events Handlers
    private void OnProjectClosed(ProjectEventArgs obj)
    {
      ActionOnGuiThread(() => {
        ProjectFilterStyles.Clear();
      });
    }
    private void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs obj)
    {
      if (obj.IncomingView == null) return;
      System.Diagnostics.Debug.WriteLine("OnActiveMapViewChanged");
      ActionOnGuiThread(async () =>
      {
        await GetLayersInMapAsync();
      });
    }
    private void OnProjectItemsChanged(ProjectItemsChangedEventArgs obj)
    {
      if (!(obj.ProjectItem is StyleProjectItem pi)) return;
      var getStyleProjectItems = (Action)(() =>
      {
        if (ProjectFilterStyles.Count == 0)
        {
          ProjectFilterStyles.Add(new SymbolSearcherSearchFilter(true));
          ProjectFilterStyles.Add(new SymbolSearcherSearchFilter());
        }
        ProjectFilterStyles.Add(new SymbolSearcherSearchFilter(false, pi));
        if (SelectedProjectFilterStyle == null && ProjectFilterStyles.Count > 0)
        {
          // select the default search type
          SelectedProjectFilterStyle = ProjectFilterStyles[0];
          SearchPauseSearching = false;
        }
      });
      ActionOnGuiThread(getStyleProjectItems);
    }
    private void OnLayersemoved(LayerEventsArgs obj)
    {
      if (obj.Layers.Count() == 0) return;
      System.Diagnostics.Debug.WriteLine("OnActiveMapViewChanged");
      ActionOnGuiThread(async () =>
      {
        await GetLayersInMapAsync();
      });
    }

    private void OnLayersAdded(LayerEventsArgs obj)
    {
      if (obj.Layers.Count() == 0) return;
      System.Diagnostics.Debug.WriteLine("OnActiveMapViewChanged");
      ActionOnGuiThread(async () =>
      {
        await GetLayersInMapAsync();
      });
    }
    private async void OnGraphicsElementSelectionChanged(ElementSelectionChangedEventArgs obj)
    {
      if (!(SelectedLayer is GraphicsLayer)) return;
      //check the container is a graphics layer - could be a Layout (or even map view)
      if (obj.ElementContainer is GraphicsLayer graphicsLayer)
      {
        //get the total selection count for the container
        var count = obj.SelectedElementCount;
        //Check count - could have been an unselect or clearselect
        if (count > 0)
        {
          //this is a selection or add to selection
          var selectedElements = graphicsLayer.GetSelectedElements().ToList();
          var selectedGraphicElements = selectedElements.ConvertAll(x => (GraphicElement)x);
          //Get the first graphic to compare against
          var firstGraphicElement = selectedGraphicElements.FirstOrDefault();
         await QueuedTask.Run(() =>
          {
            //Get type of the first element
            var firstGraphicElementType = firstGraphicElement.GetGraphic().GetType(); //This needs to run on MCT.
            //iterate to check if elements match in the selectedElements collection
            var differentGraphicTypes = selectedGraphicElements.FirstOrDefault(se => se.GetGraphic().GetType() != firstGraphicElementType);
            _isAllGraphicsSameType = differentGraphicTypes == null ? true : false;
            System.Diagnostics.Debug.WriteLine($"No. of graphics selected: {count} Are they the same type? {_isAllGraphicsSameType}");
          });
          await GetStyleItemForGraphicsLayerAsync(SelectedLayer as GraphicsLayer);
        }
      }
    }
    #endregion
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class Symbols_ShowButton : Button
  {
    protected override void OnClick()
    {
      SymbolsViewModel.Show();
    }
  }
}
