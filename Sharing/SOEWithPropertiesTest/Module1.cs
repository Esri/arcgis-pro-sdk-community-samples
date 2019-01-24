/*

   Copyright 2019 Esri

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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace SOEWithPropertiesTest
{
	/// <summary>
	/// You can extend ArcGIS Server map and image services (including map and image service extensions, such as feature services) with custom logic that can be executed in ArcGIS clients. There are two ways to extend these service types: Server object extensions (SOEs) and Server object interceptors (SOIs).
	/// SOEs are appropriate if you want to create new service operations to extend the base functionality of map and image services(including map and image service extensions, such as feature services). 
	/// For example, while publishing a map image layer to federated servers, the checkboxes left for clients to pick among Feature Access, WMS, or network/utilities as extensions, are the built-in SOEs.This SDK sample would add to the diversity and flexibility of the SOE capabilities, and allow clients to customize their own SOEs.
	///   
	/// In summary, the sample would support these following features �
	/// a. Add a customized SOE to Pro(when Pro detects the target server has customized extensions, display these SOEs at the configuration pane)
	/// b. Allow the customized SOE to analyze the publishing content, and decide if the SOE should be enabled or not
	/// c. Allow the customized SOE to provide custom UI that will be integrated into the sharing UI so that individual, SOE specific parameters can be set.
	/// </summary>    
	/// <remarks>
	/// 1. In Visual Studio click the Build menu. Then select Build Solution.
	/// 1. Click Start button to open ArcGIS Pro.
	/// 1. ArcGIS Pro will open. 
	/// 1. Open a map inside an existing project, and get connected to a 10.6.1 portal (sign in and set as active)
	/// 1. On the sharing ribbon, in the �Share As� group, select �Publish layer� 
	/// 1. When the sharing pane pops out, fill out the required information for the service, and select the �Layer Type� as Map Image Layer;
	/// 1. Switch from General tab to configuration tab.If your federated server has already installed the customized SOEs, and you have installed the Add-in successfully, then you will see customized extensions being display as in Fig 2. If not, you might only see Fig 1.
	/// 1. Check on the SOE(or SOIs) you would like to enable, then click the pencil icon to edit its properties.See in Fig 3 if you enabled the SpatialQueryREST SOE, and clicked the pencil button for editing.
	/// 1. After enabling desired SOEs, click on the back arrow, to go back the general tab, and click �Publish� to start sharing process.
	/// 1. If successfully published, you will be able to see the properties page on the server e.g. \&lt;server adaptor url\&gt;/rest/services/\&lt;service name\&gt;/MapServer/exts/\&lt;SOE name\&gt;  
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
				return _this ?? (_this = (Module1)FrameworkApplication.FindModule("SOEWithPropertiesTest_Module"));
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
