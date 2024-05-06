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
using ArcGIS.Desktop.Mapping.Events;

namespace CreateReport
{
  /// <summary>
  /// This sample demonstrate how to create a report and modify that report.
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu. Then select Build Solution. 
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open. 
  /// 1. Open any project file that contains feature layers. Activate the mapview with the feature layers.
  /// 1. In the Add-In tab, click the Create Report button. This will display the Create Report dockpane.
  /// 1. Pick a layer in the active map for which you want to use to generate a report.  Check the "Report uses only the selected features" check box if needed.
  /// 1. Modify or accept the default Report Name.
  /// 1. Pick the fields needed for the report.
  /// 1. Pick a Grouping field. This is optional.
  /// 1. Pick the templates and styling for report.
  /// 1. Pick a field used to generate field statistics if required.
  /// 1. Pick the Statistics option.
  /// 1. Click Create Report.  The Report project item is generated. You can see this in the Catalog pane.
  /// 1. Export the report to a PDF.  The report PDF file is exported to the project's home folder.
  /// ![UI](screenshots/CreateReport.png)  
  /// ###Modify an existing report by adding a new field
  /// 1. You can modify this report that was just created. To modify the report, click/add additional fields in the fields listbox.
  /// 1. The Update report button gets enabled when you add additional fields. Click Update Report.
  /// ![UI](screenshots/UpdateReport.png) 
  /// 1. Notice the new fields added to the Report view.
  /// ![UI](screenshots/ModifiedReport.png) 
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("CreateReport_Module"));
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
