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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EVChargers
{
  /// <summary>
  /// This sample uses an embeddable control on a Mapview to allow advanced filter search of USA and Canada Electrical vehicle charger locations. You can search locations, charger and connector types.
  /// The results will be displayed in a dockpane.
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data).  The sample data contains a Pro package called 'LocationOfEVChargers' in c:\Data\DisplayFilters.  
  /// 1. Open this sample in Visual Studio.  
  /// 1. Click the build menu and select Build Solution.
  /// 1. Launch the debugger to open ArcGIS Pro.
  /// 1. Open the project "c:\Data\DisplayFilters\LocationOfEVChargers" Pro project package.
  /// 1. Open and activate the EVLocations map. This map has a EVChargers feature layer. It is a point dataset with over 57,000 charger locations in the US and Canada.
  /// 1. Click on the Add-in tab. 
  /// 1. Click the 'Charger locations' button. This will activate an embeddable control that stretches to fit the width of the active MapView. This control has search filters to interact with the charger locations.
  /// ![UI](screenshots/search-filter.png)
  /// 1. Enter a location (Ex. Redlands), and pick a charger (Level 1, Level 2, Fast) or Connector type (Tesla, J1772, etc) you are interested in. Or select all of them.
  /// 1. Click the Apply button. This will set a definition query on the feature layer using your filter options. The charger locations that satisfy your filter options will be displayed and labelled.
  /// 1. The map view will zoom to the selected results. A dockpane will also appear in Pro that itemizes the results. Each result item will show more details about the charger location - Address, Charger type, connector type, etc.
  /// ![UI](screenshots/search-results.png)
  /// 1. Click on each item in the dockpane to zoom and flash to the feature on the map view.
  /// 1. Additionally, you can use the cursor to select a charger locations on the Map view. This will also select the item in the results dockpane.
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;
    /// <summary>
    /// Link to Dockpane view model which allows to update properties of dockpane
    /// </summary>
    internal static DisplayResultsViewModel DisplayResultsVM { get; set; }
    public FeatureLayer EVChargersFeatureLayer = null;
    public static List<EVChargerLocationItem> EVChargerLocationItems = new List<EVChargerLocationItem>();
    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("EVChargers_Module");
    public EVChargersUIViewModel EVViewModel;
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
