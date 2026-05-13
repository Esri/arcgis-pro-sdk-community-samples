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
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.KnowledgeGraph.FFP;
using ArcGIS.Desktop.Internal.Mapping.Controls.Histogram;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using static ArcGIS.Desktop.Internal.Editing.Editor3D;
using static ArcGIS.Desktop.Internal.Framework.EsriPBf.Tile.Types;

namespace ConstructionSplitTool
{
  /// <summary>
  /// 
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Launch the debugger to open ArcGIS Pro.
  /// 1. Open any project that has a map with multiple line, point and polygon layers, for example: C:\Data\MilitaryOverlay\MilitaryOverlay.aprx' 
  /// 1. From Edit tab, open Create Features pane.
  /// 1. Click to activate the Civilian feature template for the Air layer.
  /// ![UI](Screenshots/Screen1.png)
  /// 1. Search for ‘Canal’ feature template and create a feature using the normal Line tool 
  /// ![UI](Screenshots/Screen2.png)
  /// 1. Search for ‘Isobar-Surface’ feature template and create a line feature using the normal Line tool 
  /// ![UI](Screenshots/Screen3.png)
  /// 1. Search for ‘Jet Stream’ feature template and then activate the Split Lines tool.Create a feature that crosses the both Canal and Isobar-Surface features created in previous steps.Confirm that all 3 features are split at the intersections and seven features are now selected. 
  /// ![UI](Screenshots/Screen4.png)
  /// 1. Search for ‘Island’ feature template and create a polygon in the map. 
  /// ![UI](Screenshots/Screen5.png)
  /// 1. Search for the ‘Cold Front’ feature template then click to activate.
  /// 1. Create a feature that crosses the Island polygon. 
  /// ![UI](Screenshots/Screen6.png)
  /// </remarks>
  internal class Module1 : Module
    {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("ConstructionSplitTool_Module");

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
