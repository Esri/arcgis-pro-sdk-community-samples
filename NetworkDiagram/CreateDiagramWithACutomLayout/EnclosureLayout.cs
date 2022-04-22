using ArcGIS.Core.Data.UtilityNetwork.NetworkDiagrams;
using ArcGIS.Core.Geometry;
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
    internal int GetColorRank(string color)
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
    /// Execute the layout on the Diagram
    /// </summary>
    /// <param name="diagram"></param>
    internal void Execute(NetworkDiagram diagram)
    {
      _graphModel.Initialize(diagram);

      var junctions = _graphModel.Junctions;
      if (junctions.Count() < 1)
        return;

      int maxColorCount = 12;

      var containerIDs = new HashSet<int>();

      string json = diagram.GetContent(addDiagramInfo: false, addGeometries: false, addAttributes: true, addAggregations: false, useCodedValueNames: true);

      JObject jsonDoc = JObject.Parse(json);
      if (jsonDoc == null)
        return;

      JToken jsonEdges = jsonDoc["edges"];
      JToken jsonContainers = jsonDoc["containers"];
      if (jsonEdges == null || jsonContainers == null)
        return;

      foreach (var jsonContainer in jsonContainers)
      {
        int sourceID = (int)jsonContainer["assocSourceID"];
        if (sourceID != 9)  // Communications Device source ID
          continue;

        JToken jsonAttributes = jsonContainer["attributes"];
        if (jsonAttributes == null)
          continue;

        var container = _graphModel.GetContainer((int)jsonContainer["id"]);
        container.AssetGroup = jsonAttributes["Asset group"].ToString();
        container.AssetType = jsonAttributes["Asset type"].ToString();
      }

      foreach (var jsonEdge in jsonEdges)
      {
        int sourceID = (int)jsonEdge["assocSourceID"];
        if (sourceID != 15)  // Communications Edge Object source ID
          continue;

        JToken jsonAttributes = jsonEdge["attributes"];
        if (jsonAttributes == null)
          continue;

        string assetGroup = jsonAttributes["Asset group"].ToString();
        string assetType = jsonAttributes["Asset type"].ToString();
        if (assetGroup != "Strand" || assetType != "Fiber")
          continue;

        string groupColor = jsonAttributes["Strand Group Color"].ToString();
        string color = jsonAttributes["Strand Color"].ToString();

        int groupColorRank = GetColorRank(groupColor);
        int colorRank = GetColorRank(color);

        if (groupColorRank < 0 || colorRank < 0)
          continue;

        int rank = maxColorCount * groupColorRank + colorRank;

        int fromID = (int)jsonEdge["fromID"];
        int toID = (int)jsonEdge["toID"];

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

        while (toContainerIDs.Count() > 0)
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

      if (junctionsToSave.Count() > 0)
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
