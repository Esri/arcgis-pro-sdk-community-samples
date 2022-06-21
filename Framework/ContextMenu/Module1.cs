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
using ArcGIS.Desktop.Mapping;

namespace ContextMenu
{
	/// <summary>
	/// This sample demonstrates three methods for associating, and showing, a context menu with a map tool. It is also the accompanying sample for the ArcGIS Pro SDK ProGuide Context Menus.
	/// </summary>
	/// <remarks>
	/// 1. Example 1, ContextMenuTool1.cs illustrates the simplest and most straightforward approach - associating a predefined menu with the map tool ContextMenuID property. For example 1, right-click behavior is "built in".	/// 
	/// ![Example 1](Screenshots/Screen1.png)
	/// 1. Example 2, ContextMenuTool2.cs illustrates implementing custom right-click behavior to show the context menu "manually". The menu is the same menu used in Example 1.
	/// ![Example 2](Screenshots/Screen2.png)
	/// 1. Example 3, ContextMenuTool3.cs illustrates the most flexible approach but also the most demanding to implement: A Dynamic context menu, built on-the-fly along with custom right-click behavior to show the menu.&lt;br/&gt;
	/// In Example 3, the tool sketches a line and, from the context menu, line features can be selected to add their feature shape into the sketch. The insertion point (for the shape) is the right-click location - the same location used to popup the context menu.&lt;br/&gt;
	/// ![Example 3](Screenshots/Screen3.png)
	/// Consult the &lt;a href="https://github.com/Esri/arcgis-pro-sdk/wiki/ProGuide-Context-Menus"&gt;ArcGIS Pro SDK ProGuide Context Menus&lt;/a&gt; for more details.
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
				return _this ?? (_this = (Module1)FrameworkApplication.FindModule("ContextMenu_Module"));
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
