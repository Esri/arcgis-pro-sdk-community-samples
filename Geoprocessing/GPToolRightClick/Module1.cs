/*

   Copyright 2025 Esri

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
using System.Threading.Tasks;
using System.Windows.Input;
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

namespace GPToolRightClick
{
  /// <summary>
  /// GPToolRightClick is a sample add-in that demonstrates how to create a custom context menu for a geoprocessing tool and who to retrieve the clicked on tool's detailed information.
  /// </summary>
  /// <remarks>
  /// 1. Download this repo onto your local machine and open the GPToolRightClick solution using Visual Studio 2022.
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Click Start (debug) button to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open. 
  /// 1. Open any project and then open the Geoprocessing pane (Analysis tab).
  /// 1. Click the toolboxes tab in the Geoprocessing pane, navigate to any tool and rightclick on the tool. 
  /// 1. Navigate and click on the "GPTool right click" menu.
  /// ![UI](Screenshots/Screen1.png)
  /// 1. The tool's detail information is displayed in a message box.
  /// ![UI](Screenshots/Screen2.png)
  /// </remarks>
  internal class Module1 : Module
    {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("GPToolRightClick_Module");

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
