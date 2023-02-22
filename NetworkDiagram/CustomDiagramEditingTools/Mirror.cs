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
using ArcGIS.Core.Internal.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DiagramEditing.DiagramEditingModule;

namespace DiagramEditing
{
  internal class Mirror : MapTool
  {
    #region private members
    private MapPoint mPoint1 = null;
    private MapPoint mPoint2 = null;
    private double aLine;
    private double bLine;
    private bool IsSketchComplete = false;
    private bool mIsWorking = false;

    private ModeMiror MirorMode { get; set; }
    #endregion 

    /// <summary>
    /// Mirror mode
    /// </summary>
    public enum ModeMiror
    {
      Horizontal,
      Vertical,
      Angle
    }

    public Mirror(ModeMiror Mode)
    {
      IsSketchTool = true;
      SketchType = Mode == ModeMiror.Angle ? SketchGeometryType.Line : SketchGeometryType.Point;
      SketchOutputMode = SketchOutputMode.Map;
      UseSelection = true;

      MirorMode = Mode;
    }

    protected override async Task OnToolActivateAsync(bool active)
    {
      await base.OnToolActivateAsync(active);

      if (active)
      {
        await QueuedTask.Run(() =>
        {
          GetDiagramFromMap(MapView.Active?.Map);
        });

        if (GlobalDiagram == null)
          await DeactivateTool(true);
      }
      else
        await DeactivateTool(false);
    }


    private async Task DeactivateTool(bool deactivateTool = false)
    {
      if (deactivateTool)
        await FrameworkApplication.SetCurrentToolAsync(ExploreTool);
    }

    protected override void OnToolMouseDown(MapViewMouseButtonEventArgs e)
    {
      if (IsSketchComplete || mIsWorking)
        return;

      QueuedTask.Run(() =>
      {
        try
        {
          if (mPoint1 == null)
          {
            mPoint1 = MapView.Active.ClientToMap(e.ClientPoint);

            if (MirorMode != ModeMiror.Angle)
              IsSketchComplete = true;
          }
          else
          {
            mPoint2 = MapView.Active.ClientToMap(e.ClientPoint);
            IsSketchComplete = true;
          }
        }
        catch (Exception ex)
        { ShowException(ex); }
      });

      base.OnToolMouseDown(e);
    }

    protected override void OnToolMouseUp(MapViewMouseButtonEventArgs e)
    {
      if (!IsSketchComplete|| mIsWorking)
        return;

      InternalMoveObjects();

      IsSketchComplete = false;
      base.OnToolMouseUp(e);
      FrameworkApplication.SetCurrentToolAsync(ExploreTool);
    }

    /// <summary>
    /// Move the objects
    /// </summary>
    private void InternalMoveObjects()
    {
      QueuedTask.Run(() =>
      {
        mIsWorking = true;
        string error = GetElements(MapView.Active?.Map);
        if (!string.IsNullOrEmpty(error))
        {
          MessageBox.Show(error);
          mIsWorking = false;
          return;
        }

        if (MirorMode == ModeMiror.Angle)
        {
          if (mPoint1 == null || mPoint2 == null) return;

          aLine = (mPoint2.Y - mPoint1.Y) / (mPoint2.X - mPoint1.X);
          bLine = mPoint2.Y - aLine * mPoint2.X;
        }

        InitialiseEdit();

        foreach (long l in GlobalSelectedContainerIDs)
        {
          DiagramContainerElement dce = GlobalDiagramContainerElements.FirstOrDefault(a => a.ObjectID == l);
          if (dce != null)
            MirrorContainer(Container: dce);
        }

        foreach (long l in GlobalSelectedEdgeIDs)
        {
          DiagramEdgeElement dce = GlobalDiagramEdgeElements.FirstOrDefault(a => a.ObjectID == l);
          if (dce != null)
            MirrorEdge(Edge: dce);
        }

        foreach (long l in GlobalSelectedJunctionIDs)
        {
          DiagramJunctionElement dje = GlobalDiagramJunctionElements.FirstOrDefault(a => a.ObjectID == l);
          if (dje != null)
            MirrorJunction(Junction: dje);
        }

        SaveDiagram();
        mIsWorking = false;
        mPoint1 = null;
        mPoint2 = null;
      });
    }

    /// <summary>
    /// Mirror junction
    /// </summary>
    /// <param name="Junction">DiagramJunctionElement to mirror</param>
    private void MirrorJunction(DiagramJunctionElement Junction)
    {
      MapPoint mp = Junction.Shape as MapPoint;
      CalcDecal(mp.X, mp.Y, out double dX, out double dY);
      MoveJunction(Junction: Junction, dX: dX, dY: dY);
    }

