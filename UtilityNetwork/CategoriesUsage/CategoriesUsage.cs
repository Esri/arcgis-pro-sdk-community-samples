/*

   Copyright 2019 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace CategoriesUsage
{
	/// <summary>
	/// Utility network SDK samples require a utility network service to run.  This sample is designed to work with any utility network.  If you do not have one available, instructions for setting up and configuring this data are located on the websites below:
	/// * [ArcGIS for Electric](http://solutions.arcgis.com/electric/help/electric-utility-network-configuration/)
	/// * [ArcGIS for Gas](http://solutions.arcgis.com/gas/help/gas-utility-network-configuration/)
	/// * [ArcGIS for Water](http://solutions.arcgis.com/water/help/water-utility-network-configuration/)
	/// </summary>
	/// <remarks>
	/// 1. In Visual Studio click the Build menu.  Then select Build Solution.  
	/// 1. Click Start button to open ArcGIS Pro.
	/// 1. ArcGIS Pro will open.
	/// 1. Open a map view that contains at least one Feature Layer whose source points to a Feature Class that participates in a utility network.
	/// 1. Select a feature layer or subtype group layer that participates in a utility network or a utility network layer  
	/// 1. Click on the SDK Samples tab on the Utility Network tab group  
	/// 1. The combobox lists all the categories in the utility network
	/// 1. Selecting a category will generate and display a table that lists the feature classes, asset groups, and asset types that reference the selected category
	/// </remarks>
	internal class CategoriesUsage : Module
  {
    private static CategoriesUsage _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static CategoriesUsage Current
    {
      get
      {
        return _this ?? (_this = (CategoriesUsage)FrameworkApplication.FindModule("CategoriesUsage_Module"));
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
