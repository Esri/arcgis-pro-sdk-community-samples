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
  /// <summary>
  /// Align Junctions by Angle button class
  /// 
  internal class AlignJunctionByAngle : MapTool
  {
    private readonly bool _verticallyMode = false;
    private MapPoint mPoint1 = null;
    private MapPoint mPoint2 = null;
    private bool IsSketchComplete = false;
    private bool mIsWorking = false;

    public AlignJunctionByAngle(bool VerticallyMode)
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Line;
      SketchOutputMode = SketchOutputMode.Map;
      _verticallyMode = VerticallyMode;
    }

    protected override async Task OnToolActivateAsync(bool active)
    {
      await base.OnToolActivateAsync(active);
      if (active)
      {
        mPoint1 = null;

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

    protected override void OnToolMouseDown(MapViewMouseButtonEventArgs e)
    {
      if (IsSketchComplete || mIsWorking)
        return;

      QueuedTask.Run(() =>
      {
        try
        {
          if (mPoint1 == null)
            mPoint1 = MapView.Active.ClientToMap(e.ClientPoint);
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

    protected override async void OnToolMouseUp(MapViewMouseButtonEventArgs e)
    {
      if (!IsSketchComplete || mIsWorking)
        return;

      await InternalMoveObjects();
      mPoint1 = null;
      mPoint2 = null;

      IsSketchComplete = false;
      base.OnToolMouseUp(e);
      _ = FrameworkApplication.SetCurrentToolAsync(ExploreTool);
    }

    private async Task InternalMoveObjects()
    {
      await QueuedTask.Run(() =>
      {
        try
        {
          if (mPoint1 == null || mPoint2 == null) return;

          mIsWorking = true;

          string error = GetElements(MapView.Active?.Map);
          if (!string.IsNullOrEmpty(error))
          {
            MessageBox.Show(error);
            mIsWorking = false;
            return;
          }
          IReadOnlyList<DiagramFlag> dfList = GlobalDiagram.GetFlags(NetworkDiagramFlagType.PivotJunction).ToList();
          if (dfList.Count == 0)
          {
            MessageBox.Show(ErrorReferenceFlag);
            mIsWorking = false;
            return;
          }
          else if (dfList.Count > 1)
          {
            MessageBox.Show(ErrorFlagsNumber);
            mIsWorking = false;
            return;
          }

          InitialiseEdit();

          DiagramFlag df = dfList[0];
          MapPoint StartPoint = df.Position;

          double aLine = (mPoint2.Y - mPoint1.Y) / (mPoint2.X - mPoint1.X);
          double bLine = StartPoint.Y - aLine * StartPoint.X;

          foreach (long l in GlobalSelectedContainerIDs)
          {
            DiagramContainerElement dce = GlobalDiagramContainerElements.FirstOrDefault(a => a.ObjectID == l);
            if (dce == null) continue;

            GetContainerCenter(Container: dce, XCenter: out double xCenter, YCenter: out double yCenter, Width: out double width, Height: out double heigth);

            double newX = 0.0;
            double newY = 0.0;

            if (_verticallyMode)
              newY = (aLine * xCenter + bLine) - yCenter;
            else
              newX = ((yCenter - bLine) / aLine) - xCenter;

            MoveContainer(dce, newX, newY);
          }

          foreach (long l in GlobalSelectedJunctionIDs)
          {
            DiagramJunctionElement dje = GlobalDiagramJunctionElements.FirstOrDefault(a => a.ObjectID == l);
            if (dje == null)
              continue;

            MapPoint mp = dje.Shape as MapPoint;
            double newX = 0.0;
            double newY = 0.0;

            if (_verticallyMode)
              newY = (aLine * mp.X + bLine) - mp.Y;
            else
              newX = ((mp.Y - bLine) / aLine) - mp.X;

            MoveJunction(Junction: dje, dX: newX, dY: newY);
          }
            SaveDiagram();
          }
          catch (Exception ex)
          {
            ShowException(ex);
          }
        mIsWorking = false;
      });
    }

    private async Task DeactivateTool(bool deactivateTool = false)
    {
      if (deactivateTool)
      {
        await FrameworkApplication.SetCurrentToolAsync(ExploreTool);
        mPoint1 = null;
      }
    }

  }

  internal class AlignJunctionByAngleVertically : AlignJunctionByAngle
  {
    public AlignJunctionByAngleVertically() :
      base(true)
    { }
  }

  internal class AlignJunctionByAngleHorizontally : AlignJunctionByAngle
  {
    public AlignJunctionByAngleHorizontally() :
        base(false)
    { }
  }

}
