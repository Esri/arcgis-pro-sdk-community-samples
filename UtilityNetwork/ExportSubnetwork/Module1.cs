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

namespace ExportSubnetwork
{
  /// <summary>
  /// This add-in demonstrates how to use the Export Subnetwork GP tool to export subnetwork from the Utility Network using the Pro SDK for .Net. 
  /// The sample uses the NapervilleElectricSDKData.gdb, a file geodatabase available in the Community Sample data at C:\Data\UtilityNetwork (see under the "Resources" section for downloading sample data).  
  /// You can use any utility network data with this sample, although constant values may need to be changed.
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu.  Then select Build Solution.  
  /// 1. Click Start button to open ArcGIS Pro.  
  /// 1. ArcGIS Pro will open.  
  /// 1. Open C:\Data\UtilityNetwork\UtilityNetworkSamples.aprx or a map view that references a utility network with the correct schema  
  /// ![UI](Screenshots/Screenshot1.png)
  /// 1. Click on the **ExportSubnetwork** button on the add-in to trigger the **Export Subnetwork Tool** from the Utility Network Tools toolbox.
  /// ![UI](Screenshots/Screenshot2.png)
  /// 1. On successful completion of the export operation, the resulting output, Export.json, is exported into the path of the directory designated for temporary files.
  /// ```json
  ///  {
  ///   "controllers" : [
  ///     {
  ///       "networkSourceId" : 6,
  ///       "globalId" : "{2F82291C-ED2E-40F5-AB36-FEB0C50E3353}",
  ///       "objectId" : 9990,
  ///       "terminalId" : 16,
  ///       "assetGroupCode" : 26,
  ///       "assetTypeCode" : 503,
  ///       "geometry" : {
  ///         "x" : 1028298.19683201239,
  ///         "y" : 1863426.35552436113,
  ///         "z" : 0,
  ///         "m" : null
  ///       },
  ///       "networkSourceName" : "ElectricDevice",
  ///       "assetGroupName" : "Medium Voltage Circuit Breaker",
  ///       "assetTypeName" : "Three Phase Circuit Breaker",
  ///       "terminalName" : "CB:Line Side"
  ///     }
  ///   ],
  ///   "connectivity" : [
  ///     {
  ///       "viaNetworkSourceId" : 6,
  ///       "viaGlobalId" : "{BBB77A64-8CB6-4754-AD95-0F4E91A6B283}",
  ///       "viaObjectId" : 11132,
  ///       "viaPositionFrom" : 0,
  ///       "viaPositionTo" : 1,
  ///       "viaGeometry" : {
  ///         "x" : 1027726.15827792883,
  ///         "y" : 1863365.88910986483,
  ///         "z" : 0,
  ///         "m" : null
  ///       },
  ///       "fromNetworkSourceId" : 6,
  ///       "fromGlobalId" : "{BBB77A64-8CB6-4754-AD95-0F4E91A6B283}",
  ///       "fromObjectId" : 11132,
  ///       "fromTerminalId" : 7,
  ///       "fromGeometry" : {
  ///         "x" : 1027726.15827792883,
  ///         "y" : 1863365.88910986483,
  ///         "z" : 0,
  ///         "m" : null
  ///           .
  ///           .
  ///           .
  ///           . 
  ///  ```
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>

    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("ExportSubnetwork_Module");

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
