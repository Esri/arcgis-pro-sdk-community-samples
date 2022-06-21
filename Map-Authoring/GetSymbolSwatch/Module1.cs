/*

   Copyright 2019 Esri

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

namespace GetSymbolSwatch
{
  /// <summary>
  /// This sample shows how to create the swatches for different types of Symbology for use in WPF
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
  /// 1. Make sure that the Sample data is unzipped in c:\data 
  /// 1. The projects used for this sample are 'C:\Data\FeatureTest\FeatureTest.aprx', 'C:\Data\Interacting with Maps\Interacting with Maps.aprx', and 'C:\Data\Admin\AdminSample.aprx'
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Launch the debugger to open ArcGIS Pro.
  /// 1. Select the FeatureTest.aprx project
  /// 1. After the Map View is displayed, click on the 'Symbol Swatch' tab, and then click the 'Show Symbol Swatch Dockpane' button to show the 'Symbol Swatch Dockpane' dockpane  
  /// 1. Position and size the dockpane to see all columns in the symbol swatch grid
  /// ![UI](Screenshots/Screenshot1.png)
  /// 1. Click the 'Refresh' button on the 'Show Symbol Swatch Dockpane' dockpane to see symbol swatches in the grid  
  /// ![UI](Screenshots/Screenshot2.png)
  /// 1. Open the 'Interacting with Maps.aprx' project
  /// 1. Click the 'Refresh' button on the 'Show Symbol Swatch Dockpane' dockpane to see symbol swatches in the grid  
  /// ![UI](Screenshots/Screenshot3.png)
  /// 1. Open the AdminSample.aprx project
  /// 1. In the 'Contents' dockpane change the symbology for the 'States' layer to 'Graduated Colors'
  /// 1. Click the 'Refresh' button on the 'Show Symbol Swatch Dockpane' dockpane to see symbol swatches in the grid  
  /// ![UI](Screenshots/Screenshot4.png)
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("GetSymbolSwatch_Module"));
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
