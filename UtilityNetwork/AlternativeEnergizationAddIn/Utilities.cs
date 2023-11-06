using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace AlternativeEnergizationAddIn
{
  //   Copyright 2019 Esri
  //   Licensed under the Apache License, Version 2.0 (the "License");
  //   you may not use this file except in compliance with the License.
  //   You may obtain a copy of the License at

  //       https://www.apache.org/licenses/LICENSE-2.0

  //   Unless required by applicable law or agreed to in writing, software
  //   distributed under the License is distributed on an "AS IS" BASIS,
  //   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  //   See the License for the specific language governing permissions and
  //   limitations under the License. 
  internal class Utilities
  {
    // change these to match the desired network configuration
    internal static string domainNetworkName = "ElectricDistribution";
    internal static string tierName = "Medium Voltage";
    internal static string isolationCategory = "Protective";
    internal static string networkAttributeForOpenPoint = "Device Status";
    internal static string openPointValue = "1";
    internal static string subnetworkTapCategory = "Subnetwork Tap";

    internal static string deviceClass = "ElectricDistributionDevice";
    internal static string junctionClass = "ElectricDistributionJunction";
    internal static string lineClass = "ElectricDistributionLine";
    internal static string assemblyClass = "ElectricDistributionAssembly";
    internal static string junctionObjectClass = "ElectricDistributionJunctionObject";
    internal static string edgeObjectClass = "ElectricDistributionEdgeObject";

    internal static List<Element> _sharedOpenPointElements = null;
    internal static Element _sharedStartingPointElement = null;
    internal static List<Element> _sharedDownstreamProtectiveElements = null;
    internal static List<Element> _sharedInitialIsolationElements = null;
    internal static Geometry _sharedStartingPointGeometry = null;
    internal static FeatureLayer _sharedFeatureLayer = null;

    // Retrieve the utility network from a feature class
    internal static UtilityNetwork GetUtilityNetwork(FeatureClass featureClass)
    {
      IReadOnlyList<Dataset> controllerDatasets = featureClass.GetControllerDatasets();
      bool foundUtilityNetwork = false;
      UtilityNetwork utilityNetwork = null;

      // Make sure to dispose all non-utility network controller datasets.
      foreach (Dataset dataset in controllerDatasets)
      {
        if (!foundUtilityNetwork && dataset.Type == DatasetType.UtilityNetwork && dataset is UtilityNetwork)
        {
          foundUtilityNetwork = true;
          utilityNetwork = dataset as UtilityNetwork;
        }
        else
        {
          dataset.Dispose();
        }
      }
      return utilityNetwork;
    }


    // used to get the initial element that the user clicked in the map
    internal static void GetElement(UtilityNetwork utilityNetwork, FeatureClass featureClass, long objectID, out Element element)
    {
      element = null;
      QueryFilter queryFilter = new QueryFilter()
      {
        ObjectIDs = new List<long>() { objectID }
      };

      using RowCursor rowCursor = featureClass.Search(queryFilter, false);
      if (!rowCursor.MoveNext())
      {
        return;
      }

      using Row row = rowCursor.Current;
      try
      {
        element = utilityNetwork.CreateElement(row);
      }
      catch (Exception e)
      {
        // MessageBox.Show(e.Message);
        return;
      }
    }
  }
}

