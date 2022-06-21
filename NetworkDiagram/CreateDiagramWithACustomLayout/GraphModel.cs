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
using System;
using System.Collections.Generic;
using ArcGIS.Core.Data.NetworkDiagrams;


namespace CreateDiagramWithACustomLayout
{
  /// <summary>
  /// Custom Junction
  /// </summary>
  public class CustomJunction
  {
    /// <summary>
    /// DiagramJunctionElement
    /// </summary>
    public DiagramJunctionElement Element { get; private set; }

    /// <summary>
    /// Rank of the Element
    /// </summary>
    public int Rank { get; set; }

    /// <summary>
    /// DEID of the element
    /// </summary>
    public int ID { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="junction">DiagramJunctionElement</param>
    public CustomJunction(DiagramJunctionElement junction)
    {
      this.Element = junction;
      this.Rank = -1;
      this.ID = junction.ID;
    }
  }

  /// <summary>
  /// Custom Edge
  /// </summary>
  public class CustomEdge
  {
    /// <summary>
    /// DiagramEdgeElement
    /// </summary>
    public DiagramEdgeElement Element { get; private set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="edge">DiagramEdgeElement</param>
    public CustomEdge(DiagramEdgeElement edge)
    {
      this.Element = edge;
      this.ID = edge.ID;
    }


    /// <summary>
    /// DEID of the element
    /// </summary>
    public int ID { get; set; }
  }

  /// <summary>
  /// CustomContainer
  /// </summary>
  public class CustomContainer
  {
    /// <summary>
    /// DiagramContainerElement
    /// </summary>
    public DiagramContainerElement Element { get; private set; }

    /// <summary>
    /// DEID of the element
    /// </summary>
    public int ID { get; set; }

    /// <summary>
    /// AssetGroup
    /// </summary>
    public string AssetGroup { get; set; }

    /// <summary>
    /// AssetType
    /// </summary>
    public string AssetType { get; set; }

    /// <summary>
    /// X Position
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Y Position
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// Collection of unique From ID
    /// </summary>
    public HashSet<int> FromContainers { get; private set; }

    /// <summary>
    /// Collection of unique To ID
    /// </summary>
    public HashSet<int> ToContainers { get; private set; }

    /// <summary>
    /// Indicate if Has Fiber Input
    /// </summary>
    public bool HasInputFibers { get; set; }

    /// <summary>
    /// Indicate if Has Fiber Output
    /// </summary>
    public bool HasOutputFibers { get; set; }

    /// <summary>
    /// Min Rank From Junctiom
    /// </summary>
    public int FromJunctionRankMin { get; set; }

    /// <summary>
    /// Container Order
    /// </summary>
    public int FromContainerOrder { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="container">DiagramContainerElement</param>
    public CustomContainer(DiagramContainerElement container)
    {
      this.Element = container;
      var initialPos = container.Shape.Extent.Center;
      this.ID = container.ID;
      X = initialPos.X;
      Y = initialPos.Y;
      FromContainers = new HashSet<int>();
      ToContainers = new HashSet<int>();
      HasInputFibers = false;
      HasOutputFibers = false;
      FromJunctionRankMin = Int32.MaxValue;
      FromContainerOrder = Int32.MaxValue;
    }
  }

  /// <summary>
  /// Graph model
  /// </summary>
  public class GraphModel
  {
    private readonly Dictionary<int, CustomJunction> _junctions = new Dictionary<int, CustomJunction>();
    private readonly Dictionary<int, CustomEdge> _edges = new Dictionary<int, CustomEdge>();
    private readonly Dictionary<int, CustomContainer> _containers = new Dictionary<int, CustomContainer>();

    private readonly Dictionary<int, List<int>> _adjacentEdges = new Dictionary<int, List<int>>();
    private readonly Dictionary<int, List<int>> _containment = new Dictionary<int, List<int>>();

    /// <summary>
    /// Collection of Junction
    /// </summary>
    public IEnumerable<CustomJunction> Junctions
    {
      get
      {
        return _junctions.Values;
      }
    }

    /// <summary>
    /// Collection of Edges
    /// </summary>
    public IEnumerable<CustomEdge> Edges
    {
      get
      {
        return _edges.Values;
      }
    }

    /// <summary>
    /// Collection of Containers
    /// </summary>
    public IEnumerable<CustomContainer> Containers
    {
      get
      {
        return _containers.Values;
      }
    }

    /// <summary>
    /// Get a custom junction
    /// </summary>
    /// <param name="JunctionID">junctionID</param>
    /// <returns>CustomJunction</returns>
    public CustomJunction GetJunction(int JunctionID)
    {
      if (_junctions.TryGetValue(JunctionID, out CustomJunction junction))
        return junction;
      else
        return null;
    }

