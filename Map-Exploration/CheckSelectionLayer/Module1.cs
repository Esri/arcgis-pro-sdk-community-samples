/*

   Copyright 2024 Esri

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

namespace CheckSelectionLayer
{
  /// <summary>
  /// This Button add-in, when clicked after a layer has been selected in the map's table of content, displays if a layer was created using "Make Layer From Selected Features" or not.
  /// </summary>
  /// <remarks>
  /// 1. Rebuild the project in Visual Studio and debug. 
  /// 1. In ArcGIS Pro open a project with a map that has one feature layer in the Map's TOC.
  /// 1. Select a set of feature on that map and use the "Make Layer From Selected Features" to create a layer from selected features.
  /// ![UI](Screenshots/Screen1.png)
  /// 1. Select the newly created "selected feature" layer on the Map's table of content and click the "Layer Type" button.
  /// ![UI](Screenshots/Screen2.png)
	/// 1. A popup show the layer type of the first selected layer.
  /// </remarks>
  internal class Module1 : Module
	{
		private static Module1 _this = null;

		/// <summary>
		/// Retrieve the singleton instance to this module here
		/// </summary>
		public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("CheckSelectionLayer_Module");

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
