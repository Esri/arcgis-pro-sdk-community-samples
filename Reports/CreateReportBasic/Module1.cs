/*

   Copyright 2024 Esri

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
using ArcGIS.Desktop.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CreateReportBasic
{
  /// <summary>
  /// This sample demonstrates how to create a very simple report.  The add-in takes the first feature class, selects all records, and creates a report.   
  /// </summary>
  /// <remarks>
  /// 1. Open this solution in Visual Studio.  
  /// 2. Click the Build menu. Then select Build Solution.
  /// 3. Click Start button to open ArcGIS Pro. ArcGIS Pro will open.
  /// 4. Open a project with a map that contains at least one feature layer.  
  /// 5. Click on the Add-in tab and notice the 'Create Report' group with 3 buttons: Create Report, Show Report, and Preview Report.
  /// 6. With the map view active, click the 'Create Simple Report' button.  A report will be created for the first feature layer in the map.
  /// ![UI](Screenshots/Screen1.png)
  /// 7. Next click the 'Show Report Pane' button to display the report pane for the newly created report.
  /// ![UI](Screenshots/Screen2.png)
  /// 8. Finally click the 'Preview Report' button to preview the report.
  /// ![UI](Screenshots/Screen3.png)
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("CreateReportBasic_Module");

    public static Report SimpleReport
    { get; set; }

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
