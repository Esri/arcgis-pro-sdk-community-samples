/*

   Copyright 2026 Esri

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

namespace MapMemberIDSetControl
{
  /// <summary>
  /// Hosts MapMemberIDSetControl in a dockpane to display and manage sets of feature identifiers grouped by map layer.
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Click Start button to debug ArcGIS Pro.
  /// 1. Open any ArcGIS Pro project. Navigate to the Add-In tab on the ribbon. 
  /// ![UI](Screenshots/Screenshot1.png)
  /// 1. Click the Show MapMemberIDSetDockpane button.  This will display the MapMemberIDSetDockpane dockpane.  
  /// A combobox displays the layers in the active map.  There is also an edit box for entering an SQL where clause. 
  /// ![UI](Screenshots/Screenshot2.png)
  /// 1. Choose a layer from the Combobox and enter a where clause in the "Query where" edit box. Click the 
  /// "Verify Expression" button.
  /// ![UI](Screenshots/Screenshot3.png)
  /// 1. A message label is displayed indicating if the query has valid syntax or not. 
  /// If valid, the query results are displayed in the tree view of the MapMemberIDSetControl.
  /// ![UI](Screenshots/Screenshot4.png)
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("MapMemberIDSetControl_Module");

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
