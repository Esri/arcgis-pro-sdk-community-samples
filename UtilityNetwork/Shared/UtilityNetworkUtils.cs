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

using System;
using System.Collections.Generic;
using System.Linq;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Desktop.Mapping;

namespace UtilityNetworkSamples
{
  class UtilityNetworkUtils
  {

    /// <summary>
    /// GetUtilityNetworkFromFeatureClass - gets a utility network from a layer
    /// </summary>
    /// <param name="layer"></param>
    /// <returns>a UtilityNetwork object, or null if the layer does not reference a utility network</returns>
    public static UtilityNetwork GetUtilityNetworkFromLayer(Layer layer)
    {

      UtilityNetwork utilityNetwork = null;

      if (layer is UtilityNetworkLayer)
      {
        UtilityNetworkLayer utilityNetworkLayer = layer as UtilityNetworkLayer;
        utilityNetwork = utilityNetworkLayer.GetUtilityNetwork();
      }

      else if (layer is SubtypeGroupLayer)
      {
        CompositeLayer compositeLayer = layer as CompositeLayer;
        utilityNetwork = GetUtilityNetworkFromLayer(compositeLayer.Layers.First());
      }

      else if (layer is FeatureLayer)
      {
        FeatureLayer featureLayer = layer as FeatureLayer;
        using (FeatureClass featureClass = featureLayer.GetFeatureClass())
        {
          if (featureClass.IsControllerDatasetSupported())
          {
            IReadOnlyList<Dataset> controllerDatasets = new List<Dataset>();
            controllerDatasets = featureClass.GetControllerDatasets();
            foreach (Dataset controllerDataset in controllerDatasets)
            {
              if (controllerDataset is UtilityNetwork)
              {
                utilityNetwork = controllerDataset as UtilityNetwork;
              }
              else
              {
                controllerDataset.Dispose();
              }
            }
          }
        }
      }
      
      else if (layer is GroupLayer)
      {
        CompositeLayer compositeLayer = layer as CompositeLayer;
        foreach (Layer childLayer in compositeLayer.Layers)
        {
          utilityNetwork = GetUtilityNetworkFromLayer(childLayer);
          // Break at the first layer inside a group layer that belongs to a utility network
          if (utilityNetwork != null) break; 
        }
      }

      return utilityNetwork;
    }

    /// <summary>
    /// Fetches a Row from an Element
    /// </summary>
    /// <param name="utilityNetwork">The utility network to which the element belongs</param>
    /// <param name="element">An element in a utility network</param>
    /// <returns>The Row corresponding to the Element (if any)</returns>
    public static Row FetchRowFromElement(UtilityNetwork utilityNetwork, Element element)
    {
      // Get the table from the element
      using (Table table = utilityNetwork.GetTable(element.NetworkSource))
      {
        // Create a query filter to fetch the appropriate row
        QueryFilter queryFilter = new QueryFilter()
        {
          ObjectIDs = new List<long>() { element.ObjectID }
        };

        // Fetch and return the row
        using (RowCursor rowCursor = table.Search(queryFilter))
        {
          if (rowCursor.MoveNext())
          {
            return rowCursor.Current;
          }
        }
        return null;
      }

    }

  }
}
