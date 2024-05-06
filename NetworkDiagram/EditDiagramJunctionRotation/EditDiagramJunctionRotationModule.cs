/*

   Copyright 2022 Esri

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
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EditDiagramJunctionRotation
{
  /// <summary>
  /// This add-in demonstrates the programmatic rotation of Junctions in a Network Diagram.
  /// Attributes cannot be manually edited from network diagrams; nor the attributes stored in the diagram feature classes, nor those coming from the join with the associated network source classes.
  /// However, there are two attributes on the network diagram side that are editable by code; the diagram feature Shape and diagram junction Rotation attributes. In this add-in sample, you will learn about how to edit the Rotation attribute on the diagram junction class.
  /// NOTE: The workflow steps detailed below is based on a sample utility network dataset for which the diagram layer definition on the related diagram templates have been set up so the junction symbol rotates according to this Rotation attribute value. The code provided with this add-in sample can apply to any utility network dataset. However, to get the diagram junction symbols being rotated according to the Rotation field value, you must set up the diagram layer definition on your templates.
  /// To learn more about how to get diagram junction symbols vary by rotation, see https://pro.arcgis.com/en/pro-app/latest/help/data/network-diagrams/refine-the-diagram-layer-definition-on-a-template.htm
  ///  
  /// Community Sample data (see under the "Resources" section for downloading sample data) has a UtilityNetworkSamples.aprx 
  /// project that contains a utility network that can be used with this sample.  This project can be found under the 
  /// C:\Data\UtilityNetwork folder. Alternatively, you can also use any utility network data with this sample, although constant 
  /// values may need to be changed.
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu.  Then select Build Solution.  
  /// 1. Click Start button to open ArcGIS Pro.  
  /// 1. ArcGIS Pro will open.  
  /// 1. Open C:\Data\NetworkDiagrams\SDKSampleNetwork\SDKSampleNetwork.aprx
  /// 1. Click on the Map tab on the ribbon. Then, in the Navigate group, expand Bookmarks and click Full Extent.
  /// 1. Click on the Utility Network tab in the ribbon. Then, in the Diagram group, click Find Diagrams.
  /// 1. In the Find Diagrams pane, search for the diagram stored as RotateDiagramJunctions_Test and double click it so it opens
  ///     ![UI](Screenshots/EditDiagramJunctionRotation1.png)
  /// 1. Click on the Add-in tab on the ribbon  
  /// 1. In the Rotation group, type any degree angle value you want in the text box. For example, type 30.
  ///     ![UI](Screenshots/EditDiagramJunctionRotation2.png)
  /// 1. Then, click on the Relative tool.
  ///  All the diagram junction symbols rotate by 30 degrees.
  ///     ![UI](Screenshots/EditDiagramJunctionRotation3.png)
  /// 1. On the Contents pane, right click the Medium Voltage Transformer Bank layer and click Attribute Table.
  /// 1. Right click the Electric Junction Box layer and click Attribute Table.
  /// 1. Have a look to the Element rotation field in the two open attribute tables. Any diagram feature Element rotation is set to 30.
  ///     ![UI](Screenshots/EditDiagramJunctionRotation4.png)
  /// 1. On the Contents pane, right-click the Electric Junction Box layer and click Selection > Select All.
  /// 1. In the Rotation group, type another degree angle value in the text box. For example, type 45.
  /// 1. Then, click on the Relative tool.
  /// All the selected Electric Junction Box diagram junction symbols rotate by 45 degrees more.
  /// 1. Have a look to the Element rotation field in the Electric Junction Box attribute table. Any Element rotation field in this table is set to 75.
  ///     ![UI](Screenshots/EditDiagramJunctionRotation5.png)
  /// 1. Have a look to the Element rotation field in the Medium Voltage Transformer Bank attribute table. Any Element rotation field in this table is still set to 30.
  /// 1. Clear the selection in the diagram map.
  /// 1. Click on the Add-in tab on the ribbon  
  /// 1. In the Rotation group, type 0 as the degree angle value in the text box.
  /// 1. Then, click on the Absolute tool.
  /// All the diagram junction symbols in the diagram rotate so they are restored to their initial angles.
  ///     ![UI](Screenshots/EditDiagramJunctionRotation6.png)
  /// </remarks>
  internal class EditDiagramJunctionRotationModule : Module
  {
    private static EditDiagramJunctionRotationModule _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static EditDiagramJunctionRotationModule Current
    {
      get
      {
        return _this ?? (_this = (EditDiagramJunctionRotationModule)FrameworkApplication.FindModule("EditDiagramJunctionRotation_Module"));
      }
    }

    #region Overrides
    /// <summary>
    /// Called by Framework when ArcGIS Pro is closing
    /// </summary>
    /// <returns>False to prevent Pro from closing, otherwise True</returns>
    protected override bool CanUnload()
    {
      return true;
    }

    #endregion Overrides

    /// <summary>
    /// Rotation value
    /// </summary>
    internal static double Rotation;

    /// <summary>
    /// Get Diagram Layer from Map
    /// </summary>
    /// <param name="map">Map</param>
    /// <returns>Diagram Layer</returns>
    internal static DiagramLayer GetDiagramLayerFromMap(Map map)
    {
      if (map == null || map.MapType != MapType.NetworkDiagram)
        return null;

      IReadOnlyList<Layer> myLayers = map.Layers;
      if (myLayers == null)
        return null;

      foreach (Layer l in myLayers)
      {
        if (l.GetType() == typeof(DiagramLayer))
          return l as DiagramLayer;
      }

      return null;
    }
  }
}
