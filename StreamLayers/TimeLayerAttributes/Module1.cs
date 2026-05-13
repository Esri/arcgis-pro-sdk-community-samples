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

namespace TimeLayerAttributes
{
  /// <summary>
  /// This sample utilizes a time-enabled layer to emulate real-time data streaming through time extent change events captured by a dockpane.  The dockpane uses a data grid to display the 'streamed' (via time selection) data.
  /// </summary>
  /// <remarks>
	/// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data).  The sample data contains required data for this sample add-in.  Make sure that the Sample data is unzipped in c:\data and "C:\Data\Interacting with Maps" is available.
	/// 1. In Visual Studio click the Build menu. Then select Build Solution.
	/// 1. Launch the debugger to open ArcGIS Pro.
	/// 1. ArcGIS Pro will open.
	/// 1. Open the "C:\Data\Interacting with Maps\Interacting with Maps.aprx" project which contains the required data needed for this sample.
	/// 1. Make sure "Portland Crimes" is the active map and open the "Stream Data" tab on the ArcGIS Pro ribbon.
  /// 1. Click the "Show Stream Data Pane" button to open the "Show Timeslider stream data" dockpane.
  /// ![UI](Screenshots/Screen1.png)
	/// 1. Click the "Ready Data" button on the dockpane.
  /// ![UI](Screenshots/Screen2.png)
  /// 1. This will locate the "Portland Crimes" layer in the TOC and set the time extent of the map to the time to the first day with data.  The dockpane displays the time extent settings for the time slider.
  /// ![UI](Screenshots/Screen3.png)
  /// 1. Click the "Play Button" to start the time slider.  The dockpane listens to the time slider and updates the time extent settings and the data grid content accordingly.
  /// ![UI](Screenshots/Screen4.png)
  /// </remarks>
  internal class Module1 : Module
    {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("TimeLayerAttributes_Module");

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
