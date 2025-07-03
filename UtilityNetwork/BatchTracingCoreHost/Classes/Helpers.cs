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
using ArcGIS.Core.Data.DDL;
using ArcGIS.Core.Data.Exceptions;
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Core.SystemCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Geometry = ArcGIS.Core.Geometry.Geometry;
using QueryFilter = ArcGIS.Core.Data.QueryFilter;
using SpatialReference = ArcGIS.Core.Geometry.SpatialReference;

namespace BatchTracingCoreHost.Classes
{
    internal class Helpers
    {
        internal static T GetProperty<T>(IDictionary<string, object> properties, string propertyName, bool required = true, T defaultvalue = default(T))
        {
            if (!properties.TryGetValue(propertyName, out object value))
            {
                if (required)
                    throw new ArgumentException("Required configuration property is missing.", propertyName);

                return defaultvalue;
            }
            else
            {
                return (T)value;
            }
        }

        #region Paths and Workspaces

        internal static Geodatabase OpenGeodatabase(string workspacePath, string portalUrl = "", string portalUser = "", string portalPassword = "")
        {
            if (workspacePath.EndsWith(".geodatabase", StringComparison.InvariantCultureIgnoreCase))
                return new Geodatabase(new MobileGeodatabaseConnectionPath(new Uri(workspacePath)));
            else if (workspacePath.EndsWith(".gdb", StringComparison.InvariantCultureIgnoreCase))
                return new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(workspacePath)));
            else if (workspacePath.EndsWith(".sde", StringComparison.InvariantCultureIgnoreCase))
                return new Geodatabase(new DatabaseConnectionFile(new Uri(workspacePath)));
            else if (workspacePath.StartsWith("memory", StringComparison.InvariantCultureIgnoreCase))
                return new Geodatabase(new MemoryConnectionProperties(workspacePath));
            else if (workspacePath.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
                return OpenFeatureService(workspacePath, portalUrl, portalUser, portalPassword);
            else
                throw new ArgumentException("Unrecognized workspace type: " + workspacePath);
        }

        internal static Geodatabase OpenFeatureService(string featureServiceUrl, string portalUrl, string portalUser, string portalPassword)
        {
            var arcGisSignOn = ArcGISSignOn.Instance;
            var portalUri = new Uri(portalUrl);
            var workspaceUri = new Uri(featureServiceUrl);
            if (!arcGisSignOn.IsSignedOn(portalUri))
                arcGisSignOn.SignInWithCredentials(portalUri, portalUser, portalPassword, out var referer, out var token);
            
            return new Geodatabase(new ServiceConnectionProperties(workspaceUri));
        }

        internal static FeatureClass TryOpenOutputFeatureClass(Geodatabase outputWorkspace, string datasetName, GeometryType geometryType, SpatialReference spatialReference, int functionFieldCount = 0)
        {
            using var datasetDefinition = outputWorkspace.GetDefinition<FeatureClassDefinition>(datasetName);
            if (datasetDefinition == null)
            {
                Console.WriteLine(string.Format("Creating output feature class: {0}", datasetName));
                return CreateOutputFeatureClass(outputWorkspace, datasetName, geometryType, spatialReference, functionFieldCount);
            }
            else
                return outputWorkspace.OpenDataset<FeatureClass>(datasetName);
        }

        internal static Table TryOpenOutputTable(Geodatabase outputWorkspace, string datasetName)
        {
            using var datasetDefinition = outputWorkspace.GetDefinition<TableDefinition>(datasetName);
            if (datasetDefinition == null)
            {
                Console.WriteLine(string.Format("Creating output feature class: {0}", datasetName));
                return CreateOutputTable(outputWorkspace, datasetName);
            }

            return outputWorkspace.OpenDataset<Table>(datasetName);
        }

        #endregion

        #region Geometry Results

        internal static void StoreGeometryResults(FeatureClass outputFeatureClass, string analysisName, List<(long resultID, Geometry geometry)> geometryResults, int functionValueCount = 0, IList<IList<double>> allFunctionValues = null)
        {
            // Feature classes must belong to a workspace
            if (outputFeatureClass.GetDatastore() is not Geodatabase workspace)
                throw new NotImplementedException();
            
            // In a corehost application we call Apply Edits
            // If this were an add-in we would use EditOperations
            workspace.ApplyEdits(() =>
            {
                WriteAggregatedGeometryResults(outputFeatureClass, analysisName, geometryResults, functionValueCount, allFunctionValues);
            });
        }

