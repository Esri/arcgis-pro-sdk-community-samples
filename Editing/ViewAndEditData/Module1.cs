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
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.KnowledgeGraph;

namespace ViewAndEditData
{
  /// <summary>
  /// This sample illustrates how to display tabular data using a Table Control. Additionally, this sample also illustrates how to use the Inspector to edit records in your tabular data
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Launch the debugger to open ArcGIS Pro.
  /// 1. Open any project that has a map with multiple layers and standalone tables.
  /// 1. Click the Data Viewer button on the Add-In tab.
  /// ![UI](Screenshots/ViewData.png)
  /// 1. Activate the map. Select a feature layer or a standalone table in the map's Table of Content.
  /// ![UI](Screenshots/Select.png)
  /// 1. The Data Viewer dockpane opens up.
  /// 1. The table control seen in the dockpane displays the data behind the selected map member in the TOC. Notice the "Find" control that allows you to search for a specific record in the table.
  /// ![UI](Screenshots/DataViewerDockpane.png)  
  /// 1. Select a row in the table control in the dockpane. The attributes of the row or feature is displayed in the Inspector at the bottom.
  /// 1. You can edit the editable fields and apply the changes using the Apply button at the bottom.
  /// ![UI](Screenshots/TableControlInspector.png) 
  /// 1. Activate the Catalog window. Pick any dataset in the catalog window. These are classified as "external datasets".
  /// 1. The table control in the dockpane will now display this data. Note: You will not be able to edit this data since it is an external dataset. You can add the dataset to the map in order to edit it.
  /// ![UI](Screenshots/ExternalDatasource1.png)
  /// ![UI](Screenshots/ExternalDatasource2.png)
  /// </remarks>
  internal class Module1 : Module {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("ViewAndEditData_Module");

  #region Overrides
  /// <summary>
  /// Called by Framework when ArcGIS Pro is closing
  /// </summary>
  /// <returns>False to prevent Pro from closing, otherwise True</returns>
  protected override bool CanUnload() {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

#endregion Overrides

    }
}
