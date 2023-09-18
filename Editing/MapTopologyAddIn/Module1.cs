//   Copyright 2022 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace MapTopologyAddIn
{
  /// <summary>
  /// This sample provides two buttons on the AddIn tab in ArcGIS Pro. One of them (Build Map Topology Graph button), builds the map topology graph 
  /// for the current map view extent and shows the number of nodes and edges in that graph in a popup window.
  /// The 2nd button ("Show Topology connections") allows you to open up a custom dock pane that will help you identify all the topologically 
  /// connected features for the currently selected feature in the map.
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.  
  /// 1. Click Start button to open ArcGIS Pro.  
  /// 1. ArcGIS Pro will open.  
  /// 1. Open a map view that contains editable points, polylines and/or polygon data.  
  /// ![UI](Screenshots/MapExtent.jpg)  
  /// 1. Click on the AddIn tab on the ribbon. You will see a group named Map Topology AddIn and two buttons in that group named "build Map Topology Graph" and "Show Topology connections".  
  /// ![UI](Screenshots/AddInTab2.jpg)  
  ///   
  /// **Part I:**  
  ///   
  /// 1. Click on the "Build Map Topology Graph" button. Notice that the button is now toggled ON and appears in the color blue to indicate it is toggled ON. What this does, is it creates the map topology graph for the current extent of the map that is visible on the screen, and it highlights all the nodes and edges that are part of the graph. It also displays a window showing the number of nodes and edges in the graph created. Hit OK in the popup window.  
  /// ![UI](Screenshots/BuildGraph1.jpg)  
  /// 1. Click the "Build Map Topology Graph" button again. You will see that the button is toggled OFF and the overlay containing the nodes and edges is cleared from the map.  
  /// ![UI](Screenshots/BuildGraph2.jpg)  
  /// 1. Now, zoom or pan on the map to change it's extent or what is displayed in the map. Then try step 6 again and watch how the results vary according to the map extent. Clear the overlay when done by toggling the button OFF.  
  ///   
  /// **Part II:**  
  ///   
  /// 1. Click the "Show Topology connections" button from the AddIn tab on the ribbon.  
  /// ![UI](Screenshots/DockPane1.jpg)  
  /// 1. A custom dockpane opens up with the name "Topology connections of a feature".  
  /// ![UI](Screenshots/DockPane2.jpg)  
  /// 1. From the Edit tab or the Map tab on the ribbon, enable the selection tool and select a feature on the map.  
  /// ![UI](Screenshots/SelectionTool.jpg)  
  /// 1. All the nodes and edges connected to the selected feature on the map, will be highlighted, and all the features that are topologically connected to the selected feature will be listed on the dockpane by their Feature Class Name and Object ID.  
  /// ![UI](Screenshots/DockPane3.jpg)  
  /// 1. Click on one of the features listed on the pane, and that feature will flash on the map view.  
  /// ![UI](Screenshots/DockPane4.jpg)  
  /// 1. Select any other feature using the selection tool. You'll find that the previous overlay is cleared and the results for the newly selected feature is displayed.  
  /// </remarks>

  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>

    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("MapTopologyAddIn_Module");

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
