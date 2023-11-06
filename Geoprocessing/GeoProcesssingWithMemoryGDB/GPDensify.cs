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
using ArcGIS.Core.Data.DDL;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Geoprocessing;
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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoProcesssingWithMemoryGDB
{
  internal class GPDensify : Button
  {
    protected override async void OnClick()
    {
      try
      {
        var toolName = "edit.Densify";  // copied from Python after "arcpy."
        var inName = GPBuffer.Iteration == 0 ? $@"{Module1.TestFcName}" : $@"{Module1.TestFcName}_Buf_{GPBuffer.Iteration}";

       
        // all GP Tool input parameters in their proper sequence
        var in_features = $@"memory\{inName}";
        var ensification_method = "DISTANCE";
        var distance = "10 Meters";
        var max_deviation = "0.1 Meters";
        var max_angle = 10;
        var max_vertex_per_segment = 100;
        MemoryGDBStatsViewModel.AddStatusMessage($@"Densify: in [{in_features}]");

        var values = Geoprocessing.MakeValueArray(new object[] {
                      in_features,
                      ensification_method,
                      distance,
                      max_deviation,
                      max_angle,
                      max_vertex_per_segment
                    });

        MemoryGDBStatsViewModel.ClearGPStatusMessage();
        var gpResult = await Geoprocessing.ExecuteToolAsync(toolName, values,
                null, null, (eventName, o) =>
                {
                  MemoryGDBStatsViewModel.AddGPStatusMessage($@"GP {eventName} = {o.ToString()}");
                }, GPExecuteToolFlags.Default | GPExecuteToolFlags.AddToHistory);

        // gpResult is the returned result object from a call to ExecuteToolAsync
        if (gpResult.IsFailed)
        {
          // display error messages if the tool fails, otherwise shows the default messages
          if (gpResult.Messages.Count() != 0)
          {
            Geoprocessing.ShowMessageBox(gpResult.Messages, "GP Messages",
                            gpResult.IsFailed ?
                            GPMessageBoxStyle.Error : GPMessageBoxStyle.Default);
          }
          else
          {
            MessageBox.Show($@"GP Tool {toolName} failed with errorcode, check parameters.");
          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }

  }
}
