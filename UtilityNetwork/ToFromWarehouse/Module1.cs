//
//   Copyright 2019 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
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

namespace ToFromWarehouse
{
  /// <summary>
  /// This sample provides a quick way of moving features to and from a warehouse.  A warehouse is designated by adding a new category to the utility network. That category value can then be assinged to certain asset types.
  /// To create your own warehouse category you can follow these steps:
  /// 1. Run the gp tool Add Network Category and create a category called 'Warehouse'.
  /// 1. Run the gp tool Set Network Category and assign the newly created Warehouse category to an asset group and asset type for a polygon feature from the structure boundary class.
  /// 3. Add a field called Name to the structure boundary class.
  /// 4. Make sure to run the Add Rule geoprocessing tool to allow for containment associations between this asset group and asset type and any other features that you would like to have moved to or from it.
  /// 5. Create at least one new feature of this asset type and make sure to provide a name in the Name field.
  /// </summary>
  /// <remarks>
  /// For sample data, download CommunitySampleData-UtilityNetwork-mm-dd-yyyy.zip from https://github.com/Esri/arcgis-pro-sdk-community-samples/releases
  /// and unzip it into c:\. We will be using the project in the "c:\Data\UtilityNetwork\MoveToFromWarehouse" folder as an example for this AddIn.
  /// 1. In Visual Studio open this solution and then rebuild the solution.
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open. 
  /// 1. Open the MoveToFromWarehouse.aprx file from the "c:\Data\UtilityNetwork\MoveToFromWarehouse" folder you just downloaded.  
  /// 1. Open the 'To/From Warehouse' tab on the ribbon.  
  /// ![UI](Screenshots/Screen1.png)
  /// 1. Moving a feature to a Warehouse:  Click on the "Move To Warehouse" tool button.
  /// 1. Click on a UN feature on the map to select the feature to move to a warehouse.  Note: make sure to click on a UN class and feature.  
  /// ![UI](Screenshots/Screen2.png)
  /// 1. A dialog pops up to allow the selection of the destination warehouse
  /// ![UI](Screenshots/Screen3.png)
  /// 1. After selecting a warehouse from the dropdown and clicking 'Done' the Addin creates a new containment association between the clicked feature and the selected warehouse feature.  
  /// ![UI](Screenshots/Screen4.png)
  /// 1. Move a feature from a Warehouse: Click on the "Move From Warehouse' tool button.
  /// 1. Click on the map to set the destination location of the warehouse feature.  
  /// 1. A dialog pops up that to selection a warehouse and then a feature in that warehouse.
  /// ![UI](Screenshots/Screen5.png)
  /// 1. Select a warehouse and feature and click 'Done'. This removes the containment association between the chosen feature and the warehouse and then moves the feature to the clicked on location on the map.
  /// ![UI](Screenshots/Screen6.png)
  /// </remarks>
  internal class Module1 : Module
    {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("ToFromWarehouse_Module");

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
