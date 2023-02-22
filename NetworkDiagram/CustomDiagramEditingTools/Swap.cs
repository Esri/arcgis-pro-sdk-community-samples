/*

   Copyright 2023 Esri

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
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using static DiagramEditing.DiagramEditingModule;

namespace DiagramEditing
{
  internal class Swap : Button
  {
    protected override void OnClick()
    {
      QueuedTask.Run(() =>
      {
        try
        {
          string error = GetElements(MapView.Active?.Map);
          if (!string.IsNullOrEmpty(error))
          {
            MessageBox.Show(error);
            return;
          }

          if (SelectionCount != 2)
          {
            MessageBox.Show("This tool works on only two selected features");
            return;
          }

          InitialiseEdit();

          // Swapping of edges
          DiagramEdgeElement edge1 = null;
          DiagramEdgeElement edge2 = null;
          if (GlobalSelectedEdgeIDs.Count > 0) edge1 = GetEdgeByObjectID(ObjectID: GlobalSelectedEdgeIDs[0]);
          if (GlobalSelectedEdgeIDs.Count > 1) edge2 = GetEdgeByObjectID(ObjectID: GlobalSelectedEdgeIDs[1]);

          if (!SwapEdges(edge1, edge2))
          {
            // When swapping of edges doesn't work since at least one edge is null

            DiagramJunctionElement jct1 = null;
            DiagramJunctionElement jct2 = null;
            if (GlobalSelectedJunctionIDs.Count > 0) jct1 = GetJunctionByObjectID(GlobalSelectedJunctionIDs[0]);
            if (GlobalSelectedJunctionIDs.Count > 1) jct2 = GetJunctionByObjectID(GlobalSelectedJunctionIDs[1]);

            // Swapping of junctions
            if (!SwapJunctions(jct1, jct2))
            {
              // When swapping of junctions doesn't work since at least one junction is null

              DiagramContainerElement cnt1 = null;
              DiagramContainerElement cnt2 = null;
              if (GlobalSelectedContainerIDs.Count > 0) cnt1 = GetContainerByObjectID(GlobalSelectedContainerIDs[0]);
              if (GlobalSelectedContainerIDs.Count > 1) cnt2 = GetContainerByObjectID(GlobalSelectedContainerIDs[1]);

              // Swapping of containers
              if (!SwapContainers(cnt1, cnt2))
              {
                // When swapping of containers doesn't work since at least one container is null

                // Swapping between an edge and a junction
                if (!SwapJunctionWithEdge(jct1, edge1))
                {
                  // Swapping between a container and a junction
                  if (!SwapContainerWithJunction(cnt1, jct1))
                  {
                    // Swapping between a container and an edge
                    SwapContainerWithEdge(cnt1, edge1);
                  }
                }
              }
            }
          }

          SaveDiagram();
        }

        catch (Exception ex)
        {
          ShowException(exception: ex);
        }
      });
    }

    internal static bool SwapEdges(DiagramEdgeElement edge1, DiagramEdgeElement edge2)
    {
      if (edge1 == null || edge2 == null)
        return false;

      Envelope env = edge1.Shape.Extent;

      double dXCenter1 = (env.XMax - env.XMin) / 2 + env.XMin;
      double dYCenter1 = (env.YMax - env.YMin) / 2 + env.YMin;

      env = edge2.Shape.Extent;
      double dXCenter2 = (env.XMax - env.XMin) / 2 + env.XMin;
      double dYCenter2 = (env.YMax - env.YMin) / 2 + env.YMin;

      double dX = dXCenter1 - dXCenter2;
      double dY = dYCenter1 - dYCenter2;

      if (KeepVertices)
      {
        MoveEdge(edge1, -dX, -dY);
        MoveEdge(edge2, dX, dY);
      }
      else
      {
        DiagramJunctionElement from1 = GlobalDiagramJunctionElements.Single(a => a.ID == edge1.FromID);
        DiagramJunctionElement to1 = GlobalDiagramJunctionElements.Single(a => a.ID == edge1.ToID);
        DiagramJunctionElement from2 = GlobalDiagramJunctionElements.Single(a => a.ID == edge2.FromID);
        DiagramJunctionElement to2 = GlobalDiagramJunctionElements.Single(a => a.ID == edge2.ToID);
        if (to1.ID == to2.ID)
        {
          SwapJunctions(from1, from2);
        }
        else if (from1.ID == from2.ID)
        {
          SwapJunctions(to1, to2);
        }
        else if (from1.ID == to2.ID)
        {
          SwapJunctions(from2, to1);
        }
        else if (from2.ID == to1.ID)
        {
          SwapJunctions(from1, to2);
        }
        else
        {
          SwapJunctions(from1, from2);
          SwapJunctions(to1, to2);
        }
      }
      return true;
    }

    private static bool SwapJunctions(DiagramJunctionElement junction1, DiagramJunctionElement junction2)
    {
      if (junction1 == null || junction2 == null) return false;

      MapPoint pt1 = junction1.Shape.Clone() as MapPoint;
      MapPoint pt2 = junction2.Shape.Clone() as MapPoint;

      junction1.Shape = pt2;
      junction2.Shape = pt1;

      GlobalJunctionsToSave.Add(junction1);
      GlobalJunctionsToSave.Add(junction2);

      return true;
    }

    internal static bool SwapContainers(DiagramContainerElement container1, DiagramContainerElement container2)
    {
      if (container1 == null || container2 == null) return false;

      if (container2.ID == container1.ContainerID) return true;
      if (IsContaintInParent(container1, container2.ContainerID)) return true;
      if (IsContaintInParent(container2, container1.ContainerID)) return true;

      GetContainerCenter(container1, out double posXContainerRef1, out double posYContainerRef1, out double WidthRef1, out double HeightRef1);

      GetContainerCenter(container2, out double posXContainerRef2, out double posYContainerRef2, out double WidthRef2, out double HeightRef2);

      double dX = posXContainerRef2 - posXContainerRef1;
      double dY = posYContainerRef2 - posYContainerRef1;

      // move container1
      IEnumerable<DiagramContainerElement> lContainers = GlobalDiagramContainerElements.Where(a => a.ContainerID == container1.ID);
      foreach (var v in lContainers)
        MoveContainer(v, dX, dY);

      // move edges in container1
      IEnumerable<DiagramEdgeElement> lEdges = GlobalDiagramEdgeElements.Where(a => a.ContainerID == container1.ID);
      foreach (var v in lEdges)
        MoveEdge(v, dX, dY);

      // Move junctions in container1
      IEnumerable<DiagramJunctionElement> lJunctions = GlobalDiagramJunctionElements.Where(a => a.ContainerID == container1.ID);
      foreach (var v in lJunctions)
        MoveJunction(v, dX, dY);

      // Move container2
      lContainers = GlobalDiagramContainerElements.Where(a => a.ContainerID == container2.ID);
      foreach (var v in lContainers)
        MoveContainer(v, -dX, -dY);

      // move edges in container2
      lEdges = GlobalDiagramEdgeElements.Where(a => a.ContainerID == container2.ID);
      foreach (var v in lEdges)
        MoveEdge(v, -dX, -dY);

      // Move junctions in container2
      lJunctions = GlobalDiagramJunctionElements.Where(a => a.ContainerID == container2.ID);
      foreach (var v in lJunctions)
        MoveJunction(v, -dX, -dY);

      return true;
    }

    internal static bool SwapJunctionWithEdge(DiagramJunctionElement junction, DiagramEdgeElement edge)
    {
      if (edge == null || junction == null) return false;

      DiagramJunctionElement from1 = GlobalDiagramJunctionElements.Single(a => a.ID == edge.FromID);
      DiagramJunctionElement to1 = GlobalDiagramJunctionElements.Single(a => a.ID == edge.ToID);

      if (from1 == null || to1 == null) return false;
      if (from1 == junction || to1 == junction) return true; // no swap when the junction is an edge extremity.

      Envelope env = edge.Shape.Extent;

      double XCenter = (env.XMax - env.XMin) / 2 + env.XMin;
      double YCenter = (env.YMax - env.YMin) / 2 + env.YMin;

      MapPoint junctionPoint = junction.Shape as MapPoint;

      double dX = junctionPoint.X - XCenter;
      double dY = junctionPoint.Y - YCenter;

      if (KeepVertices)
        MoveEdge(edge, dX, dY);
      else
      {
        MoveJunction(from1, dX, dY);
        MoveJunction(to1, dX, dY);
      }

      MoveJunction(junction, -dX, -dY);

      return true;
    }

    internal static bool SwapContainerWithJunction(DiagramContainerElement container, DiagramJunctionElement junction)
    {
      if (container == null || junction == null) return false;

      if (IsContaintInParent(container, junction.ContainerID)) return true;

      MapPoint p2 = junction.Shape.Clone() as MapPoint;
      GetContainerCenter(container, out double posXContainer, out double posYContainer, out double WidthRef1, out double HeightRef1);

      double dX = posXContainer - p2.X;
      double dY = posYContainer - p2.Y;

      MoveJunction(junction, dX, dY);

      IEnumerable<DiagramContainerElement> lContainers = GlobalDiagramContainerElements.Where(a => a.ContainerID == container.ID);
      // move container in container
      foreach (var v in lContainers)
        MoveContainer(v, -dX, -dY);

      // move edges in container
      IEnumerable<DiagramEdgeElement> lEdges = GlobalDiagramEdgeElements.Where(a => a.ContainerID == container.ID);
      foreach (var v in lEdges)
        MoveEdge(v, -dX, -dY);

      IEnumerable<DiagramJunctionElement> lJunctions = GlobalDiagramJunctionElements.Where(a => a.ContainerID == container.ID);
      // Move junctions in container
      foreach (var v in lJunctions)
        MoveJunction(v, -dX, -dY);

      return true;
    }

    internal static bool SwapContainerWithEdge(DiagramContainerElement container, DiagramEdgeElement edge)
    {
      if (edge == null || container == null) return false;

      if (IsContaintInParent(container, edge.ContainerID)) return true;
      if (IsEdgeStartInContainer(edge, container.ID)) return true;

      Envelope env = edge.Shape.Extent;

      double edgeX = (env.XMax - env.XMin) / 2 + env.XMin;
      double edgeY = (env.YMax - env.YMin) / 2 + env.YMin;

      GetContainerCenter(container, out double XCenter, out double YCenter, out double Width, out double Height);
      double dX = XCenter - edgeX;
      double dY = YCenter - edgeY;

      MoveContainer(container, -dX, -dY);

      if (KeepVertices)
        MoveEdge(edge, dX, dY);
      else
      {
        DiagramJunctionElement junction = GlobalDiagramJunctionElements.SingleOrDefault(a => a.ID == edge.FromID);
        MoveJunction(junction, dX, dY);
        junction = GlobalDiagramJunctionElements.SingleOrDefault(a => a.ID == edge.ToID);
        MoveJunction(junction, dX, dY);
      }

      return true;
    }

    internal static bool IsContaintInParent(int ContainerId, int ElementContainerId)
    {
      if (ElementContainerId == 0) return false;

      if (ElementContainerId == ContainerId)
        return true;

      DiagramElement Parent = GlobalDiagramContainerElements.FirstOrDefault(a => a.ID == ContainerId);
      if (Parent == null)
        return false;

      return IsContaintInParent(Parent, ElementContainerId);
    }

    internal static bool IsContaintInParent(DiagramElement Container, int ElementContainerId)
    {
      if (ElementContainerId == 0) return false;

      if (ElementContainerId == Container.ID)
        return true;

      DiagramElement Parent = GlobalDiagramContainerElements.FirstOrDefault(a => a.ID == Container.ContainerID);
      if (Parent == null)
        return false;

      return IsContaintInParent(Parent, ElementContainerId);
    }

    internal static bool IsEdgeStartInContainer(DiagramEdgeElement edge, int ElementContainerId)
    {
      DiagramJunctionElement from1 = GlobalDiagramJunctionElements.Single(a => a.ID == edge.FromID);
      DiagramJunctionElement to1 = GlobalDiagramJunctionElements.Single(a => a.ID == edge.ToID);

      if (from1 != null && IsContaintInParent(from1.ContainerID, ElementContainerId)) return true;
      if (to1 != null && IsContaintInParent(to1.ContainerID, ElementContainerId)) return true;
      return false;
    }
  }
}
