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


namespace MemoryGeodatabase
{
  internal class MemoryGDBStatsViewModel : DockPane
  {
    private const string _dockPaneID = "MemoryGeodatabase_MemoryGDBStats";

    protected MemoryGDBStatsViewModel() 
    {
      Module1.MemoryGDBStatsViewModel = this;
      Cycles = Module1.Cycles.ToString();
    }

    /// <summary>
    /// Show the DockPane.
    /// </summary>
    internal static void Show()
    {
      DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
      if (pane == null)
        return;

      pane.Activate();
    }

    private string _FileStatus = "N/A";
    public string FileStatus
    {
      get => _FileStatus;
      set => SetProperty(ref _FileStatus, value);
    }

    private string _MemoryStatus = "N/A";
    public string MemoryStatus
    {
      get => _MemoryStatus;
      set => SetProperty(ref _MemoryStatus, value);
    }

    private string _FilePerformance = "N/A";
    public string FilePerformance
    {
      get => _FilePerformance;
      set => SetProperty(ref _FilePerformance, value);
    }

    private string _MemoryPerformance = "N/A";
    public string MemoryPerformance
    {
      get => _MemoryPerformance;
      set => SetProperty(ref _MemoryPerformance, value);
    }

    private string _FileCount = "N/A";
    public string FileCount
    {
      get => _FileCount;
      set => SetProperty(ref _FileCount, value);
    }

    private string _MemoryCount = "N/A";
    public string MemoryCount
    {
      get => _MemoryCount;
      set => SetProperty(ref _MemoryCount, value);
    }

    private string _cycles = "N/A";
    public string Cycles
    {
      get => _cycles;
      set => SetProperty(ref _cycles, value);
    }

    /// <summary>
    /// Text shown near the top of the DockPane.
    /// </summary>
    private string _heading = "GDB Performance test results";
    public string Heading
    {
      get => _heading;
      set => SetProperty(ref _heading, value);
    }
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class MemoryGDBStats_ShowButton : Button
  {
    protected override void OnClick()
    {
      MemoryGDBStatsViewModel.Show();
    }
  }
}
