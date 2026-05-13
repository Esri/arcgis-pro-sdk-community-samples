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
using ArcGIS.Desktop.Framework.Dialogs;

namespace FilteredFindPathAnalysis
{
  /// <summary>
  /// This sample demonstrates how to use the Knowledge Graph API to do Filtered Find Path Analysis.
  /// </summary>
  /// <remarks>
  /// **Sample Data:**
  /// This sample uses the freely available[Esri Developer BumbleBees Knowledge Graph] (https://developers.arcgis.com/javascript/latest/sample-code/knowledgegraph-query/) dataset included with the[Query Knowledge Graphs](https://developers.arcgis.com/javascript/latest/sample-code/knowledgegraph-query/) javascript sample.
  /// 
  /// 1. Open ArcGIS Pro and add a portal connection to the [sampleserver7 arcgisonline portal](https://sampleserver7.arcgisonline.com/portal) using the following URL `https://sampleserver7.arcgisonline.com/portal`. At the time of this writing, the username and pwd are:
  /// User: viewer01
  /// Pwd:  I68VGU^nMurF
  /// If the password has changed, check the[Query Knowledge Graphs] (https://developers.arcgis.com/javascript/latest/sample-code/knowledgegraph-query/) esri developer's page for the current user/pwd for the sample dataset.
  /// 1. In Visual Studio click the Build menu.Then select Build Solution.
  /// 1. Launch the debugger to open ArcGIS Pro.
  /// 1. Create a new blank Map project.
  /// 1. Set your active portal to be the sampleserver7.arcgisonline portal.In the Catalog dockpane,
  /// 1. select the "Portal" tab and then the "My Organization" secondary tab. Scroll down to find the
  /// 1. `BumbleBees` knowledge graph. Right click on BumbleBees and execute the "Add to New Investigation"
  /// 1. context menu item.
  /// ![UI] (screenshots/Catalog_view.png)
  /// 1. To create a link chart to use with the sample, either create one from the Investigation UI
  /// 1. using the Pro UI or simply execute the "CreateLinkChart" button on the sample "FFP Analysis Samples"
  /// 1. tab on the ribbon.
  /// ![UI] (screenshots/ffp_analysis_sample_ribbon.png)
  /// 1. If you choose to run the "CreateLinkChart" button, it will create a link chart that looks like
  /// 1. this:
  /// ![UI] (screenshots/default_link_chart.png)
  /// 1. You can now run the sample analysis options. There is one button on the Tab for each of the
  /// 1. supported KG FFP Analysis:
  ///  * FFP
  ///  * Expand
  ///  * Filtered Expand
  ///  * Connect
  ///  * Find Between
  ///  Each of these options conforms to built-in options of the same name on the Pro "Link Chart" tab.  Expand, Filtered Expand, and Find Between each require that at least one enity, sometimes two, be selected.
  ///  Consult the [ArcGIS Pro help](https://pro.arcgis.com/en/pro-app/latest/help/data/knowledge/add-relationships-missing-between-entities-in-the-link-chart.htm) for detailed explanations of these options
  /// 1. Note: Expand, Filtered Expand, and Find Between require, minimum, one or two entities to be 
  /// 1. selected on the view otherwise these options will be unavailable/disabled on the tab.
  /// </remarks>
  internal class Module1 : Module
	{
		private static Module1 _this = null;

		//This is a publicly available KG and is provided as part of the Esri Developer
		//sample data. Refer to https://developers.arcgis.com/javascript/latest/sample-code/knowledgegraph-query/
		//for the related javascript sample. The user and pwd for the service at the time
		//of writing is:
		//
		//user: viewer01
		//pwd: I68VGU^nMurF
		//
		//Check the above link to the developer javascript query sample for the updated password
		//if needed. You may also need to add a portal connection to your ArcGIS Pro Portals on the Pro
		//backstage tab: "Portals".
		//https://sampleserver7.arcgisonline.com/portal

		internal static readonly string KG_URL =
			 @"https://sampleserver7.arcgisonline.com/server/rest/services/Hosted/BumbleBees/KnowledgeGraphServer";


		public string FFP_Error_Msg { get; set; } = "";

		public void ShowAnyError(string title)
		{
			if (!string.IsNullOrEmpty(FFP_Error_Msg))
			{
				MessageBox.Show(
					$"{FFP_Error_Msg}",
					$"{title} Error",
					System.Windows.MessageBoxButton.OK,
					System.Windows.MessageBoxImage.Error);
			}
		}

		/// <summary>
		/// Retrieve the singleton instance to this module here
		/// </summary>
		public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("FilteredFindPathAnalysis_Module");

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