        private static void WriteAggregatedGeometryResults(FeatureClass outputFeatureClass, string analysisName, List<(long resultID, Geometry geometry)> geometryResults, int functionValueCount = 0, IList<IList<double>> allFunctionValues = null)
        {
            using var outputFeatureClassDefinition = outputFeatureClass.GetDefinition();
            var featureClassName = outputFeatureClassDefinition.GetName();

            Console.WriteLine(string.Format("{0}: {1} - Deleting previous trace results...", DateTime.Now.ToString(), featureClassName));
            outputFeatureClass.DeleteRows(new QueryFilter { });

            Console.WriteLine(string.Format("{0}: {1} - Start inserting new results", DateTime.Now.ToString(), featureClassName));
            using var insertcursor = outputFeatureClass.CreateInsertCursor();
            using var rowBuffer = outputFeatureClass.CreateRowBuffer();

            var resultFieldIndex = rowBuffer.FindField(ResultNameFieldName);
            if (resultFieldIndex == -1)
                throw new Exception(string.Format("Unable to find result field '{0}' on output layer: '{1}'", ResultNameFieldName, featureClassName));

            // This is an optional field
            var analysisFieldIndex = -1;
            if (!string.IsNullOrEmpty(AnalysisFieldName))
            {
                analysisFieldIndex = rowBuffer.FindField(AnalysisFieldName);
                if (analysisFieldIndex == -1)
                    Console.WriteLine(string.Format("{0}: {1} - Unable to find analysis field '{2}' on output layer: '{3}'", DateTime.Now.ToString(), analysisName, AnalysisFieldName, featureClassName));
                else
                    rowBuffer[analysisFieldIndex] = analysisName;
            }

            var shapeFieldName = outputFeatureClassDefinition.GetShapeField();
            var shapeFieldIndex = rowBuffer.FindField(shapeFieldName);
            if (shapeFieldIndex == -1)
                throw new Exception(string.Format("Unable to find shape field '{0}' on output layer: '{1}'", shapeFieldName, featureClassName));

            var functionFieldIndices = Enumerable.Range(0, functionValueCount)
                .Select(i => outputFeatureClassDefinition.FindField(string.Format("function{0}", i)));
            if (functionValueCount == 0 || functionFieldIndices.All(i => i == -1))
                functionFieldIndices = null;

            foreach (var result in geometryResults)
            {
                rowBuffer[resultFieldIndex] = result.resultID;
                rowBuffer[shapeFieldIndex] = result.geometry;

                // Persist any function results
                if (functionFieldIndices != null && allFunctionValues.Count > 0)
                {
                    var functionValues = allFunctionValues[0];
                    allFunctionValues.RemoveAt(0);

                    var offset = -1;
                    foreach (var functionFieldIndex in functionFieldIndices)
                    {
                        offset += 1;
                        if (functionFieldIndex == -1 || offset > functionValues.Count)
                            rowBuffer[functionFieldIndex] = null;
                        else
                            rowBuffer[functionFieldIndex] = functionValues[offset];
                    }
                }

                insertcursor.Insert(rowBuffer);
            }

            Console.WriteLine(string.Format("{0}: {1} - Finished inserting results", DateTime.Now.ToString(), featureClassName));
        }

        internal static void StoreGeometryResults(FeatureClass outputFeatureClass, string analysisName, IDictionary<long, string> resultNames, IList<(long resultID, Geometry geometry)> geometryResults, int functionValueCount = 0, IList<IList<double>> allFunctionValues = null)
        {
            // Feature classes must belong to a workspace
            if (outputFeatureClass.GetDatastore() is not Geodatabase workspace)
                throw new NotImplementedException();

            // In a corehost application we call Apply Edits
            // If this were an add-in we would use EditOperations
            workspace.ApplyEdits(() =>
            {
                WriteAggregatedGeometryResults(outputFeatureClass, analysisName, resultNames, geometryResults, functionValueCount, allFunctionValues);
            });
        }

