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
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using static CustomizeNetworkDiagramLayoutExecution.CustomizeNetworkDiagramLayoutExecutionModule;

namespace CustomizeNetworkDiagramLayoutExecution
{
  class CustomDispatchLayout
  {
    private double _dispatchFactor = 1;
    private int _NumberOfIterations = 20;
    private int _controlDistance = 1;
    private const double _epsilon = 0.000000000001;

    private readonly GraphModel _graphModel = new GraphModel();

    public double DispatchFactor { get => _dispatchFactor; set => _dispatchFactor = value; }

    public int NumberOfIterations { get => _NumberOfIterations; set => _NumberOfIterations = value; }

    public int ControlDistance { get => _controlDistance; set => _controlDistance = value; }

    /// <summary>
    /// Apply the layout to the Diagram Layer
    /// </summary>
    /// <param name="diagramLayer"></param>
    public void Apply(DiagramLayer diagramLayer)
    {
      if (diagramLayer == null) return;
      QueuedTask.Run(() =>
      {
        try
        {
          NetworkDiagram Diagram = diagramLayer.GetNetworkDiagram();

          LoadDiagramFeatures(Diagram, out SelectionSet selection);

          // Execute the algorithm
          Execute(Diagram);

          // Save the diagram
          SaveDiagram(Diagram);
          if (selection != null && selection.Count > 0)
            MapView.Active.Map.SetSelection(selection);

          if (FrameworkApplication.CurrentTool.Contains("_networkdiagrams_"))
            FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");
        }
        catch (Exception ex)
        {
          ShowException(ex);
        }

        CleanModule();
      });
    }

    /// <summary>
    /// Restore default parameters
    /// </summary>
    internal void RestoreDefault()
    {
      DispatchFactor = _dispatchFactor;
      NumberOfIterations = _NumberOfIterations;
      ControlDistance = _controlDistance;
    }

