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

namespace Calculate_Flow_Arrows
{
    /// <summary>
    /// This add-in allows a user to calculate and draw flow arrows for a subnetwork and display them in their map.
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio open this solution and then rebuild the solution.
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open. 
    /// 1. Open a project and map that contains a utility network with one or more valid subnetworks.
    /// 1. Open the 'Add-in' tab on the Pro ribbon and note the 'Show Flow Arrows' command.
    /// ![UI] (Screenshots/LaunchAddIn.png)
    /// 1. Click the 'Show Flow Arrows' command to open the dock pane for the add-in.
    /// ![UI] (Screenshots/OpenDockpane.png)
    /// 1. Select the Tier you want to analyze in the dropdown, this will populate the clean subnetworks for that tier.
    /// 1. Select the Subnetwork you want to analyze and click the Analyze Subnetwork button.
    /// ![UI] (Screenshots/SelectedSubnetwork.png)
    /// 1. The tool will output several log messages as it exports and analyzes the subnetwork.
    /// ![UI](Screenshots/AnalysisComplete.png)
    /// 1. Once the tool has finished, add the FlowLines layer to your map to visualize the results. A sample layer file is included in the project that includes suggested symbology.
    /// ! [UI](Screenshots / CalculatedFlow.png)
    /// 1.If the tool was run with the Apply Filter option selected, a definition query will be placed on the FlowLines layer and the map will zoom to the selected subnetwork.
    /// 1.You can change the active subnetwork, or remove the filter, by modifying the definition query on the Flow Lines layer using the Ribbon or the layer properties dialog.
    /// ![UI](Screenshots / DefinitionQuery.png)
    /// 
    /// # Options
    /// - Clear Previous Results -Choose whether to delete the contents of the FlowLines layer each time a subnetwork is analyzed.
    /// - Apply Filter - Choose whether a definition query will be added to the FlowLines layer in the map.
    /// -Include Dirty Subnetworks -The subnetworks dropdown doesn't show dirty subnetworks by default. Check this option to show clean and dirty subnetworks. Invalid subnetworks are never shown.
    /// 
    /// # Known Limitations
    /// - The tool does not include propagators in it's analysis of flow. If you want the calcaulations to use propagation you will need to modify the community sample and recompile the add-in.
    /// - The tool only uses a single condition barrier, and the barrier is inferred from the subnetwork definition.If you require something more sophisticated you will need to modify the community sample and recompile the add-in.
    /// -The tool only analyzes a single subnetwork at a time.There is a hidden Analyze Tier button you can enable at your own risk. This requires modifying and recompiling the community sample.
    /// -By default, the tool only shows the first 3,000 subnetworks.If your dataset contains more subnetworks you can modify the command to show more, but you may experience performance issues.
    /// - By default, the tool does not limit the list of subnetworks to the current extent.Implementing logic like this would require modifying the community sample.
    /// -The tool does not include the ability to synthesize geometry or visualize the connectivity of non - spatial obects.
    /// </remarks>
    internal class FlowModule : Module
    {
        private static FlowModule _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static FlowModule Current => _this ??= (FlowModule)FrameworkApplication.FindModule("Calculate_Flow_Arrows_Module");

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