        private static void WriteAggregatedGeometryResults(FeatureClass outputFeatureClass, string analysisName, IDictionary<long, string> resultNames, IList<(long resultID, Geometry geometry)> geometryResults, int functionValueCount = 0, IList<IList<double>> allFunctionValues = null)
        {
            using var outputFeatureClassDefinition = outputFeatureClass.GetDefinition();
            var featureClassName = outputFeatureClassDefinition.GetName();

            Console.WriteLine(string.Format("{0}: {1} - Deleting previous trace results '{2}'", DateTime.Now.ToString(), analysisName, featureClassName));
            outputFeatureClass.DeleteRows(new QueryFilter { });

            Console.WriteLine(string.Format("{0}: {1} - Start inserting new results '{2};", DateTime.Now.ToString(), analysisName, featureClassName));
            using var insertcursor = outputFeatureClass.CreateInsertCursor();
            using var rowBuffer = outputFeatureClass.CreateRowBuffer();

            var resultFieldIndex = rowBuffer.FindField(ResultNameFieldName);
            if (resultFieldIndex == -1)
                throw new Exception(string.Format("Unable to find result field '{0}' on output layer: '{1}'", ResultNameFieldName, featureClassName));

            // This is an optional field
            var analysisFieldIndex = -1;
            if (!string.IsNullOrEmpty(AnalysisFieldName))
            {
                analysisFieldIndex = rowBuffer.FindField(AnalysisFieldName);
                if (analysisFieldIndex == -1)
                    Console.WriteLine(string.Format("{0}: {1} - Unable to find analysis field '{2}' on output layer: '{3}'", DateTime.Now.ToString(), analysisName, AnalysisFieldName, featureClassName));
                else
                    rowBuffer[analysisFieldIndex] = analysisName;
            }

            var shapeFieldName = outputFeatureClassDefinition.GetShapeField();
            var shapeFieldIndex = rowBuffer.FindField(shapeFieldName);
            if (shapeFieldIndex == -1)
                throw new Exception(string.Format("Unable to find shape field '{0}' on output layer: '{1}'", shapeFieldName, featureClassName));

            var functionFieldIndices = Enumerable.Range(0, functionValueCount)
                .Select(i => outputFeatureClassDefinition.FindField(string.Format("function{1}", i)));
            if (functionValueCount == 0 || functionFieldIndices.All(i => i == -1))
                functionFieldIndices = null;

            foreach (var result in geometryResults)
            {
                rowBuffer[resultFieldIndex] = resultNames[result.resultID];
                rowBuffer[shapeFieldIndex] = result.geometry;

                // Persist any function results
                if (functionFieldIndices != null)
                {
                    var functionValues = allFunctionValues[0];
                    allFunctionValues.RemoveAt(0);

                    var offset = -1;
                    foreach (var functionFieldIndex in functionFieldIndices)
                    {
                        offset += 1;
                        if (functionFieldIndex == -1 || offset > functionValues.Count)
                            rowBuffer[functionFieldIndex] = null;
                        else
                            rowBuffer[functionFieldIndex] = functionValues[offset];
                    }
                }

                insertcursor.Insert(rowBuffer);
            }

            Console.WriteLine(string.Format("{0}: {1} - Finished inserting results '{2}'", DateTime.Now.ToString(), analysisName, featureClassName));
        }

        #endregion

        #region Tabular Results

        internal static Table CreateOutputTable(Geodatabase geodatabase, string tableName)
        {
            var fieldDescriptions = new[]{
                new FieldDescription(AnalysisFieldName, FieldType.String),
                new FieldDescription(ResultNameFieldName, FieldType.String),
                new FieldDescription(IdentifierFieldName, FieldType.String),
                new FieldDescription(NetworkSourceIDFieldName, FieldType.SmallInteger),
                new FieldDescription(NetworkSourceNameFieldName, FieldType.String),
                new FieldDescription(ElementObjectID, FieldType.Integer), // Calculated risk
                new FieldDescription(ElementGlobalID, FieldType.String),
                new FieldDescription(AssetGroupCodeFieldName, FieldType.Integer),
                new FieldDescription(AssetGroupNameFieldName, FieldType.String),
                new FieldDescription(AssetTypeCodeFieldName, FieldType.SmallInteger),
                new FieldDescription(AssetTypeNameFieldName, FieldType.String),
                new FieldDescription(TerminalIDFieldName, FieldType.SmallInteger),
                new FieldDescription(TerminalNameFieldName, FieldType.String),
                };
            var tableDescription = new TableDescription(tableName, fieldDescriptions);
            var schemaBuilder = new SchemaBuilder(geodatabase);
            var tableToken = schemaBuilder.Create(tableDescription);
            if (!schemaBuilder.Build())
            {
                Console.WriteLine(string.Format("Error creating output table: {0}", tableName));
                foreach (var errorMessage in schemaBuilder.ErrorMessages)
                    Console.WriteLine(errorMessage);

                throw new Exception(string.Format("Unable to create output table: {0}", tableName));
            }
            return geodatabase.OpenDataset<Table>(tableName);
        }
        
