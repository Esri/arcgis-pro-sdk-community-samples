//
//   Copyright 2019 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Mapping.Controls.Histogram;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace AlternativeEnergizationAddIn
{
    /// <summary>
    /// This sample a tool to click on a utility network feature and run an isolation trace and then check for alternative energization capabilities.  The 'alternative energization' button shows red circle graphics on features that need to be disabled to isolate the selected feature and green circles on features that need to be enabled.
    /// </summary>
    /// <remarks>
    /// For sample data, download CommunitySampleData-UtilityNetwork-mm-dd-yyyy.zip from https://github.com/Esri/arcgis-pro-sdk-community-samples/releases
    /// and unzip it into c:\. We will be using the project in the "c:\Data\UtilityNetwork\" folder as an example for this AddIn.
    /// 1. In Visual Studio open this solution and then rebuild the solution.
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open. 
    /// 1. Open the AlternativeEnergization.aprx file from the "C:\Data\UtilityNetwork\AlternativeEnergization" folder you just downloaded.  
    /// 1. Make sure that the map view that contains utility network data is the active map view.
    /// 1. Open the 'Utility Network' tab on the Pro ribbon and note the 'Alternative Energization' group.
    /// ![UI](Screenshots/Screenshot1.png)
    /// 1. Click the 'Isolation Outage' tool button and click on a network feature.
    /// ![UI](Screenshots/Screenshot2.png)
    /// 1. This runs an isolation trace to find the upstream feature(s) that need to be opened in order to de-energize the selected network feature location.
    /// ![UI](Screenshots/Screenshot3.png)
    /// 1. Click the 'Alternative Energization' button to find potential alternative energization points.
    /// ![UI](Screenshots/Screenshot4.png)
    /// 1. If alternative methods of energization are found the add-in symbolizes Red circles to indicate features to disable and Green circles to indicate features to enable.
    /// ![UI](Screenshots/Screenshot5.png)
    /// 1. Click the 'Alternative Outage' button to then run a trace showing what the outage will look like if the alternative energization features are disabled and enabled according to the Red and Green circles.
    /// ![UI](Screenshots/Screenshot6.png)
    /// </remarks>
    internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
        public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("AlternativeEnergizationAddIn_Module");

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
