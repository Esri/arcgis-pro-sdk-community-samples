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
using ArcGIS.Core.Data.UtilityNetwork.Trace;
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

namespace GeoProcesssingEventsWithUI
{
  internal class GPExample : Button
  {
    internal static int Iteration = 0;

    protected override async void OnClick()
    {
      // if _Buffer layer exists use that as an input
      // check if we ran this command before and if so we use that as the input
      var inName = Iteration == 0 ? $@"{Module1.TestFcName}" : $@"{Module1.TestFcName}_Buf_{Iteration}";
      var outName = $@"{Module1.TestFcName}_Buf_{Iteration+1}";
      try
      {
        var toolName = "sa.AddSurfaceInformation";  // copied from Python after "arcpy."

        // all GP Tool input parameters in their proper sequence
        var in_featureclass = $@"citieslong";
        var in_surface = $@"https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer";
        var out_property = "Z";
        var method = "BILINEAR";
        var sample_distance = "None";
        var z_factor = "1";
        var pyramid_level_resolution = "0";
        var noise_filtering = "";
        GPFeedbackViewModel.AddGPStatusMessage($@"Ready to run: Buffer: in [{in_featureclass}] out [{in_surface}] by {out_property}");
        var values = Geoprocessing.MakeValueArray([
                      in_featureclass,
                      in_surface,
                      out_property,
                      method,
                      sample_distance,
                      z_factor,
                      pyramid_level_resolution,
                      noise_filtering
                    ]);
        GPFeedbackViewModel.ClearGPStatusMessage();
        var gpResult = await Geoprocessing.ExecuteToolAsync(toolName, values,
                null, null, (eventName, o) =>
                {
                  //     • "OnValidate" - (o as IGPMessage[])
                  //     • "OnMessage" - (o as IGPMessage)
                  //     • "OnProgressMessage" - (string)o
                  //     • "OnProgressPos" - (int)o
                  //     • "OnBeginExecute" - o = null
                  //     • "OnEndExecute" - (o as IGPResult)
                  switch (eventName)
                  {
                    case "OnValidate":
                      {
                        var messages = o as IGPMessage[];
                        foreach (var message in messages)
                        {
                          GPFeedbackViewModel.AddGPStatusMessage($@"GP {eventName} = {message.Text}");
                        }
                      }
                      break;
                    case "OnMessage":
                      {
                        var message = o as IGPMessage;
                        GPFeedbackViewModel.AddGPStatusMessage($@"GP {eventName} = {message.Text}");
                      }
                      break;
                    case "OnProgressMessage":
                      {
                        var message = o as string;
                        GPFeedbackViewModel.AddGPStatusMessage($@"GP {eventName} = {message}");
                      }
                      break;
                    case "OnProgressPos":
                      {
                        var pos = (int)o;
                        GPFeedbackViewModel.AddGPStatusMessage($@"GP {eventName} = position: {pos}");
                        GPFeedbackViewModel.ProgressBarValue(Convert.ToDouble(pos));
                      }
                      break;
                    case "OnBeginExecute":
                      {
                        var gpResult = o as IGPResult;
                        GPFeedbackViewModel.AddGPStatusMessage($@"GP {eventName}");
                      }
                      break;
                    case "OnEndExecute":
                      {
                        var result = o as IGPResult;
                        if (result.IsFailed || result.ErrorCode != 0)
                        {
                          var errorMsg = string.Join(Environment.NewLine, result.ErrorMessages.Select(m => m.Text));
                          GPFeedbackViewModel.AddGPStatusMessage($@"GP {eventName} execute failed: {errorMsg}");
                        }
                        else
                        {
                          var messages = string.Join(Environment.NewLine, result.Messages);
                          GPFeedbackViewModel.AddGPStatusMessage($@"GP {eventName} finished: {messages}");
                        }
                      }
                      break;
                    default:
                      GPFeedbackViewModel.AddGPStatusMessage($@"GP {eventName} = {o}");
                      break;
                  }
                }, GPExecuteToolFlags.Default|GPExecuteToolFlags.AddToHistory);
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
            var errorMsg = string.Join(Environment.NewLine, gpResult.ErrorMessages.Select(m => m.ErrorCode.ToString()));
            MessageBox.Show($@"GP Tool {toolName} failed with '{errorMsg}', check parameters.", "GP Tool failed", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
          }
        }
        else
        {
          Iteration++;
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }

  }
}