        internal static FeatureClass CreateOutputFeatureClass(Geodatabase geodatabase, string tableName, GeometryType geometryType, SpatialReference spatialReference, int functionFieldcount = 0)
        {
            var fieldDescriptions = new List<FieldDescription> {
                new(AnalysisFieldName, FieldType.String),
                new(ResultNameFieldName, FieldType.String),
                };

            for (var i = 0; i < functionFieldcount; i++)
                fieldDescriptions.Add(new(string.Format("Function{0}", i), FieldType.Double));

            // This must be explicitly set, otherwise Z-Values aren't allowed in the class
            var shapeDescription = new ShapeDescription(geometryType, spatialReference) { HasZ = true };
            
            var tableDescription = new FeatureClassDescription(tableName, fieldDescriptions, shapeDescription);

            var schemaBuilder = new SchemaBuilder(geodatabase);
            var tableToken = schemaBuilder.Create(tableDescription);
            if (!schemaBuilder.Build())
            {
                Console.WriteLine(string.Format("Error creating output feature class: {0}", tableName));
                foreach (var errorMessage in schemaBuilder.ErrorMessages)
                    Console.WriteLine(errorMessage);

                throw new Exception(string.Format("Unable to create output feature class: {0}", tableName));
            }

            return geodatabase.OpenDataset<FeatureClass>(tableName);
        }

        private const string AnalysisFieldName = "AnalysisName";
        private const string ResultNameFieldName = "TraceName";
        private const string IdentifierFieldName = "SourceIdentifier";
        private const string NetworkSourceIDFieldName = "NetworkSourceID";
        private const string NetworkSourceNameFieldName = "NetworkSourceName";
        private const string ElementObjectID = "ElementObjectID";
        private const string ElementGlobalID = "ElementGuid";
        private const string AssetGroupCodeFieldName = "AssetGroupCode";
        private const string AssetGroupNameFieldName = "AssetGroupName";
        private const string AssetTypeCodeFieldName = "AssetTypeCode";
        private const string AssetTypeNameFieldName = "AssetTypeName";
        private const string TerminalIDFieldName = "TerminalID";
        private const string TerminalNameFieldName = "TerminalName";

        internal static void StoreElementResults(Table outputTable, string analysisName, 
            IDictionary<long, string> resultNames, IList<(long resultID, IEnumerable<Element> elements)> elementResults,
            List<IDictionary<(int networkSourceID, long objectID, int terminalID), string>> resultElementInfo = null)
        {
            // Feature classes must belong to a workspace
            if (outputTable.GetDatastore() is not Geodatabase workspace)
                throw new NotImplementedException();

            // In a corehost application we call Apply Edits
            // If this were an add-in we would use EditOperations
            workspace.ApplyEdits(() =>
            {
                WriteElementResults(outputTable, analysisName, resultNames, elementResults, resultElementInfo);
            });
        }

        internal static void StoreFeatureElementResults(Table outputTable, string analysisName, IList<IEnumerable<FeatureElement>> elementResults)
        {
            // Feature classes must belong to a workspace
            if (outputTable.GetDatastore() is not Geodatabase workspace)
                throw new NotImplementedException();

            // In a corehost application we call Apply Edits
            // If this were an add-in we would use EditOperations
            workspace.ApplyEdits(() =>
            {
                WriteFeatureElementResults(outputTable, analysisName, elementResults);

            });
        }

