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
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.DDL;
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.Geometry;
using ArcGIS.Desktop.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Show_Flow_Arrows.FlowButton
{
    internal class SubnetworkParser
    {
        public SubnetworkParser(UtilityNetworkDefinition utilityNetworkDefinition, Tier sourceTier=null)
        {
            if (sourceTier != null)
                _tierName = sourceTier.Name;
        }

        private long _lastNetworkID = 1;
        private long GetNextNetworkID()
        {
            return _lastNetworkID++;
        }

        private Dictionary<int, string> _DirectionLookup =
            new()
            {
                {1, "With Digitized Direcction"},
                {2, "Against Digitized Direcction"},
                {3, "Indeterminate"},
                {4, "Bi-Directional"}
            };

        /// <summary>
        /// Collection of nodes in the graph. Look up the network ID for each node by its network source id, guid, and terminal
        /// </summary>
        private Dictionary<(long networkSourceId, string globalId, long terminalId), long> _nodes = new Dictionary<(long networkSourceId, string globalId, long terminalId), long>();
        private Dictionary<long, (long networkSourceId, string globalId, long terminalId)> _nodesReverseLookup = new Dictionary<long, (long networkSourceId, string globalId, long terminalId)>();

        /// <summary>
        /// Collection of edges in the graph. Look up the network ID for each edge by its network source id, guid, from position, and to position
        /// </summary>
        private Dictionary<(long networkSourceId, string globalId, double fromPosition, double toPosition), long> _edges = new Dictionary<(long networkSourceId, string globalId, double fromPosition, double toPosition), long>();
        private Dictionary<long, (long networkSourceId, string globalId, double fromPosition, double toPosition)> _edgesReverseLookup = new Dictionary<long, (long networkSourceId, string globalId, double fromPosition, double toPosition)>();

        private Dictionary<long, JObject> _edgeGeometry = new Dictionary<long, JObject>();
        private Dictionary<long, JObject> _pointGeometry = new Dictionary<long, JObject>();
        private Dictionary<long, int> _flowDirections = new Dictionary<long, int>();
        private Dictionary<long, string> _propagatedValues = new Dictionary<long, string>();

        private string _tierName = string.Empty;
        private bool _multiplePropagators = false;


        public int ParseTraceWithFlowDirection(string subnetworkName, string subnetworkExportPath, bool deletePreviousResults = false)
        {

            #region Prepare internal members to store the results

            _flowDirections.Clear();
            _edgeGeometry.Clear();
            _pointGeometry.Clear();

            var connectivity = new List<JObject>();
            var featureElements = new List<JObject>();
            var sourceMapping = new List<JObject>();
            JObject spatialReferenceElement = null;

            #endregion

            #region Parse the JSON file

            // The subnetwork name isn't stored in the file ... so we remember the export name
            var exportName = Path.GetFileNameWithoutExtension(subnetworkExportPath);

            using Stream fileStream = new FileStream(subnetworkExportPath, FileMode.Open);
            using var streamReader = new StreamReader(fileStream);
            var jsonReader = new JsonTextReader(streamReader);
            var serializer = new Newtonsoft.Json.JsonSerializer();

            // Read the opening object
            if (!jsonReader.Read())
                throw new ArgumentException("Unable to process file: Unable to read file.", "Subnetwork export");
            if (jsonReader.TokenType != JsonToken.StartObject)
                throw new ArgumentException("Unable to process file: Invalid start token", "Subnetwork export");

            while (jsonReader.Read())
            {
                if (jsonReader.TokenType != JsonToken.PropertyName)
                    continue;

                var propertyName = jsonReader.Value.ToString();
                if (!jsonReader.Read())
                    break;

                switch (jsonReader.TokenType)
                {
                    case JsonToken.StartObject:
                        // Debug.WriteLine("Object: " + propertyName);
                        var thisObject = serializer.Deserialize<Newtonsoft.Json.Linq.JObject>(jsonReader);
                        //Debug.WriteLine(thisObject);

                        if (propertyName.Equals("spatialReference", StringComparison.InvariantCultureIgnoreCase))
                            spatialReferenceElement = thisObject;
                        break;
                    case JsonToken.StartArray:
                        // Debug.WriteLine("Array: " + propertyName);
                        var elementCount = 0;
                        while (jsonReader.TokenType != JsonToken.EndArray)
                        {
                            elementCount += 1;
                            jsonReader.Read();
                            if (jsonReader.TokenType == JsonToken.EndArray)
                                break;

                            var thisElement = serializer.Deserialize<Newtonsoft.Json.Linq.JObject>(jsonReader);
                            switch (propertyName)
                            {
                                case "featureElements":
                                    featureElements.Add(thisElement);
                                    break;
                                case "sourceMapping":
                                    sourceMapping.Add(thisElement);
                                    break;
                            }
                        }
                        // Debug.WriteLine(elementCount + " elements");
                        break;
                    default:
                        Debug.WriteLine("Unhandled property: " + propertyName);
                        break;
                }
            }

            jsonReader.Close();
            streamReader.Close();
            fileStream.Close();

            #endregion

            #region Load Features

            if (featureElements == null)
                throw new ArgumentException("Unable to process file: No feature element", "Subnetwork export");
            else if (featureElements.Count == 0)
                throw new ArgumentException("Unable to process file: No features", "Subnetwork export");

            // Processing features will load geometry and read the flow geometry from the feetures
            ProcessFeatureFlowdirection(featureElements);

            #endregion

            return _flowDirections.Count;
        }

        internal IEnumerable<long> GetStartingElementKeys(IEnumerable<Element> startingElements)
        {
            return startingElements.Select(element => {
                long networkId;
                switch(element.NetworkSource.Type)
                {
                    case SourceType.Junction:
                        (long networkSourceId, string globalId, long terminalId) nodeKey = new(
                            element.NetworkSource.ID,
                            string.Format("{{{0}}}", element.GlobalID).ToUpper(),
                            element.Terminal.ID);

                        if (_nodes.TryGetValue(nodeKey, out networkId))
                            return networkId;

                        networkId = GetNextNetworkID();
                        _nodes[nodeKey] = networkId;
                        _nodesReverseLookup[networkId] = nodeKey;
                        return networkId;
                    case SourceType.Edge:
                        var fromPositionValue = element.PositionFrom;
                        var toPositionValue = element.PositionTo;
                        (long networkSourceId, string globalId, double fromPosition, double toPosition) edgeKey = new(
                            element.NetworkSource.ID,
                            string.Format("{{{0}}}", element.GlobalID).ToUpper(),
                            fromPositionValue,
                            toPositionValue);

                        if (_edges.TryGetValue(edgeKey, out networkId))
                            return networkId;

                        networkId = GetNextNetworkID();
                        _edges[edgeKey] = networkId;
                        _edgesReverseLookup[networkId] = edgeKey;
                        return networkId;
                    default:
                        throw new Exception("Starting element must be a junction or an edge: " + element.NetworkSource.Type);
                }
            });
        }

        private long GetKey(JObject element, string networkSourceIdField = "networkSourceId", string globalIdField = "globalId",
            string terminalIdField = "terminalId", string fromPositionField = "positionFrom", string toPositionField = "positionTo")
        {
            if (element.ContainsKey(fromPositionField))
            {
                // when from or to position is a 0 or 1 it is loaded as a 64-bit integer that requires some casting
                var fromPositionValue = (JValue)element.GetValue(fromPositionField);
                var toPositionValue = (JValue)element.GetValue(toPositionField);
                (long networkSourceId, string globalId, double fromPosition, double toPosition) edgeKey = new (
                    (long)((JValue)element.GetValue(networkSourceIdField)).Value,
                    (string)((JValue)element.GetValue(globalIdField)).Value,
                    fromPositionValue.Type == JTokenType.Integer ? Convert.ToDouble(fromPositionValue.Value) : (double)fromPositionValue.Value,
                    toPositionValue.Type == JTokenType.Integer ? Convert.ToDouble(toPositionValue.Value) : (double)toPositionValue.Value);

                if (_edges.TryGetValue(edgeKey, out var networkId))
                    return networkId;

                networkId = GetNextNetworkID();
                _edges[edgeKey] = networkId;
                _edgesReverseLookup[networkId] = edgeKey;
                return networkId;
            }
            else
            {
                (long networkSourceId, string globalId, long terminalId) nodeKey = new(
                    (long)((JValue)element.GetValue(networkSourceIdField)).Value,
                    (string)((JValue)element.GetValue(globalIdField)).Value,
                    (long)((JValue)element.GetValue(terminalIdField)).Value);

                if (_nodes.TryGetValue(nodeKey, out var networkId))
                    return networkId;

                networkId = GetNextNetworkID();
                _nodes[nodeKey] = networkId;
                _nodesReverseLookup[networkId] = nodeKey;
                return networkId;
            }
        }

        
        private void ProcessFeatureFlowdirection(IList<JObject> featureElements)
        {
            foreach (var featureElement in featureElements)
            {
                LoadGeometry(featureElement);

                // Parse all the feature elements to determine barriers
                LoadFlowDirection(featureElement);

                LoadPropagatedValues(featureElement);
            }
        }

        private void LoadGeometry(JObject featureElement)
        {
            // Only store the geometry when there is a position from, this filters out internal edges
            if (!featureElement.ContainsKey("geometry"))
                return;

            var featureKey = GetKey(featureElement);
            if (featureElement.ContainsKey("positionFrom"))
                _edgeGeometry[featureKey] = (JObject)featureElement["geometry"];
            else
                _pointGeometry[featureKey] = (JObject)featureElement["geometry"];
        }

        private void LoadFlowDirection(JObject featureElement)
        {
            if (!featureElement.TryGetValue("flowDirection", out JToken flowDirectionElement))
                return;

            // Some elements like devices with terminals can have multiple elements, we only store the first calcaulated flow
            var key = GetKey(featureElement);
            if (_flowDirections.TryGetValue(key, out int currentDirection) && currentDirection >= 0)
                return;

            var flowDirectionValue = flowDirectionElement as JValue;
            if (flowDirectionValue == null) return;

            int flowDirection = -1;
            switch (flowDirectionValue.Value)
            {
                case "withDigitized":
                    // Flow was calculated to originate at the first vertex and travel towards the last vertex of the edge.
                    flowDirection = 1;
                    break;
                case "againstDigitized":
                    // Flow was calculated to originate at the last vertex and travel towards the first vertex of the edge.
                    flowDirection = 2;
                    break;
                case "indeterminate":
                    //Indeterminate or bi-directional flow
                    flowDirection = 3;
                    break;
                default:
                    // There are only three possible enumerated values
                    flowDirection = -1;
                    break;
            }

            if (flowDirection == -1)
                return;

            _flowDirections[key] = flowDirection;
        }

        private void LoadPropagatedValues(JObject featureElement)
        {
            if (!featureElement.TryGetValue("propagatedValues", out var propagatedValues) || propagatedValues == null)
                return;

            var featureKey = GetKey(featureElement);

            //bool matched = false;
            foreach(JObject propagatedKeyValue in propagatedValues.Children())
            {
                if (!propagatedKeyValue.TryGetValue("propagatedValue", out var propagatedValue) || propagatedValue == null)
                    continue;

                //if (matched)
                //{
                //    _multiplePropagators = true;
                //    break;
                //}
                //
                //matched = true;
                _propagatedValues[featureKey] = Convert.ToString(propagatedValue);
                break;
            }
        }

        internal static Geodatabase GetOutputGeodatabase(string geodatabasePath)
        {
            if (geodatabasePath.EndsWith(".geodatabase", StringComparison.InvariantCultureIgnoreCase))
                return new Geodatabase(new MobileGeodatabaseConnectionPath(new Uri(geodatabasePath)));
            else if (geodatabasePath.EndsWith(".gdb", StringComparison.InvariantCultureIgnoreCase))
                return new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(geodatabasePath)));
            else if (geodatabasePath.EndsWith(".sde", StringComparison.InvariantCultureIgnoreCase))
                return new Geodatabase(new DatabaseConnectionFile(new Uri(geodatabasePath)));
            else
                throw new ArgumentException("Unrecognized output geodatabase type: " + geodatabasePath);
        }
        internal static FeatureClass GetOutputLineClass(Geodatabase geodatabase)
        {
            return geodatabase.OpenDataset<FeatureClass>("FlowArrows");
        }

        internal static FeatureClass GetOutputPointClass(Geodatabase geodatabase)
        {
            return geodatabase.OpenDataset<FeatureClass>("FlowPoints");
        }

        internal FeatureClass OutputGeometry(SpatialReference spatialReference, string exportName, string geodatabasePath = null, bool deleteAllRows = false, bool replaceRows = false)
        {
            if(string.IsNullOrEmpty(geodatabasePath))
            {
                var activeProject = Project.Current;
                geodatabasePath = activeProject.DefaultGeodatabasePath;
            }

            Geodatabase geodatabase = null;
            FeatureClass outputLineClass = null;
            FeatureClass outputPointClass = null;

            GetOutputClasses(geodatabasePath, deleteAllRows, spatialReference, out geodatabase, out outputLineClass, out outputPointClass);

            using var lineClassDefinition = outputLineClass.GetDefinition();
            var lineShapeFieldName = lineClassDefinition.GetShapeField();
            using var pointClassDefinition = outputPointClass.GetDefinition();
            var pointShapeFieldName = pointClassDefinition.GetShapeField();

            if (deleteAllRows)
            {
                outputLineClass.DeleteRows(new QueryFilter());
                outputPointClass.DeleteRows(new QueryFilter());
            }
            else if(replaceRows)
            {
                outputLineClass.DeleteRows(new QueryFilter() { WhereClause = "ExportName='" + exportName + "'" });
                outputPointClass.DeleteRows(new QueryFilter() { WhereClause = "ExportName='" + exportName + "'" });
            }

            OutputGeometry(geodatabase, exportName, outputLineClass, outputPointClass);

            return outputLineClass;
        }

        internal void GetOutputClasses(string geodatabasePath, bool deleteAll, SpatialReference spatialReference, out Geodatabase geodatabase, out FeatureClass outputLineClass, out FeatureClass outputPointClass)
        {
            if (string.IsNullOrEmpty(geodatabasePath))
            {
                var activeProject = Project.Current;
                geodatabasePath = activeProject.DefaultGeodatabasePath;
            }

            geodatabase = null;
            if (geodatabasePath.EndsWith(".geodatabase", StringComparison.InvariantCultureIgnoreCase))
                geodatabase = new Geodatabase(new MobileGeodatabaseConnectionPath(new Uri(geodatabasePath)));
            else if (geodatabasePath.EndsWith(".gdb", StringComparison.InvariantCultureIgnoreCase))
                geodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(geodatabasePath)));
            else if (geodatabasePath.EndsWith(".sde", StringComparison.InvariantCultureIgnoreCase))
                geodatabase = new Geodatabase(new DatabaseConnectionFile(new Uri(geodatabasePath)));
            else
                throw new ArgumentException("Unrecognized output geodatabase type: " + geodatabasePath);

            outputLineClass = null;
            outputPointClass = null;

            try
            {
                //TODO::Add a point layer if we have propagation
                outputLineClass = GetOutputLineClass(geodatabase);
            }
            catch (Exception e)
            {
                outputLineClass = CreateOutputLineClass(geodatabase, spatialReference);
            }

            try
            {
                //TODO::Add a point layer if we have propagation
                outputPointClass = GetOutputPointClass(geodatabase);
            }
            catch (Exception e)
            {
                outputPointClass = CreateOutputPointClass(geodatabase, spatialReference);
            }

            if(deleteAll)
            {
                outputLineClass.DeleteRows(new QueryFilter());
                outputPointClass.DeleteRows(new QueryFilter());
            }
        }

        internal void OutputGeometry(Geodatabase geodatabase, string exportName, FeatureClass outputLineClass, FeatureClass outputPointClass)
        {
            bool hasPropagatedValues = _propagatedValues.Count > 0;
            var lastCalculated = DateTime.Now;

            using var lineClassDefinition = outputLineClass.GetDefinition();
            var lineShapeFieldName = lineClassDefinition.GetShapeField();
            using var pointClassDefinition = outputPointClass.GetDefinition();
            var pointShapeFieldName = pointClassDefinition.GetShapeField();

            geodatabase.ApplyEdits(new Action(() => {
                using var lineCursor = outputLineClass.CreateInsertCursor();
                try
                {
                    var aggregatedGeometries = new Dictionary<Tuple<string,string>, List<Polyline>>();

                    foreach (var flowDirection in _flowDirections)
                    {
                        // Skip over any internal edges
                        if (!_edgeGeometry.ContainsKey(flowDirection.Key))
                            continue;

                        var edgeJson = _edgeGeometry[flowDirection.Key];
                        var lineGeometry = ConstructLineGeometry(edgeJson);

                        // Don't output zero-length lines
                        if (lineGeometry == null || lineGeometry.Length <= 0)
                            continue;

                        if (!_DirectionLookup.TryGetValue(flowDirection.Value, out string flowDirectionName))
                            flowDirectionName = "Unknown";

                        if (!_propagatedValues.TryGetValue(flowDirection.Key, out string propagatedValue))
                            propagatedValue = null;

                        var key = new Tuple<string, string>(flowDirectionName, propagatedValue);
                        if (!aggregatedGeometries.TryGetValue(key, out var geometryCollection))
                        {
                            geometryCollection = new List<Polyline>();
                            aggregatedGeometries[key] = geometryCollection;
                        }
                        geometryCollection.Add(lineGeometry);
                    }

                    using var rowBuffer = outputLineClass.CreateRowBuffer();
                    foreach(var aggregatedInfo in aggregatedGeometries)
                    {
                        rowBuffer["SubnetworkFlow"] = aggregatedInfo.Key.Item1;
                        rowBuffer["PropagatedValue"] = aggregatedInfo.Key.Item2;
                        rowBuffer["ExportName"] = exportName;
                        rowBuffer["TierName"] = _tierName;
                        rowBuffer["LastCalculated"] = lastCalculated;

                        rowBuffer[lineShapeFieldName] = PolylineBuilder.CreatePolyline(aggregatedInfo.Value);

                        lineCursor.Insert(rowBuffer);
                    }
                }
                finally
                {
                    lineCursor.Flush();
                }

                if (_multiplePropagators)
                    Debug.WriteLine("Network contains multiple propagators, only the first propoaged value will be output.");

                using var pointCursor = outputPointClass.CreateInsertCursor();
                try
                {
                    var aggregatedGeometries = new Dictionary<string, List<MapPoint>>();
                    foreach (var pointInfo in _pointGeometry)
                    {
                        if (!_propagatedValues.TryGetValue(pointInfo.Key, out string propagatedValue))
                            continue;

                        if (!aggregatedGeometries.TryGetValue(propagatedValue, out var geometryCollection))
                        {
                            geometryCollection = new List<MapPoint>();
                            aggregatedGeometries[propagatedValue] = geometryCollection;
                        }

                        var pointGeometry = ConstructPointGeometry(pointInfo.Value);
                        if (pointGeometry != null)
                            geometryCollection.Add(pointGeometry);
                    }

                    using var rowBuffer = outputPointClass.CreateRowBuffer();
                    foreach (var aggregatedInfo in aggregatedGeometries)
                    {
                        if (aggregatedInfo.Value.Count == 0)
                            continue;

                        rowBuffer["PropagatedValue"] = aggregatedInfo.Key;
                        rowBuffer["ExportName"] = exportName;
                        rowBuffer["TierName"] = _tierName;
                        rowBuffer["LastCalculated"] = lastCalculated;

                        rowBuffer[pointShapeFieldName] = MultipointBuilder.CreateMultipoint(aggregatedInfo.Value);

                        pointCursor.Insert(rowBuffer);
                    }
                }
                finally
                {
                    pointCursor.Flush();
                }
            }));
        }

        private Polyline ConstructLineGeometry(JObject jsonGeometry)
        {
            try
            {
                if (jsonGeometry.Count == 0)
                    return null;
                var newGeometry = PolylineBuilderEx.FromJson(jsonGeometry.ToString());
                return newGeometry;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return null;
        }
        private MapPoint ConstructPointGeometry(JObject jsonGeometry)
        {
            try
            {
                if (jsonGeometry.Count == 0)
                    return null;
                var newGeometry = MapPointBuilderEx.FromJson(jsonGeometry.ToString());
                return newGeometry;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return null;
        }

        private FeatureClass CreateOutputLineClass(Geodatabase geodatabase, SpatialReference spatialReference)
        {
            var fieldDescriptions = new List<FieldDescription> {
                new("SubnetworkFlow", FieldType.String),
                new("TierName", FieldType.String),
                new("ExportName", FieldType.String),
                new("LastCalculated", FieldType.Date),
                new("PropagatedValue", FieldType.String),
                };

            // This must be explicitly set, otherwise Z-Values aren't allowed in the class
            var shapeDescription = new ShapeDescription(GeometryType.Polyline, spatialReference) { HasZ = true };

            var tableDescription = new FeatureClassDescription("FlowArrows", fieldDescriptions, shapeDescription);

            var schemaBuilder = new SchemaBuilder(geodatabase);
            var tableToken = schemaBuilder.Create(tableDescription);
            if (!schemaBuilder.Build())
            {
                Console.WriteLine(string.Format("Error creating output feature class: {0}", "FlowArrows"));
                foreach (var errorMessage in schemaBuilder.ErrorMessages)
                    Console.WriteLine(errorMessage);

                throw new Exception(string.Format("Unable to create output feature class: {0}", "FlowArrows"));
            }

            return geodatabase.OpenDataset<FeatureClass>("FlowArrows");
        }
        private FeatureClass CreateOutputPointClass(Geodatabase geodatabase, SpatialReference spatialReference)
        {
            var fieldDescriptions = new List<FieldDescription> {
                new("TierName", FieldType.String),
                new("ExportName", FieldType.String),
                new("LastCalculated", FieldType.Date),
                new("PropagatedValue", FieldType.String)
                };

            // This must be explicitly set, otherwise Z-Values aren't allowed in the class
            var shapeDescription = new ShapeDescription(GeometryType.Multipoint, spatialReference) { HasZ = true };

            var tableDescription = new FeatureClassDescription("FlowPoints", fieldDescriptions, shapeDescription);

            var schemaBuilder = new SchemaBuilder(geodatabase);
            var tableToken = schemaBuilder.Create(tableDescription);
            if (!schemaBuilder.Build())
            {
                Console.WriteLine(string.Format("Error creating output feature class: {0}", "FlowPoints"));
                foreach (var errorMessage in schemaBuilder.ErrorMessages)
                    Console.WriteLine(errorMessage);

                throw new Exception(string.Format("Unable to create output feature class: {0}", "FlowPoints"));
            }

            return geodatabase.OpenDataset<FeatureClass>("FlowPoints");
        }

    }
}
