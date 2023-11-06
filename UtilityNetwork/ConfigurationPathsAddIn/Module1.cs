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

namespace ConfigurationPathsAddIn
{
  /// <summary>
  /// This sample provides a mechanism for a user to change a network path for a feature that has multiple paths configured in the terminal configuration.
  /// The conditions are:  
  /// - The selected feature is from the Device or Junction Object class
  /// - The selected feature is configured with a terminal configration
  /// - and there is more that one path
  /// </summary>
  /// <remarks>
  /// For sample data, download CommunitySampleData-UtilityNetwork-mm-dd-yyyy.zip from https://github.com/Esri/arcgis-pro-sdk-community-samples/releases
  /// and unzip it into c:\. We will be using the project in the "c:\Data\UtilityNetwork\ConfigurationPaths" folder as an example for this AddIn.
  /// 1. In Visual Studio open this solution and then rebuild the solution.
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open. 
  /// 1. Open the ConfigurationPaths.aprx file from the "c:\Data\UtilityNetwork\ConfigurationPaths" folder you just downloaded.  
  /// 1. Make sure that the map view that contains utility network data is the active map view.
  /// 1. Open the 'Utility Network' tab on the Pro ribbon and note the 'Configuration Paths' group.
  /// 1. Also note the shortest path between the two end points below.
  /// ![UI](Screenshots/Screenshot1.png)
  /// 1. Click on the 'Change Path' tool and click on a feature from the Device or Junction Object class that is configured with a terminal configration.
  /// ![UI](Screenshots/Screenshot2.png)
  /// 1. Click on a valid Device or Junction Object class feature and the the 'Change Path' dialog opens. 
  /// 1. Change the 'Configuration Path' dropdown list to a different path and click 'OK'.
  /// ![UI](Screenshots/Screenshot3.png)
  /// 1. Rerun the shortest path and note that the path has now changed.  
  /// ![UI](Screenshots/Screenshot4.png)
  /// </remarks>
  internal class Module1 : Module
    {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("ConfigurationPathsAddIn_Module");

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