        private static void WriteElementResults(Table outputTable, string analysisName, 
            IDictionary<long, string> resultNames, IList<(long resultID, IEnumerable<Element> elements)> elementResults,
            List<IDictionary<(int networkSourceID,long objectID, int terminalID),string>> resultElementInfo = null)
        {
            using var tableDefinition = outputTable.GetDefinition();

            var analysisFieldIndex = tableDefinition.FindField(AnalysisFieldName);
            var resultNameFieldIndex = tableDefinition.FindField(ResultNameFieldName);
            var networkSourceIDFieldIndex = tableDefinition.FindField(NetworkSourceIDFieldName);
            var networkSourceNameFieldIndex = tableDefinition.FindField(NetworkSourceNameFieldName);
            var elementObjectIDFieldIndex = tableDefinition.FindField(ElementObjectID);
            var elementGlobaLIDFieldIndex = tableDefinition.FindField(ElementGlobalID);
            var assetGroupCodeFieldIndex = tableDefinition.FindField(AssetGroupCodeFieldName);
            var assetGroupNameFieldIndex = tableDefinition.FindField(AssetGroupNameFieldName);
            var assetTypeCodeFieldIndex = tableDefinition.FindField(AssetTypeCodeFieldName);
            var assetTypeNameFieldIndex = tableDefinition.FindField(AssetTypeNameFieldName);
            var terminalIDFieldIndex = tableDefinition.FindField(TerminalIDFieldName);
            var terminalNameFieldIndex = tableDefinition.FindField(TerminalNameFieldName);
            var identifierFieldIndex = tableDefinition.FindField(IdentifierFieldName);

            var tableName = tableDefinition.GetName();

            Console.WriteLine(string.Format("{0}: {1} - Deleting previous trace results '{2}'", DateTime.Now.ToString(), analysisName, tableName));
            outputTable.DeleteRows(new QueryFilter { });

            Console.WriteLine(string.Format("{0}: {1} - Start inserting new results '{2}'", DateTime.Now.ToString(), analysisName, tableName));
            using var insertcursor = outputTable.CreateInsertCursor();
            using var rowBuffer = outputTable.CreateRowBuffer();

            // This is an optional field
            rowBuffer[analysisFieldIndex] = analysisName;

            foreach (var result in elementResults)
            {
                if (resultNameFieldIndex > -1)
                    rowBuffer[resultNameFieldIndex] = resultNames[result.resultID];

                var sourceElementInfo = resultElementInfo != null && resultElementInfo.Count > 0
                    ? resultElementInfo[0]
                    : null;
                if (sourceElementInfo != null)
                    resultElementInfo.RemoveAt(0);

                foreach (var element in result.elements)
                {
                    var networkSource = element.NetworkSource;
                    var assetGroup = element.AssetGroup;
                    var assetType = element.AssetType;
                    var terminal = element.Terminal;

                    // Skip associations and network junctions
                    if (assetGroup == null)
                        continue;

                    rowBuffer[elementObjectIDFieldIndex] = element.ObjectID;
                    rowBuffer[elementGlobaLIDFieldIndex] = string.Format("{{{0}}}", element.GlobalID);

                    rowBuffer[networkSourceIDFieldIndex] = networkSource.ID;
                    rowBuffer[networkSourceNameFieldIndex] = networkSource.Name;

                    rowBuffer[assetGroupCodeFieldIndex] = assetGroup.Code;
                    rowBuffer[assetGroupNameFieldIndex] = assetGroup.Name;

                    rowBuffer[assetTypeCodeFieldIndex] = assetType.Code;
                    rowBuffer[assetTypeNameFieldIndex] = assetType.Name;

                    // Output terminal information
                    if (terminal == null)
                    {
                        rowBuffer[terminalIDFieldIndex] = null;
                        rowBuffer[terminalNameFieldIndex] = null;
                    }
                    else
                    {
                        rowBuffer[terminalIDFieldIndex] = terminal.ID;
                        rowBuffer[terminalNameFieldIndex] = terminal.Name;
                    }

                    if (identifierFieldIndex > -1)
                    {
                        if (sourceElementInfo == null)
                            rowBuffer[identifierFieldIndex] = "Content";
                        else
                        {
                            (int networkSourceID, long objectID, int terminalID) elementKey = new (networkSource.ID, element.ObjectID, terminal == null ? -1 : terminal.ID);
                            if (sourceElementInfo.TryGetValue(elementKey, out var elementInfo))
                                rowBuffer[identifierFieldIndex] = elementInfo;
                            else
                                //If we aren't told, we assume content. Knowledge of barrier, aggregate, etc. is done in the analytic
                                rowBuffer[identifierFieldIndex] = "Content";
                        }
                    }

                    insertcursor.Insert(rowBuffer);
                }
            }

            Console.WriteLine(string.Format("{0}: {1} - Finished inserting results '{2}'", DateTime.Now.ToString(), analysisName, tableName));
        }

