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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace CategoriesUsage
{
  /// <summary>
  /// This add-in creates temporary tables listing out the asset types in a utility network that support a particular 
  /// utility network category.
  /// 
  /// Community Sample data (see under the "Resources" section for downloading sample data) has a UtilityNetworkSamples.aprx 
  /// project that contains a utility network that can be used with this sample.  This project can be found under the 
  /// C:\Data\UtilityNetwork folder. Alternatively, you can also use any utility network data with this sample.
  /// 
  /// </summary>
  /// <remarks>
  /// 
  /// 1. In Visual Studio click the Build menu.  Then select Build Solution.
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open.
  /// 1. Open C:\Data\UtilityNetwork\UtilityNetworkSamples.aprx or a map view that references a utility network
  /// 1. Click on the Add-in tab on the ribbon
  /// 1. Select a feature layer or subtype group layer that participates in a utility network or a utility network layer
  /// 1. The Category Assignments combobox lists all the categories in the utility network
  /// ![UI](Screenshots/Screenshot1.png)
  /// 1. Selecting a category will generate and display a table that lists the feature classes, asset groups, and asset types that reference the selected category
  /// ![UI](Screenshots/Screenshot2.png)
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
