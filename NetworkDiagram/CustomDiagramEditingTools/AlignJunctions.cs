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
  /// <summary>
  /// Align Junctions buttons class
  /// 
  internal class AlignJunctions : Button
  {
    private readonly bool _verticallyMode = false;
    internal AlignJunctions(bool VerticallyMode)
    {
      _verticallyMode = VerticallyMode;
    }

    /// <summary>
    /// Align the currently selected junctions according the specified pivot flag
    /// </summary>
    protected override void OnClick()
    {
      QueuedTask.Run(() =>
      {
        string error = GetElements(MapView.Active?.Map);
        if (!string.IsNullOrEmpty(error))
        {
          MessageBox.Show(error);
          return;
        }

        IReadOnlyList<DiagramFlag> dfList = GlobalDiagram.GetFlags(NetworkDiagramFlagType.PivotJunction).ToList();
        if (dfList.Count == 0)
        {
          MessageBox.Show(ErrorReferenceFlag);
          return;
        }
        else if (dfList.Count > 1)
        { MessageBox.Show(ErrorFlagsNumber);
          return;
        }

        InitialiseEdit();

        DiagramFlag df = dfList[0]; // Use the first pivot flag as the reference point

        MapPoint StartPoint = df.Position;

        try
        {
          AlignInternal(XRef: StartPoint.X, YRef: StartPoint.Y);
          SaveDiagram();
        }
        catch (Exception ex)
        {
          ShowException(exception: ex);
        }
      });
    }

    /// <summary>
    /// Align the objects with the specified pivot flag
    /// </summary>
    /// <param name="XRef">Pivot flag X position</param>
    /// <param name="YRef">Pivot flag Y position</param>
    private void AlignInternal(double XRef, double YRef)
    {
      List<long> mTreatedCont = new();
      List<long> mTreatedJunc = new();

      foreach (var v in GlobalSelectedContainerIDs)
      {
        DiagramContainerElement cont = GetContainerByObjectID(v);
        if (cont != null && cont.ContainerID == 0)
        {
          // The main container must be treated first, so all its contents are automatically moved
          AlignContainer(Container: cont, XRef: XRef, YRef: YRef, TreatedCont: mTreatedCont, TreatedJunc: mTreatedJunc);
        }
      }

      foreach (var v in GlobalSelectedContainerIDs)
      {
        DiagramContainerElement cont = GetContainerByObjectID(v);
        AlignContainer(Container: cont, XRef: XRef, YRef: YRef, TreatedCont: mTreatedCont, TreatedJunc: mTreatedJunc);
      }

      foreach (var v in GlobalSelectedJunctionIDs)
      {
        DiagramJunctionElement junc = GetJunctionByObjectID(v);
        AlignJunction(Junction: junc, XRef: XRef, YRef: YRef, TreatedJunc: mTreatedJunc);
      }
    }

    /// <summary>
    /// Align a diagram junction
    /// </summary>
    /// <param name="Junction">DiagramJunctionElement</param>
    /// <param name="XRef">Pivot flag X position</param>
    /// <param name="YRef">Pivot flag Y position</param>
    /// <param name="TreatedJunc">List of junctions already treated</param>
    private void AlignJunction(DiagramJunctionElement Junction, double XRef, double YRef, List<long> TreatedJunc)
    {
      if (Junction == null) return;
      if (TreatedJunc.Contains(Junction.ID)) return;

      MapPoint mp = Junction.Shape as MapPoint;
      double dX = 0.0;
      double dY = 0.0;
      if (_verticallyMode)
        dX = XRef - mp.X;
      else
        dY = YRef - mp.Y;

      MoveJunction(Junction, dX, dY);
      TreatedJunc.Add(Junction.ID);
    }

    /// <summary>
    /// Align a diagram container
    /// </summary>
    /// <param name="Container">DiagramContainerElement</param>
    /// <param name="XRef">Pivot flag X position</param>
    /// <param name="YRef">Pivot flag Y position</param>
    /// <param name="TreatedCont">List of containers already treated</param>
    /// <param name="TreatedJunc">List of junctions already treated</param>
    private void AlignContainer(DiagramContainerElement Container, double XRef, double YRef, List<long> TreatedCont, List<long> TreatedJunc)
    {
      if (Container == null) return;
      if (TreatedCont.Contains(Container.ID)) return;

      IEnumerable<DiagramJunctionElement> lJunctions = GlobalDiagramJunctionElements.Where(a => a.ContainerID == Container.ID);
      foreach (DiagramJunctionElement junc in lJunctions)
      {
        if (!TreatedJunc.Contains(junc.ID))
        {
          MapPoint mp = junc.Shape as MapPoint;
          double dX = 0.0;
          double dY = 0.0;
          if (_verticallyMode)
            dX = XRef - mp.X;
          else
            dY = YRef - mp.Y;

          MoveJunction(junc, dX, dY);
          TreatedJunc.Add(junc.ID);
        }
      }

      IEnumerable<DiagramContainerElement> lCont = GlobalDiagramContainerElements.Where(a => a.ContainerID == Container.ID);
      foreach (DiagramContainerElement cont in lCont)
        MoveContainer(cont, XRef, YRef);

      TreatedCont.Add(Container.ID);
    }

  }

  /// <summary>
  /// Align junctions vertically
  /// 
  internal class AlignJunctions_Vertically : AlignJunctions
  {
    public AlignJunctions_Vertically() :
       base(true)
    { }
  }

  /// <summary>
  /// Align junctions horizontally
  /// 
  internal class AlignJunctions_Horizontally : AlignJunctions
  {
    public AlignJunctions_Horizontally() :
       base(false)
    { }
  }

}
