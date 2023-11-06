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

namespace SubstitutionAddIn
{
  /// <summary>
  /// This sample provides a mechanism for a user to choose to turn a simple substitution setting into a permanent substitution.  
  /// This means it will end up copying the calculated values for propagated attribute into the original attribute that was propagated.  
  /// In electric networks this usually means copying the phases energized values back into the phases current values.
  /// </summary>
  /// <remarks>
  /// For sample data, download CommunitySampleData-UtilityNetwork-mm-dd-yyyy.zip from https://github.com/Esri/arcgis-pro-sdk-community-samples/releases
  /// and unzip it into c:\. We will be using the project in the "c:\Data\UtilityNetwork\Substitution" folder as an example for this AddIn.
  /// 1. In Visual Studio open this solution and then rebuild the solution.
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open. 
  /// 1. Open the Substitution.aprx file from the "c:\Data\UtilityNetwork\Substitution" folder you just downloaded.  
  /// 1. Make sure that the map view that contains utility network data is the active map view.
  /// 1. Open the 'Utility Network' tab on the Pro ribbon and note the 'Substitution' group.
  /// ![UI](Screenshots/Screenshot1.png)
  /// 1. Click on the 'Substitution' tool and click on a feature that is configured with the 'Attribute Substitution' category.  In this electric network this is a Tap feature.
  /// ![UI](Screenshots/Screenshot2.png)
  /// 1. Once you clicked on a feature the add-in checks that its assettype has the 'Attribute Substitution' category.  Then the add-in finds the network attribute that has a Network Attribute to Substitute set on it, then finds the field for the substitution by checking assignments, checking if it already has a value and then opens the 'Substitution' dialog. 
  /// ![UI](Screenshots/Screenshot3.png)
  /// 1. Change the substitution from the dropdown list and check the 'Permanent Change' checkbox and click 'OK'.
  /// ![UI](Screenshots/Screenshot3.png)
  /// Once the ok button is pressed the update is displayed on the map.  
  /// ![UI](Screenshots/Screenshot4.png)
  /// </remarks>
  internal class Module1 : Module
    {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("SubstitutionAddIn_Module");

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