        private static void WriteFeatureElementResults(Table outputTable, string analysisName, IList<IEnumerable<FeatureElement>> elementResults)
        {
            using var tableDefinition = outputTable.GetDefinition();

            var analysisFieldIndex = tableDefinition.FindField(AnalysisFieldName);
            var resultNameFieldIndex = tableDefinition.FindField(ResultNameFieldName);
            var networkSourceIDFieldIndex = tableDefinition.FindField(NetworkSourceIDFieldName);
            var networkSourceNameFieldIndex = tableDefinition.FindField(NetworkSourceNameFieldName);
            var elementObjectIDFieldIndex = tableDefinition.FindField(ElementObjectID);
            var elementGlobaLIDFieldIndex = tableDefinition.FindField(ElementGlobalID);
            var assetGroupCodeFieldIndex = tableDefinition.FindField(AssetGroupCodeFieldName);
            var assetGroupNameFieldIndex = tableDefinition.FindField(AssetGroupNameFieldName);
            var assetTypeCodeFieldIndex = tableDefinition.FindField(AssetTypeCodeFieldName);
            var assetTypeNameFieldIndex = tableDefinition.FindField(AssetTypeNameFieldName);
            var terminalIDFieldIndex = tableDefinition.FindField(TerminalIDFieldName);
            var terminalNameFieldIndex = tableDefinition.FindField(TerminalNameFieldName);
            var identifierFieldIndex = tableDefinition.FindField(IdentifierFieldName);

            var tableName = tableDefinition.GetName();

            Console.WriteLine(string.Format("{0}: {1} - Deleting previous trace results '{2}'", DateTime.Now.ToString(), analysisName, tableName));
            outputTable.DeleteRows(new QueryFilter { });

            Console.WriteLine(string.Format("{0}: {1} - Start inserting new results '{2}'", DateTime.Now.ToString(), analysisName, tableName));
            using var insertcursor = outputTable.CreateInsertCursor();
            using var rowBuffer = outputTable.CreateRowBuffer();

            // This is an optional field
            if (analysisFieldIndex > -1)
                rowBuffer[analysisFieldIndex] = analysisName;

            int count = 0;
            foreach (var result in elementResults)
            {
                count++;
                rowBuffer[resultNameFieldIndex] = count;

                foreach (var element in result)
                {
                    var networkSource = element.NetworkSource;
                    var assetGroup = element.AssetGroup;
                    var assetType = element.AssetType;
                    var terminal = element.Terminal;

                    // Skip associations and network junctions
                    if (assetGroup == null)
                        continue;
                    
                    rowBuffer[elementObjectIDFieldIndex] = element.ObjectID;
                    rowBuffer[elementGlobaLIDFieldIndex] = string.Format("{{{0}}}", element.GlobalID);

                    rowBuffer[networkSourceIDFieldIndex] = networkSource.ID;
                    rowBuffer[networkSourceNameFieldIndex] = networkSource.Name;

                    rowBuffer[assetGroupCodeFieldIndex] = assetGroup.Code;
                    rowBuffer[assetGroupNameFieldIndex] = assetGroup.Name;

                    rowBuffer[assetTypeCodeFieldIndex] = assetType.Code;
                    rowBuffer[assetTypeNameFieldIndex] = assetType.Name;

                    // Output terminal information
                    if (terminal == null)
                    {
                        rowBuffer[terminalIDFieldIndex] = null;
                        rowBuffer[terminalNameFieldIndex] = null;
                    }
                    else
                    {
                        rowBuffer[terminalIDFieldIndex] = terminal.ID;
                        rowBuffer[terminalNameFieldIndex] = terminal.Name;
                    }

                    // Ouptut field information
                    if (identifierFieldIndex > -1)
                    {
                        if (element.ResultFieldValues != null && element.ResultFieldValues.Count > 0)
                        {
                            // We force the element to only contain the value we want from that feature
                            var outputValue = element.ResultFieldValues.FirstOrDefault();
                            if (outputValue != null)
                                rowBuffer[identifierFieldIndex] = Convert.ToString(outputValue.Value);
                            else
                                rowBuffer[identifierFieldIndex] = null;
                        }
                        else
                            rowBuffer[identifierFieldIndex] = null;
                    }

                    insertcursor.Insert(rowBuffer);
                }
            }

            Console.WriteLine(string.Format("{0}: {1} - Finished inserting results '{2}'", DateTime.Now.ToString(), analysisName, tableName));
        }

