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

namespace ChromiumWebBrowserSample
{
	/// <summary>
	/// This sample shows how to implement pane that contains a Chromium web browser control.  For details on how to utilize the Chromium web browser control in an add-in see here: https://github.com/Esri/arcgis-pro-sdk/wiki/ProConcepts-Framework#chromiumwebbrowser  
	/// </summary>    
	/// <remarks>    
	/// 1. In Visual Studio click the Build menu. Then select Build Solution.
	/// 1. Click Start button to open ArcGIS Pro.
	/// 1. ArcGIS Pro will open. 
	/// 1. Open any project file. 
	/// 1. Click on the Add-in tab on the ribbon and then on the "Open ChromePane" button.  
	/// 1. The pane opens showing the https://github.com/esri/arcgis-pro-sdk-community-samples page.  
	/// ![UI](Screenshots/Screen1.png)\
	/// 
	/// Note: Custom scheme names must conform to RFC 3986.
	/// &lt;a href="https://tools.ietf.org/pdf/rfc3986.pdf"/&gt;. Namely:
	/// &quot;Section 3.1 Scheme:&lt;br/&gt;
	/// .....Scheme names consist of a sequence of characters beginning with a letter and followed by any combination of letters, digits, plus ("+"), period ("."), or hyphen("-"). Although schemes are case-insensitive, the canonical form is lowercase and documents that specify schemes must do so with lowercase letters.&quot;
	/// 
	/// Place your scheme name in the Config.daml within your custom scheme resource
	/// handler registration DAML.In this sample, the corresponding resource
	/// handler registration in the DAML looks like this:
	/// </remarks>
	/// <example>
	/// <code>
	/// <updateCategory refID="esri_cef_customSchemes">
	///			
	///			<insertComponent id="ChromiumWebBrowserSample_Custom_ResourceHandler" 
	///			                 className="ChromiumWebBrowserSample.Custom.ImageResourceSchemeHandler">
	///				<content SchemeName="resource"/>
	///			</insertComponent>
	///
	///			<insertComponent id="ChromiumWebBrowserSample_Custom_ResourceHandler2" 
	///			                 className="ChromiumWebBrowserSample.Custom.EmbeddedResourceSchemeHandler">
	///				<content SchemeName="embeddedresource"/>
	///			</insertComponent>
	///
	///		</updateCategory>
	/// </code>
	/// </example>
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
				return _this ?? (_this = (Module1)FrameworkApplication.FindModule("ChromiumWebBrowserSample_Module"));
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
