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
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Data.Exceptions;

namespace BatchTracingCoreHost.Classes
{
    internal class PartitionAnalysis
    {
        internal static void Execute(string analysisName, IDictionary<string, object> configuration)
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
            var outputResultField = Helpers.GetProperty<string>(configuration, "outputResultField", false);
            var outputAnalysisField = Helpers.GetProperty<string>(configuration, "outputAnalysisField", false);

            #endregion

            PartitionUsingPaths(inputWorkspace, sourceUtilityNetwork, networkSourceName, assetGroupCode, definitionQuery, sourceResultField,
                outputWorkspace, outputPolygons, outputPolylines, outputPoints, outputTable, functionFieldCount,
                outputAnalysisField, outputResultField, namedTraceConfigurtion, analysisName);
        }
        
        private static void PartitionUsingPaths(string inputWorkspacePath, string utilityNetworkClassName, string networkSourceName, int assetGroupCode, string definitionQuery, string sourceResultField,
            string outputWorkspacePath, string polygonClassName, string polylineClassName, string pointClassName, string outputTableName, int functionFieldCount,
            string analysisFieldName, string resultFieldName, string namedConfigurationName, string analysisName)
        {
            try
            {
                if (string.IsNullOrEmpty(inputWorkspacePath))
                    throw new ArgumentNullException("Missing required parameter", "Input Workspace Path");
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

                Analyze(utilityNetwork, polygonClass, polylineClass, pointClass, outputTable,
                    networkSourceName, assetGroupCode, definitionQuery, sourceResultField, analysisFieldName, resultFieldName,
                    namedConfigurationName, analysisName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void Analyze(UtilityNetwork utilityNetwork,
            FeatureClass polygonFeatureClass, FeatureClass polylineFeatureClass, FeatureClass pointFeatureClass, Table outputTable,
            string networkSourceName, int assetGroupCode, string definitionQuery, string sourceFieldName, string analysisFieldName, string resultFieldName, string namedConfigurationName,
            string analysisName)
        {
            Helpers.CheckUtilityNetwork(utilityNetwork);

            #region Load all the starting elements

            using var utilityNetworkDefinition = utilityNetwork.GetDefinition();
            var assetGroupFieldName = utilityNetworkDefinition.GetAssetGroupField();
            var assetTypeFieldName = utilityNetworkDefinition.GetAssetTypeField();

            using var networkSource = utilityNetworkDefinition.GetNetworkSource(networkSourceName);
            if (networkSource == null)
                throw new ArgumentException(string.Format("Unable to find network source: '{0}'", networkSourceName));

            var networkSourceID = networkSource.ID;

            using var networkTable = utilityNetwork.GetTable(networkSource);
            using var tableDefinition = networkTable.GetDefinition();
            var objectIdFieldName = tableDefinition.GetObjectIDField();
            var globalIdFieldName = tableDefinition.GetGlobalIDField();

            // Because we constrain to a single network source, Object ID is fine
            var startTime = DateTime.Now;
            Console.WriteLine(string.Format("{0}: Loading starting elements", DateTime.Now.ToString()));
            var startingElements = new Dictionary<long, Element>();

            using var featureCursor = networkTable.Search(new QueryFilter
            {
                WhereClause = string.IsNullOrEmpty(definitionQuery)
                    ? string.Format("{0}={1}", assetGroupFieldName, assetGroupCode)
                    : string.Format("{0}={1} AND ({2})", assetGroupFieldName, assetGroupCode, definitionQuery),
                SubFields = string.Format("{0},{1},{2},{3}", objectIdFieldName, globalIdFieldName, assetGroupFieldName, assetTypeFieldName)
            });
            while (featureCursor.MoveNext())
            {
                var currentRow = featureCursor.Current;
                var startingElement = utilityNetwork.CreateElement(currentRow);
                startingElements[startingElement.ObjectID] = startingElement;
            }

            var endTime = DateTime.Now;
            Console.WriteLine(string.Format("{0}: Finished loading starting elements ({1:n0} elements discovered)", DateTime.Now.ToString(), startingElements.Count));
            Console.WriteLine(string.Format("{0}: Time elapsed {1}", DateTime.Now.ToString(), (endTime - startTime).TotalSeconds));

            if (startingElements.Count == 0)
                throw new Exception(string.Format("Unable to find any starting feature for network source: '{0}'. Asset Group Code {1}, Filter: '{2}'", networkSourceName, assetGroupCode, definitionQuery));

            #endregion

            #region Create the tracer

            var traceManager = utilityNetwork.GetTraceManager();
            var namedTraceConfiguration = traceManager.GetNamedTraceConfigurations(new NamedTraceConfigurationQuery { Names = [namedConfigurationName] }).FirstOrDefault();
            if (namedTraceConfiguration == null)
                throw new ArgumentException(string.Format("Unable to find named trace configuration: '{0}'", namedConfigurationName));

            var networkTracer = traceManager.GetTracer(namedTraceConfiguration);

            #endregion

            #region Define the output

            // Its cheaper to look up the field names up front to ensure they're valid, than to blindly pass them in
            var resultFields = new Dictionary<NetworkSource, List<string>>();

            var resultTypes = new List<ResultType>();
            if (polygonFeatureClass != null || polylineFeatureClass != null || pointFeatureClass != null)
                resultTypes.Add(ResultType.AggregatedGeometry);
            if (outputTable == null)
                resultTypes.Add(ResultType.Element);
            else
            {
                resultTypes.Add(ResultType.Feature);
                if (!string.IsNullOrEmpty(sourceFieldName))
                {
                    foreach(var thisNetworkSource in utilityNetworkDefinition.GetNetworkSources())
                    {
                        using var thisNetworkTable = utilityNetwork.GetTable(networkSource);
                        using var thisTableDefinition = thisNetworkTable.GetDefinition();
                        var sourceFieldIndex = thisTableDefinition.FindField(sourceFieldName);
                        if (sourceFieldIndex > -1)
                            resultFields[networkSource] = [sourceFieldName];

                        thisNetworkSource.Dispose();
                    }
                }
            }

            #endregion

            #region Perform the analysis

            var discoveredFeatures = new HashSet<long>();

            var polygonOutput = new List<(long resultID, Geometry geometry)>();
            var polylineOutput = new List<(long resultID, Geometry geometry)>();
            var pointOutput = new List<(long resultID, Geometry geometry)>();
            var tableOutputElements = new List<IEnumerable<FeatureElement>>();
            var allFunctionOutput = new List<IList<double>>();

            startTime = DateTime.Now;
            var count = 0;
            var totalElementCount = 0;
            var functionOutputCount = 0;
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
                if ((count % 100) == 0) Console.WriteLine(string.Format("{0}: Traced {1:n0} elements ({2:n0} elements discovered)", DateTime.Now, count, discoveredFeatures.Count));

                var traceArgument = new TraceArgument(namedTraceConfiguration, [startingInfo.Value]);

                // This is a workaround for a bug with the ArcGIS Pro SDK 3.5.0 with MaxHops
                // Only uncomment it if you are using that specific release
                // Helpers.RemoveMaxHops(traceArgument);

                if (traceArgument.Configuration.Functions != null)
                {
                    functionOutputCount = traceArgument.Configuration.Functions.Count;
                    resultTypes.Add(ResultType.FunctionValue);
                }

                traceArgument.ResultTypes = resultTypes;

                if (resultFields.Count > 0)
                    traceArgument.ResultOptions = new ResultOptions
                    {
                        ResultFields = resultFields,
                    };

                try
                {
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
                        else if (result is ElementResult elementResult)
                        {
                            var uniqueIDs = new HashSet<string>();
                            var elements = elementResult.Elements;
                            //Console.WriteLine(string.Format("{0}: Trace discovered {1} features", DateTime.Now.ToString(), elements.Count));
                            foreach (var element in elements)
                            {
                                if (networkSourceID != element.NetworkSource.ID ||
                                    assetGroupCode != element.AssetGroup.Code)
                                    continue;

                                discoveredFeatures.Add(element.ObjectID);
                            }
                        }
                        else if (result is FeatureElementResult featureElementResult)
                        {
                            var uniqueIDs = new HashSet<string>();
                            var elements = featureElementResult.FeatureElements;
                            //Console.WriteLine(string.Format("{0}: Trace discovered {1} features", DateTime.Now.ToString(), elements.Count));
                            foreach (var element in elements)
                            {
                                if (networkSourceID != element.NetworkSource.ID ||
                                    assetGroupCode != element.AssetGroup.Code)
                                    continue;

                                discoveredFeatures.Add(element.ObjectID);
                            }

                            if (elements.Count > 0)
                            {
                                totalElementCount += elements.Count;
                                tableOutputElements.Add(elements);
                            }
                        }
                        else if (result is FunctionOutputResult functionOutputResult)
                        {
                            allFunctionOutput.Add(functionOutputResult.FunctionOutputs.Select(output => Convert.ToDouble(output.Value)).ToList());
                        }
                    }
                }
                catch (NotSupportedException notSupportException)
                {
                    Console.WriteLine(string.Format("{0}: Error tracing element {1}\r\n{2}", DateTime.Now.ToString(), startingInfo.Value.GlobalID, notSupportException.Message));
                    return;
                }
                catch (GeodatabaseUtilityNetworkException geoUnException)
                {
                    // This handles things like starting point outside the current tier
                    Console.WriteLine(string.Format("{0}: Error tracing element {1}\r\n{2}", DateTime.Now.ToString(), startingInfo.Value.GlobalID, geoUnException.Message));
                    continue;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw new Exception(string.Format("Encountered an error while tracing element {0}\r\n{1}", startingInfo.Value.GlobalID, ex.Message));
                }
            }

            endTime = DateTime.Now;
            Console.WriteLine(string.Format("{0}: Traced {1:n0} elements ({2:n0} elements discovered)", DateTime.Now.ToString(), count, discoveredFeatures.Count));
            Console.WriteLine(string.Format("{0}: Time elapsed {1:n0}", DateTime.Now.ToString(), (endTime - startTime).TotalSeconds));

            #endregion

            #region Output the results

            if (polygonFeatureClass != null)
            {
                Console.WriteLine(string.Format("{0}: {1} - {2:n0} polygon result(s)", DateTime.Now.ToString(), analysisName, polygonOutput.Count));
                Helpers.StoreGeometryResults(polygonFeatureClass, analysisName, polygonOutput, functionOutputCount, allFunctionOutput);
            }
            if (polylineFeatureClass != null)
            {
                Console.WriteLine(string.Format("{0}: {1} - {2:n0} line result(s)", DateTime.Now.ToString(), analysisName, polylineOutput.Count));
                Helpers.StoreGeometryResults(polylineFeatureClass, analysisName, polylineOutput, functionOutputCount, allFunctionOutput);
            }
            if (pointFeatureClass != null)
            {
                Console.WriteLine(string.Format("{0}: {1} - {2:n0} point result(s)", DateTime.Now.ToString(), analysisName, polylineOutput.Count));
                Helpers.StoreGeometryResults(pointFeatureClass, analysisName, pointOutput, functionOutputCount, allFunctionOutput);
            }
            if (outputTable != null)
            {
                Console.WriteLine(string.Format("{0}: {1} - {2:n0} element result(s)", DateTime.Now.ToString(), analysisName, tableOutputElements.Count));
                Console.WriteLine(string.Format("{0}: {1} - {2:n0} total element(s)", DateTime.Now.ToString(), analysisName, totalElementCount));
                Helpers.StoreFeatureElementResults(outputTable, analysisName, tableOutputElements);
            }

            #endregion

        }
    }
}