    /// <summary>
    /// Get a custom edge
    /// </summary>
    /// <param name="EdgeID">Edge ID</param>
    /// <returns>CustomEdge</returns>
    public CustomEdge GetEdge(int EdgeID)
    {
      if (_edges.TryGetValue(EdgeID, out CustomEdge edge))
        return edge;
      else
        return null;
    }

    /// <summary>
    /// Get a custom Container
    /// </summary>
    /// <param name="ContainerID">Container ID</param>
    /// <returns>CustomContainer</returns>
    public CustomContainer GetContainer(int ContainerID)
    {
      if (_containers.TryGetValue(ContainerID, out CustomContainer container))
        return container;
      else
        return null;
    }

    /// <summary>
    /// Get Contained Elements ID
    /// </summary>
    /// <param name="ContainerID">ContainerID</param>
    /// <returns>Collection of Contained Element ID</returns>
    public IEnumerable<int> GetContainedElements(int ContainerID)
    {
      if (!_containment.TryGetValue(ContainerID, out List<int> list))
      {
        list = new List<int>();
        _containment[ContainerID] = list;
      }
      return list;
    }

    /// <summary>
    /// Get Connected Junctions ID
    /// </summary>
    /// <param name="JunctionID">JunctionID</param>
    /// <param name="ConnectedJunctions">ConnectedJunctions</param>
    public void GetConnectedJunctions(int JunctionID, out IList<int> ConnectedJunctions)
    {
      ConnectedJunctions = new List<int>();

      if (!_adjacentEdges.TryGetValue(JunctionID, out List<int> edgeList))
        return;

      foreach (var edgeID in edgeList)
      {
        if (!_edges.TryGetValue(edgeID, out CustomEdge customEdge))
          continue;

        if (JunctionID != customEdge.Element.FromID)
          ConnectedJunctions.Add(customEdge.Element.FromID);
        else if (JunctionID != customEdge.Element.ToID)
          ConnectedJunctions.Add(customEdge.Element.ToID);
      }
    }

    /// <summary>
    /// Initialize class
    /// </summary>
    /// <param name="Diagram">NetworkDiagram</param>
    public void Initialize(NetworkDiagram Diagram)
    {
      _junctions.Clear();
      _edges.Clear();
      _containers.Clear();
      _adjacentEdges.Clear();
      _containment.Clear();

      DiagramElementQueryByElementTypes query = new DiagramElementQueryByElementTypes
      {
        QueryDiagramContainerElement = true,
        QueryDiagramEdgeElement = true,
        QueryDiagramJunctionElement = true
      };

      DiagramElementQueryResult deqr = Diagram.QueryDiagramElements(query);

      foreach (var container in deqr.DiagramContainerElements)
      {
        _containers[container.ID] = new CustomContainer(container);

        if (container.ContainerID > 0)
          AddContainment(container.ContainerID, container.ID);
      }

      foreach (var junction in deqr.DiagramJunctionElements)
      {
        _junctions[junction.ID] = new CustomJunction(junction);

        if (junction.ContainerID > 0)
          AddContainment(junction.ContainerID, junction.ID);
      }

      foreach (var edge in deqr.DiagramEdgeElements)
      {
        _edges[edge.ID] = new CustomEdge(edge);

        AddAdjacentEdge(edge.FromID, edge.ID);
        AddAdjacentEdge(edge.ToID, edge.ID);

        if (edge.ContainerID > 0)
          AddContainment(edge.ContainerID, edge.ID);
      }
    }

    /// <summary>
    /// Add edge to _adjacentEdges collection
    /// </summary>
    /// <param name="JunctionID">JunctionID</param>
    /// <param name="EdgeID">EdgeID</param>
    private void AddAdjacentEdge(int JunctionID, int EdgeID)
    {
      if (!_adjacentEdges.TryGetValue(JunctionID, out List<int> list))
      {
        list = new List<int>();
        _adjacentEdges.Add(JunctionID, list);
      }

      list.Add(EdgeID);
    }

    /// <summary>
    /// Add ContainedID to _containment collection
    /// </summary>
    /// <param name="ContainerID">ContainerID</param>
    /// <param name="ContainedID">ContainedID</param>
    private void AddContainment(int ContainerID, int ContainedID)
    {
      if (!_containment.TryGetValue(ContainerID, out List<int> list))
      {
        list = new List<int>();
        _containment.Add(ContainerID, list);
      }

      list.Add(ContainedID);
    }
  }
}
