/*

   Copyright 2024 Esri

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
using System;
using System.Collections.Generic;
using System.Linq;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using NetworkElement = ArcGIS.Core.Data.UtilityNetwork.Element;
using NetworkTraceConfiguration = ArcGIS.Core.Data.UtilityNetwork.Trace.TraceConfiguration;

namespace ControllerPath
{
  internal class FindPath : Button
  {
    private IReadOnlyList<Result> TraceNetwork<T>(TraceManager traceManager, NetworkTraceConfiguration traceConfiguration, TraceArgument traceArgument) where T : Tracer
    {
      T controllerTracer = traceManager.GetTracer<T>();

      // Only return the potential subnetwork controllers
      if (controllerTracer is not SubnetworkControllerTracer)
      {
        CategoryComparison subnetworkControllerCategory = new CategoryComparison(CategoryOperator.IsEqual, "Subnetwork Controller");
        traceConfiguration.OutputCondition = subnetworkControllerCategory;
      }

      // Add an additional condition to stop on potential subnetwork controllers if it is a connectivity trace
      if (controllerTracer is ConnectedTracer)
      {
        Traversability traversabilityBarriers = traceConfiguration.Traversability;
        ConditionalExpression conditionBarriers = (ConditionalExpression)traversabilityBarriers.Barriers;
        CategoryComparison subnetworkControllerCategory = new CategoryComparison(CategoryOperator.IsEqual, "Subnetwork Controller");
        Or subnetworkConditionBarrier = new Or(conditionBarriers, subnetworkControllerCategory);
        traceConfiguration.Traversability.Barriers = subnetworkConditionBarrier;
      }

      IReadOnlyList<Result> controllerResults = null;
      try
      {
        controllerResults = controllerTracer.Trace(traceArgument);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }

      if (controllerResults == null)
      {
        MessageBox.Show("No subnetwork controllers discovered.");
        return null;
      }

      return controllerResults;
    }

    private IEnumerable<NetworkElement> FindPossiblePaths(TraceManager traceManager, NetworkTraceConfiguration traceConfiguration, NetworkAttribute shapeLengthAttribute,
      IEnumerable<NetworkElement> controllerElements, IList<NetworkElement> startingElements)
    {
      if (controllerElements == null)
      {
        return null;
      }

      if (startingElements == null)
      {
        return null;
      }

      List<NetworkElement> networkElements = new List<NetworkElement>();
      ShortestPathTracer shortestPathTracer = traceManager.GetTracer<ShortestPathTracer>();
      traceConfiguration.OutputCondition = null;
      traceConfiguration.ShortestPathNetworkAttribute = shapeLengthAttribute;
      foreach (NetworkElement controllerElement in controllerElements)
      {
        // Select the corresponding layers on the map
        if (startingElements.Count > 1)
        {
          startingElements.RemoveAt(1);
        }

        startingElements.Add(controllerElement);

        TraceArgument shortestPathTraceArgument = new TraceArgument(startingElements)
        {
          Configuration = traceConfiguration,
          ResultTypes = new List<ResultType> { ResultType.Element }
        };
        IReadOnlyList<Result> pathResults = null;
        try
        {
          pathResults = shortestPathTracer.Trace(shortestPathTraceArgument);
        }
        catch (Exception ex)
        {
          MessageBox.Show(ex.Message);
        }

        if (pathResults == null)
        {
          continue;
        }

        NetworkElement[] pathElements = pathResults.OfType<ElementResult>().SelectMany(result => result.Elements).ToArray();
        if (pathElements.Length > 0)
        {
          networkElements.AddRange(pathElements);
        }
      }

      return networkElements;
    }

    private UtilityNetwork GetUtilityNetworkFromSelection(SelectionSet activeSelection)
    {
      Dictionary<MapMember, List<long>> selectionByLayer = activeSelection.ToDictionary();
      foreach (KeyValuePair<MapMember, List<long>> kvp in selectionByLayer)
      {
        MapMember layer = kvp.Key;
        FeatureLayer featureLayer = layer as FeatureLayer;
        FeatureClass featureClass = featureLayer?.GetFeatureClass();

        if (featureClass != null && !featureClass.IsControllerDatasetSupported())
        {
          continue;
        }

        UtilityNetwork utilityNetwork = null;
        IReadOnlyList<Dataset> controllerDatasets = featureClass?.GetControllerDatasets();
        foreach (Dataset controllerDataset in controllerDatasets ?? Enumerable.Empty<Dataset>())
        {
          utilityNetwork = controllerDataset as UtilityNetwork;
          if (utilityNetwork != null)
          {
            break;
          }
          controllerDataset.Dispose();
        }
        return utilityNetwork;
      }

      return null;
    }

    private IList<NetworkElement> GetStartingElementsFromSelection(SelectionSet activeSelection, UtilityNetwork utilityNetwork)
    {
      List<NetworkElement> startingElements = new List<NetworkElement>();

      Dictionary<MapMember, List<long>> selectionByLayer = activeSelection.ToDictionary();
      foreach (KeyValuePair<MapMember, List<long>> kvp in selectionByLayer)
      {
        MapMember layer = kvp.Key;
        FeatureLayer featureLayer = layer as FeatureLayer;
        FeatureClass featureClass = featureLayer?.GetFeatureClass();

        if (featureClass != null)
        {
          RowCursor featureCursor = featureClass.Search(new QueryFilter() { ObjectIDs = kvp.Value, SubFields = "" }, true);
          while (featureCursor.MoveNext())
          {
            NetworkElement startingElement = utilityNetwork.CreateElement(featureCursor.Current);
            startingElements.Add(startingElement);
            break;
          }
        }
      }
      return startingElements;
    }

    private Dictionary<NetworkSource, List<string>> GetFieldAttributesByNetworkSource(UtilityNetwork utilityNetwork, IReadOnlyList<NetworkSource> networkSources)
    {
      Dictionary<NetworkSource, List<string>> fieldAttributesByNetworkSource = new Dictionary<NetworkSource, List<string>>();

      foreach (NetworkSource networkSource in networkSources ?? Enumerable.Empty<NetworkSource>())
      {
        using Table table = utilityNetwork.GetTable(networkSource);
        if (table is FeatureClass featureClass)
        {
          using FeatureClassDefinition featureClassDefinition = featureClass.GetDefinition();
          if (featureClassDefinition != null)
          {
            IReadOnlyList<Field> fields = featureClassDefinition.GetFields();
            List<string> fieldNames = fields?.Select(f => f.Name).ToList();

            fieldAttributesByNetworkSource.Add(networkSource, fieldNames);
          }
        }
      }
      return fieldAttributesByNetworkSource;
    }

    private IDictionary<int, IList<MapMember>> GetUtilityNetworkLayers(Map activeMap, IDictionary<string, int> networkSourceByName)
    {
      Dictionary<int, IList<MapMember>> networkSourceLayers = new Dictionary<int, IList<MapMember>>();
      foreach (Layer layer in activeMap.GetLayersAsFlattenedList())
      {
        // This doesn't take into account non-spatial objects
        FeatureLayer featureLayer = layer as FeatureLayer;
        FeatureClass featureClass = featureLayer?.GetFeatureClass();
        if (featureClass == null)
        {
          continue;
        }

        string featureClassName = featureClass.GetName();
        if (!networkSourceByName.TryGetValue(featureClassName.ToLower(), out int featureSourceId))
        {
          continue;
        }

        if (!networkSourceLayers.TryGetValue(featureSourceId, out IList<MapMember> mapMembers))
        {
          mapMembers = new List<MapMember>();
          networkSourceLayers[featureSourceId] = mapMembers;
        }

        mapMembers.Add(featureLayer);
      }
      return networkSourceLayers;
    }

    private async void GetSubnetworkFeatures<T>(Map activeMap) where T : Tracer
    {
      await QueuedTask.Run(() =>
      {
        SelectionSet activeSelection = activeMap.GetSelection();
        if (activeSelection.Count != 1)
        {
          MessageBox.Show(string.Format("Must select a single network feature"), "Trace Results",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
          return;
        }

        using UtilityNetwork utilityNetwork = GetUtilityNetworkFromSelection(activeSelection);
        if (utilityNetwork == null)
        {
          MessageBox.Show("No utility network found in selection.");
          return;
        }

        IList<NetworkElement> startingElements = GetStartingElementsFromSelection(activeSelection, utilityNetwork);
        if (startingElements.Count != 1)
        {
          MessageBox.Show(string.Format("Must select a single network feature"), "Trace Results",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
          return;
        }

        // Get properties from the utility network
        using TraceManager traceManager = utilityNetwork.GetTraceManager();
        using UtilityNetworkDefinition networkDefinition = utilityNetwork.GetDefinition();
        using NetworkAttribute shapeLengthAttribute = networkDefinition.GetNetworkAttribute("Shape Length");
        IReadOnlyList<NetworkAttribute> networkAttributes = networkDefinition.GetNetworkAttributes();
        List<string> networkAttributeNames = networkAttributes.Select(f => f.Name).ToList();

        // Get the map members associated with each network source, so we can create a selection set
        IReadOnlyList<NetworkSource> networkSources = networkDefinition.GetNetworkSources();
        Dictionary<string, int> networkSourceByName = networkSources.ToDictionary((source) => source.Name.ToLower(), (source) => source.ID);

        IDictionary<int, IList<MapMember>> networkSourceLayer = GetUtilityNetworkLayers(activeMap, networkSourceByName);
        Dictionary<NetworkSource, List<string>> fieldAttributesByNetworkSource = GetFieldAttributesByNetworkSource(utilityNetwork, networkSources);


        IReadOnlyList<DomainNetwork> domainNetworks = networkDefinition.GetDomainNetworks();
        if (domainNetworks.Count == 1)
        {
          MessageBox.Show(string.Format("No non-struture domain networks in utility network"), "Trace Results", System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Warning);
          return;
        }

        // Load the default condition barriers
        DomainNetwork firstDomain = domainNetworks.FirstOrDefault(domain => !domain.IsStructureNetwork);
        if (firstDomain == null)
        {
          MessageBox.Show(string.Format("No non-struture domain networks in utility network"), "Trace Results", System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Warning);
          return;
        }

        if (firstDomain.Tiers.Count == 0)
        {
          MessageBox.Show(string.Format("No non-struture domains in utility network"), "Trace Results", System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Warning);
          return;
        }

        //Get the first tier, if there's only one.
        //Otherwise get the first distribution or pressure tier.
        Tier firstTier = firstDomain.Tiers.FirstOrDefault(tier =>
                      tier.Name.Contains("Distribution", StringComparison.InvariantCultureIgnoreCase) ||
                      tier.Name.Contains("Pressure", StringComparison.InvariantCultureIgnoreCase));

        //If there are no distribution/pressure tiers, then look at any system or area tiers
        firstTier ??= firstDomain.Tiers.FirstOrDefault(tier =>
                      tier.Name.Contains("System", StringComparison.InvariantCultureIgnoreCase) ||
                      tier.Name.Contains("Area", StringComparison.InvariantCultureIgnoreCase));

        //If we have exhausted all other options, just use the first tier.
        firstTier ??= firstDomain.Tiers[0];

        NetworkTraceConfiguration traceConfiguration = firstTier.GetTraceConfiguration();
        traceConfiguration.ValidateConsistency = false;

        // Construct the trace arguments
        TraceArgument subnetworkControllerTraceArgument = new TraceArgument(startingElements)
        {
          Configuration = traceConfiguration,
          ResultTypes = new List<ResultType> { ResultType.Element, ResultType.Feature },

          // Options for feature result type
          ResultOptions = new ResultOptions
          {
            IncludeGeometry = true,
            NetworkAttributes = networkAttributeNames,
            ResultFields = fieldAttributesByNetworkSource
          }
        };

        IReadOnlyList<Result> traceNetworkResults = TraceNetwork<T>(traceManager, traceConfiguration, subnetworkControllerTraceArgument);

        // Analyze feature elements
        IEnumerable<FeatureElement> featureElements = traceNetworkResults.OfType<FeatureElementResult>().SelectMany(result => result.FeatureElements).ToList();
        foreach (FeatureElement featureElement in featureElements)
        {
          // Iterate feature elements to find field values and network attributes
          IReadOnlyList<TraceResultFieldValue> fieldValues = featureElement.ResultFieldValues;
          IReadOnlyList<TraceResultFieldValue> networkAttributeValues = featureElement.ResultNetworkAttributeValues;
        }

        //  Analyze elements 
        IEnumerable<NetworkElement> controllerElements = traceNetworkResults.OfType<ElementResult>().SelectMany(result => result.Elements).ToList();

        if (!controllerElements.Any())
        {
          MessageBox.Show(string.Format("No potential subnetwork controllers found"), "Trace Results", System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Warning);
          return;
        }

        IEnumerable<NetworkElement> allPathElements = FindPossiblePaths(traceManager, traceConfiguration, shapeLengthAttribute, controllerElements, startingElements);
        if (allPathElements == null || !allPathElements.Any())
        {
          MessageBox.Show(string.Format("No paths to subnetwork controllers found"), "Trace Results", System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Warning);
          return;
        }

        // Group the elements by layer
        Dictionary<MapMember, long[]> selectionsByLayer = new Dictionary<MapMember, long[]>();
        foreach (IGrouping<int, long> grouping in allPathElements.GroupBy(element => element.NetworkSource.ID, element => element.ObjectID))
        {
          // Get the layer from the map
          int networkSourceId = grouping.Key;
          if (!networkSourceLayer.TryGetValue(networkSourceId, out IList<MapMember> layers))
          {
            continue;
          }

          long[] objectIds = grouping.ToArray();
          foreach (MapMember layer in layers)
          {
            selectionsByLayer[layer] = objectIds;
          }
        }

        // Select results in the map
        SelectionSet newSelection = SelectionSet.FromDictionary(selectionsByLayer);
        activeMap.SetSelection(newSelection);

        MessageBox.Show(string.Format("{0} subnetwork controllers and {1} elements found", controllerElements.Count(), allPathElements.Count()),
          $"Shortest Path: {firstTier.Name}", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
      });
    }

    protected override void OnClick()
    {
      try
      {
        Map activeMap = MapView.Active?.Map;
        if (activeMap == null)
        {
          return;
        }

        try
        {
          // Try it first with subnetwork controller
          GetSubnetworkFeatures<SubnetworkControllerTracer>(activeMap);
        }
        catch (Exception ex)
        {
          MessageBox.Show(ex.Message);

          //Fall back on connectivity in case there are no controllers
          GetSubnetworkFeatures<ConnectedTracer>(activeMap);
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }
  }
}