    /// <summary>
    /// Execute the layout on the diagram
    /// </summary>
    /// <param name="diagram">NetworkDiagram for executing layout</param>
    internal void Execute(NetworkDiagram diagram)
    {
      g_JunctionsToSave = new List<DiagramJunctionElement>();
      g_EdgesToSave = new List<DiagramEdgeElement>();

      _graphModel.Initialize();

      Envelope diagramExtent = diagram.GetDiagramInfo().DiagramExtent as Envelope;

      var junctions = _graphModel.Junctions;
      var edges = _graphModel.Edges;

      long nbJunctions = junctions.Count();
      if (nbJunctions < 2)
        return;

      // Compute scale factors
      double side = 10;
      double xFactor = diagramExtent.Width / side;
      double yFactor = diagramExtent.Height / side;

      if (xFactor == 0)
        xFactor = 1;

      if (yFactor == 0)
        yFactor = 1;

      // Initialize parameters
      //
      double startT = _controlDistance;
      double endT = 0.4;

      double stepT = (endT - startT) / _NumberOfIterations;
      double area = side * side; // area of a cell
      double kV = _dispatchFactor * Math.Sqrt(area / nbJunctions);
      double kV2 = kV * kV;
      double temperature = startT;

      // Initialize junctions coordinates and displacement
      foreach (var junction in junctions)
      {
        MapPoint point = junction.Info.Shape as MapPoint;

        double x = (point.X - diagramExtent.XMin) / xFactor;
        double y = (point.Y - diagramExtent.YMin) / yFactor;

        junction.Attributes = new JunctionAttributes(x, y);
      }

      // Execute algorithm
      double ijDistance, depDistance;

      Coordinate2D position = new Coordinate2D();
      Coordinate2D ijVector = new Coordinate2D();
      Coordinate2D depVector = new Coordinate2D();

      // Iterate until number of iteration is reached
      for (int i = 0; i < _NumberOfIterations; i++)
      {
        // Compute repulsive forces
        foreach (var junction1 in junctions)
        {
          JunctionAttributes attributes1 = junction1.Attributes as JunctionAttributes;

          // initialize displacement to 0.0
          attributes1.dX = 0;
          attributes1.dY = 0;

          position.X = attributes1.X;
          position.Y = attributes1.Y;

          foreach (var junction2 in junctions)
          {
            if (junction2.Info.ID == junction1.Info.ID)
              continue;

            JunctionAttributes attributes2 = junction2.Attributes as JunctionAttributes;

            ijVector = position;
            ijVector.X -= attributes2.X;
            ijVector.Y -= attributes2.Y;

            // squared distance
            ijDistance = ijVector.X * ijVector.X + ijVector.Y * ijVector.Y;

            if (ijDistance > _epsilon)
            {
              ijVector.X *= kV2 / ijDistance;
              ijVector.Y *= kV2 / ijDistance;
            }

            // incremental displacement
            attributes1.dX += ijVector.X;
            attributes1.dY += ijVector.Y;
          }
        }

        // Compute attractive forces
        CustomJunction fromJunction;
        CustomJunction toJunction;

        foreach (var edge in edges)
        {
          // Get connected junctions
          fromJunction = _graphModel.GetJunction(edge.Info.FromID);
          toJunction = _graphModel.GetJunction(edge.Info.ToID);

          if (fromJunction == null || toJunction == null)
            continue;

          JunctionAttributes fromAttributes = fromJunction.Attributes as JunctionAttributes;
          JunctionAttributes toAttributes = toJunction.Attributes as JunctionAttributes;

          ijVector.X = toAttributes.X - fromAttributes.X;
          ijVector.Y = toAttributes.Y - fromAttributes.Y;

          ijDistance = Math.Sqrt(ijVector.X * ijVector.X + ijVector.Y * ijVector.Y);

          double factor = ijDistance / kV;
          if (ijDistance > 0.000001)
          {
            ijVector.X *= factor;
            ijVector.Y *= factor;
          }
          else
          {
            ijVector.X *= factor * factor;
            ijVector.Y *= factor * factor;
          }

          fromAttributes.dX += ijVector.X;
          fromAttributes.dY += ijVector.Y;

          toAttributes.dX -= ijVector.X;
          toAttributes.dY -= ijVector.Y;
        }

        // Limit the maximum displacement to the temperature and then
        // prevent from being displaced outside frame
        // Move the junctions
        foreach (var junction in junctions)
        {
          JunctionAttributes attributes = junction.Attributes as JunctionAttributes;

          depVector.X = attributes.dX;
          depVector.Y = attributes.dY;

          depDistance = Math.Sqrt(depVector.X * depVector.X + depVector.Y * depVector.Y);

          if (depDistance > temperature)
          {
            depVector.X *= temperature / depDistance;
            depVector.Y *= temperature / depDistance;
          }

          // Move the junction
          attributes.X += depVector.X;
          attributes.Y += depVector.Y;
        }

        // Reduce the temperature as the layout approaches a better configuration
        temperature += stepT;
      }

      // Getting the new bounding box
      Coordinate2D minCoord = new Coordinate2D(double.MaxValue, double.MaxValue);
      Coordinate2D maxCoord = new Coordinate2D(double.MinValue, double.MinValue);

      foreach (var junction in junctions)
      {
        JunctionAttributes attributes = junction.Attributes as JunctionAttributes;

        // compute new envelope of selected junctions
        minCoord.X = Math.Min(minCoord.X, attributes.X);
        minCoord.Y = Math.Min(minCoord.Y, attributes.Y);
        maxCoord.X = Math.Max(maxCoord.X, attributes.X);
        maxCoord.Y = Math.Max(maxCoord.Y, attributes.Y);
      }

      // Special cases (horizontal or vertical alignment)
      bool junctionsXMvt = true;
      bool junctionsYMvt = true;

      double avrX = 0.5 * (diagramExtent.XMin + diagramExtent.XMax);
      double avrY = 0.5 * (diagramExtent.YMin + diagramExtent.YMax);

      if (Math.Abs(maxCoord.X - minCoord.X) < _epsilon)
        junctionsXMvt = false;

      if (Math.Abs(maxCoord.Y - minCoord.Y) < _epsilon)
        junctionsYMvt = false;

      double ratioX = 1;
      double ratioY = 1;

      if (junctionsXMvt)
        ratioX = (diagramExtent.XMax - diagramExtent.XMin) / (maxCoord.X - minCoord.X);

      if (junctionsYMvt)
        ratioY = (diagramExtent.YMax - diagramExtent.YMin) / (maxCoord.Y - minCoord.Y);

      // Compute final location of the junctions and the final extent

      Coordinate2D finalMinPoint = new Coordinate2D(double.MaxValue, double.MaxValue);
      Coordinate2D finalMaxPoint = new Coordinate2D(double.MinValue, double.MinValue);

      foreach (var junction in junctions)
      {
        JunctionAttributes attributes = junction.Attributes as JunctionAttributes;

        if (junctionsXMvt)
          attributes.X = (attributes.X - minCoord.X) * ratioX + diagramExtent.XMin;
        else
          attributes.X = avrX;

        if (junctionsYMvt)
          attributes.Y = (attributes.Y - minCoord.Y) * ratioY + diagramExtent.YMin;
        else
          attributes.Y = avrY;

        finalMinPoint.X = Math.Min(finalMinPoint.X, attributes.X);
        finalMinPoint.Y = Math.Min(finalMinPoint.Y, attributes.Y);
        finalMaxPoint.X = Math.Max(finalMaxPoint.X, attributes.X);
        finalMaxPoint.Y = Math.Max(finalMaxPoint.Y, attributes.Y);
      }

      // Update the diagram elements geometry

      // must be called on the MCT. use QueuedTask.Run
      SpatialReference spatialRef = diagram.GetDiagramInfo().DiagramExtent.SpatialReference;
      MapPointBuilder mapPointBuilder = new MapPointBuilder(spatialRef);

      foreach (var junction in junctions)
      {
        JunctionAttributes attributes = junction.Attributes as JunctionAttributes;
        MapPoint p = junction.Info.Shape.Clone() as MapPoint;
        mapPointBuilder.X = attributes.X;
        mapPointBuilder.Y = attributes.Y;
        mapPointBuilder.Z = 0.0;

        junction.Info.Shape = mapPointBuilder.ToGeometry();

        if (p.X != attributes.X || p.Y != attributes.Y)
          g_JunctionsToSave.Add(junction.Info);
      }

      foreach (var edge in edges)
      {
        JunctionAttributes attributes;
        List<MapPoint> listPoints = new List<MapPoint>();

        CustomJunction from = _graphModel.GetJunction(edge.Info.FromID);
        CustomJunction to = _graphModel.GetJunction(edge.Info.ToID);

        if (from == null || to == null)
          continue;

        attributes = from.Attributes as JunctionAttributes;
        listPoints.Add(MapPointBuilder.CreateMapPoint(attributes.X, attributes.Y, 0.0, spatialRef));

        attributes = to.Attributes as JunctionAttributes;
        listPoints.Add(MapPointBuilder.CreateMapPoint(attributes.X, attributes.Y, 0.0, spatialRef));

        Polyline polyline = PolylineBuilder.CreatePolyline(listPoints, spatialRef);
        edge.Info.Shape = polyline;

        g_EdgesToSave.Add(edge.Info);
      }
    }
  }

