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
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PublishFeatureLyrPython
{
  internal class RunMyPython : Button
  {
    protected override async void OnClick()
    {
      {
        try
        {
          // get the first feature layer from the current active map
          var featureLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault();
          if (featureLayer == null)
          {
            MessageBox.Show("There are no feature layers in the current map.");
            return;
          }
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
          // Prepare to call the python script via ExecuteToolAsync
          var serviceName = "My Feature Service";
          var serviceSummary = "This is a summary of the web layer service.";
          var serviceDescription = "This is a description of the web layer.";
          var serviceCredits = "Your credits here.";
          var serviceUseLimits = "Your use limitations here.";
          var servicePortalFolder = "MyFolder2";
          var serviceTags = "Tag1, Tag2";
          var sharingParams = new object[]
          {
          featureLayer,   // in_feature
          serviceName,   // service_name
          serviceSummary,  // service_summary
          serviceDescription, // service_description
          serviceCredits,  // service_credits
          serviceUseLimits, // service_use_limits
          servicePortalFolder, // service_portal_folder
          serviceTags, // service_tags
          };

          var parameters = Geoprocessing.MakeValueArray(sharingParams);
          var gpResult = await Geoprocessing.ExecuteToolAsync("CustomSharing.PublishFeatureLyr", parameters, null, null, null);
          if (gpResult.IsFailed)
          {
            var errorMsg = string.Join(Environment.NewLine, gpResult.ErrorMessages.Select(m => m.Text));
            MessageBox.Show($"Error occurred while executing the python script. {errorMsg}");
          }
          else
          {
            MessageBox.Show("The feature layer has been successfully uploaded as a web layer.");
          }
        }
        catch (Exception ex)
        {
          MessageBox.Show($@"An error occurred in RunPyScriptButton: {ex.ToString()}");
        }
      }
    }
  }
}
