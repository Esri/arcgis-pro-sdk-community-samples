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

namespace AttributesCustomDockpane
{
  /// <summary>
  /// Provides a custom attributes dockpane that combines feature-selection controls with an embedded Inspector for viewing and editing selected features.
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Click Start button to debug ArcGIS Pro.
  /// 1. Open any ArcGIS Pro project. Navigate to the Add-In tab on the ribbon. 
  /// ![UI](Screenshots/Screenshot1.png)
  /// 1. Click the Show Custom Attributes button.  This will display the Custom Attributes dockpane.  
  /// The SelectionToolPicker will display "Select one or more features" or "Change the selection"
  /// depending on whether a selection exists in the map. The current map selection (if any) will be displayed in the treeview.
  /// Highlight an item in the treeview to populate the bottom half of the dockpane with the attributes. 
  /// ![UI](Screenshots/Screenshot2.png)
  /// 1. Interact with the dockpane, highlighting different items in the treeview, activating the 
  /// SelectionToolPicker to make a different selection or choosing a different selection tool from the SelectionToolPicker dropdown. 
  /// ![UI](Screenshots/Screenshot3.png)
  /// 1. You can also activate a different map and the dockpane will respond and display the current selection of that map. 
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("AttributesCustomDockpane_Module");

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
