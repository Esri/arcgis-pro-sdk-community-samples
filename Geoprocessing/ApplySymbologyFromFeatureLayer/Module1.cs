/*

   Copyright 2022 Esri

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
using System.Threading.Tasks;
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

namespace ApplySymbologyFromFeatureLayer
{
  /// <summary>
  /// This sample demonstrates how to the "ApplySymbologyFromLayer_management" geoprocessing task
  /// </summary>
  /// <remarks>
  /// Requirements to run this sample:
  /// * the first layer in TOC is WITHOUT any symbology
  /// * the second layer HAS the symbology
  /// * symbology from the 2nd layer will be applied to the first layer
  /// * As a sample you can use following sample data: C:\Data\Admin\AdminData.gdb\counties
  /// * Both layers must have fields with the POP2000 name.  The NAME field is used, the input parameters for the "ApplySymbologyFromLayer_management" geoprocessing task
  /// 1. fieldType = "VALUE_FIELD";
  /// 1. sourceField = "POP2000";
  /// 1. targetField = "POP2000";
  /// 
  /// Using the sample:
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. Open any project in ArcGIS Pro with a Map that contains a the two feature layers shown as requirements above.
  /// ![UI](Screenshots/Screen1.png)
  /// 1. After the map view is open switch on the Pro Ribbon to the 'Add-in' tab.
  /// 1. Click the "Apply Symbology From Feature Layer" button.  Notice the Symbology has been applied to the topmost layer.
  /// ![UI](Screenshots/Screen2.png)
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
		return _this ?? (_this = (Module1)FrameworkApplication.FindModule("ApplySymbologyFromFeatureLayer_Module"));
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
