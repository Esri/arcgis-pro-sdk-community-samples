/*

   Copyright 2025 Esri

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
using System.Threading.Tasks;
using System.Windows.Input;
using AddInShared;
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

namespace AddInOne
{  /// <summary>
	 /// This Add-in demonstrates who to consume the IAddInToAddIn interface to communicate between two Add-ins.
	 /// An add-in can raise an event and another add-in can subscribe to that event.
	 /// </summary>
	 /// <remarks>
	 /// 1. In Visual Studio click the Build menu. Then select Build Solution.
	 /// 1. Click Start button to open ArcGIS Pro.
	 /// 1. Open any project and then open the "AddIn 2 AddIn" tab on the ArcGIS Pro ribbon.  
	 /// 1. You see two groups for respective Add-ins: "AddIn One" and "AddIn Two"
	 /// ![UI](Screenshots/Screen1.png)
	 /// 1. Both Add-ins are implementing the IAddInToAddIn interface in their module class that allows the Add-ins to raise events and subscribe to these events.
	 /// 1. When you change the selection in the drop-down of either Add-in an event is raised.
	 /// 1. The event subscription displays the raised event information in the Custom control.
	 /// ![UI](Screenshots/Screen2.png)
	 /// ![UI](Screenshots/Screen3.png)
	 /// </remarks>
	internal class ModuleAddInOne : Module, IAddInToAddIn
	{
		private static ModuleAddInOne _this = null;

		/// <summary>
		/// Retrieve the singleton instance to this module here
		/// </summary>
		public static ModuleAddInOne Current => _this ??= (ModuleAddInOne)FrameworkApplication.FindModule("AddInOne_Module");

		public event AddInToAddInEventHandler AddInToAddInEvent;

		public void OnAddInToAddInEvent(AddInToAddInEventArgs e)
		{
			if (AddInToAddInEvent != null)
			{
				AddInToAddInEvent(this, e);
			}
		}

		internal IAddInToAddIn Subscribe(string moduleId)
		{
			var subscribeModule = FrameworkApplication.FindModule(moduleId) as IAddInToAddIn;
			return subscribeModule;
		}

		private void ProcessAddInToAddInEvent(object sender, AddInToAddInEventArgs e)
		{
			OnAddInToAddInEvent(e);
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