    /// <summary>
    /// Mirror container
    /// </summary>
    /// <param name="Junction">DiagramContainerElement to mirror</param>
    private void MirrorContainer(DiagramContainerElement Container)
    {
      List<DiagramContainerElement> theContainers = GlobalDiagramContainerElements.Where(a => a.ContainerID == Container.ID).ToList();

      foreach (DiagramContainerElement dce in theContainers)
        MirrorContainer(dce);

      List<DiagramEdgeElement> theEdges = GlobalDiagramEdgeElements.Where(a => a.ContainerID == Container.ID).ToList();
      foreach (DiagramEdgeElement dee in theEdges)
        MirrorEdge(dee);

      List<DiagramJunctionElement> theJunctions = GlobalDiagramJunctionElements.Where(a => a.ContainerID == Container.ID).ToList();
      foreach (DiagramJunctionElement dee in theJunctions)
        MirrorJunction(dee);
    }

    /// <summary>
    /// Mirror junction
    /// </summary>
    /// <param name="Junction">DiagramJunctionElement to mirror</param>
    private void MirrorEdge(DiagramEdgeElement Edge)
    {
      if (GlobalEdgesToSave.FirstOrDefault(a => a.ObjectID == Edge.ObjectID) != null)
        return;
      Polyline pol = Edge.Shape as Polyline;

      List<MapPoint> thePoints = new();

      foreach (MapPoint mp in pol.Points)
      {
        CalcDecal(mp.X, mp.Y, out double dX, out double dY);

        if (mp.HasM && mp.HasZ)
          thePoints.Add(MapPointBuilder.CreateMapPoint(mp.X + dX, mp.Y + dY, mp.Z, mp.M, mp.SpatialReference));
        else if (mp.HasZ)
          thePoints.Add(MapPointBuilder.CreateMapPoint(mp.X + dX, mp.Y + dY, mp.Z, mp.SpatialReference));
        else
          thePoints.Add(MapPointBuilder.CreateMapPoint(mp.X + dX, mp.Y + dY, mp.SpatialReference));
      }

      Edge.Shape = PolylineBuilder.CreatePolyline(thePoints);

      DiagramJunctionElement dje = GlobalDiagramJunctionElements.FirstOrDefault(a => a.ID == Edge.FromID);
      if (dje != null)
      {
        dje.Shape = thePoints[0];
        AddUniqueJunctionToList(dje, ref GlobalJunctionsToSave);
      }

      dje = GlobalDiagramJunctionElements.FirstOrDefault(a => a.ID == Edge.ToID);
      if (dje != null)
      {
        dje.Shape = thePoints[^1];
        AddUniqueJunctionToList(dje, ref GlobalJunctionsToSave);
      }

      AddUniqueEdgeToList(Edge, ref GlobalEdgesToSave);
    }

    /// <summary>
    /// Calculate the gap to move object
    /// </summary>
    /// <param name="XObject">Initial X Position</param>
    /// <param name="YObject">Initial Y Position</param>
    /// <param name="dX">X gap to move</param>
    /// <param name="dY">Y gap to move</param>
    private void CalcDecal(double XObject, double YObject, out double dX, out double dY)
    {
      dX = 0.0;
      dY = 0.0;

      if (MirorMode == ModeMiror.Horizontal)
      {
        dX = (mPoint1.X - XObject) * 2;
        return;
      }

      if (MirorMode == ModeMiror.Vertical)
      {
        dY = (mPoint1.Y - YObject) * 2;
        return;
      }

      double cLine = YObject + (XObject / aLine);
      double X0 = aLine * (cLine - bLine) / (aLine * aLine + 1);
      double Y0 = aLine * X0 + bLine;

      dX = 2 * (X0 - XObject);
      dY = 2 * (Y0 - YObject);
    }

    protected override void OnToolKeyDown(MapViewKeyEventArgs k)
    {
      base.OnToolKeyDown(k);
      if (k.Key == System.Windows.Input.Key.Escape)
      {
        k.Handled = true;

        FrameworkApplication.SetCurrentToolAsync(ExploreTool);
      }
    }

  }
  internal class MirrorVertically : Mirror
  {
    public MirrorVertically() : base(ModeMiror.Vertical)
    { }
  }

  internal class MirrorHorizontally : Mirror
  {
    public MirrorHorizontally() : base(ModeMiror.Horizontal)
    { }
  }

  internal class MirrorAngle : Mirror
  {
    public MirrorAngle() : base(ModeMiror.Angle)
    { }
  }

}
