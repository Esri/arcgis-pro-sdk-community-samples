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

namespace DDLCreateFeatureClass
{
	/// <summary>
	/// This sample demonstrates how to create a new FeatureClass using the DDL functionality in the Pro API.
	/// </summary>
	/// <remarks>
	/// Using the sample:
	/// 1. In Visual Studio click the Build menu. Then select Build Solution.
	/// 1. Click Start button to open ArcGIS Pro.
	/// 1. Open any project in ArcGIS Pro with a Map that contains a feature layer with a File Geodatabase or Enterprise Geodatabase data source. 
	/// 1. After the map view is open switch on the Pro Ribbon to the 'Add-in' tab.
	/// ![UI](Screenshots/Screen1.png)
	/// 1. Select a feature layer on the map's content dockpane.  Make sure the feature layer's datasource is either a file Geodatabase or an Enterprise Geodatabase. 
	/// 1. The selected feature layer's datasource will be used to create a new Point feature class using the DLL API.  
	/// 1. Click the "Create Test FeatureClass" button.  Refresh the Databases node on the Catalog dockpane to see the newly created feature layer.
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
				return _this ?? (_this = (Module1)FrameworkApplication.FindModule("DDLCreateFeatureClass_Module"));
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
