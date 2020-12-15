using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Threading.Tasks;
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
using ArcGIS.Desktop.Mapping.Events;

namespace SymbolControls
{
  /// <summary>
  /// This sample demonstrate how to work with the SymbolPicker and SymbolSearcher controls.
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu. Then select Build Solution. 
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open. 
  /// 1. Open any ArcGIS Pro project that contains feature layers and graphics layers. Activate the map view with the feature layers.
  /// 1. In the Add-In tab, click the Show Symbols button. This will display the "View and Apply Symbols" dockpane.
  /// 1. In the Style Item Types combo box, you can pick Points, Line, Polygon style item types to search and view.
  /// 1. In the Symbol Searcher control, you can add Search strings such as circle, ship, etc. The style items with the search string in their names will be displayed in the Symbol Picker control below.
  /// 1. By default, the Symbol Controls allow you to display the symbols in the entire project. Filter options available are: All Styles, Project Styles,ArcGIS 2D, ArcGIS 3D, ArcGIS Colors, etc. Using the combo box to the right of the Symbol Searcher control, you can set this filter.
  /// 1. Using the "Pick StyleX File" button, you can add your own .stylX file to the project and view style items in it.
  /// 1. Using this custom dockpane, you can change the symbology for any of the layers in the active map.
  /// #### Change Feature Layer Symbology
  /// 1. In the Select Layer combo box, pick any feature layer that is rendered with a "Single Symbol".
  /// 1. The "Style Item Type" combo box will automatically select the shape type of the selected feature layer.  The Symbol picker control will display all the symbols for that style item type.
  /// 1. Select a symbol in the symbol picker control.
  /// 1. Click the Apply Symbology button to apply the selected symbol to the feature layer.
  /// #### Change Graphic elements Symbology
  /// 1. In the Select Layer combo box, pick any GraphicsLayer in the Active map.
  /// 1. Using the Graphics selection tool, select any graphics. Note: Selected Graphics need to have the same shape type. So you have to select all point graphics, for example.
  /// 1. The "Style Item Type" combo box will automatically select the shape type of the selected graphics in the Graphics layer.  The Symbol picker control will display all the symbols for that style item type.
  /// 1. Select a symbol in the symbol picker control.
  /// 1. Click the Apply Symbology button to apply the selected symbol to the selected graphics.
  /// ![UI](screenshots/SymbolViewer.png)  
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("SymbolControls_Module"));
      }
    }


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
   
    #endregion Overrides

  }
}
