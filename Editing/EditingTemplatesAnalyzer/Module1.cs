/*

   Copyright 2026 Esri

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

using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace EditingTemplatesAnalyzer
{
  /// <summary>
  /// Analyzes group editing templates for warnings and errors, lists problematic templates in a dockpane, and removes selected templates.
  /// 
  /// This add-in can be used to identify group templates with warnings and errors.The add-in will report the following:
  /// * Group template is missing the primary feature template, or one or more secondary feature template
  /// * Group template may create non-visible features due to a definition query
  /// * Missing feature builder
  /// * Missing a tag or a description
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Launch the debugger to open ArcGIS Pro.
  /// 1. Open any project that has a map with multiple line, point and polygon layers that have group templates. 
  /// 1. Open the Group Templates Inspector pane from the Add-In tab on the ribbon.
  /// 1. Click the "Find templates" button to find all group templates in the map that are missing primary templates, secondary templates, builder, or have definition queries, or have preset builder child templates missing features, or are missing tags or description.
  /// 1. Choose a category from "Filter results by" to see the templates that are broken for that reason.
  /// 1. Select a template from the list, click Delete, and then confirm to delete the template. Use Undo button if you want to restore the deleted template.
  /// </remarks>
  internal class Module1 : Module
	{
		private static Module1 _this = null;
		    
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("EditingTemplatesAnalyzer_Module");

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
