/*

   Copyright 2019 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

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

namespace TableControl
{
  /// <summary>
  /// This sample demonstrates using the TableControl on a dockpane. 
  /// In this example we will populate the tablecontrol with content from the selected item in the Catalog pane. 
  /// We will add toolbar buttons to the dockPane to modify the selected rows of the tablecontrol data. We will also customize the default row context menu. 
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
  /// 1. Make sure that the Sample data is unzipped in c:\data
  /// 1. In Visual Studio click the Build menu.Then select Build Solution.
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open.  
  /// 1. Open any project.
  /// 1. Click on the ADD-IN TAB.  
  /// 1. Click on the* Show Preview* button.   
  /// 1. A dockpane will be displayed with an empty TableControl.
  /// 1. Open the Catalog pane and navigate to a datasource from the Community Samples Data.   
  /// 1. The TableControl will populate with data when a datasource is highlighted.
  /// ![UI](screenshots/TableControl_1.png) 
  /// 1. Click on the Toggle Sel, Select All and Clear Sel to select or unselect the table rows.
  /// 1. Click the Add to Map button to add the data to the active map.    
  /// 1. Right click on a row and see the custom context menu. Choose Zoom to Row to zoom to the active row in the map. 
  /// ![UI](screenshots/TableControl_ContextMenu.png)
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("TableControl_Module"));
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
