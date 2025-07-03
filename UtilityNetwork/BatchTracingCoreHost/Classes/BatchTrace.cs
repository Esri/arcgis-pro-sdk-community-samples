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

using ArcGIS.Core.Data;
using ArcGIS.Core.Data.Exceptions;
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BatchTracingCoreHost.Classes
{
    internal class BatchTrace
    {
        internal static void Execute(string analysisName, IDictionary<string,object> configuration)
        {

            #region Load the properties from the configuration

            var inputWorkspace = Helpers.GetProperty<string>(configuration, "inputWorkspace");
            var outputWorkspace = Helpers.GetProperty<string>(configuration, "outputWorkspace");
            var sourceUtilityNetwork = Helpers.GetProperty<string>(configuration, "sourceUtilityNetwork");
            var namedTraceConfigurtion = Helpers.GetProperty<string>(configuration, "namedTraceConfigurtion");
            var networkSourceName = Helpers.GetProperty<string>(configuration, "networkSourceName");
            var definitionQuery = Helpers.GetProperty<string>(configuration, "definitionQuery");
            var assetGroupCode = Helpers.GetProperty<int>(configuration, "assetGroupCode");
            var outputPoints = Helpers.GetProperty<string>(configuration, "outputPoints", false);
            var outputPolylines = Helpers.GetProperty<string>(configuration, "outputPolylines", false);
            var outputPolygons = Helpers.GetProperty<string>(configuration, "outputPolygons", false);
            var outputTable = Helpers.GetProperty<string>(configuration, "outputTable", false);
            var functionFieldCount = Helpers.GetProperty<int>(configuration, "outputFunctionCount", false, 0);
            var sourceResultField = Helpers.GetProperty<string>(configuration, "sourceResultField", false);
            var terminalName = Helpers.GetProperty<string>(configuration, "terminalName", false);

            #endregion

            BatchTraceUsingPaths(inputWorkspace, sourceUtilityNetwork, networkSourceName, assetGroupCode, definitionQuery, sourceResultField,
                outputWorkspace, outputPolygons, outputPolylines, outputPoints, outputTable, functionFieldCount,
                analysisName, namedTraceConfigurtion, terminalName);
        }

        private static void BatchTraceUsingPaths(string inputWorkspacePath, string utilityNetworkClassName, string networkSourceName, int assetGroupCode, string definitionQuery, string sourceFieldName,
            string outputWorkspacePath, string polygonClassName, string polylineClassName, string pointClassName, string outputTableName, int functionFieldCount,
            string analysisName, string namedConfigurationName, string terminalName = "")
        {
            try
            {
                if (string.IsNullOrEmpty(inputWorkspacePath))
                    throw new ArgumentNullException("Missing required parameter", "Input Workspace");
                if (string.IsNullOrEmpty(utilityNetworkClassName))
                    throw new ArgumentNullException("Missing required parameter", "Utility Network");

                using var utilityNetworkWorkspace = Helpers.OpenGeodatabase(inputWorkspacePath);
                using var outputWorkspace = Helpers.OpenGeodatabase(outputWorkspacePath);
                using var utilityNetwork = utilityNetworkWorkspace.OpenDataset<UtilityNetwork>(utilityNetworkClassName);

                #region Load or create the output classes

                FeatureClass polygonClass, polylineClass, pointClass;
                Table outputTable;
                Helpers.LoadOutputs(polygonClassName, polylineClassName, pointClassName, outputTableName, functionFieldCount, outputWorkspace, utilityNetwork, out polygonClass, out polylineClass, out pointClass, out outputTable);

                #endregion

                Trace(utilityNetwork, polygonClass, polylineClass, pointClass, outputTable,
                    networkSourceName, assetGroupCode, definitionQuery, sourceFieldName,
                    analysisName, namedConfigurationName, terminalName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void Trace(UtilityNetwork utilityNetwork, FeatureClass polygonFeatureClass, FeatureClass polylineFeatureClass, FeatureClass pointFeatureClass, Table outputTable,
            string networkSourceName, int assetGroupCode, string definitionQuery, string sourceFieldName,
            string analysisName, string namedConfigurationName, string terminalName = "")
        {
            Helpers.CheckUtilityNetwork(utilityNetwork);

            using var utilityNetworkDefinition = utilityNetwork.GetDefinition();

            #region Load the definitions from the utility network

            using var networkSource = utilityNetworkDefinition.GetNetworkSource(networkSourceName) 
                ?? throw new ArgumentException(string.Format("Unable to find network source: '{0}'", networkSourceName));
            var networkSourceID = networkSource.ID;

            using var networkTable = utilityNetwork.GetTable(networkSource);

            #endregion

            #region Terminal Names

            // Look at all the terminal configurations for that asset group to find the target terminal for each asset type.
            // Because terminal names are not unique this is the correct way.
            // I applaud your creativity if you use the same terminal name for different configurations in the same asset group
            var assetTypeTerminals = new Dictionary<(int networkSource, int assetGroup, int assetType), Terminal>();
            if (!string.IsNullOrEmpty(terminalName))
            {
                var assetGroup = networkSource.GetAssetGroups().FirstOrDefault(assetGroup => assetGroup.Code == assetGroupCode)
                    ?? throw new Exception(string.Format("Unable to find asset group with code {1} on network source: '{0}'", networkSourceName, assetGroupCode));

                foreach (var assetType in assetGroup.GetAssetTypes())
                {
                    if (!assetType.IsTerminalConfigurationSupported()) continue;
                    var terminalConfiguration = assetType.GetTerminalConfiguration();
                    var thisTerminal = terminalConfiguration.Terminals
                        .FirstOrDefault(terminal => terminal.Name.Equals(terminalName, StringComparison.InvariantCultureIgnoreCase));

                    if (thisTerminal != null)
                        assetTypeTerminals[new (networkSourceID, assetGroup.Code, assetType.Code)] = thisTerminal;
                }

                if (assetTypeTerminals.Count == 0)
                    throw new Exception(string.Format("Unable to find terminal: '{0}' on asset group {1}", terminalName, assetGroup.Name));
            }

            #endregion

            #region Load all starting elements

            using var tableDefinition = networkTable.GetDefinition();
            var assetGroupFieldName = utilityNetworkDefinition.GetAssetGroupField();
            var assetTypeFieldName = utilityNetworkDefinition.GetAssetTypeField();

            var objectIdFieldName = tableDefinition.GetObjectIDField();
            var globalIdFieldName = tableDefinition.GetGlobalIDField();
            var sourceFieldIndex = tableDefinition.FindField(sourceFieldName);
            if (sourceFieldIndex == -1)
                throw new Exception(string.Format("Unable to find source field '{0}' on network source: '{1}'", sourceFieldName, networkSourceName));

            var startingElements = new List<Element>();
            var resultNameLookup = new Dictionary<long, string>();
            var allFunctionOutput = new List<IList<double>>();

            using var featureCursor = networkTable.Search(new QueryFilter
            {
                WhereClause = string.IsNullOrEmpty(definitionQuery)
                    ? string.Format("{0}={1}", assetGroupFieldName, assetGroupCode)
                    : string.Format("{0}={1} AND ({2})", assetGroupFieldName, assetGroupCode, definitionQuery),
                SubFields = string.Format("{0},{1},{2},{3},{4}", objectIdFieldName, globalIdFieldName, assetGroupFieldName, assetTypeFieldName, sourceFieldName)
            });
            while (featureCursor.MoveNext())
            {
                using var currentRow = featureCursor.Current;
                var startingElement = utilityNetwork.CreateElement(currentRow);

                if (assetTypeTerminals.TryGetValue(new (networkSourceID, assetGroupCode, startingElement.AssetType.Code), out Terminal targetTerminal))
                    startingElement.Terminal = targetTerminal;

                startingElements.Add(startingElement);

                resultNameLookup[startingElement.ObjectID] = Convert.ToString(currentRow[sourceFieldIndex]);
            }

            if (startingElements.Count == 0)
                throw new Exception(string.Format("Unable to find any starting feature for network source: '{0}'. Asset Group Code {1}, Filter: '{2}'", networkSourceName, assetGroupCode, definitionQuery));

            #endregion

            #region Create the tracer

            using var traceManager = utilityNetwork.GetTraceManager();
            var namedTraceConfiguration = traceManager.GetNamedTraceConfigurations(new NamedTraceConfigurationQuery { Names = [namedConfigurationName] }).FirstOrDefault() ?? throw new ArgumentException(string.Format("Unable to find named trace configuration: '{0}'", namedConfigurationName));
            var networkTracer = traceManager.GetTracer(namedTraceConfiguration);

            #endregion

            #region Define the output

            // We always need the elements to do the analysis
            var resultTypes = new List<ResultType>();
            if (polygonFeatureClass != null || polylineFeatureClass != null | pointFeatureClass != null)
                resultTypes.Add(ResultType.AggregatedGeometry);
            if (outputTable != null)
                resultTypes.Add(ResultType.Element);

            #endregion

            #region Perform the analysis

            var polygonOutput = new List<(long resultID, Geometry geometry)>();
            var polylineOutput = new List<(long resultID, Geometry geometry)>();
            var pointOutput = new List<(long resultID, Geometry geometry)>();
            var elementResults = new List<(long resultID, IEnumerable<Element> elements)>();
            var totalElementCount = 0;
            var functionOutputCount = 0;

            var startTime = DateTime.Now;
            var count = 0;
            Console.WriteLine(string.Format("{0}: {1} - Start tracing {2} elements", DateTime.Now.ToString(), analysisName, startingElements.Count));
            foreach (var startingElement in startingElements)
            {
                count += 1;

                if ((count % 100) == 0) Console.WriteLine(string.Format("{0}: {1} - Traced {2} elements", DateTime.Now, analysisName, count));

                // Force the aggregated geometry result type
                var traceArgument = new TraceArgument(namedTraceConfiguration, [startingElement]);

                // This is a workaround for a bug with the ArcGIS Pro SDK 3.5.2 or earlier with MaxHops
                // Only uncomment it if you are using that specific release
                // Helpers.RemoveMaxHops(traceArgument);

                // This should be unnecessary given that the trace configuration includes functions
                if (traceArgument.Configuration.Functions != null)
                {
                    functionOutputCount = traceArgument.Configuration.Functions.Count;
                    resultTypes.Add(ResultType.FunctionValue);
                }

                traceArgument.ResultTypes = resultTypes;

                try
                {
                    var traceResults = networkTracer.Trace(traceArgument);
                    foreach (var result in traceResults)
                    {
                        if (result is AggregatedGeometryResult aggregatedGeometryResult)
                        {
                            if (aggregatedGeometryResult.Polygon != null && polygonFeatureClass != null)
                                polygonOutput.Add(new (startingElement.ObjectID, aggregatedGeometryResult.Polygon));
                            if (aggregatedGeometryResult.Line != null && polylineFeatureClass != null)
                                polylineOutput.Add(new (startingElement.ObjectID, aggregatedGeometryResult.Line));
                            if (aggregatedGeometryResult.Point != null && pointFeatureClass != null)
                                pointOutput.Add(new (startingElement.ObjectID, aggregatedGeometryResult.Point));
                        }
                        else if (result is FunctionOutputResult functionOutputResult)
                        {
                            allFunctionOutput.Add(functionOutputResult.FunctionOutputs.Select(output => Convert.ToDouble(output.Value)).ToList());
                        }
                        else if (result is ElementResult elementResult)
                        {
                            if (outputTable == null)
                                continue;

                            var elements = elementResult.Elements;
                            if (elements == null || elements.Count == 0)
                                continue;

                            totalElementCount += elements.Count;
                            elementResults.Add(new (startingElement.ObjectID, elements));
                        }
                    }
                }
                catch (NotSupportedException notSupportException)
                {
                    Console.WriteLine(string.Format("{0}: Error tracing element {1}\r\n{2}", DateTime.Now.ToString(), startingElement.GlobalID, notSupportException.Message));
                    return;
                }
                catch (GeodatabaseUtilityNetworkException geoUnException)
                {
                    // This handles things like starting point outside the current tier
                    Console.WriteLine(string.Format("{0}: Error tracing element {1}\r\n{2}", DateTime.Now.ToString(), startingElement.GlobalID, geoUnException.Message));
                    continue;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Debug.WriteLine(ex);
                    throw new Exception(string.Format("Encountered an error while tracing element {0}", startingElement.GlobalID));
                }

            }
            var endTime = DateTime.Now;
            Console.WriteLine(string.Format("{0}: {1} - Finished tracing {2:n0} elements", DateTime.Now.ToString(), analysisName, count));
            Console.WriteLine(string.Format("{0}: {1} - Time elapsed {2:n0}", DateTime.Now.ToString(), analysisName, (endTime - startTime).TotalSeconds));

            #endregion

            #region Output the results

            if (polygonFeatureClass != null)
            {
                Console.WriteLine(string.Format("{0}: {1} - {2:n0} polygon result(s)", DateTime.Now.ToString(), analysisName, polygonOutput.Count));
                Helpers.StoreGeometryResults(polygonFeatureClass, analysisName, resultNameLookup, polygonOutput);
            }
            if (polylineFeatureClass != null)
            {
                Console.WriteLine(string.Format("{0}: {1} - {2:n0} line result(s)", DateTime.Now.ToString(), analysisName, polylineOutput.Count));
                Helpers.StoreGeometryResults(polylineFeatureClass, analysisName, resultNameLookup, polylineOutput);
            }
            if (pointFeatureClass != null)
            {
                Console.WriteLine(string.Format("{0}: {1} - {2} point result(s)", DateTime.Now.ToString(), analysisName, polylineOutput.Count));
                Helpers.StoreGeometryResults(pointFeatureClass, analysisName, resultNameLookup, pointOutput);
            }
            if (outputTable != null)
            {
                Console.WriteLine(string.Format("{0}: {1} - {2:n0} element result(s)", DateTime.Now.ToString(), analysisName, elementResults.Count));
                Console.WriteLine(string.Format("{0}: {1} - {2:n0} total element(s)", DateTime.Now.ToString(), analysisName, totalElementCount));
                Helpers.StoreElementResults(outputTable, analysisName, resultNameLookup, elementResults);
            }

            #endregion

        }

    }
}
