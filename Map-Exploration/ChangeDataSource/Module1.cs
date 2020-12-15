/*

   Copyright 2020 Esri

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
using ArcGIS.Desktop.Mapping;

namespace ChangeDataSource
{
	/// <summary>
	/// This sample demonstrates how to change the underlying datasource of a feature layer.
	/// </summary>
	/// <remarks>
	/// 1. Download the Community Sample data (see under the "Resources" section for downloading sample data).  The sample data contains a project called "FeatureTest.aprx" with data suitable for this sample.  Make sure that the Sample data is unzipped in c:\data and "C:\Data\FeatureTest\FeatureNewTest.gdb" is available.
	/// 1. In Visual Studio click the Build menu. Then select Build Solution.
	/// 1. Click Start button to open ArcGIS Pro.
	/// 1. In ArcGIS Pro open "C:\Data\FeatureTest\FeatureTest.aprx"
	/// 1. Open the 'Add-in' tab and click on the "Show Change DataSource" button to open the "Change DataSource" dockpane 
	/// 1. On the "Change DataSource" dockpane browse to the "C:\Data\FeatureTest\FeatureNewTest.gdb" database which will be used to replace the current datasource for all layers.
	/// 1. Click the 'Replace Layer Datasources' button to replace all data connections.
	/// ![UI](Screenshots/Screen1.png)
	/// 1. Verify that all data connection have been changed:
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
				return _this ?? (_this = (Module1)FrameworkApplication.FindModule("ChangeDataSource_Module"));
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
