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
using ArcGIS.Desktop.Mapping.Events;
//using LASDatasetAPISamples.FilterSettings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace LASDatasetAPISamples
{
  /// <summary>
  /// This sample illustrates LAS Dataset layer Api methods available.
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
  /// 1. Make sure that the Sample data is unzipped in c:\data
  /// 1. The project used for this sample is 'C:\Data\3DAnalyst\3DLayersMap.ppkx'
  /// 1. In Visual Studio click the Build menu.Then select Build Solution.
  /// 1. Launch the debugger to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open, select the 3DLayersMap.ppkx project package. Note: Alternatively, you can open any map with a LAS layer. Using this sample, you will be retrieving LAS points and storing them in PointsFromLASLayer point feature class available in the project's home folder.  So this feature class needs to be in your map with the LAS layer.
  /// 1. If using 3DLayersMap.ppkx, activate the Las map. This map has a Tile_136000_451000.zlas and a PointsFromLASLayer layer. The PointsFromLASLayer layer is empty.
  /// 1. Click the LAS Dataset API tab. Explore the controls available on this tab.
  /// ![UI](Screenshots/LasApiTab.png)
  /// 1. The "Filter Settings" button opens the "Las Dataset Filter" dockpane. Pick the Las dataset layer from the drop down (if you have multiple LAS layers in your map). This dockpane allows you to set the options for the LAS Dataset's classification codes, return values and classification flags. The current values for these settings from the LAS layer are pre-populated in the dockpane. The dockpane is shown below. 
  /// ![UI](Screenshots/FilterSettingsDockpane.png)
  /// 1. You can now modify these setting values for the class codes, return values and the flags. Click the Display Filter or the Retrieve Points buttons.
  /// 1. Display Filter button: This button allows you to visualize the points that satisfy filters set for Classification Codes, Return Values and Classification Flags.
  /// 1. Retrieve Points button: This button allows you to retrieve the points in the LAS dataset that satisfy the criteria set in the filter. The retrieved points will be saved in the PointsFromLASLayer feature class.
  /// 1. The "Las Symbology" gallery allows you to symbolize the LAS Dataset using points, lines or surface renderers.
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;
    
    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("LASDatasetAPISamples_Module");

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
    protected override bool Initialize()
    {
      return base.Initialize();
    }  
  }
}
