/*

   Copyright 2026 Esri

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

namespace PublishFeatureLyrPython
{
  internal class RunGPToolInspector : Button
  {
    protected override async void OnClick()
    {      
      // GP Tool: "PublishFeatureLyr"
      await QueuedTask.Run(async () =>
      {
        // Parameters PublishFeatureLyr
        // check if we have a valid connection to arcgis portal
        var active_portal = ArcGISPortalManager.Current?.GetActivePortal();
        if (active_portal == null)
        {
          MessageBox.Show("No valid connection to portal. Please sign in to your portal.");
          return;
        }
        var isSignedIn = await QueuedTask.Run<bool>(() =>
        {
          return active_portal.IsSignedOn();
        });
        if (!isSignedIn)
        {
          MessageBox.Show("Please sign in to your active portal.");
          return;
        }
        // in_feature (Required):
        // datatype: Feature Layer
        // find the first feature layer from the current active map to use as input for in_feature parameter
        var in_feature = MapView.Active?.Map?.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault();

        // serviceName (Required):
        // datatype: String
        var serviceName = "My Feature Service";

        // serviceSummary (Required):
        // datatype: String
        var serviceSummary = "This is a summary of the web layer service.";

        // serviceDescription (Required):
        // datatype: String
        var serviceDescription = "This is a description of the web layer.";

        // serviceCredits (Required):
        // datatype: String
        var serviceCredits = "Your credits here.";

        // serviceUseLimits (Required):
        // datatype: String
        var serviceUseLimits = "Your use limitations here.";

        // servicePortalFolder (Required):
        // datatype: String
        var servicePortalFolder = "MyFolder2";

        // serviceTags (Required):
        // datatype: String
        var serviceTags = "Tag1, Tag2";

        // out_result (Derived):
        // datatype: String
        var out_result = "";

        var parameters = Geoprocessing.MakeValueArray(
          in_feature,
          serviceName,
          serviceSummary,
          serviceDescription,
          serviceCredits,
          serviceUseLimits,
          servicePortalFolder,
          serviceTags,
          out_result
        );

        // Derived output for PublishFeatureLyr
        // Running tool: PublishFeatureLyr
        var toolName = "CustomSharing.PublishFeatureLyr";
        IGPResult gpResult = await Geoprocessing.ExecuteToolAsync(toolName, parameters);
        if (gpResult != null)
        {
          Geoprocessing.ShowMessageBox(
            gpResult.Messages,
            "Geoprocessing Result",
            gpResult.IsFailed ? GPMessageBoxStyle.Error : GPMessageBoxStyle.Default
          );
          // get multiple output results for PublishFeatureLyr
          if (gpResult.Values != null && gpResult.ValueTypes != null)
          {
            using IEnumerator<string> enumeratValues = gpResult.Values.GetEnumerator();
            using IEnumerator<string> enumeratValueTypes = gpResult.ValueTypes.GetEnumerator();
            while (enumeratValues.MoveNext() && enumeratValueTypes.MoveNext())
            {
              System.Diagnostics.Trace.WriteLine($"Value: {enumeratValues.Current}, Type: {enumeratValueTypes.Current}");
            }
          }
        }
      });
    }
  }
}
