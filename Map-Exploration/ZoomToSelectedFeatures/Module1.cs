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
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ZoomToSelectedFeatures
{
	/// <summary>
	/// This sample uses a dropdown of all layers with selected features and once a layer with selections is chosen two button allow scrolling through the selected features..
	/// </summary>
	/// <remarks>
	/// 1. In Visual studio rebuild the solution.
	/// 1. Debug the add-in.
	/// 1. ArcGIS Pro opens, choose any project with feature layers.
	/// 1. With the active map view, select the 'Zoom Selected' tab on the Pro ribbon.
	/// 1. Choose the 'Select Tool' and select some features in the map view.
	/// ![UI](Screenshots/Screen1.png)
	/// 1. Once features are selected, the 'Select Layer' dropdown will be populated with the layers that have selected features.
	/// 1. Use the dropdown to select a layer with selected features.
	/// ![UI](Screenshots/Screen2.png)
	/// 1. Use the 'Move Back' and 'Move Forward' buttons to scroll through the selected features.
	/// ![UI](Screenshots/Screen3.png)
	/// 1. Use the 'Expand ratio' drop down to choose the expand ratio for the zoom.  0 uses the extent of the selected feature, 100 expands the extent by 100%.
	/// </remarks>
	internal class Module1 : Module
	{
		private static Module1 _this = null;

		/// <summary>
		/// Retrieve the singleton instance to this module here
		/// </summary>
		public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("ZoomToSelectedFeatures_Module");

		public static SelectLayerComboBox TheSelectLayerComboBox { get; set; }
		public static MoveBack TheMoveBackButton { get; set; }
		public static MoveForward TheMoveForwardButton { get; set; }

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
