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

namespace EditOperationRowEvent
{
  /// <summary>
  /// This sample shows how to reference and use the running EditOperation in row events.
  /// </summary>
  /// <remarks>
  /// This sample shows an example of the edit log being populated from either the default tools or a custom edit tool using the running Edit Operation within row events.
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data).  The sample data contains a map package called 'CrowdPlannerProject.ppkx' which is required for this sample.
  /// 1. Open this solution in Visual Studio. 
  /// 1. Click the build menu and select Build Solution.
  /// 1. Launch the debugger to open ArCGIS Pro.
  /// 1. Open the map package "CrowdPlannerProject.ppkx" located in the "C:\Data\CrowdPlanner" folder since this project contains all required data.
  /// 1. Click on the Add-in tab and see that an 'Edit log' group has been added with two controls; 'Initialize' and 'Create Zone'.
  /// 1. Click on the 'Initialize' button. This creates an edit log table that will record edits to the Crowdplanning layer.
  /// 1. Dock the edit table with map so you can see records being added to the table as you edit the layer.
  /// 1. With the default edit tools create some new poly polygons and make edits. You should see edit log records appear in the table.
  /// 1. Undo an edit to see the correspnding row in the edit table be deleted.
  /// 1. Click on the 'Create Zone' button. This creates a crowd planning polygon of a fixed size.
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("EditOperationRowEvent_Module"));
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
