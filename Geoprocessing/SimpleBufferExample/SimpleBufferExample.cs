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
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBufferExample
{
  internal class SimpleBufferExample : Button
  {
    protected override async void OnClick()
    {
      try
      {

        var toolName = "analysis.Buffer";  // copied from Python after "arcpy."

        // all GP Tool input parameters in their proper sequence
        var in_features = @"SouthShoreImpact";
        var out_feature_class = @"C:\Data\ParcelFabric\Island\ParcelSample.gdb\SouthShoreImpact_Buffer";
        var buffer_distance_or_field = "50 Meters";
        var line_side = "FULL";
        var line_end_type = "ROUND";
        var dissolve_option = "NONE";
        // dissolve_field = None => make this parameter a null
        var method = "PLANAR";

        var values = Geoprocessing.MakeValueArray(new object[] {
                      in_features,
                      out_feature_class,
                      buffer_distance_or_field, 
                      line_side,
                      line_end_type,
                      dissolve_option, 
                      null,
                      method
                    });

        var gpResult = await Geoprocessing.ExecuteToolAsync(toolName, values,
                null, null, null, GPExecuteToolFlags.Default);
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
