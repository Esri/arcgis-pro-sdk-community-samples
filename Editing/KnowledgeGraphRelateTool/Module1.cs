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

namespace KnowledgeGraphRelateTool
{
  /// <summary>
  /// This sample illustrates how to build a set of KnowledgeGraph relate records using the EditOperation object. 
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
  /// 1. Select the 'Add-In' tab on the ArcGIS Pro ribbon and activate the 'Create Relate records' tool.
  /// ![UI](Screenshots/relateTool.png)      
  /// 1. In the UI, choose a source entity type, destination entity type, relate type from the dropdowns and 
  /// specify the relate direction. 
  /// 1. Click and drag a rectangle to identify a single source entity. 
  /// 1. Click and drag a rectangle to identify destination entities. 
  /// 1. The application will create a relate record for each of the destination entities. 
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("KnowledgeGraphRelateTool_Module");

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
