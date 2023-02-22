/*

   Copyright 2022 Esri

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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExecuteSnap
{
  internal class ExecuteSnap : Button
  {
    protected override async void OnClick()
    {
      try
      {
        GPExecuteToolFlags executeFlags = GPExecuteToolFlags.AddOutputsToMap | GPExecuteToolFlags.GPThread | GPExecuteToolFlags.AddToHistory;

        string toolName = "edit.Snap";
        var environments = Geoprocessing.MakeEnvironmentArray(overwriteoutput: true);

        // make a copy of the input data - this inputFeatures dataset will be modified by being
        // snapped to data specified in snap environment
        var inputFeatures = @"C:\Data\GeoProcessing\EditSnap\data.gdb\Redlines";

        // snapEnvironment parameter is of ValueTable type: arcpy.edit.Snap("Redlines", "Greenline_2 EDGE '75 Meters'")
        var snapEnvironments = @"C:\Data\GeoProcessing\EditSnap\data.gdb\Greenline_2 END '50 Meters'";

        var parameters = Geoprocessing.MakeValueArray(inputFeatures, snapEnvironments);
        var gpResult = await Geoprocessing.ExecuteToolAsync(toolName, parameters, environments, null, null, executeFlags);
        Geoprocessing.ShowMessageBox(gpResult.Messages, "GP Messages", gpResult.IsFailed ? GPMessageBoxStyle.Error : GPMessageBoxStyle.Default);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }
  }
}
