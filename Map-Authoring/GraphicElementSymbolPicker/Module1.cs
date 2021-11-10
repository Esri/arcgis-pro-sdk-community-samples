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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GraphicElementSymbolPicker
{
  /// <summary>
  /// This sample demonstrates creating graphic elements in a map and a layout.
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Click Start button to open ArcGIS Pro. 
  /// 1. ArcGIS Pro will open. 
  /// 1. Open any project that contains a Map with a graphics layer and/or layout.
  /// 1. You will need a Graphics Layer in the map to work with this sample.  If the map doesn't have a Graphics Layer, click the Map tab and use the Add Graphics layer button to insert a new layer in the map.
  /// 1. In the Graphic tab on the Pro ribbon, this add-in adds a new "Symbol Picker" group.
  ///       ![UI](screenshots/SymbolPicker.png)
  /// 1. In the graphic elements creation tool gallery, activate the point, line, polygon or a text graphic element creation tool in order to create graphics elements.
  /// 1. In the Symbol selector gallery, select the point, line, polygon or text symbol you want to use.
  ///       ![UI](screenshots/SymbolSelector.png)
  /// 1. Using the activated tool, sketch the point, line, polygon or text. The graphics elements are created using the selected symbol.
  /// 1. Open a Layout view.  
  /// 1. The Insert tab on the ribbon includes the same Symbol Picker group with the Tool and symbol gallery.
  /// 1. These tools can also be used to add Graphic Elements to the layout view.
  ///       ![UI](screenshots/LayoutGraphicElement.png)
  /// 1. To create multiple graphic elements one after the other using the same active tool, you can click the "Keep Last Tool active" button.
  ///       ![UI](screenshots/keepLastToolActive.png)
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current
    {
      get
      {
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("GraphicElementSymbolPicker_Module"));
      }
    }
    private static CIMSymbol _selectedSymbol;
    public static CIMSymbol SelectedSymbol { 
      get
      {
          return _selectedSymbol;
      }
      internal set {
        _selectedSymbol = value;
      }
    }
    //Symbol gallery items: Points, lines, polys and text
    public static ObservableCollection<object> GallerySymbolItems = new ObservableCollection<object>();

    public static List<GeometrySymbolItem> PointGallerySymbolItems = new List<GeometrySymbolItem>();
    public static List<GeometrySymbolItem> LineGallerySymbolItems = new List<GeometrySymbolItem>();
    public static List<GeometrySymbolItem> PolygonGallerySymbolItems = new List<GeometrySymbolItem>();
    public static List<GeometrySymbolItem> TextGallerySymbolItems = new List<GeometrySymbolItem>();

    //Tool gallery items
    public static ObservableCollection<object> GalleryElementToolItems = new ObservableCollection<object>();

    //private static ToolType _activeGeometry;
    public static ToolType ActiveToolGeometry = ToolType.Point;

    private LayoutOptions _layoutOptions;
    public LayoutOptions LayoutOptions
    {
      get { return ApplicationOptions.LayoutOptions; }
      set
      {
        _layoutOptions = value;
      }
    }
    public enum ToolType
    {
      Point,
      Line,
      Polygon,
      Text,
      None
    }
    #region Overrides
    protected  override bool Initialize()
    {
      LayoutOptions = ApplicationOptions.LayoutOptions;
      System.Diagnostics.Debug.WriteLine($"LayoutOptions.KeepLastToolActive: {LayoutOptions.KeepLastToolActive}");
      ArcGIS.Desktop.Core.Events.ProjectOpenedEvent.Subscribe(OnProjectOpened);
      //Gather the tool items from the DAML
      foreach (var component in Categories.GetComponentElements("GraphicsElementTools_Category"))
      {
        try
        {
          var content = component.GetContent();
          //This flavor (off component) returns empty string
          //if the attribute is not there
          var group = component.ReadAttribute("group") ?? "";
          var name = component.ReadAttribute("name") ?? "";
          //check we get a plugin
          var plugin = FrameworkApplication.GetPlugInWrapper(component.ID);
          if (plugin != null)
          {
            GalleryElementToolItems.Add(new ElementCreationToolGalleryItem(component.ID, group, plugin, name));
          }
        }
        catch (Exception e)
        {
          string x = e.Message;
        }
      }        
      return base.Initialize();
    }
    /// <summary>
    /// When the project opens, gather all the symbols. This will save time when you toggle between the tools on the gallery
    /// </summary>
    /// <param name="obj"></param>
    private async void OnProjectOpened(ProjectEventArgs obj)
    {
      if (obj.Project == null) return;
      PointGallerySymbolItems = await GetStyleItemsAsync(StyleItemType.PointSymbol);
      System.Diagnostics.Debug.WriteLine($"No of point symbols: {PointGallerySymbolItems.Count}");

      LineGallerySymbolItems = await GetStyleItemsAsync(StyleItemType.LineSymbol);
      System.Diagnostics.Debug.WriteLine($"No of line symbols: {LineGallerySymbolItems.Count}");
      PolygonGallerySymbolItems = await GetStyleItemsAsync(StyleItemType.PolygonSymbol);

      System.Diagnostics.Debug.WriteLine($"No of Polygon symbols: {PolygonGallerySymbolItems.Count}");
      TextGallerySymbolItems = await GetStyleItemsAsync(StyleItemType.TextSymbol);
      System.Diagnostics.Debug.WriteLine($"No of Text symbols: {TextGallerySymbolItems.Count}");

      //Point symbols are default. Populate GallerySymbolItems collection with Points to begin with.
      foreach (var ptSymbol in PointGallerySymbolItems)
      {
        GallerySymbolItems.Add(ptSymbol);
      }
    }

    /// <summary>
    /// Called by Framework when ArcGIS Pro is closing
    /// </summary>
    /// <returns>False to prevent Pro from closing, otherwise True</returns>
    protected override bool CanUnload()
    {
      //TODO - add your business logic
      //return false to ~cancel~ Application close
      return true;
    }

    #endregion Overrides

    public static async Task<List<GeometrySymbolItem>> GetStyleItemsAsync(StyleItemType styleItemType)
    {
      //Collection to hold the symbols in the various Style project items
      var geometrySymbolStyleItems = new List<GeometrySymbolItem>();
      //Collection to hold all the StyleProjectitems
      List<StyleProjectItem> styleProjectItems = new List<StyleProjectItem>();
      
      await QueuedTask.Run(() => {
        //First get the Favorite style
        var containerStyle = Project.Current.GetProjectItemContainer("Style");
        styleProjectItems.Add(containerStyle.GetItems().OfType<StyleProjectItem>().First(item => item.TypeID == "personal_style"));
        //All other styles like 2D, 3D etc
        styleProjectItems.AddRange(Project.Current?.GetItems<StyleProjectItem>());
        //Search for specific symbols and add them to geometrySymbolStyleItems collection
        foreach (var styleProjectitem in styleProjectItems)
        {
          foreach (var styleItem in styleProjectitem.SearchSymbols(styleItemType, ""))
          {
            geometrySymbolStyleItems.Add(new GeometrySymbolItem(styleItem, styleProjectitem.Name));
          }
        }  
      });
      return geometrySymbolStyleItems;
    }
  }
}
