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
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VersionUpdatePerformance
{
  /// <summary>
  /// This sample exercises the update performance to Enterprise Geodatabases.  The updates can be measured against version updates and default version updates.
  /// </summary>
  /// <remarks>
  /// 1. This sample requires an ArcGIS Pro project with a connection file to an Enterprise Geodatabase (.SDE).  When running the performance test the test will create a copy of a source feature class, so 'create' level access is required.
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Launch the debugger to open ArcGIS Pro.
  /// 1. ArcGIS Pro opens, open an aprx project that allows you to connect to an Enterprise Geodatabase
  /// 1. Click on the 'Performance' tab on the ArcGIS Pro ribbon and click the 'Update /w Versions' button to open the 'Version Update' dockpane.
  /// 1. Click the 'Select the Feature Class Source' button and select an Enterprise Connection file from on the popup dialog, the select a suitable test Feature Class (note that all features from this feature class will be copied for the performance test).
  /// ![UI](Screenshots/Screenshot1.png)
  /// 1. Enter a name for a new destination Feature class.  The destination will use the source's Enterprise Geodatabase connection and if the destination doesn't exist the schema of the source is used to create the destination Feature Class.
  /// ![UI](Screenshots/Screenshot2.png)
  /// 1. In order to select a specific version for your test you have to click the 'Version Refresh' first.
  /// ![UI](Screenshots/Screenshot3.png)
  /// 1. After using the 'Version Refresh' button use the version drop-down to select an existing version to perform the performance test, or enter a new version name to create a new version.
  /// ![UI](Screenshots/Screenshot4.png)
  /// 1. Click the 'Copy' button to start the performance test.
  /// ![UI](Screenshots/Screenshot5.png)
  /// 1. Performance Stats are displayed in this text box:
  /// ![UI](Screenshots/Screenshot6.png)
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("VersionUpdatePerformance_Module");

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
