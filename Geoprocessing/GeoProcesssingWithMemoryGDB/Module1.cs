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

namespace GeoProcesssingWithMemoryGDB
{
  /// <summary>
  /// This sample shows how to create create a memory Geodatabase and then uses the Memory Geodatabase for geo processing.
  /// </summary>
  /// <remarks>
  /// 1. In Visual studio rebuild the solution.
  /// 1. Debug the add-in.
  /// 1. ArcGIS Pro opens, create a new Map project using the Map template.
  /// 1. Select the "GP with Memory GDB" tab.
  /// 1. On the "Memory GDB Setup" Group click each button from left to right.  This displays the performance stats dockpane, and adds one feature to a test polygon feature class and displays the single feature on the map.
  /// 1. Click the "Refresh" button on the "GDB Performance Stats" dockpane. This displays the current feature class content of the memory geodatabase.  To start out you should only see the test polygon feature class.
  /// ![UI](Screenshots/Screen1.png)
  /// 1. On the "Memory GDB Setup" Group click any of the GP Tool button to test memory GDB performance.  
  /// ![UI](Screenshots/Screen2.png)
  /// 1. Click the "Undo" button to start from scratch. 
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("GeoProcesssingWithMemoryGDB_Module");

    internal static string TestFcName = "xyzTest";

    internal static string MemoryLayerName = "Memory GDB Layer";

    internal static MemoryGDBStatsViewModel MemoryGDBStatsViewModel
    {
      get; set;
    }

    internal static void OpenStatsDockpane ()
    {
      if (Module1.MemoryGDBStatsViewModel == null)
      {
        MemoryGDBStatsViewModel.Show();
      }
    }

    #region Count records in FC

    internal static long GetRecordCountFeatureClass(MemoryConnectionProperties memoryCPs,
      string fcName)
    {
      using var geoDb = new Geodatabase(memoryCPs);
      var fc = geoDb.OpenDataset<FeatureClass>(fcName);
      return fc.GetCount();
    }

    internal static long GetRecordCountFeatureClass(FileGeodatabaseConnectionPath connectionPath,
      string fcName)
    {
      using var geoDb = new Geodatabase(connectionPath);
      var fc = geoDb.OpenDataset<FeatureClass>(fcName);
      return fc.GetCount();
    }

    #endregion

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
