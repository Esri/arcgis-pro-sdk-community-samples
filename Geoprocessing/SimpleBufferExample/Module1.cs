/*

   Copyright 2023 Esri

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

namespace SimpleBufferExample
{
	/// <summary>
	/// This sample shows how to call the 'Buffer' GP Tool from .Net.  It also calls the GP Tool in a loop to show the Progress dialog across multiple GP Tool calls.
	/// </summary>
	/// <remarks>
	/// 1. You will need CommunitySampleData-ParcelFabric-mm-dd-yyyy.zip down-loadable as an asset under the latest release.
	/// 1. In Visual studio rebuild the solution.
	/// 1. Debug the add-in.
	/// 1. When ArcGIS Pro opens open 'C:\Data\ParcelFabric\Island\ParcelIsland.aprx'.
	/// 1. Select the "Simple Buffer" tab.
	/// ![UI](Screenshots/Screen1.png)
	/// 1. Click the "Make Simple Buffer" button to create a buffer around the 'SouthShoreImpact' layer, the resulting buffer is called 'SouthShoreImpact_Buffer' and will be added to the map. 
	/// ![UI](Screenshots/Screen2.png)
	/// 1. In order to see the Progress Dialog being displayed you need to run ArcGIS Pro outside the Visual Studio debugger.  Running from with the VS Debugger will dispable the Progress Dialog display.
	/// 1. Click the "Multi Buffer tool" and digitize a complex polygon onto the map.  Complete the polygon to start the 'Buffer GP Tool' loop.
	/// ![UI](Screenshots/Screen3.png)
	/// ![UI](Screenshots/Screen4.png)
	/// </remarks>
	internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("SimpleBufferExample_Module");

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
