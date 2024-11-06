//
// Copyright 2024 Esri 
//
// 
//   Licensed under the Apache License, Version 2.0 (the "License"); 
//   you may not use this file except in compliance with the License. 
//   You may obtain a copy of the License at 
//
//       https://www.apache.org/licenses/LICENSE-2.0 
//
//   Unless required by applicable law or agreed to in writing, software 
//   distributed under the License is distributed on an "AS IS" BASIS, 
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//   See the License for the specific language governing permissions and 
//   limitations under the License. 


using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;


namespace KnowledgeGraphConstructionTools
{
  /// <summary>
  /// This sample illustrates how to build construction tools for KnowledgeGraph entity and relationships. 
  /// The sketch tool displays a UI on the overlay using the OverlayControlID property of the MapTool. 
  /// The UI requires the user to specify the source and destination entity types along with the relate type and 
  /// the relate direction. 
  /// The tool uses the SketchTip property to direct the user to identify a single source entity. Once a source entity
  /// has been found, the SketchTip is updated to direct the user to identify destination entities. 
  /// Once destination entities have been found, the edit is performed for each source and destination entity pair. 
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu.Then select Build Solution.
  /// 1. Launch the debugger to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open, select a project containing a link chart with a KnowledgeGraph. 
  /// 1. Open the Create features window and activate an entity template to see the custom construction tool for entities.
  /// ![UI](Screenshots/EntityConstructionTool.png)      
  /// 1. Activate the custom construction tool and click on the link chart to create an entity. 
  /// 1. Open the Create features window and activate a relate template to see the custom construction tool for relationships.
  /// ![UI](Screenshots/RelateConstructionTool.png)      
  /// 1. Activate the custom construction tool and move your mouse over the link chart.  Note the SketchTip and cursor
  /// ![UI](Screenshots/RelateTool_1_ClickOnFromEntity.png)      
  /// 1. Hover over an entity and see the SketchTip and Cursor change.
  /// ![UI](Screenshots/RelateTool_2_HoverOriginEntity.png)      
  /// 1. Click on an entity to identify it as the origin entity. Notice the SketchTip and the connecting line draw 
  /// on the overlay as you move the mouse.
  /// ![UI](Screenshots/RelateTool_3_IdentifiedOriginEntity.png)      
  /// 1. Hover over a second entity. Notice the SketchTip, Cursor and overlay change.
  /// ![UI](Screenshots/RelateTool_4_HoverDestinationEntity.png)      
  /// 1. Click on the entity to identify it as the destination entity. The relationship record is created.
  /// ![UI](Screenshots/RelateTool_5_RelationshipCreated.png)      
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("KnowledgeGraphConstructionTools_Module");

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
