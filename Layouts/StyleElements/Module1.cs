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
using ArcGIS.Desktop.Core.Events;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Mapping.Symbology;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StyleElements
{
  /// <summary>
  /// This sample illustrates how apply a style to graphic elements in a MapView or layout elements in a Layout View. Graphics in a Map View can be point, line, polygon and text graphics. In the Layout view, additional graphics such as North Arrow, Scale bar, table frame and Grids and Graticule can be changed using the Pro SDK.
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio, build the solution.
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. In Pro, open any project with Map Graphics and Layout graphic elements such as north arrow, scale bar, etc.
  /// 1. Activate the map so that there is an active map view.
  /// 1. Select any graphics in the MapView using the Select tool on the Graphics tab.
  /// 1. In the Add-In tab, click the Style Elements button.
  /// ![UI](Screenshots/StyleElements.png)
  /// 1. The "Apply Styles" custom dockpane is displayed, hosting all the style items for the selected graphic. In the screen shot above, Text graphic element is selected on the map.  The custom dockpane displays all the text style items available for this selected element.
  /// 1. Click any text style in the Apply Style dockpane. The style is applied to the selected text graphic.
  /// 1. Open a layout with various layout elements such as a map frame, legend, table, scale bar, north arrow.
  /// 1. When you select any of these layout elements, the Apply Style  dockpane will update with the style items available for that selected layout element.In the screen shot below, you can see the Table Frame layout elements is selected. Notice the dockpane is updated with all the table frame styles. Selecting a table style in the dockpane will apply this style to the selected table.
  /// ![UI](Screenshots/LayoutTableFrameStyle.png)    
  /// 1. Select the North Arrow element on the layout. Notice that the dockpane is updated with all the North Arrow style items. You can change the North Arrow style in the layout by selecting any one of the displayed North Arrow styles in the Apply Style dockpane.
  /// ![UI](Screenshots/LayoutNorthArrow.png)   
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("StyleElements_Module");

    #region Collections that hold the symbol items.
    public List<GeometrySymbolItem> NorthArrowsStyleItems { get; private set; }
    public List<GeometrySymbolItem> LegendStyleItems { get; private set; }
    public List<GeometrySymbolItem> LegendItemStyleItems { get; private set; }
    public List<GeometrySymbolItem> ScaleBarsStyleItems { get; private set; }
    public List<GeometrySymbolItem> TableFrameStyleItems { get; private set; }
    public List<GeometrySymbolItem> GridStyleItems { get; private set; }
    public List<GeometrySymbolItem> TextStyleItems { get; private set; }
    public List<GeometrySymbolItem> PointStyleItems { get; private set; }
    public List<GeometrySymbolItem> LineStyleItems { get; private set; }
    public List<GeometrySymbolItem> PolygonStyleItems { get; private set; }
    #endregion
    #region Overrides
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
    protected override bool Initialize()
    {
      ArcGIS.Desktop.Core.Events.ProjectOpenedEvent.Subscribe(OnProjectOpened);
      return true;
    }
    /// <summary>
    /// Gather the Style items in the project when the Project opens. 
    /// These items are used to populate the style item gallery in the dockpane.
    /// </summary>
    /// <param name="obj"></param>
    private async void OnProjectOpened(ProjectEventArgs obj)
    {
      if (obj.Project != null)
      {
        LegendStyleItems = await GetStyleItemsAsync(StyleItemType.Legend);
        LegendItemStyleItems = await GetStyleItemsAsync(StyleItemType.LegendItem);
        NorthArrowsStyleItems = await GetStyleItemsAsync(StyleItemType.NorthArrow);
        TableFrameStyleItems = await GetStyleItemsAsync(StyleItemType.TableFrame);
        GridStyleItems = await GetStyleItemsAsync(StyleItemType.Grid);
        ScaleBarsStyleItems = await GetStyleItemsAsync(StyleItemType.ScaleBar);
        TextStyleItems = await GetStyleItemsAsync(StyleItemType.TextSymbol);
        PointStyleItems = await GetStyleItemsAsync(StyleItemType.PointSymbol);
        LineStyleItems = await GetStyleItemsAsync(StyleItemType.LineSymbol);
        PolygonStyleItems = await GetStyleItemsAsync(StyleItemType.PolygonSymbol);
      }
    }
    #endregion Overrides
    /// <summary>
    /// Helper method that returns a list of Styles for the specific  style type requested.
    /// So if you pass in StyleItemType.Point, you get all the point style items in the project.
    /// </summary>
    /// <param name="styleItemType"></param>
    /// <returns></returns>
    public static async Task<List<GeometrySymbolItem>> GetStyleItemsAsync(StyleItemType styleItemType)
    {
      //Collection to hold the symbols in the various Style project items
      var geometrySymbolStyleItems = new List<GeometrySymbolItem>();
      //Collection to hold all the StyleProjectitems
      List<StyleProjectItem> styleProjectItems = new List<StyleProjectItem>();

      await QueuedTask.Run(() =>
      {
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
            geometrySymbolStyleItems.Add(new GeometrySymbolItem(styleItem, styleItemType));
          }
        }
      });
      return geometrySymbolStyleItems;
    }
  }
}