        #endregion

        #region Misc

        internal static void LoadOutputs(string polygonClassName, string polylineClassName, string pointClassName, string outputTableName, int functionFieldCount, Geodatabase outputWorkspace, UtilityNetwork utilityNetwork, out FeatureClass polygonClass, out FeatureClass polylineClass, out FeatureClass pointClass, out Table outputTable)
        {
            // This doesn't support Z-values!
            using var networkDefinition = utilityNetwork.GetDefinition();
            var serviceTerritoryEnvelope = networkDefinition.GetServiceTerritoryEnvelope();
            var territorySpatialReference = serviceTerritoryEnvelope.SpatialReference;

            polygonClass = null;
            polylineClass = null;
            pointClass = null;
            outputTable = null;
            if (!string.IsNullOrEmpty(polygonClassName))
            {
                polygonClass = Helpers.TryOpenOutputFeatureClass(outputWorkspace, polygonClassName, GeometryType.Polygon, territorySpatialReference, functionFieldCount);
                if (polygonClass == null)
                {
                    Console.WriteLine("Unable to open or create polygon output class.");
                    return;
                }
            }
            if (!string.IsNullOrEmpty(polylineClassName))
            {
                polylineClass = Helpers.TryOpenOutputFeatureClass(outputWorkspace, polylineClassName, GeometryType.Polyline, territorySpatialReference, functionFieldCount);
                if (polylineClass == null)
                {
                    Console.WriteLine("Unable to open or create polyline output class.");
                    return;
                }
            }
            if (!string.IsNullOrEmpty(pointClassName))
            {
                pointClass = Helpers.TryOpenOutputFeatureClass(outputWorkspace, pointClassName, GeometryType.Multipoint, territorySpatialReference, functionFieldCount);
                if (pointClass == null)
                {
                    Console.WriteLine("Unable to open or create point output class.");
                    return;
                }
            }

            // Optional table from the output workspace
            if (!string.IsNullOrEmpty(outputTableName))
            {
                outputTable = Helpers.TryOpenOutputTable(outputWorkspace, outputTableName);
                if (outputTable == null)
                {
                    Console.WriteLine("Unable to open or create point output class.");
                    return;
                }
            }
        }


        internal static void CheckUtilityNetwork(UtilityNetwork utilityNetwork)
        {
            var utilityNetworkState = utilityNetwork.GetState();
            if (!utilityNetworkState.IsNetworkTopologyEnabled)
            {
                Console.WriteLine("Utility Network topology is currently disabled. Enable the network topology and try again.");
                return;
            }

            if (utilityNetworkState.HasErrors)
                Console.WriteLine("Warning: Utility Network contains one or more errors. Traces that validate consistency will fail.");
            else if (utilityNetworkState.HasDirtyAreas)
                Console.WriteLine("Warning: Utility Network contains one or more un-validated dirty areas. Traces that validate consistency will fail.");
        }

        internal static void RemoveMaxHops(TraceArgument traceArgument)
        {
            var traceArgtype = traceArgument.GetType();
            var hiddenTraceConfig = traceArgtype.InvokeMember("_traceConfiguration", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, Type.DefaultBinder, traceArgument, null);

            var traceConfigType = hiddenTraceConfig.GetType();
            traceConfigType.InvokeMember("MaxHops", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, Type.DefaultBinder, hiddenTraceConfig, [-1]);
            traceConfigType.InvokeMember("NumPaths", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, Type.DefaultBinder, hiddenTraceConfig, [-1]);

            traceArgtype.InvokeMember("_traceConfiguration", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField, Type.DefaultBinder, traceArgument, [hiddenTraceConfig]);
        }

        #endregion

    }
}
