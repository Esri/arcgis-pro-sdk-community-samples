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
using ArcGIS.Desktop.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Calculate_Flow_Arrows.FlowButton
{
    internal class SubnetworkParser
    {
        public SubnetworkParser(UtilityNetworkDefinition utilityNetworkDefinition, Tier sourceTier, bool synthesizeGeometry = true)
        {
            _isSourceBasedDomain = sourceTier.DomainNetwork.SubnetworkControllerType == SubnetworkControllerType.Source;
            _tierName = sourceTier.Name;
            _synthesizeGeometry = synthesizeGeometry;
            LoadTerminalConfigurations(utilityNetworkDefinition);
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
        /// The graph stores all the connectivity for each element.
        /// The key is the element's unique identifier
        /// The value is the edge and to element
        /// </summary>
        private Dictionary<long, List<(long viaElement, long toElement, bool withDigitizedDirection)>> _graph = new Dictionary<long, List<(long viaElement, long toElement, bool withDigitizedDirection)>>();

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

        private Dictionary<long, (long fromNode, long toNode)> _edgeNodes = new Dictionary<long, (long fromNode, long toNode)>();

        private Dictionary<long, bool> _barriers = new Dictionary<long, bool>();

        private Dictionary<long, JObject> _edgeGeometry = new Dictionary<long, JObject>();
        private Dictionary<long, JObject> _pointGeometry = new Dictionary<long, JObject>();

        private Dictionary<long, Terminal> _terminalDefinitions = new Dictionary<long, Terminal>();

        private bool _isSourceBasedDomain = true;
        private bool _synthesizeGeometry = true;
        private string _tierName = string.Empty;
        private long _singleTerminalId = -1;

        private void LoadTerminalConfigurations(UtilityNetworkDefinition utilityNetworkDefinition)
        {
            _terminalDefinitions = utilityNetworkDefinition.GetTerminalConfigurations()
                .SelectMany(terminalConfiguration => terminalConfiguration.Terminals)
                .ToDictionary(terminal => Convert.ToInt64(terminal.ID), terminal => terminal);
            _singleTerminalId = _terminalDefinitions
                .First(terminal => terminal.Value.Name.Equals("Single Terminal", StringComparison.InvariantCultureIgnoreCase)).Key;
        }

        public int ParseSubnetwork(string subnetworkName, string subnetworkExportPath, string statusNetworkAttribute = "E:Device Status", long statusNetworkValue = 1, bool deletePreviousResults = false, IEnumerable<long> startingElements = null)
        {

            #region Prepare internal members to store the results

            // In case a user calls this method twice
            if (startingElements == null)
            {
                _graph.Clear();

                _nodes.Clear();
                _nodesReverseLookup.Clear();

                _edges.Clear();
                _edgesReverseLookup.Clear();
                _edgeGeometry.Clear();

                _edgeNodes.Clear();
            }

            _barriers.Clear();

            var connectivity = new List<JObject>();
            var controllers = new List<JObject>();
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
                                case "connectivity":
                                    connectivity.Add(thisElement);
                                    break;
                                case "controllers":
                                    controllers.Add(thisElement);
                                    break;
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

            #region Load Features / Barriers

            if (featureElements == null)
                throw new ArgumentException("Unable to process file: No feature element", "Subnetwork export");
            else if (featureElements.Count == 0)
                return 0;

            // Processing features will load geometry and flag any features as barriers
            // We currently only support open/closed status.
            // There is currently no support for additional barriers (lifecycle status) or propagation
            // Process features before connectivity, so we can synthesize geometries
            ProcessFeatures(featureElements, statusNetworkAttribute, statusNetworkValue);

            #endregion

            #region Load Connectivity

            if (connectivity == null)
                throw new ArgumentException("Unable to process file: No connectivity element", "Subnetwork export");
            else if (connectivity.Count == 0)
                return 0;

            LoadGraph(connectivity);
            //Debug.WriteLine("Nodes: " + _nodes.Count);
            //Debug.WriteLine("Edges: " + _edges.Count);
            //Debug.WriteLine("Connections: " + _graph.Count);

            #endregion

            #region Set subnetwork controllers as starting elements (if not provided)

            // Breadth first is fast, but has known issues with how it detects loops
            if (startingElements == null)
            {
                if (controllers == null)
                    throw new ArgumentException("Unable to process file: No subnetwork controller element", "Subnetwork export");
                else if (controllers.Count == 0)
                    throw new ArgumentException("Unable to process file: No subnetwork controllers", "Subnetwork export");

                startingElements = controllers.Select(controller => GetKey(controller));
            }

            #endregion

            #region Walk the graph

            // Depth first is better at loop detection
            _flowDirections = WalkGraphDepthFirst(startingElements);

            #endregion

            return featureElements.Count;
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

        private void LoadGraph(IList<JObject> connectivity)
        {
            foreach (var connectivityElement in connectivity)
            {
                var fromKey = GetKey(connectivityElement, networkSourceIdField: "fromNetworkSourceId", globalIdField: "fromGlobalId", terminalIdField: "fromTerminalId");
                var toKey = GetKey(connectivityElement, networkSourceIdField: "toNetworkSourceId", globalIdField: "toGlobalId", terminalIdField: "toTerminalId");
                var viaKey = GetKey(connectivityElement, networkSourceIdField: "viaNetworkSourceId", globalIdField: "viaGlobalId", fromPositionField: "viaPositionFrom", toPositionField: "viaPositionTo");

                if (!_graph.TryGetValue(fromKey, out var edges))
                    edges = _graph[fromKey] = new List<(long viaElement, long toElement, bool withDigitizedDirection)>();
                edges.Add(new(viaKey, toKey, true));

                if (!_graph.TryGetValue(toKey, out edges))
                    edges = _graph[toKey] = new List<(long viaElement, long toElement, bool withDigitizedDirection)>();
                edges.Add(new(viaKey, fromKey, false));

                _edgeNodes[viaKey] = (fromKey, toKey);

                if (_edgeGeometry.ContainsKey(viaKey))
                    continue;

                // Only proceed if there is a via geometry
                if (connectivityElement.TryGetValue("viaGeometry", out var viaGeometryObject))
                {
                    if (viaGeometryObject is not JObject viaGeometry)
                        continue;

                    // Only proceed if the geometry is present
                    if (!viaGeometry.TryGetValue("geometry", out var geometryObject))
                    {
                        if (!_synthesizeGeometry)
                            continue;

                        if (!_pointGeometry.TryGetValue(fromKey, out var fromGeometry))
                        {
                            if (!connectivityElement.TryGetValue("fromGeometry", out var fromGeometryObject))
                                continue;
                            fromGeometry = (JObject)fromGeometryObject;
                            if (!fromGeometry.ContainsKey("x"))
                                continue;
                        }
                        if (!_pointGeometry.TryGetValue(toKey, out var toGeometry))
                        {
                            if (!connectivityElement.TryGetValue("toGeometry", out var toGeometryObject))
                                continue;
                            toGeometry = (JObject)toGeometryObject;
                            if (!toGeometry.ContainsKey("x"))
                                continue;
                        }

                        geometryObject = new JObject
                        {
                            ["hasZ"] = true,
                            ["hasM"] = false,
                            ["paths"] = new JArray
                                {
                                    new JArray
                                    {
                                        new JArray{ fromGeometry["x"], fromGeometry["y"], fromGeometry["z"], fromGeometry["m"] },
                                        new JArray{toGeometry["x"], toGeometry["y"], toGeometry["z"], toGeometry["m"] },
                                    }
                                }
                        };
                    }

                    _edgeGeometry[viaKey] = (JObject)geometryObject;
                }
            }
        }


        private List<(long elementId, bool withDigitizedDirection)> _flow = new List<(long elementId, bool withDigitizedDirection)>();
        private HashSet<long> _visitedJunctions = new HashSet<long>();
        private HashSet<long> _visitedEdges = new HashSet<long>();
        private HashSet<long> _loopedElements = new HashSet<long>();
        private List<long> _currentPath = new List<long>();
        private Stack<(int depth, long junctionId)> _pendingPaths = new Stack<(int depth, long junctionId)>();
        private Dictionary<long, long[]> _visitedPaths = new Dictionary<long, long[]>();
        private Dictionary<long, int> _flowDirections = new Dictionary<long, int>();

        private Dictionary<long, int> WalkGraphDepthFirst(IEnumerable<long> startingElements)
        {
            _flow.Clear();
            _visitedJunctions.Clear();
            _visitedEdges.Clear();
            _loopedElements.Clear();
            _currentPath.Clear();
            _pendingPaths.Clear();
            _visitedPaths.Clear();

            // Process each subnetwork controller separately
            // If you decide to implement phasing you can use this to process phases separately
            foreach (var startingElement in startingElements)
            {
                _pendingPaths.Clear();
                _visitedPaths.Clear();
                _visitedJunctions.Clear();
                _visitedEdges.Clear();

                _visitedJunctions.Add(startingElement);

                _currentPath.Clear();
                _currentPath.Add(startingElement);

                // If the controller isn't connected to any lines, skip it
                if (!_graph.ContainsKey(startingElement))
                    continue;

                RecurseGraph(startingElement, 1);
            }

            // Group the resulting calculated flow for each network element
            var flowDirections = _flow.GroupBy(tuple => tuple.elementId, tuple => tuple.withDigitizedDirection)
                .Select(grouping =>
                {
                    (long elementId, bool[] flowDirections) flow = new(grouping.Key, grouping.Distinct().ToArray());
                    return flow;
                })
                .ToArray();

            // These are the coded values used for flow direction in this add-in
            // Note that we flip the digitized direction based on whether we are source/sink based
            var withDigitizedDirection = _isSourceBasedDomain ? 1 : 2;
            var againstDigitizedDirection = _isSourceBasedDomain ? 2 : 1;
            var indeterminate = 3;
            var biDirectional = 4;

            // Check to see if each element was discovered as part of a loop during analysis
            return flowDirections.ToDictionary(flow => flow.elementId, flow =>

                // If its part of a loop, report it as indeterminate
                _loopedElements.Contains(flow.elementId)
                    ? indeterminate

                    // If it was discovered to have opposing directions with regards to the subnetwork controllers, it is bi-directional.
                    // Othwise report out with/against digitized direction
                    : flow.flowDirections.Length > 1
                        ? biDirectional
                        : flow.flowDirections[0] ? withDigitizedDirection : againstDigitizedDirection);
        }

        private bool GetCommonAncestorPath(long commonAncestorElement)
        {
            // If the common ancestor is in the current path, use the current path to identify the looped elements
            var commonAncestorIndex = _currentPath.IndexOf(commonAncestorElement);
            if (commonAncestorIndex >= 0)
            {
                foreach (var loopedelement in _currentPath.Slice(commonAncestorIndex, _currentPath.Count - commonAncestorIndex))
                    _loopedElements.Add(loopedelement);
                return true;
            }

            // If we have not processed a path for the common ancestor element, fail. This shouldn't happen
            var canonicalPath = _visitedPaths[commonAncestorElement];
            commonAncestorIndex = Array.IndexOf(canonicalPath, commonAncestorElement);
            if (commonAncestorIndex == -1)
                return false;

            // Find the common path between the current element and the common ancestor element
            var slice = new ArraySegment<long>(canonicalPath, commonAncestorIndex, canonicalPath.Length - commonAncestorIndex);
            foreach (var loopedelement in slice)
                _loopedElements.Add(loopedelement);
            return true;
        }

        private void RecurseGraph(long fromElement, int depth)
        {
            var connections = _graph[fromElement];
            foreach (var connection in connections)
            {
                // This is a recursive algorithm
                // When we finish processing a branch, we need to prune the current path back to the depth of the branching element
                while (depth < _currentPath.Count)
                    _currentPath.RemoveAt(_currentPath.Count - 1);

                var toElement = connection.toElement;
                var viaElement = connection.viaElement;

                var fromNode = _nodesReverseLookup[fromElement];
                var toNode = _nodesReverseLookup[toElement];
                var viaEdge = _edgesReverseLookup[viaElement];
                if (_visitedEdges.Contains(connection.viaElement))
                    continue;

                (long fromElement, long viaElement, long toElement) fullConnection = new(fromElement, viaElement, toElement);

                // If the edge is a barrier, we don't attempt to connect to anything.
                if (_barriers.TryGetValue(viaElement, out var edgeBarrier) && edgeBarrier)
                    continue;

                // If this is an internal edge we need see if the directionality of terminals allows us to proceed
                if (fromNode.networkSourceId == viaEdge.networkSourceId &&
                    fromNode.globalId == viaEdge.globalId &&
                    fromNode.networkSourceId == toNode.networkSourceId &&
                    fromNode.globalId == toNode.globalId)
                {
                    var thisTerminal = _terminalDefinitions[fromNode.terminalId];
                    var otherTerminal = _terminalDefinitions[toNode.terminalId];

                    /// If we're in a source based system
                    /// We can't traverse from the downstream terminal to the upstream terminal via an internal edge
                    if (_isSourceBasedDomain &&
                        !thisTerminal.IsUpstreamTerminal &&
                        otherTerminal.IsUpstreamTerminal)
                        continue;

                    /// If we're in a sink based system
                    /// We can't traverse from the upstream terminal to the downstream terminal via an internal edge
                    else if (!_isSourceBasedDomain &&
                        thisTerminal.IsUpstreamTerminal &&
                        !otherTerminal.IsUpstreamTerminal)
                        continue;
                }

                if (_currentPath.Contains(toElement) || _visitedJunctions.Contains(toElement))
                {
                    //We've got a loop!
                    _loopedElements.Add(viaElement);
                    _loopedElements.Add(toElement);

                    _flow.Add(new(connection.viaElement, connection.withDigitizedDirection));
                    //Debug.WriteLine("Depth {3}: {0} > {1} > {2} (loop detected)", fromNode.globalId, viaEdge.globalId, toNode.globalId, depth);

                    if(!GetCommonAncestorPath(toElement))
                        Debug.WriteLine("Depth {3}: {0} > {1} > {2} (unable to find common ancestor for loop)", fromNode.globalId, viaEdge.globalId, toNode.globalId, depth);

                    continue;
                }

                // Things get more nuanced with a device/junction barrier
                if (_barriers.TryGetValue(toElement, out var pointBarrier) && pointBarrier)
                {
                    /// There are four outcomes here:
                    /// 1 - We can't cross an internal edge on an open device
                    /// 2 - If the destination doesn't have terminals, we can cross the edge but must stop
                    /// 3 - If the destination has terminals we can proceed as normal to any connections on the same terminal
                    /// 3a - We already checked to see if the directionality of the terminals is a barrier in an earlier step

                    // 1- If the from/via/to are all the same feature, we stop without processing anything
                    if (fromNode.networkSourceId == viaEdge.networkSourceId &&
                        fromNode.globalId == viaEdge.globalId &&
                        fromNode.networkSourceId == toNode.networkSourceId &&
                        fromNode.globalId == toNode.globalId)
                        continue;

                    if (toNode.terminalId == _singleTerminalId)
                    {
                        // 2 - If the other side is a barrier, but doesn't have terminals, we process the edge and mark it as visited
                        _visitedEdges.Add(viaElement);
                        _flow.Add(new(connection.viaElement, connection.withDigitizedDirection));
                    }
                    else
                    {
                        // 3 - If the other side is a barrier, but has terminals, we traverse the edge normally
                        _visitedEdges.Add(viaElement);

                        _flow.Add(new(connection.viaElement, connection.withDigitizedDirection));
                        //Debug.WriteLine("Depth {3}: {0} > {1} > {2}", fromNode.globalId, viaEdge.globalId, toNode.globalId, depth);

                        _visitedPaths[fromElement] = _currentPath.ToArray();
                        _currentPath.Add(viaElement);
                        _currentPath.Add(toElement);
                        _visitedPaths[toElement] = _currentPath.ToArray();
                        RecurseGraph(toElement, depth + 2);
                    }

                    continue;
                }

                if (!_visitedEdges.Contains(viaElement))
                {
                    _visitedEdges.Add(viaElement);
                    _visitedJunctions.Add(toElement);
                    
                    _flow.Add(new(connection.viaElement, connection.withDigitizedDirection));
                    //Debug.WriteLine("Depth {3}: {0} > {1} > {2}", fromNode.globalId, viaEdge.globalId, toNode.globalId, depth);

                    _visitedPaths[fromElement] = _currentPath.ToArray();
                    _currentPath.Add(viaElement);
                    _currentPath.Add(toElement);
                    _visitedPaths[toElement] = _currentPath.ToArray();
                    RecurseGraph(toElement, depth + 2);
                }
            }
        }

        private void ProcessFeatures(IList<JObject> featureElements, string statusNetworkAttribute = "E:Device Status", long statusNetworkValue = 1)
        {
            foreach (var featureElement in featureElements)
            {
                LoadGeometry(featureElement);

                // Parse all the feature elements to determine barriers
                CheckConditionBarrier(featureElement, statusNetworkAttribute, statusNetworkValue);
            }
        }

        private void LoadGeometry(JObject featureElement)
        {
            // Only store the geometry when there is a position from, this filters out internal edges
            if (!featureElement.ContainsKey("geometry"))
                return;

            var featureKey= GetKey(featureElement);
            if (featureElement.ContainsKey("positionFrom"))
            {
                _edgeGeometry[featureKey] = (JObject)featureElement["geometry"];
                return;
            }

            if (!_synthesizeGeometry) return;

            _pointGeometry[featureKey] = (JObject)featureElement["geometry"];
            return;
        }

        private bool CheckConditionBarrier(JObject featureElement, string statusNetworkAttribute = "E:Device Status", long statusNetworkValue = 1)
        {
            // No attributes, no barriers
            if (!featureElement.TryGetValue("networkAttributeValues", out JToken networkAttributes))
                return false;

            var key = GetKey(featureElement);
            if (_barriers.TryGetValue(key, out var isBarrier))
                return isBarrier;

            foreach (var networkAttribute in networkAttributes)
            {
                var attribute = networkAttribute.First as JProperty;
                if (attribute is null) continue;

                var attributeName = attribute.Name;
                if (!attributeName.Equals(statusNetworkAttribute, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                // It has a device status, but its not closed
                var attributeJsonValue = attribute.Value as JValue;
                if (attributeJsonValue == null || attributeJsonValue.Type != JTokenType.Integer)
                    break;

                // Confirmed to be closed
                if ((long)attributeJsonValue.Value == statusNetworkValue)
                    isBarrier = true;

                break;
            }

            _barriers[key] = isBarrier;
            return isBarrier;
        }

        internal static FeatureClass GetOutputClass(string geodatabasePath)
        {
            Geodatabase geodatabase = null;
            if (geodatabasePath.EndsWith(".geodatabase", StringComparison.InvariantCultureIgnoreCase))
                geodatabase = new Geodatabase(new MobileGeodatabaseConnectionPath(new Uri(geodatabasePath)));
            else if (geodatabasePath.EndsWith(".gdb", StringComparison.InvariantCultureIgnoreCase))
                geodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(geodatabasePath)));
            else if (geodatabasePath.EndsWith(".sde", StringComparison.InvariantCultureIgnoreCase))
                geodatabase = new Geodatabase(new DatabaseConnectionFile(new Uri(geodatabasePath)));
            else
                throw new ArgumentException("Unrecognized output geodatabase type: " + geodatabasePath);

            try
            {
                return geodatabase.OpenDataset<FeatureClass>("FlowLines");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }

        internal FeatureClass OutputGeometry(SpatialReference spatialReference, string exportName, string geodatabasePath = null, bool deleteAllRows = false)
        {
            if(string.IsNullOrEmpty(geodatabasePath))
            {
                var activeProject = Project.Current;
                geodatabasePath = activeProject.DefaultGeodatabasePath;
            }

            Geodatabase geodatabase = null;
            if (geodatabasePath.EndsWith(".geodatabase", StringComparison.InvariantCultureIgnoreCase))
                geodatabase = new Geodatabase(new MobileGeodatabaseConnectionPath(new Uri(geodatabasePath)));
            else if (geodatabasePath.EndsWith(".gdb", StringComparison.InvariantCultureIgnoreCase))
                geodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(geodatabasePath)));
            else if (geodatabasePath.EndsWith(".sde", StringComparison.InvariantCultureIgnoreCase))
                geodatabase = new Geodatabase(new DatabaseConnectionFile(new Uri(geodatabasePath)));
            else
                throw new ArgumentException("Unrecognized output geodatabase type: " + geodatabasePath);

            FeatureClass outputClass = null;

            try
            {
                outputClass = geodatabase.OpenDataset<FeatureClass>("FlowLines");
            }
            catch (Exception e)
            {
                outputClass = CreateOutputClass(geodatabase, spatialReference);
            }

            using var outputClassDefinition = outputClass.GetDefinition();
            var shapeFieldName = outputClassDefinition.GetShapeField();

            if (deleteAllRows)
                outputClass.DeleteRows(new QueryFilter());
            else
                outputClass.DeleteRows(new QueryFilter() { WhereClause = "ExportName='" + exportName + "'" });

            var lastCalculated = DateTime.Now;

            geodatabase.ApplyEdits(new Action(() => {

                using var rowBuffer = outputClass.CreateRowBuffer();
                using var insertCursor = outputClass.CreateInsertCursor();
                foreach (var flowDirection in _flowDirections)
                {
                    // Skip over any internal edges
                    if (!_edgeGeometry.ContainsKey(flowDirection.Key))
                        continue;

                    var edgeJson = _edgeGeometry[flowDirection.Key];
                    var lineGeometry = ConstructGeometry(edgeJson);

                    // Don't output zero-length lines
                    if (lineGeometry == null || lineGeometry.Length <= 0)
                        continue;

                    if (!_DirectionLookup.TryGetValue(flowDirection.Value, out var flowDirectionName))
                        flowDirectionName = "Unknown";

                    rowBuffer["CalculatedFlow"] = flowDirectionName;
                    rowBuffer["ExportName"] = exportName;
                    rowBuffer["TierName"] = _tierName;
                    rowBuffer["LastCalculated"] = lastCalculated;
                    rowBuffer[shapeFieldName] = lineGeometry;
                    
                    insertCursor.Insert(rowBuffer);
                }

                insertCursor.Flush();
            }));

            return outputClass;
        }

        private Polyline ConstructGeometry(JObject jsonGeometry)
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

        private FeatureClass CreateOutputClass(Geodatabase geodatabase, SpatialReference spatialReference)
        {
            var fieldDescriptions = new List<FieldDescription> {
                new("CalculatedFlow", FieldType.String),
                new("TierName", FieldType.String),
                new("ExportName", FieldType.String),
                new("LastCalculated", FieldType.Date),
                };

            // This must be explicitly set, otherwise Z-Values aren't allowed in the class
            var shapeDescription = new ShapeDescription(GeometryType.Polyline, spatialReference) { HasZ = true };

            var tableDescription = new FeatureClassDescription("FlowLines", fieldDescriptions, shapeDescription);

            var schemaBuilder = new SchemaBuilder(geodatabase);
            var tableToken = schemaBuilder.Create(tableDescription);
            if (!schemaBuilder.Build())
            {
                Console.WriteLine(string.Format("Error creating output feature class: {0}", "FlowLines"));
                foreach (var errorMessage in schemaBuilder.ErrorMessages)
                    Console.WriteLine(errorMessage);

                throw new Exception(string.Format("Unable to create output feature class: {0}", "FlowLines"));
            }

            return geodatabase.OpenDataset<FeatureClass>("FlowLines");
        }
    }
}
