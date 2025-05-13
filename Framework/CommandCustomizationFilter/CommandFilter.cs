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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CommandCustomizationFilter
{
	internal class CommandFilter : CustomizationFilter
	{
		#region Properties to control the filter

		internal bool FilterOn { get; set; } = false;

		internal bool UIEnableOn { get; set; } = false;

		#endregion Properties to control the filter

		/// <summary>
		/// Gives a customization filter the ability to disable commands.
		/// </summary>
		/// <param name="DamlID">The DAML ID of the command about to execute.</param>
		/// <param name="moduleID">The command's parent module DAML ID.</param>
		/// <returns>false to disable the command; true to enable the command</returns>
		protected override bool OnCanExecuteCommand(string DamlID, string moduleID)
		{
			if (!UIEnableOn) return true;
			if (DamlID == "esri_editing_ShowEditFeaturesBtn"
				|| DamlID == "CommandCustomizationFilter_TestButton")
			{
				// disable the Modify Features button
				return false;
			}
			return true;
		}

		/// <summary>
		/// Implement your custom filtering logic here. OnCommandExecute is called every time
		/// a command is clicked on the UI.
		/// </summary>
		/// <param name="DamlID">DAML ID of the command that is executing</param>
		/// <returns>true to execute the command; false to NOT execute the command</returns>
		protected override bool OnExecuteCommand(string DamlID)
		{
			if (!FilterOn) return true;
			if (DamlID == "esri_editing_ShowCreateFeaturesBtn"
				 || DamlID == "CommandCustomizationFilter_TestButton")
			{
				MessageBox.Show("Create Features button is disabled");
				return false;
			}
			return true;
		}

		/// <summary>
		/// Register for command filtering. Customization filters must be registered before they are
		/// called.
		/// </summary>
		public void Register()
		{
			FrameworkApplication.RegisterCustomizationFilter(this);
		}

		/// <summary>
		/// Unregister for command filtering
		/// </summary>
		public void UnRegister()
		{
			FrameworkApplication.UnregisterCustomizationFilter(this);
		}
	}
}