  /// <summary>
  /// Graph model for the custom layout
  /// </summary>
  internal class GraphModel
  {
    private readonly Dictionary<int, CustomJunction> _junctions = new Dictionary<int, CustomJunction>();
    private readonly Dictionary<int, CustomEdge> _edges = new Dictionary<int, CustomEdge>();
    private readonly Dictionary<int, CustomContainer> _containers = new Dictionary<int, CustomContainer>();

    private readonly Dictionary<int, List<int>> _adjacentEdges = new Dictionary<int, List<int>>();
    private readonly Dictionary<int, List<int>> _containment = new Dictionary<int, List<int>>();

    /// <summary>
    /// List of CustomJunction
    /// </summary>
    internal IEnumerable<CustomJunction> Junctions
    {
      get
      {
        return _junctions.Values;
      }
    }

    /// <summary>
    /// List of CustomEdge
    /// </summary>
    internal IEnumerable<CustomEdge> Edges
    {
      get
      {
        return _edges.Values;
      }
    }

    /// <summary>
    /// List of CustomContainer
    /// </summary>
    internal IEnumerable<CustomContainer> Containers
    {
      get
      {
        return _containers.Values;
      }
    }

    /// <summary>
    /// Get a junction from the CustomJunction list
    /// </summary>
    /// <param name="junctionID">Junction ID</param>
    /// <returns>CustomJunction</returns>
    internal CustomJunction GetJunction(int junctionID)
    {
      if (_junctions.TryGetValue(junctionID, out CustomJunction junction))
        return junction;
      else
        return null;
    }

