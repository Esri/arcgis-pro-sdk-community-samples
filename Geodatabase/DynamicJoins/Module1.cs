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

namespace DynamicJoins
{
	/// <summary>
	/// This sample illustrates the use of dynamic joins of tables and feature classes.
	/// </summary>
	/// <remarks>
	/// 1. In Visual studio click the Build menu. Then select Build Solution.
	/// 1. Click Start button to open ArcGIS Pro.
	/// 1. ArcGIS Pro will open, select any project
	/// 1. Open the Add-in tab and click the "Show Joins Dockpane" button to open "Joins" dockpane.
	/// ![UI](Screenshots/Screen1.png)
	/// 1. Select two data sources to join
	/// 1. select the Join fields
	/// ![UI](Screenshots/Screen2.png)
	/// 1. Define remaining options including layer name
	/// 1. Click the "Generate Join" button to create the layer
	/// 1. Open the new layer's attribute table to view the join result.
	/// ![UI](Screenshots/Screen3.png)
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("DynamicJoins_Module"));
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
