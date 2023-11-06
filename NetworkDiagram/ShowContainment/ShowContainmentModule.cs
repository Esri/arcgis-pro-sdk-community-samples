/*

   Copyright 2023 Esri

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

namespace ShowContainment
{
  /// <summary>
  /// This is a very simple add-in code sample. The purpose is to demonstrate how to create a new diagram from diagram features selected in any open diagram.
  ///
  /// In this sample, the two add-in command codes expect a diagram feature representing a container feature or container object in the utility network to be selected in any open diagram map.
  /// Then they use this utility network container feature or object as the only input network element to generate another diagram.
  ///
  /// The Show All Content Levels command creates a diagram starting from this input network element based on the ExpandContainers template available by default with any utility network.
  ///
  /// The Show 1st Content Level command starts the creation of the resulting diagram using the Basic template. Then, it runs the Extend operation with the ExtendByContainment type to extend the diagram content by on containment level.
  /// > NOTE: These two sample add-in commands are generic and work with any utility network when the Basic and ExpandContainers diagram templates installed by default at utility network creation still exist without any alteration of their settings.
  ///
  /// </summary>
  /// <remarks>
  /// The main workflow steps to test these commands are detailed below:
  /// 
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. Load any utility network map.
  /// 1. In the Contents pane window, right-click the utility network layer and click Properties.
  /// 1. Click the Network Diagrams tab and have a look to the templates listed in the Diagram Templates table.
  /// ![UI](Screenshots/CheckDefaultDiagramTemplateExistence.png)
  /// 1. Verify that both the Basic and ExpandContainers templates exist in this list. Check also that the Extend Diagram cell displays Enabled for the Basic template.
  /// 1. These are pre-requisites for the add in commands to work without errors.
  /// 1. Search for any container feature or container object in your network and select one or more. You can also select some of its neighbor features if you want.
  /// 1. On the Utility Network tab in the ribbon, in the Diagram group, click the drop down arrow under New and click either CollapseContainers or Basic.
  /// ![UI](Screenshots/InitialDiagramSample.png)
  /// 1. In the newly open diagram map, identify any diagram junction representing a utility network container and select it.
  /// 1. Click on the Add-In tab on the ribbon and click Show All Contents.
  /// ![UI](Screenshots/ShowContainmentToolsGroupInAddInTab.png)
  /// 1. A diagram based on the ExpandContainers template opens in a new diagram map. It shows the whole containment hierarchy related to the utility network container you selected in your first diagram.
  /// ![UI](Screenshots/ExpandContainersFromSelectedDiagramJunctionA.png)
  /// 1. Go back to the first diagram map you created and make it the active diagram map.
  /// 1. Make sure the utility network container you selected in it is still selected.
  /// 1. Click on the Add-In tab on the ribbon and click Show 1st Content Level.
  /// ![UI](Screenshots/ShowContainmentToolsGroupInAddInTabBIS.png)
  /// 1. A diagram based on the Basic template opens in a new diagram map. This time, the new diagram shows the first level of containment related to the utility network container you selected.
  /// ![UI](Screenshots/BasicExtentOneLevelFromSelectedDiagramJunctionB.png)
  /// 1. For the case the newly open diagram references diagram junctions that represent utility network containers, select one of this junction, click on the Add-In tab on the ribbon and click Show 1st Content Level.
  /// ![UI](Screenshots/BasicExtentOneLevelFromSelectedDiagramJunctionC.png)
  /// 1. Another diagram based on the Basic template opens in a new diagram map showing the 1st level of containment related to this particular utility network container.
  /// 1. Starting from this new diagram, you can repeat the operation until you reach the very last containment hierarchy level if you want.
  /// ![UI](Screenshots/BasicExtentOneLevelFromSelectedDiagramJunctionD.png)
  /// </remarks>
  internal class ShowContainmentModule : Module
  {
    private static ShowContainmentModule _this = null;


    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>

    public static ShowContainmentModule Current
    {
      get
      {
        return _this ?? (_this = (ShowContainmentModule)FrameworkApplication.FindModule("ShowContainment_Module"));
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
