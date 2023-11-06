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
using System.Text;
using System.Threading.Tasks;

namespace ExecuteModelTool
{
  internal class PromptModelTool : Button
  {
    protected override async void OnClick()
    {
      try
      {
        FeatureLayer theLayer = null;
        var selectedLayer = MapView.Active.GetSelectedLayers().OfType<FeatureLayer>().FirstOrDefault((lyr) => lyr.ShapeType == esriGeometryType.esriGeometryPolyline);
        if (selectedLayer != null)
        {
          theLayer = selectedLayer;
        }
        if (theLayer == null)
        {
          theLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault((lyr) => lyr.ShapeType == esriGeometryType.esriGeometryPolyline);
        }
        if (theLayer == null)
        {
          throw new Exception("A Line Feature Layer is required.");
        }
        CancelableProgressorSource cps = new("GP Tool: Buffering", "Canceled");
        var outputFC = System.IO.Path.Combine(Project.Current.DefaultGeodatabasePath, $@"{theLayer.Name}Prompt{Module1.BufferSuffix}");
        var argsModelTool = Geoprocessing.MakeValueArray(theLayer,
          outputFC,
          "100 Meters",  // Distance__value_or_field_="60 Meters",
          "PLANAR"      // Method = "PLANAR"
        );
        GPExecuteToolFlags mapOutput = GPExecuteToolFlags.AddOutputsToMap;
        // set overwrite flag           
        var env = Geoprocessing.MakeEnvironmentArray(overwriteoutput: true);
        await PromptGPTool("MyModelTools.MyRedlineModelTool", argsModelTool, cps.Progressor, env, mapOutput);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }

    public async Task PromptGPTool(string GPTool,
                            IReadOnlyList<string> parameters,
                            CancelableProgressor prog,
                            IEnumerable<KeyValuePair<string, string>> env = null,
                            GPExecuteToolFlags flag = GPExecuteToolFlags.Default)
    {
      try
      {
        var gp_result = await Geoprocessing.OpenToolDialogAsync(GPTool, parameters, env);
        if (gp_result != null) Geoprocessing.ShowMessageBox(gp_result.Messages, "Geoprocessing Result",
          gp_result.IsFailed ? GPMessageBoxStyle.Error :
            GPMessageBoxStyle.Default);
      }
      catch (Exception ex) 
      {
        MessageBox.Show(ex.ToString());
      }
    }
  }
}
