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

namespace CommandCustomizationFilter
{
	/// <summary>
	/// This sample implements "Command Filters", a.k.a. "Customization Filters", giving developers the opportunity to limit ArcGIS Pro DAML command functionality by stopping a command before its execution or by disabling a command in the UI.  This is useful when you want to prevent a command from executing based on certain conditions, such as the current state of the application or the user's permissions.
	/// ** Note:** For your "Command Filter" class to work, the class must be registered with ArcGIS Pro using the `ArcGIS.Desktop.Framework.FrameworkApplication.RegisterCustomizationFilter` method.
	/// </summary>    
	/// <remarks>    
	/// 1. In Visual Studio click the Build menu. Then select Build Solution.
	/// 1. Click Start button to open ArcGIS Pro.
	/// 1. ArcGIS Pro will open. 
	/// 1. Open any project file with a map that contains at least one editable feature layer.
	/// 1. Open a MapView and open the 'Command Filter' tab.
	/// 1. Use the 'Filter Option' drop down to exercise three 'Command Filter' options:
	/// 1. Select "Filter Off / UI Enabled" from the 'Filter Option' drop down. Notice that no filter functions are active.
	/// ![UI](Screenshots/Screen1.png)
	/// 1. Select "Filter On" from the 'Filter Option' drop down. Notice that clicking the "Test" button and the "Create Features" buttons are not being executed.
	/// ![UI](Screenshots/Screen2.png)
	/// 1. Select "UI Disabled" from the 'Filter Option' drop down. Notice that the "Test" button and the "Modify Features" button are disabled on the UI.
	/// ![UI](Screenshots/Screen3.png)
	/// </remarks>
	internal class Module1 : Module
	{
		private static Module1 _this = null;

		/// <summary>
		/// Retrieve the singleton instance to this module here
		/// </summary>
		public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("CommandCustomizationFilter_Module");

		#region Overrides for CommandFilter registration

		internal CommandFilter MyCommandFilter { get; set; }

		protected override bool Initialize()
		{
			// register the command filter
			MyCommandFilter = new CommandFilter();
			MyCommandFilter.Register();
			return base.Initialize();
		}

		protected override void Uninitialize()
		{
			// unregister the command filter
			if (MyCommandFilter != null)
			{
				MyCommandFilter.UnRegister();
				MyCommandFilter = null;
			}
			base.Uninitialize();
		}

		#endregion Overrides for CommandFilter registration

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
