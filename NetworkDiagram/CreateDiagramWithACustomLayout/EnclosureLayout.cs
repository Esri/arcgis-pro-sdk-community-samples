/*

   Copyright 2022 Esri

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
using ArcGIS.Core.Data.NetworkDiagrams;
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace CreateDiagramWithACustomLayout
{
  /// <summary>
  /// Telco Custom Layout
  /// </summary>
  internal class EnclosureLayout
  {
    private readonly GraphModel _graphModel = new GraphModel();

    /// <summary>
    /// Get color rank
    /// </summary>
    /// <param name="color">Color Name</param>
    /// <returns>Rank of the color</returns>
    internal static int GetColorRank(string color)
    {
      // Blue, Orange, Green, Brown, Slate, White, Red, Black, Yellow, Violet, Rose, Aqua

      if (color.StartsWith("Blue"))
        return 0;
      else if (color.StartsWith("Orange"))
        return 1;
      else if (color.StartsWith("Green"))
        return 2;
      else if (color.StartsWith("Brown"))
        return 3;
      else if (color.StartsWith("Slate"))
        return 4;
      else if (color.StartsWith("White"))
        return 5;
      else if (color.StartsWith("Red"))
        return 6;
      else if (color.StartsWith("Black"))
        return 7;
      else if (color.StartsWith("Yellow"))
        return 8;
      else if (color.StartsWith("Violet"))
        return 9;
      else if (color.StartsWith("Rose"))
        return 10;
      else if (color.StartsWith("Aqua"))
        return 11;

      return -1;
    }

    /// <summary>
    /// Apply the layout to the layer
    /// </summary>
    /// <param name="diagramLayer">Diagram Layer</param>
    public void Apply(DiagramLayer diagramLayer)
    {
      if (diagramLayer == null)
        return;
      QueuedTask.Run(() =>
      {
        try
        {
          NetworkDiagram Diagram = diagramLayer.GetNetworkDiagram();
          Execute(Diagram);

          MapView.Active.Redraw(true);
          MapView.Active.ZoomTo(Diagram.GetDiagramInfo().DiagramExtent.Expand(1.08, 1.08, true));
        }
        catch (Exception ex)
        {
          CreateDiagramWithACustomLayoutModule.ShowException(ex);
        }
      });
    }


    /// <summary>
    /// Fill the dictionary with Element Id, Properties
    /// </summary>
    /// <param name="Diagram">Network Diagram</param>
    /// <param name="theSources">List of network sources</param>
    /// <param name="SourceID">Source Id</param>
    /// <param name="IsEdge">Flag for edges treatement</param>
    /// <returns>Dictionary</returns>
    private static Dictionary<int, Dictionary<string, string>> FillSources(NetworkDiagram Diagram, IReadOnlyList<NetworkSource> theSources, int SourceID, bool IsEdge = false)
    {
      Dictionary<int, Dictionary<string, string>> AttributesByEID = new Dictionary<int, Dictionary<string, string>>();

      NetworkSource source = theSources.FirstOrDefault(a => a.ID == SourceID);
      if (source != null)
      {
        string[] attributeNames;
        if (IsEdge)
          attributeNames = new string[4] { "Asset group", "Asset type", "Strand Group Color", "Strand Color" };
        else
          attributeNames = new string[2] { "Asset group", "Asset type" };

        string content = Diagram.GetSourceAttributeValues(attributeNames: attributeNames, sourceName: source.Name, useCodedValueNames: true);
        JObject attributesList = JObject.Parse(content);
        JToken elements;

        if (IsEdge)
        {
          elements = attributesList["edges"];
          if (!(elements == null || !elements.Any()))
          {
            foreach (var element in elements)
            {
              int id = (int)element["id"];
              if (!AttributesByEID.TryGetValue(id, out Dictionary<string, string> IdProperties))
              {
                IdProperties = new Dictionary<string, string>();
                AttributesByEID.Add(id, IdProperties);
              }

              if (element["attributes"] is JObject attributes)
              {
                IdProperties.Add("Asset group", attributes["Asset group"].ToString());
                IdProperties.Add("Asset type", attributes["Asset type"].ToString());
                IdProperties.Add("Strand Group Color", attributes["Strand Group Color"].ToString());
                IdProperties.Add("Strand Color", attributes["Strand Color"].ToString());
              }
            }
          }
        }
        else
        {
          elements = attributesList["containers"];
          if (!(elements == null || !elements.Any()))
          {
            foreach (var element in elements)
            {
              int id = (int)element["id"];
              if (!AttributesByEID.TryGetValue(id, out Dictionary<string, string> IdProperties))
              {
                IdProperties = new Dictionary<string, string>();
                AttributesByEID.Add(id, IdProperties);
              }
              if (element["attributes"] is JObject attributes)
              {
                IdProperties.Add("Asset group", attributes["Asset group"].ToString());
                IdProperties.Add("Asset type", attributes["Asset type"].ToString());
              }
            }
          }

          elements = attributesList["junctions"];
          if (!(elements == null || !elements.Any()))
          {
            foreach (var element in elements)
            {
              int id = (int)element["id"];
              if (!AttributesByEID.TryGetValue(id, out Dictionary<string, string> IdProperties))
              {
                IdProperties = new Dictionary<string, string>();
                AttributesByEID.Add(id, IdProperties);
              }
              if (element["attributes"] is JObject attributes)
              {
                IdProperties.Add("Asset group", attributes["Asset group"].ToString());
                IdProperties.Add("Asset type", attributes["Asset type"].ToString());
              }
            }
          }
        }
      }

      return AttributesByEID;
    }

    /// <summary>
    /// Execute the layout on the Diagram
    /// </summary>
    /// <param name="diagram"></param>
    internal void Execute(NetworkDiagram diagram)
    {
      _graphModel.Initialize(diagram);

      var junctions = _graphModel.Junctions;
      if (!junctions.Any())
        return;

      int maxColorCount = 12;

      var containerIDs = new HashSet<int>();

      UtilityNetwork un = diagram.DiagramManager?.GetNetwork<UtilityNetwork>();

      IReadOnlyList<NetworkSource> theSources = un.GetDefinition().GetNetworkSources();

      if (!_graphModel.Containers.Any() || !_graphModel.Edges.Any())
        return;

      Dictionary<int, Dictionary<int, Dictionary<string, string>>> attributesBySourceID = new Dictionary<int, Dictionary<int, Dictionary<string, string>>>();
      foreach (var jsonContainer in _graphModel.Containers)
      {
        int sourceID = jsonContainer.Element.AssociatedSourceID;
        if (sourceID != 9)  // Communications Device source ID
          continue;

        if (!attributesBySourceID.TryGetValue(sourceID, out Dictionary<int, Dictionary<string, string>> AttributesByEID))
        {
          AttributesByEID = FillSources(Diagram: diagram, theSources: theSources, SourceID: sourceID);
          attributesBySourceID.Add(sourceID, AttributesByEID);
        }

        if (AttributesByEID.TryGetValue(jsonContainer.ID, out Dictionary<string, string> IdProperties))
        {
          jsonContainer.AssetGroup = IdProperties["Asset group"];
          jsonContainer.AssetType = IdProperties["Asset type"];
        }
      }

      foreach (var jsonEdge in _graphModel.Edges)
      {
        int sourceID = jsonEdge.Element.AssociatedSourceID;
        if (sourceID != 15)  // Communications Edge Object source ID
          continue;

        if (!attributesBySourceID.TryGetValue(sourceID, out Dictionary<int, Dictionary<string, string>> AttributesByEID))
        {
          AttributesByEID = FillSources(Diagram: diagram, theSources: theSources, SourceID: sourceID, IsEdge: true);
          attributesBySourceID.Add(sourceID, AttributesByEID);
        }

        if (AttributesByEID.TryGetValue(jsonEdge.ID, out Dictionary<string, string> IdProperties))
        {
          string assetGroup = IdProperties["Asset group"];
          string assetType = IdProperties["Asset type"];

          if (assetGroup != "Strand" || assetType != "Fiber")
            continue;

          string groupColor = IdProperties["Strand Group Color"].ToString();
          string color = IdProperties["Strand Color"].ToString();

          int groupColorRank = GetColorRank(groupColor);
          int colorRank = GetColorRank(color);

          if (groupColorRank < 0 || colorRank < 0)
            continue;

          int rank = maxColorCount * groupColorRank + colorRank;

          int fromID = jsonEdge.Element.FromID;
          int toID = jsonEdge.Element.ToID;

          int fromContainerID = 0;
          int toContainerID = 0;

          var fromJunction = _graphModel.GetJunction(fromID);
          fromJunction.Rank = rank;
          fromContainerID = fromJunction.Element.ContainerID;
          if (fromContainerID > 0)
            containerIDs.Add(fromContainerID);

          var toJunction = _graphModel.GetJunction(toID);
          toJunction.Rank = rank;
          toContainerID = toJunction.Element.ContainerID;
          if (toContainerID > 0)
            containerIDs.Add(toContainerID);

          if (fromContainerID > 0 && toContainerID > 0)
          {
            var fromContainer = _graphModel.GetContainer(fromContainerID);
            fromContainer.ToContainers.Add(toContainerID);
            fromContainer.HasOutputFibers = true;

            var toContainer = _graphModel.GetContainer(toContainerID);
            toContainer.FromContainers.Add(fromContainerID);
            toContainer.HasInputFibers = true;
            if (fromJunction.Rank < toContainer.FromJunctionRankMin)
              toContainer.FromJunctionRankMin = fromJunction.Rank;
          }
        }
      }

      attributesBySourceID = null;

      foreach (var junction in junctions)
      {
        if (junction.Element.ContainerID < 1)
          continue;

        if (junction.Rank >= 0)
          continue;

        _graphModel.GetConnectedJunctions(junction.Element.ID, out IList<int> connectedJunctions);

        int fromRank = -1;
        int toRank = -1;
        CustomContainer fromContainer = null;
        CustomContainer toContainer = null;

        int count = Math.Min(connectedJunctions.Count, 2);
        for (int i = 0; i < count; i++)
        {
          var connectedJunction = _graphModel.GetJunction(connectedJunctions[i]);
          var container = _graphModel.GetContainer(connectedJunction.Element.ContainerID);
          if (container.HasInputFibers)
          {
            fromRank = connectedJunction.Rank;
            fromContainer = container;
          }
          else if (container.HasOutputFibers)
          {
            toRank = connectedJunction.Rank;
            toContainer = container;
          }
        }

        if (fromRank >= 0 || toRank >= 0)
        {
          junction.Rank = fromRank >= 0 ? fromRank : toRank;

          var container = _graphModel.GetContainer(junction.Element.ContainerID);
          containerIDs.Add(container.Element.ID);

          if (fromContainer != null)
          {
            container.FromContainers.Add(fromContainer.Element.ID);
            fromContainer.ToContainers.Add(container.Element.ID);
          }

          if (toContainer != null)
          {
            container.ToContainers.Add(toContainer.Element.ID);
            toContainer.FromContainers.Add(container.Element.ID);
            if (junction.Rank < toContainer.FromJunctionRankMin)
              toContainer.FromJunctionRankMin = junction.Rank;
          }
        }
      }

      var diagramInfo = diagram.GetDiagramInfo();
      var diagramExtent = diagramInfo.DiagramExtent;
      double ySpacing = diagramInfo.ContainerMargin;
      double xSpacingMin = diagramInfo.ContainerMargin * 15;

      var startContainers = new List<CustomContainer>();

      foreach (var containerID in containerIDs)
      {
        var container = _graphModel.GetContainer(containerID);
        if (!(container is null) && (container.FromContainers.Count > 0 || container.ToContainers.Count < 1))
          continue;

        if (container.Element is null)
          continue;

        var parent = _graphModel.GetContainer(container.Element.ContainerID);
        if (parent == null)
          continue;

        if (parent.AssetType != "Hub Terminator" && parent.AssetType != "Mid Cable Splice Enclosure")
          continue;

        startContainers.Add(container);
      }

      double startY = diagramExtent.YMax;

      foreach (var startContainer in startContainers)
      {
        if (startContainer.FromContainerOrder != Int32.MaxValue)
          continue;

        var toContainerIDs = new HashSet<int>();
        foreach (var toContainerID in startContainer.ToContainers)
          toContainerIDs.Add(toContainerID);

        double startX = diagramExtent.XMin;

        startContainer.X = startX;
        startContainer.Y = startY;

        while (toContainerIDs.Count > 0)
        {
          double y = startY;
          bool first = true;

          var toContainers = new List<CustomContainer>();
          foreach (var containerID in toContainerIDs)
          {
            var toContainer = _graphModel.GetContainer(containerID);
            if (startContainer.FromContainerOrder == Int32.MaxValue)
              toContainers.Add(toContainer);
          }

          var sortedContainers = toContainers.OrderBy(cntr => cntr.FromContainerOrder + 0.001 * cntr.FromJunctionRankMin);

          int vertivalOrder = 0;
          foreach (var container in sortedContainers)
          {
            int containerID = container.Element.ID;

            container.Y = y;

            y -= (_graphModel.GetContainedElements(containerID).Count() * ySpacing + 7 * diagramInfo.ContainerMargin);

            if (first)
            {
              first = false;
              container.X = startX + xSpacingMin;
              startX = container.X;
            }
            else
              container.X = startX;

            foreach (var toContainerID in container.ToContainers)
            {
              var toContainer = _graphModel.GetContainer(toContainerID);
              if (toContainer.FromContainerOrder == Int32.MaxValue)
              {
                toContainer.FromContainerOrder = vertivalOrder;
                toContainerIDs.Add(toContainerID);
              }
            }

            vertivalOrder++;

            toContainerIDs.Remove(containerID);
          }
        }

        startY -= (_graphModel.GetContainedElements(startContainer.Element.ID).Count() * ySpacing + 4 * diagramInfo.ContainerMargin);
      }

      IList<DiagramJunctionElement> junctionsToSave = new List<DiagramJunctionElement>();

      SpatialReference spatialRef = diagramInfo.DiagramExtent.SpatialReference;
      MapPointBuilder mapPointBuilder = new MapPointBuilder(spatialRef)
      {
        Z = 0
      };

      foreach (var containerID in containerIDs)
      {
        var container = _graphModel.GetContainer(containerID);
        if (container == null)
          continue;

        int rankCount = maxColorCount * maxColorCount;

        BitArray isEmpty = new BitArray(rankCount, true);

        double yTop = container.Y;

        var containedJunctions = _graphModel.GetContainedElements(containerID);
        var unconnectedJunctions = new Stack<int>();

        foreach (var junctionID in containedJunctions)
        {
          var junction = _graphModel.GetJunction(junctionID);
          if (junction == null)
            continue;

          if (junction.Rank < 0)
          {
            unconnectedJunctions.Push(junction.Element.ID);
            continue;
          }

          isEmpty[junction.Rank] = false;

          mapPointBuilder.X = container.X;
          mapPointBuilder.Y = yTop - junction.Rank * ySpacing;
          junction.Element.Shape = mapPointBuilder.ToGeometry();

          junctionsToSave.Add(junction.Element);
        }

        int rank = 0;
        while (unconnectedJunctions.Count > 0 && rank < rankCount)
        {
          if (isEmpty[rank])
          {
            var junction = _graphModel.GetJunction(unconnectedJunctions.Pop());
            if (junction != null)
            {
              mapPointBuilder.X = container.X;
              mapPointBuilder.Y = yTop - rank * ySpacing;
              junction.Element.Shape = mapPointBuilder.ToGeometry();
              junctionsToSave.Add(junction.Element);
            }
          }
          rank++;
        }
      }

      if (junctionsToSave.Count > 0)
      {
        NetworkDiagramSubset nds = new NetworkDiagramSubset
        {
          DiagramJunctionElements = junctionsToSave
        };

        diagram.SaveLayout(nds, true);
      }
    }
  }
}
