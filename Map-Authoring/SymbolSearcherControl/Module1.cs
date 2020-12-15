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
using ArcGIS.Desktop.Mapping;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace SymbolSearcherControl
{
  /// <summary>
  /// SymbolSearcherControl exercises both the SymbolSearcherControl and the SymbolPickerControl controls.  Both controls can be used to provide a Pro like user interface experience when searching and selecting symbology and/or as standalone controls.
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
  /// 1. Open this solution in Visual Studio.  
  /// 1. Click the build menu and select Build Solution.  
  /// 1. Click the Start button to open ArCGIS Pro. ArcGIS Pro will open.    
  /// 1. Open the "C:\Data\FeatureTest\FeatureTest.aprx" project.  
  /// 1. The Add-in tab contains four buttons to exercise the SymbolSearchControl and the SymbolPickerControl controls in four dockpanes:
  /// 1. The "Pro Symbol Searcher" shows a Pro like user interface that combines symbol searcher and symbol picker:
  /// ![UI](Screenshots/Screen1.png)
  /// 1. The "Pro Symbol Searcher" shows a simplified user interface that combines symbol searcher and symbol picker by using default settings:
  /// ![UI](Screenshots/Screen2.png)
  /// 1. The "Simple Symbol Searcher" shows a user interface that uses the symbol searcher control and a listbox for symbol selection:
  /// ![UI](Screenshots/Screen3.png)
  /// 1. The "Show Symbol Picker" shows a user interface that contains the symbol picker as a standalone control:
  /// ![UI](Screenshots/Screen4.png)
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("SymbolPickerControl_Module"));
      }
    }
    protected override bool Initialize()
    {
      return true;
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
