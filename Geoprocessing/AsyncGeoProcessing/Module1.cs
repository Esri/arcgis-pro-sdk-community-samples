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

namespace AsyncGeoProcessing
{
	/// <summary>
	/// This sample shows who to run GP tools with and without progressors and the effect on UI responsiveness.
	/// </summary>
	/// <remarks>
	/// 1. In Visual studio rebuild the solution.
	/// 1. Debug the add-in.
	/// 1. When ArcGIS Pro opens open any project with a map containing at least one polygon layer.  
	/// 1. Select the "Async GP Tools" tab.  Select a polygon feature in the map.
	/// ![UI](Screenshots/Screen1.png)
	/// 1. Click the "Run w/ Progress" button to create a series of buffers around the selected polygon feature. 
	/// 1. A progressor is displayed while the tool is running.  Notice that the UI is not responsive while the tool is running.
	/// ![UI](Screenshots/Screen2.png)
	/// 1. Click the "Run w/o Progress" button to create a series of buffers around the selected polygon feature. 
	/// 1. No progressor is displayed while this tool is running.  Notice that the UI is now responsive while the tool is running.  With each newly created polygon the table of content is updated.
	/// ![UI](Screenshots/Screen3.png)
	/// 1. Click the "Run w/o Interface" button to create a series of buffers around the selected polygon feature. 
	/// 1. No progressor is displayed while this tool is running and the table of content is not updated until the whole process is complete.  Notice that the UI is fully responsive while the tool is running.  With each newly created polygon the table of content is updated.
	/// ![UI](Screenshots/Screen4.png)
	/// </remarks>
	internal class Module1 : Module
	{
		private static Module1 _this = null;

		/// <summary>
		/// Retrieve the singleton instance to this module here
		/// </summary>
		public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("AsyncGeoProcessing_Module");

		public uint GPCycles { get; internal set; }

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