    /// <summary>
    /// Inititalisze the graph model
    /// </summary>
    internal void Initialize()
    {
      _junctions.Clear();
      _edges.Clear();
      _containers.Clear();
      _adjacentEdges.Clear();
      _containment.Clear();

      foreach (var container in g_DiagramContainerElements)
      {
        _containers[container.ID] = new CustomContainer(container);

        if (container.ContainerID > 0)
          AddContainment(container.ContainerID, container.ID);
      }

      foreach (var junction in g_DiagramJunctionElements)
      {
        _junctions[junction.ID] = new CustomJunction(junction);

        if (junction.ContainerID > 0)
          AddContainment(junction.ContainerID, junction.ID);
      }

      foreach (var edge in g_DiagramEdgeElements)
      {
        _edges[edge.ID] = new CustomEdge(edge);

        AddAdjacentEdge(edge.FromID, edge.ID);
        AddAdjacentEdge(edge.ToID, edge.ID);

        if (edge.ContainerID > 0)
          AddContainment(edge.ContainerID, edge.ID);
      }
    }

    /// <summary>
    /// Add an edge ID to the adjacent list of a junction
    /// </summary>
    /// <param name="junctionID">junction ID</param>
    /// <param name="edgeID">edge ID</param>
    private void AddAdjacentEdge(int junctionID, int edgeID)
    {
      if (!_adjacentEdges.TryGetValue(junctionID, out List<int> list))
      {
        list = new List<int>();
        _adjacentEdges.Add(junctionID, list);
      }

      list.Add(edgeID);
    }

    /// <summary>
    /// Add a contained ID to the containment list of a container
    /// </summary>
    /// <param name="containerID">Container ID</param>
    /// <param name="containedID">Contained ID</param>
    private void AddContainment(int containerID, int containedID)
    {
      if (!_containment.TryGetValue(containerID, out List<int> list))
      {
        list = new List<int>();
        _containment.Add(containerID, list);
      }

      list.Add(containedID);
    }
  }

  /// <summary>
  /// Custom attributes abstrat class
  /// </summary>
  internal abstract class CustomAttributes
  {
  }

  /// <summary>
  /// Junction Attributes
  /// </summary>
  internal class JunctionAttributes : CustomAttributes
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="x">X position of the junction</param>
    /// <param name="y">Y position of the junction</param>
    internal JunctionAttributes(double x, double y)
    {
      this.X = x;
      this.Y = y;
      this.dX = 0;
      this.dY = 0;
    }

    /// <summary>
    /// Get X position of the junction
    /// </summary>
    internal double X { get; set; }

    /// <summary>
    /// Get Y position of the junction
    /// </summary>
    internal double Y { get; set; }

    /// <summary>
    /// Get the shift X position of the junction
    /// </summary>
    internal double dX { get; set; }

    /// <summary>
    /// Get the shift Y position of the junction
    /// </summary>
    internal double dY { get; set; }
  }

  /// <summary>
  /// Custion junction class used in GraphModel
  /// </summary>
  internal class CustomJunction
  {
    /// <summary>
    /// Diagram Junction Element
    /// </summary>
    internal DiagramJunctionElement Info { get; private set; }

    /// <summary>
    /// Custom attribute of a junction
    /// </summary>
    internal CustomAttributes Attributes { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="junction">Diagram Junction Element</param>
    /// <param name="attributes">Custom Attributes</param>
    internal CustomJunction(DiagramJunctionElement junction, CustomAttributes attributes = null)
    {
      this.Info = junction;
      this.Attributes = attributes;
    }
  }

  /// <summary>
  /// Custion edge class used in GraphModel
  /// </summary>
  internal class CustomEdge
  {
    /// <summary>
    /// Diagram Edge Element
    /// </summary>
    internal DiagramEdgeElement Info { get; private set; }

    /// <summary>
    /// Custom attribute of an edge
    /// </summary>
    internal CustomAttributes Attributes { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="edge">Diagram Edge Element</param>
    /// <param name="attributes">Custom Attributes</param>
    internal CustomEdge(DiagramEdgeElement edge, CustomAttributes attributes = null)
    {
      this.Info = edge;
      this.Attributes = attributes;
    }
  }

  /// <summary>
  /// Custom container class used in GraphModel
  /// </summary>
  internal class CustomContainer
  {
    /// <summary>
    /// Diagram Edge Element
    /// </summary>
    internal DiagramContainerElement Info { get; private set; }
    internal CustomAttributes Attributes { get; set; }

    internal CustomContainer(DiagramContainerElement container, CustomAttributes attributes = null)
    {
      this.Info = container;
      this.Attributes = attributes;
    }
  }

}
