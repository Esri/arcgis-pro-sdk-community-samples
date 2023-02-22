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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Internal.Catalog;
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

        // Get UtilityNetwork path
        string utilityNetworkPath = GetUtilityNetworkPath(layers);

        // Dataset specific subnetwork information
        string domainNetwork = "Electric";
        string tier = "Electric Distribution";
        string subnetworkName = "RMT001";
        string outputJson = $"{Path.GetTempPath()}\\Export.json";

        // Export options
        string exportAcknowledged = "NO_ACKNOWLEDGE";
        string includeBarriers = "INCLUDE_BARRIERS";
        string traversabilityScope = "BOTH_JUNCTIONS_AND_EDGES";
        string includeGeometry = "INCLUDE_GEOMETRY";
        string includeDomainDescriptions = "INCLUDE_DOMAIN_DESCRIPTIONS";

        string conditionBarriers = "";
        string functionBarriers = "";
        string propagators = "";

        // Result types
        string resultTypes = "CONNECTIVITY";
        string resultNetworkAttribute = "";
        string resultFields = "";

        // Export options as a readonly array 
        IReadOnlyList<string> paramArray = Geoprocessing.MakeValueArray(utilityNetworkPath,
          domainNetwork,
          tier,
          subnetworkName,
          exportAcknowledged,
          outputJson,
          conditionBarriers,
          functionBarriers,
          includeBarriers,
          traversabilityScope,
          propagators,
          includeGeometry,
          resultTypes,
          resultNetworkAttribute,
          resultFields,
          includeDomainDescriptions
          );

        try
        {
          // Execute geoprocessing tool
          IGPResult result = Geoprocessing.ExecuteToolAsync("un.ExportSubnetwork", paramArray, Geoprocessing.MakeEnvironmentArray(overwriteoutput: true),
            CancelableProgressor.None, GPExecuteToolFlags.AddToHistory).Result;

          // Check if GP operation completed successfully
          if (result.IsFailed)
          {
            IEnumerable<IGPMessage> errors = result.ErrorMessages;
            // Iterate errors
          }
        }
        catch (Exception ex)
        {
          // Handle exceptions
        }
      });
    }

    /// <summary>
    /// Get the utility network path
    /// </summary>
    /// <param name="layers">A set of layers in ArcGIS Pro's TOC</param>
    /// <returns>Utility network path</returns>
    private string GetUtilityNetworkPath(IEnumerable<Layer> layers)
    {
      string unPath = null;
      foreach (Layer layer in layers)
      {
        if (layer is UtilityNetworkLayer utilityNetworkLayer)
        {
          // Get the UN path with feature dataset name
          unPath = GetUNPathHelper(utilityNetworkLayer);
          break;
        }
      }
      return unPath;
    }

    /// <summary>
    /// Get the appropriate utility network path
    /// </summary>
    /// <remarks>
    /// The Pro SDK 3.1 or earlier has a bug in SDK that doesn't return appropriate path on UtilityNetworkLayer.GetPath() or UtilityNetworkLayer.GetUtilityNetwork().GetPath().
    /// Therefore, this helper method is created.
    /// </remarks>
    /// <param name="utilityNetworkLayer">The utility network layer</param>
    /// <returns>Utility network path</returns>
    private string GetUNPathHelper(UtilityNetworkLayer utilityNetworkLayer)
    {
      string unName = utilityNetworkLayer.Name.Split(" ").First();
      string gdbPath = System.IO.Path.GetDirectoryName(utilityNetworkLayer.GetUtilityNetwork().GetPath().AbsolutePath);
      string unFeatureDatasetName = "UtilityNetwork";

      string unPath = $"{gdbPath}\\{unFeatureDatasetName}\\{unName}";

      return unPath;
    }
  }
}
