/*

   Copyright 2025 Esri

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

using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using TraceConfiguration = ArcGIS.Core.Data.UtilityNetwork.Trace.TraceConfiguration;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Data.Exceptions;

namespace BatchTracingCoreHost.Classes
{
    internal class InferSubnetworks
    {
        enum AnalysisTerminals
        {
            Upstream=1,
            Downstream=2,
            Both=3
        }

        internal static void Execute(string analysisName, IDictionary<string, object> configuration)
        {

            #region Load the properties from the configuration

            var inputWorkspace = Helpers.GetProperty<string>(configuration, "inputWorkspace");
            var outputWorkspace = Helpers.GetProperty<string>(configuration, "outputWorkspace");
            var sourceUtilityNetwork = Helpers.GetProperty<string>(configuration, "sourceUtilityNetwork");
            var domainNetworkName = Helpers.GetProperty<string>(configuration, "domainNetworkName");
            var tierName = Helpers.GetProperty<string>(configuration, "tierName");
            var terminals = Helpers.GetProperty<string>(configuration, "terminals"); // Upstream, Downstream, or Both
            var definitionQuery = Helpers.GetProperty<string>(configuration, "definitionQuery", false);
            var useDigitizedDirection = Helpers.GetProperty(configuration, "useDigitizedDirection", false, false);
            var outputPoints = Helpers.GetProperty<string>(configuration, "outputPoints");
            var outputPolylines = Helpers.GetProperty<string>(configuration, "outputPolylines");
            var outputPolygons = Helpers.GetProperty<string>(configuration, "outputPolygons");
            var outputTable = Helpers.GetProperty<string>(configuration, "outputTable", false);
            var outputFile = Helpers.GetProperty<string>(configuration, "outputFile", false);
            var functionFieldCount = Helpers.GetProperty<int>(configuration, "outputFunctionCount", false, 0);
            var identifyBarriers = Helpers.GetProperty<bool>(configuration, "identifyBarriers", false, false);
            var portalUrl = Helpers.GetProperty<string>(configuration, "portalUrl", false);
            var portalUsername = Helpers.GetProperty<string>(configuration, "portalUsername", false);
            var portalPassword = Helpers.GetProperty<string>(configuration, "portalPassword", false);

            AnalysisTerminals analysisTerminals;
            if (terminals == "Upstream")
                analysisTerminals = AnalysisTerminals.Upstream;
            else if (terminals == "Downstream")
                analysisTerminals = AnalysisTerminals.Downstream;
            else if (terminals == "Both")
                analysisTerminals = AnalysisTerminals.Both;
            else
                throw new ArgumentException("Terminals must be 'Upstream', 'Downstream', or 'Both'", "terminals");

            #endregion

            PartitionUsingSubnetworkControllers(inputWorkspace, sourceUtilityNetwork, domainNetworkName, tierName,
                definitionQuery ,useDigitizedDirection, identifyBarriers, analysisTerminals,
                outputWorkspace, outputPolygons, outputPolylines, outputPoints, outputTable,
                portalUrl, portalUsername, portalPassword, outputFile, analysisName, functionFieldCount);
        }
        
        private static void PartitionUsingSubnetworkControllers(string inputWorkspacePath, string utilityNetworkClassName, string domainNetworkName, string tierName,
            string definitionQuery, bool useDigitizedDirection, bool identifyBarriers, AnalysisTerminals analysisTerminals,
            string outputWorkspacePath, string polygonClassName, string polylineClassName, string pointClassName, string outputTableName,
            string portalUrl, string portalUsername, string portalPassword, string outputFile, string analysisName, int functionFieldCount)
        {
            try
            {
                if (string.IsNullOrEmpty(inputWorkspacePath))
                    throw new ArgumentNullException("Missing required parameter", "Input Workspace");
                if (string.IsNullOrEmpty(utilityNetworkClassName))
                    throw new ArgumentNullException("Missing required parameter", "Utility Network");

                using (var utilityNetworkWorkspace = Helpers.OpenGeodatabase(inputWorkspacePath, portalUrl, portalUsername, portalPassword))
                using (var outputWorkspace = Helpers.OpenGeodatabase(outputWorkspacePath))
                using (var utilityNetwork = utilityNetworkWorkspace.OpenDataset<UtilityNetwork>(utilityNetworkClassName))
                {
                    #region Load or create the output classes

                    FeatureClass polygonClass, polylineClass, pointClass;
                    Table outputTable;
                    Helpers.LoadOutputs(polygonClassName, polylineClassName, pointClassName, outputTableName, functionFieldCount, outputWorkspace, utilityNetwork, out polygonClass, out polylineClass, out pointClass, out outputTable);

                    #endregion

                    Analyze(utilityNetwork, polygonClass, polylineClass, pointClass, outputTable,
                        domainNetworkName, tierName,
                        definitionQuery, useDigitizedDirection, identifyBarriers, analysisTerminals,
                        outputFile, analysisName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void Analyze(UtilityNetwork utilityNetwork, FeatureClass polygonFeatureClass, FeatureClass polylineFeatureClass, FeatureClass pointFeatureClass, Table outputTable,
            string domainNetworkName, string tierName, string definitionQuery, 
            bool useDigitizedDirection, bool identifyBarriers, AnalysisTerminals analysisTerminals,
            string outputFile, string analysisName)
        {
            Helpers.CheckUtilityNetwork(utilityNetwork);

            #region Load the definitions from the utility network

            using var utilityNetworkDefinition = utilityNetwork.GetDefinition();
            var assetGroupFieldName = utilityNetworkDefinition.GetAssetGroupField();
            var assetTypeFieldName = utilityNetworkDefinition.GetAssetTypeField();

            // How we define barriers depends on whether the network is hierarchical or partitioned
            TierDefinition tierDefinition = TierDefinition.None;
            SubnetworkControllerType subnetworkControllerType = SubnetworkControllerType.None;

            TraceConfiguration tierTraceConfiguration = null;
            NetworkSource deviceNetworkSource = null;
            IReadOnlyList<AssetType> subnetworkControllerAssetTypes = null;
            IReadOnlyList<AssetType> subnetworkLineAssetTypes = null;
            HashSet<(int networkSource, int assetGroup, int assetType)> subnetworkLineLookup = null;
            HashSet<(int networkSource, int assetGroup, int assetType)> subnetworkControllerLookup = null;
            var validControllers = new Dictionary<int, IList<int>>();
            var controllersAndTerminals = new Dictionary<(int assetGroup, int assetType),IEnumerable<Terminal>>();

            foreach(var domainNetwork in utilityNetworkDefinition.GetDomainNetworks())
            {
                if (!domainNetwork.Name.Equals(domainNetworkName, StringComparison.OrdinalIgnoreCase))
                    continue;
                
                foreach(var tier in domainNetwork.Tiers)
                {
                    if (!tier.Name.Equals(tierName, StringComparison.OrdinalIgnoreCase))
                        continue;

                    tierDefinition = domainNetwork.TierDefinition;
                    subnetworkControllerType = domainNetwork.SubnetworkControllerType;

                    tierTraceConfiguration = tier.GetTraceConfiguration();
                    foreach(var networkSource in domainNetwork.NetworkSources)
                    {
                        if (networkSource.UsageType != SourceUsageType.Device)
                            continue;
                        deviceNetworkSource = networkSource;
                        break;
                    }

                    // For optimization
                    subnetworkControllerAssetTypes = tier.ValidSubnetworkControllers;
                    subnetworkLineAssetTypes = tier.ValidSubnetworkLines;

                    foreach (var controllerAssetType in tier.ValidSubnetworkControllers)
                    {
                        if(!validControllers.TryGetValue(controllerAssetType.AssetGroup.Code, out var assetTypes))
                            validControllers[controllerAssetType.AssetGroup.Code] = new List<int>() { controllerAssetType.Code };
                        else
                            assetTypes.Add(controllerAssetType.Code);

                        (int assetGroup, int assetType) assetGroupAndType = new (controllerAssetType.AssetGroup.Code, controllerAssetType.Code);
                        
                        var terminalConfiguration = controllerAssetType.GetTerminalConfiguration();
                        var desiredTerminals = terminalConfiguration.Terminals.Where(terminal =>
                                (terminal.IsUpstreamTerminal && (analysisTerminals & AnalysisTerminals.Upstream) > 0)
                                || (!terminal.IsUpstreamTerminal && (analysisTerminals & AnalysisTerminals.Downstream) > 0))
                            .ToArray();

                        controllersAndTerminals[assetGroupAndType] = desiredTerminals;
                    }

                    // We only support one tier
                    break;
                }
            }

            if (tierTraceConfiguration == null)
                throw new Exception(string.Format("Unable to find tier configuration associated with domain '{0}' and tier '{1}'", domainNetworkName, tierName));
            if (deviceNetworkSource == null)
                throw new Exception(string.Format("Unable to find Device network source associated with domain '{0}' and tier '{1}'", domainNetworkName, tierName));

            #endregion

            #region Load all the starting elements

            var controllerDefinitionQuery = string.Empty;
            foreach (var assetGroupAndTypes in validControllers)
            {
                var validAssetTypes = string.Join(",", assetGroupAndTypes.Value);
                controllerDefinitionQuery += string.Format("({0} = {1} AND {2} IN ({3})) OR ",
                    assetGroupFieldName, assetGroupAndTypes.Key, assetTypeFieldName, validAssetTypes);
            }
            controllerDefinitionQuery = controllerDefinitionQuery.Remove(controllerDefinitionQuery.Length - 4);
            if (!string.IsNullOrEmpty(definitionQuery))
                controllerDefinitionQuery = string.Format("{0} AND ({1})", definitionQuery, controllerDefinitionQuery);
            var networkSourceID = deviceNetworkSource.ID;

            using var networkTable = utilityNetwork.GetTable(deviceNetworkSource);
            using var tableDefinition = networkTable.GetDefinition();
            var objectIdFieldName = tableDefinition.GetObjectIDField();
            var globalIdFieldName = tableDefinition.GetGlobalIDField();

            // Because we constrain to a single network source, Object ID is fine
            var startTime = DateTime.Now;
            Console.WriteLine(string.Format("{0}: Loading starting elements: {1:n0}", DateTime.Now.ToString(), controllerDefinitionQuery));
            var startingElements = new Dictionary<(long objectID, int terminalID), Element>();

            using var featureCursor = networkTable.Search(new QueryFilter
            {
                WhereClause = controllerDefinitionQuery,
                SubFields = string.Format("{0},{1},{2},{3}", objectIdFieldName, globalIdFieldName, assetGroupFieldName, assetTypeFieldName)
            });
            while (featureCursor.MoveNext())
            {
                using var currentRow = featureCursor.Current;
                var startingElement = utilityNetwork.CreateElement(currentRow);
                (int assetGroup, int assetType) key = new (startingElement.AssetGroup.Code, startingElement.AssetType.Code);

                var startingTerminals = controllersAndTerminals[key];
                foreach (var terminal in startingTerminals)
                {
                    startingElement.Terminal = terminal;
                    startingElements[new (startingElement.ObjectID, terminal.ID)] = startingElement;
                }
            }

            var endTime = DateTime.Now;
            Console.WriteLine(string.Format("{0}: Finished loading starting elements ({1:n0} elements discovered)", DateTime.Now.ToString(), startingElements.Count));
            Console.WriteLine(string.Format("{0}: Time elapsed {1}", DateTime.Now.ToString(), (endTime - startTime).TotalSeconds));

            if (startingElements.Count == 0)
                throw new Exception(string.Format("Unable to find any starting feature to act as controllers for domain '{0}' and tier '{1}' with query: {2}",
                    domainNetworkName, tierName, controllerDefinitionQuery));

            #endregion

            #region Create the tracer

            using var traceManager = utilityNetwork.GetTraceManager();

            // Digitized direciton tracing uses upstream/downstream with direction
            // Otherwise we use a connected trace
            Tracer networkTracer = null;
            if (!useDigitizedDirection)
                networkTracer = traceManager.GetTracer<ConnectedTracer>();
            else
            {
                if (subnetworkControllerType == SubnetworkControllerType.Source)
                    networkTracer = traceManager.GetTracer<DownstreamTracer>();
                else if (subnetworkControllerType == SubnetworkControllerType.Sink)
                    networkTracer = traceManager.GetTracer<UpstreamTracer>();
            }

            var newConfiguration = new TraceConfiguration
            {
                ValidateConsistency = false,
                Functions = tierTraceConfiguration.Functions,
                UseDigitizedDirection = useDigitizedDirection
            };
            var barrierConditions = tierTraceConfiguration.Traversability.Barriers;

            #endregion

            #region Modify the barriers for the trace

            // Load all the network attributes
            var allAttributes = utilityNetworkDefinition.GetNetworkAttributes();
            var networkSourceAttribute = allAttributes.FirstOrDefault(attribute => attribute.IsSystemAttribute && attribute.Name.Equals("Source ID", StringComparison.InvariantCultureIgnoreCase));
            var assetGroupAttribute = allAttributes.FirstOrDefault(attribute => attribute.IsSystemAttribute && attribute.Name.Equals("Asset group", StringComparison.InvariantCultureIgnoreCase));
            var assetTypeAttribute = allAttributes.FirstOrDefault(attribute => attribute.IsSystemAttribute && attribute.Name.Equals("Asset type", StringComparison.InvariantCultureIgnoreCase));

            if (tierDefinition == TierDefinition.Hierarchical)
            {
                // For a hierarchical domain network, we only treat controllers in the current tier as barriers
                foreach(var validController in validControllers)
                {
                    foreach(var assetType in validController.Value)
                    {
                        var assetTypeCondition = 
                            new And(new NetworkAttributeComparison(networkSourceAttribute, Operator.Equal, networkSourceID),
                                new And(new NetworkAttributeComparison(assetGroupAttribute, Operator.Equal, validController.Key),
                                    new NetworkAttributeComparison(assetTypeAttribute, Operator.Equal, assetType)));
                        barrierConditions = barrierConditions == null
                            ? assetTypeCondition
                            : new Or(assetTypeCondition, (ConditionalExpression)barrierConditions);
                    }
                }

                newConfiguration.Traversability.Barriers = barrierConditions;
            }
            else
            {
                // For a partitioned domain network, we treat every subntwork controller as a barrier
                barrierConditions = barrierConditions == null
                    ? new CategoryComparison(CategoryOperator.IsEqual, "Subnetwork Controller")
                    : new Or(new CategoryComparison(CategoryOperator.IsEqual, "Subnetwork Controller"), (ConditionalExpression)barrierConditions);
                newConfiguration.Traversability.Barriers = barrierConditions;
            }

            #endregion

            #region Define the output

            // We always need the elements to do the analysis
            var resultTypes = new List<ResultType>() { ResultType.Element };
            if(polylineFeatureClass != null || pointFeatureClass != null)
                resultTypes.Add(ResultType.AggregatedGeometry);
            if (newConfiguration.Functions != null)
                resultTypes.Add(ResultType.FunctionValue);

            Condition subnetworkLineConditions = null;
            var outputAssetTypes = new List<AssetType>(subnetworkControllerAssetTypes);
            if (subnetworkLineAssetTypes != null &&
                subnetworkLineAssetTypes.Count > 0)
            {
                // Create a lookup for all the subnetwork line asset types
                subnetworkLineLookup = new HashSet<(int networkSource, int assetGroup, int assetType)>(subnetworkLineAssetTypes
                    .Select<AssetType, (int networkSource, int assetGroup, int assetType)>(assetType => new (assetType.AssetGroup.NetworkSource.ID, assetType.AssetGroup.Code, assetType.Code)));

                // Add the subnetwork line asset types to the output asset types or output condition barriers, as appropriate
                if (!identifyBarriers)
                    outputAssetTypes.AddRange(subnetworkLineAssetTypes);
                else
                {
                    foreach (var subnetworkLineAssetType in subnetworkLineAssetTypes)
                    {
                        var assetTypeCondition =
                                new And(new NetworkAttributeComparison(networkSourceAttribute, Operator.Equal, subnetworkLineAssetType.AssetGroup.NetworkSource.ID),
                                    new And(new NetworkAttributeComparison(assetGroupAttribute, Operator.Equal, subnetworkLineAssetType.AssetGroup.Code),
                                        new NetworkAttributeComparison(assetTypeAttribute, Operator.Equal, subnetworkLineAssetType.Code)));
                        subnetworkLineConditions = subnetworkLineConditions == null
                            ? assetTypeCondition
                            : new Or(assetTypeCondition, (ConditionalExpression)subnetworkLineConditions);
                    }
                }
            }

            // Create a lookup for all the subnetwork controller asset types
            subnetworkControllerLookup = new HashSet<(int networkSource, int assetGroup, int assetType)>(subnetworkControllerAssetTypes
                .Select<AssetType, (int networkSource, int assetGroup, int assetType)>(assetType => new (assetType.AssetGroup.NetworkSource.ID, assetType.AssetGroup.Code, assetType.Code)));

            // If we are identifying barrieres, we need to use conditions barriers to include the barriers in the output
            // Otherwise, we just output subnetwork controllers and subnetwork lines
            if (identifyBarriers)
                newConfiguration.OutputCondition = subnetworkLineConditions == null
                   ? barrierConditions
                    : new Or((ConditionalExpression)subnetworkLineConditions, (ConditionalExpression)barrierConditions);
            else
                newConfiguration.OutputAssetTypes = outputAssetTypes;

            #endregion

            #region Perform the analysis

            startTime = DateTime.Now;
            var count = 0;
            var totalElements = 0;
            var elementResults = new List<(long resultID, IEnumerable<Element> elements)>();
            var resultNameLookup = new Dictionary<long, string>();
            var resultElementInfo = new List<IDictionary<(int networkSourceID,long objectID, int terminalID), string>>();
            var allFunctionOutput = new List<IList<double>>();

            var polygonOutput = new List<(long resultID, Geometry geometry)>();
            var polylineOutput = new List<(long resultID, Geometry geometry)>();
            var pointOutput = new List<(long resultID, Geometry geometry)>();

            var discoveredFeatures = new HashSet<(long objectID, int terminalID)>();
            var subnetworksAndControllers = new Dictionary<int, IList<Element>>();
            var internalSubnetworks = new Dictionary<int, IList<Element>>();

            Console.WriteLine(string.Format("{0}: Start tracing {1:n0} elements", DateTime.Now.ToString(), startingElements.Count));

            foreach (var startingInfo in startingElements)
            {
                // We can use the Object ID instead of the Global ID because starting elements are constrained to a single network source
                if (discoveredFeatures.Contains(startingInfo.Key))
                    continue;

                // Shouldn't need to do this, because the starting point should be included in the results...
                // Better safe than stack overflow
                discoveredFeatures.Add(startingInfo.Key);

                count += 1;
                if ((count % 100) == 0) Console.WriteLine(string.Format("{0}: Traced {1:n0} elements ({2:n0} starting elements discovered)", DateTime.Now, count, discoveredFeatures.Count));

                // We need at least element and aggregated geometry
                // We don't need feature because we don't need the geometry or attribute information
                var traceArgument = new TraceArgument([startingInfo.Value])
                {
                    Configuration = newConfiguration,
                    ResultTypes = resultTypes,
                };

                // This is a workaround for a bug with the ArcGIS Pro SDK 3.5.0 with MaxHops
                // Only uncomment it if you are using that specific release
                // Helpers.RemoveMaxHops(traceArgument);

                try
                {
                    var ignoredTerminals = new List<Element>();
                    var sourceElementInfo = new Dictionary<(int networkSourceID, long objectID, int terminalID), string>();
                    var traceResults = networkTracer.Trace(traceArgument);
                    foreach (var result in traceResults)
                    {
                        if (result is AggregatedGeometryResult aggregatedGeometryResult)
                        {
                            if (aggregatedGeometryResult.Polygon != null && polygonFeatureClass != null)
                                polygonOutput.Add(new (count, aggregatedGeometryResult.Polygon));
                            if (aggregatedGeometryResult.Line != null && polylineFeatureClass != null)
                                polylineOutput.Add(new (count, aggregatedGeometryResult.Line));
                            if (aggregatedGeometryResult.Point != null && pointFeatureClass != null)
                                pointOutput.Add(new (count, aggregatedGeometryResult.Point));
                        }
                        else if (result is FunctionOutputResult functionOutputResult)
                        {
                            allFunctionOutput.Add(functionOutputResult.FunctionOutputs.Select(output => Convert.ToDouble(output.Value)).ToList());
                        }
                        else if( result is ElementResult elementResult)
                        {
                            var uniqueIDs = new HashSet<string>();
                            var featureElements = elementResult.Elements;
                            var theseControllers = new List<Element>();
                            
                            foreach (var featureElement in featureElements)
                            {
                                // Determine whether the element is a subnetwork line
                                (int networkSource, int assetGroup, int assetType) fullClassification = new (featureElement.NetworkSource.ID, featureElement.AssetGroup.Code, featureElement.AssetType.Code);
                                if (subnetworkLineLookup.Contains(fullClassification))
                                {
                                    sourceElementInfo[new (featureElement.NetworkSource.ID, featureElement.ObjectID, -1)] = "Subnet Line";
                                    continue;
                                }

                                // If our output conditions are restrictive we assume subnet lines are subnet lines. Even when they are barriers.
                                // Any remaining features that aren't controllers are barriers
                                if (identifyBarriers &&!subnetworkControllerLookup.Contains(fullClassification))
                                {
                                    sourceElementInfo[new (featureElement.NetworkSource.ID, featureElement.ObjectID, featureElement.Terminal.ID)] = "Barrier";
                                    continue;
                                }

                                // Only consider elements in the source layer
                                (int assetGroup, int assetType) assetGroupAndCode = new (featureElement.AssetGroup.Code, featureElement.AssetType.Code);
                                if (featureElement.NetworkSource.ID != networkSourceID ||
                                    !controllersAndTerminals.ContainsKey(assetGroupAndCode))
                                    continue;

                                // If we're only looking at upstream or downstream terminals, we need to ignore elements
                                // for terminals that aren't being considered
                                (long objectID, int terminalID) objectIdAndTerminal = new (featureElement.ObjectID, featureElement.Terminal.ID);
                                if (!startingElements.ContainsKey(objectIdAndTerminal))
                                {
                                    ignoredTerminals.Add(featureElement);
                                    continue;
                                }
                                
                                discoveredFeatures.Add(objectIdAndTerminal);
                                theseControllers.Add(featureElement);
                            }

                            // Separate out the duplicated controllers as being internal
                            var duplicatedElements = theseControllers.Concat(ignoredTerminals)
                                .GroupBy(element => element.GlobalID)
                                .Where(grouping=>grouping.Count()>1)
                                .ToArray();
                            if (duplicatedElements.Length > 0)
                            {
                                Console.WriteLine("Trace {0} has {1:n0} internal controllers. These will be output to a separate file.", count, duplicatedElements.Length);
                                foreach (var duplicatedElementGrouping in duplicatedElements)
                                {
                                    foreach (var duplicatedElement in duplicatedElementGrouping)
                                    {
                                        theseControllers.Remove(duplicatedElement);
                                        sourceElementInfo[new (duplicatedElement.NetworkSource.ID, duplicatedElement.ObjectID, duplicatedElement.Terminal.ID)] = "Internal Controller";
                                    }
                                }

                                internalSubnetworks[count] = duplicatedElements.SelectMany(item => item).ToList();
                            }

                            // Mark elements as subnetwork controllers
                            foreach(var subnetworkController in theseControllers)
                                sourceElementInfo[new (subnetworkController.NetworkSource.ID, subnetworkController.ObjectID, subnetworkController.Terminal.ID)] = "Subnetwork Controller";

                            // Record the subnetwork controllers for this trace
                            subnetworksAndControllers[count] = theseControllers;
                            if (outputTable != null)
                            {
                                totalElements += featureElements.Count;
                                resultNameLookup[count] = Convert.ToString(count);
                                elementResults.Add(new (count, featureElements));
                                resultElementInfo.Add(sourceElementInfo);
                            }
                        }
                    }
                }
                catch(NotSupportedException notSupportException)
                {
                    Console.WriteLine(string.Format("{0}: Error tracing element {1}\r\n{2}", DateTime.Now.ToString(), startingInfo.Key.Item1, notSupportException.Message));
                    return;
                }
                catch (GeodatabaseUtilityNetworkException geoUnException)
                {
                    // This handles things like starting point outside the current tier
                    Console.WriteLine(string.Format("{0}: Error tracing element {1}\r\n{2}", DateTime.Now.ToString(), startingInfo.Key.Item1, geoUnException.Message));
                    continue;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("{0}: Error tracing element {1}\r\n{2}", DateTime.Now.ToString(), startingInfo.Key.Item1, ex.Message));
                }
            }

            endTime = DateTime.Now;
            Console.WriteLine(string.Format("{0}: Traced {1:n0} elements ({2:n0} starting elements discovered)", DateTime.Now.ToString(), count, discoveredFeatures.Count));
            Console.WriteLine(string.Format("{0}: Time elapsed {1:n0}", DateTime.Now.ToString(), (endTime - startTime).TotalSeconds));

            #endregion

            #region Output the results

            if (polygonFeatureClass != null)
            {
                Console.WriteLine(string.Format("{0}: {1} - {2:n0} subnetwork polygon result(s)", DateTime.Now.ToString(), analysisName, polygonOutput.Count));
                Helpers.StoreGeometryResults(polygonFeatureClass, analysisName, polygonOutput, newConfiguration.Functions.Count, allFunctionOutput);
            }
            if (polylineFeatureClass != null)
            {
                Console.WriteLine(string.Format("{0}: {1} - {2:n0} subnetwork line result(s)", DateTime.Now.ToString(), analysisName, polylineOutput.Count));
                Helpers.StoreGeometryResults(polylineFeatureClass, analysisName, polylineOutput, newConfiguration.Functions.Count, allFunctionOutput);
            }
            if (pointFeatureClass != null)
            {
                Console.WriteLine(string.Format("{0}: {1} - {2:n0} subnetwork point result(s)", DateTime.Now.ToString(), analysisName, pointOutput.Count));
                Helpers.StoreGeometryResults(pointFeatureClass, analysisName, pointOutput, newConfiguration.Functions.Count, allFunctionOutput);
            }
            if (outputTable != null)
            {
                Console.WriteLine(string.Format("{0}: {1} - {2:n0} element result(s)", DateTime.Now.ToString(), analysisName, elementResults.Count));
                Console.WriteLine(string.Format("{0}: {1} - {2:n0} total element(s)", DateTime.Now.ToString(), analysisName, totalElements));

                if (resultElementInfo.Count > 0)
                    Helpers.StoreElementResults(outputTable, analysisName, resultNameLookup, elementResults, resultElementInfo);
                else
                    Helpers.StoreElementResults(outputTable, analysisName, resultNameLookup, elementResults);
            }

            Console.WriteLine(string.Format("{0}: {1} Subnetworks identified", DateTime.Now.ToString(), subnetworksAndControllers.Count));
            if (subnetworksAndControllers.Count > 0)
            {
                if (string.IsNullOrEmpty(outputFile))
                {
                    Console.WriteLine(string.Format("{0}: No output file specified.", DateTime.Now.ToString()));
                    return;
                }

                OutputControllersCsv(subnetworksAndControllers, tierName, analysisName, outputFile);

                if (internalSubnetworks.Count > 0)
                {
                    var internalControllerFile = outputFile.Remove(outputFile.Length - 4) + "_Internal.csv";
                    Console.WriteLine(string.Format("{0}: Outputting internal controllers for {1} subnetworks to file: {2}.", DateTime.Now.ToString(), internalSubnetworks.Count, internalControllerFile));
                    OutputControllersCsv(internalSubnetworks, tierName, analysisName, internalControllerFile);
                }
            }

            #endregion

        }

        private static void OutputControllersCsv(Dictionary<int,IList<Element>> subnetworksAndControllers, string tierName, string analysisName, string outputCsvPath)
        {
            var columns = new string[]
            {
                "TIERNAME",
                "SUBNETWORKNAME",
                "SUBNETWORKCONTROLLERNAME",
                "FEATURECLASSNAME",
                "FEATUREGLOBALID",
                "FEATUREASSETGROUP",
                "FEATUREASSETTYPE",
                "FEATURETERMINAL",
                "DESCRIPTION",
                "NOTES"
            };

            using var outputFile = System.IO.File.CreateText(outputCsvPath);
            outputFile.WriteLine(string.Join(",", columns));

            foreach (var subnetworkAndcontrollers in subnetworksAndControllers)
            {
                var subnetworkName = string.Format("{0} {1}", analysisName, subnetworkAndcontrollers.Key);
                foreach (var subnetworkController in subnetworkAndcontrollers.Value)
                {
                    outputFile.WriteLine(string.Join(",", new string[]
                    {
                            tierName,
                            subnetworkName,
                            string.Format("{0} {1}",subnetworkController.GlobalID, subnetworkController.Terminal.Name),
                            subnetworkController.NetworkSource.Name,
                            string.Format("{{{0}}}",subnetworkController.GlobalID),
                            subnetworkController.AssetGroup.Name,
                            subnetworkController.AssetType.Name,
                            subnetworkController.Terminal.Name,
                            "",
                            ""
                    }));
                }
            }
        }
    }
}
