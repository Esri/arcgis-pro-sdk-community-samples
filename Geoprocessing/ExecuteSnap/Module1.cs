/*

   Copyright 2022 Esri

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

namespace ExecuteSnap
{
  /// <summary>
  /// This sample runs the Snap (Editing) geoprocessing tool.  The tool moves points or vertices to coincide exactly with the vertices, edges, or end points of other features. 
  /// [Snap (Editing) help](https://pro.arcgis.com/en/pro-app/latest/tool-reference/editing/snap.htm)
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data).  The sample data contains a folder called 'C:\Data\GeoProcessing' with sample data required for this solution.  Make sure that the Sample data is unzipped in c:\data and "C:\Data\GeoProcessing" is available.
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.  
  /// 1. Launch the debugger to open ArcGIS Pro.
  /// 1. Open the C:\Data\GeoProcessing\EditSnap\EditSnap.aprx project. 
  /// 1. Notice the Redline layer's geometries, some of the vertices are close to the Greenline geometrie's ends. 
  /// ![UI](Screenshots/Redline.png)
  /// 1. Under the Add-in tab, click the "Execute Snap" button.  This will take all 'Redline' features and snap any vertices that are closer than 50 meters to the any vertex of the Greenline layer.
  /// ![UI](Screenshots/Snapped.png)
  /// </remarks>
  internal class Module1 : Module
    {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>

        public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("ExecuteSnap_Module");

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
