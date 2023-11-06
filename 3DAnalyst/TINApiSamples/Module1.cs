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
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.Analyst3D;
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

namespace TINApiSamples
{
  /// <summary>
  /// This sample illustrates TIN Api methods available to iterate and work with TIN Nodes, edges and Triangles.
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
  /// 1. Make sure that the Sample data is unzipped in c:\data
  /// 1. The project used for this sample is 'C:\Data\3DAnalyst\3DLayersMap.ppkx'
  /// 1. In Visual Studio click the Build menu.Then select Build Solution.
  /// 1. Launch the debugger to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open, select the 3DLayersMap.ppkx project package. Note: Alternatively, you can open any map with a TIN layer and a graphics layer. Using this sample, you will be adding points, lines and polygon graphic elements to explore the TIN layer's nodes, edges and triangles.
  /// 1. If using 3DLayersMap.ppkx, activate the TIN map. This map has a graphics layer. 
  /// 1. Click the TIN API tab. Explore the tools and buttons available on this tab.
  /// ![UI](Screenshots/TINApiTab.png)
  /// 1. The "Search elements using filters" group has three tools: Nodes, Edges, and Triangles. There is also a combo box that allows you to set the FilterType setting (All, InsideDataArea, InsideTin) and a check box to set the "DataElementsOnly" property of the Search filter.  Active the tool and sketch a small rectangle on the TIN. Nodes, Edges and Triangles in the sketch rectangle will be found. Graphic elements to represent these elements will be drawn on the map.
  /// ![UI](Screenshots/SearchTINElements.png)
  /// 1. The "Proximity Analysis" group has 4 tools: Nearest Node, Nearest Edge, Nearest Triangle, Triangle Neighbors. Activate any of these tools and sketch a point on the TIN. Nodes, Edges and Triangles closest to the sketched point will be detected.  Graphic elements to represent these elements will be drawn on the map.
  /// 1. The "Get Extents" button calculates the TIN dataset's "Extent", "DataArea Extent", "Full Extent" and the "Super Node Extent". Polygon graphics to represent these extents are drawn on the map.
  /// 1. The "TIN Symbology" gallery allows you to symbolize the TIN using points, lines or surface renderers.
  /// ![UI](Screenshots/TINRenderers.png)
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;
    internal TinFilterType _tinFilterType = TinFilterType.All;
    internal bool _dataElementsOnlyValue = false;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("TINApiSamples_Module");

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

    public static bool IsView2D()
    {
      //Get the active map view.
      var mapView = MapView.Active;
      if (mapView == null)
        return false;

      //Return whether the viewing mode is SceneLocal or SceneGlobal
      return mapView.ViewingMode == MapViewingMode.Map;
    }
  }
}
