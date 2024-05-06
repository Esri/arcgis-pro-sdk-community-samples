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
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Desktop.Framework.Dialogs;
using NetworkAttribute = ArcGIS.Core.Data.UtilityNetwork.NetworkAttribute;
using NetworkSource = ArcGIS.Core.Data.UtilityNetwork.NetworkSource;
using Path = System.IO.Path;

namespace ExportSubnetwork
{
  internal class ExportSubnetwork : Button
  {
    protected override void OnClick()
    {
      QueuedTask.Run(() =>
      {
        // Get active layers in the TOC
        IEnumerable<Layer> layers = MapView.Active.Map.Layers;

        // Dataset specific subnetwork name
        string subnetworkName = "RMT001";

        // Path to store export result
        string exportResultJsonPath = $"{Path.GetTempPath()}SubnetworkExportResult.json";

        // Get a utility network 
        using (UtilityNetwork utilityNetwork = GetUtilityNetwork(layers))
        using (UtilityNetworkDefinition utilityNetworkDefinition = utilityNetwork.GetDefinition())
        using (SubnetworkManager subnetworkManager = utilityNetwork.GetSubnetworkManager())
        {
          Subnetwork subnetwork = subnetworkManager.GetSubnetwork(subnetworkName);

          IReadOnlyList<NetworkAttribute> networkAttributes = utilityNetworkDefinition.GetNetworkAttributes();
          IReadOnlyList<NetworkSource> networkSources = utilityNetworkDefinition.GetNetworkSources();

          NetworkSource electricDevice = networkSources.First(f => f.Name.Contains("ElectricDevice"));

          // Set subnetwork export options
          SubnetworkExportOptions subnetworkExportOptions = new SubnetworkExportOptions()
          {
            SetAcknowledged = false,
            IncludeDomainDescriptions = true,
            IncludeGeometry = true,
            ServiceSynchronizationType = ServiceSynchronizationType.Asynchronous,

            SubnetworkExportResultTypes = new List<SubnetworkExportResultType>()
            {
              SubnetworkExportResultType.Connectivity,
              SubnetworkExportResultType.Features
            },

            ResultNetworkAttributes = new List<NetworkAttribute>(networkAttributes),

            ResultFieldsByNetworkSourceID = new Dictionary<int, List<string>>()
              { { electricDevice.ID, new List<string>() { "AssetID" } } }
          };

          try
          {
            // Execute subnetwork export
            subnetwork.Export(new Uri(exportResultJsonPath), subnetworkExportOptions);
          }
          catch (Exception ex)
          {
            MessageBox.Show(ex.Message);
          }
        }

      });
    }

    /// <summary>
    /// Get a utility network 
    /// </summary>
    /// <param name="layers">A set of layers in ArcGIS Pro's TOC</param>
    /// <returns>A utility network </returns>
    private UtilityNetwork GetUtilityNetwork(IEnumerable<Layer> layers)
    {
      foreach (Layer layer in layers)
      {
        if (layer is UtilityNetworkLayer utilityNetworkLayer)
        {
          return utilityNetworkLayer.GetUtilityNetwork();
        }
      }
      return null;
    }
  }
}
