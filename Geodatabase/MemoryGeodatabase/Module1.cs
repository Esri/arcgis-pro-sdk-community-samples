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

namespace MemoryGeodatabase
{
  /// <summary>
  /// This sample shows how to create create a memory Geodatabase and then compares the performances of a Memory GDB Point Feature class with a File GDB Point Feature class.
  /// </summary>
  /// <remarks>
  /// 1. In Visual studio rebuild the solution.
  /// 1. Debug the add-in.
  /// 1. ArcGIS Pro opens, create a new Map project using the Map template.
  /// 1. Click the "Show Memory GDB Stats" button to show the "GDB Performance Stats" dockpane. 
  /// 1. Click the "Create GDB" button to show create a memory and add a Test Point feature class to both the Memory GDB and the default File GDB.
  /// 1. Click the "Add Records" button to add "Cycle" number of records to both File and Memory Test Feature Classes.
  /// 1. See the Perfomance stats on the dockpane
  /// ![UI](Screenshots/Screen1.png)
  /// 1. Click the "Calculate Fields" button to update all records in both File and Memory Test Feature Classes. 
  /// ![UI](Screenshots/Screen2.png)
  /// 1. Click the "Add Test F/Cs to Map" button to add the Test Feature classes to the map.
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("MemoryGeodatabase_Module");

    internal static string TestFcName = "xyzTestPoint";

    internal static MemoryGDBStatsViewModel MemoryGDBStatsViewModel
    {
      get ; set;
    }

    private static uint _cycles;
    internal static uint Cycles {
      get {
        return _cycles;
      }
      set {
        _cycles = value;
        if (MemoryGDBStatsViewModel != null)
        {
          MemoryGDBStatsViewModel.Cycles = _cycles.ToString();
        }
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
