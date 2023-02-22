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
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.NetworkDiagrams;
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using ArcGIS.Desktop.Mapping.Events;
using ArcGIS.Desktop.Core.Events;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DiagramEditing
{
  /// <summary>
  /// This add-in demonstrates how to develop some helpful custom diagram editing commands to swap two diagram features, mirror a set of diagram features, and align diagram junctions.
  /// These custom commands are available on the Add-In tab in the Diagram Editing group:
  /// ![UI](Screenshots/CustomDiagramEditingTools_Tab.png)
  /// NOTE: The CustomDiagramEditingTools add-in code is a generic code sample that applies to any network diagram related to any utility or trace network dataset.This add-in demonstrates how to develop some helpful custom diagram editing commands to swap two diagram features, mirror a set of diagram features, and align diagram junctions.
  /// These custom commands are available on the Add-In tab in the Diagram Editing group:
  /// ![UI](Screenshots/CustomDiagramEditingTools_Tab.png)
  /// NOTE: The CustomDiagramEditingTools add-in code is a generic code sample that applies to any network diagram related to any utility or trace network dataset.
  /// </summary>
  /// <remarks>
  /// Using the sample:  
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.  
  /// 1. Start ArcGIS Pro.  
  /// 1. Open your favorite utility network or trace network ArcGIS Pro project.
  /// 1. Open the network diagram you want to edit or create a new network diagram.
  /// 1. Click the Add-In tab on the ribbon.
  /// When there is no diagram features currently selected in the diagram map, most of the commands in the Diagram Editing group are disabled:
  /// ![UI](Screenshots/CustomDiagramEditingTools_Tab.png)
  /// The following sections detail how to use each custom diagram editing command installed with this add-in.
  /// --- Set the custom diagram editing tool options ---
  /// 1. On the Add-In tab, in the Diagram Editing group, click the Diagram Editing Options dialog box launcher.
  /// ![UI](Screenshots/DiagramEditingOptions.png)
  /// 1. If you want these custom editing commands to consider the vertices along any processed diagram edges, make sure the 'Keep vertices along edges when aligning, swapping and mirroring a diagram feature set' box is checked. This is the default.
  /// 1. If you don't care about the vertices along the processed diagram edges, uncheck the 'Keep vertices along edges when aligning, swapping and mirroring a diagram feature set' box; all vertices along those diagram edges will be removed.
  /// --- Swap the positions of two diagram features ---
  /// 1. In the active diagram map, select the two diagram features you want to swap.
  /// ![UI](Screenshots/BeforeSwap.png)
  /// 1. Click Swap.
  /// RESULT: The two diagram features are swapped. Any diagram edges related to the swapped diagram features are properly reconnected. Any diagram container or content diagram features related to the swapped diagram features are properly redrawn or repositioned.
  /// ![UI](Screenshots/AfterSwap.png)
  /// NOTE: When the 'Keep vertices along edges when aligning, swapping and mirroring a diagram feature set' option is checked, the Swap command keeps any vertices along any diagram edges connecting the swapped junctions. When the option is unchecked, all vertices along these connected edges are removed. You get an error message when applying the command while there are more than two diagram features selected in the diagram map.
  /// --- Mirror a diagram feature set ---
  /// 1. Select the set of diagram features you want to mirror.
  /// ![UI](Screenshots/BeforeMirror.png)
  /// 1. On the Add-In tab, in the Diagram Editing group, click the drop down arrow under Mirror and click Horizontally Mirror.
  /// 1. Click the diagram background at the location you want.
  /// RESULT: The Y coordinate of the clicked location is used to determine the horizontal axis along which the reflection operates. The selected diagram junctions are moved to their reflected positions at the opposite side of this horizontal axis.
  /// ![UI](Screenshots/AfterMirror1.png)
  /// 1. On the Add-In tab, in the Diagram Editing group, click the drop down arrow under Mirror and click Vertically Mirror.
  /// 1. Click the diagram background at the location you want.
  /// RESULT: The X coordinate of the clicked location is used to determine the vertical axis along which the reflection operates. The selected diagram junctions are moved to their reflected positions at the opposite side of this vertical axis.
  /// ![UI](Screenshots/AfterMirror2.png)
  /// 1. On the Add-In tab, in the Diagram Editing group, click the drop down arrow under Mirror and click Angle Mirror.
  /// 1. Sketch a line on the diagram background by clicking the origin location of that line, then clicking its end location.
  /// ![UI](Screenshots/SketchedLineBeforeMirror3.png)
  /// RESULT: The sketched line is the axis along which the reflection operates. The selected diagram junctions are moved so they are placed at the opposite side of this line.
  /// ![UI](Screenshots/AfterMirror3.png)
  /// NOTE: When the 'Keep vertices along edges when aligning, swapping and mirroring a diagram feature set' option is checked, those three Mirror commands process any vertices along the selected diagram edges in the same way they process diagram junctions. When the option is unchecked, these diagram edges display as straight lines joining their two connected diagram junctions.
  /// --- Align a set of diagram junctions ---
  /// 1. Select one or several diagram junctions in the active diagram map.
  /// ![UI](Screenshots/BeforeAlign.png)
  /// 1. On the Add-In tab, in the Diagram Editing group, click the Set Reference Point tool and click the diagram junction you want to be considered as the reference point for the alignment operation.
  /// RESULT: A gray star overlays the clicked junction when it is correctly set as a reference point
  /// ![UI](Screenshots/BeforeAlign_ReferencePointSet.png)
  /// 1. Click the drop down arrow under Align Junctions and click Vertically Align Junctions or Horizontally Align Junctions.
  /// RESULT: The selected junctions move so they are vertically/horizontally aligned with the specified reference point.
  /// ![UI](Screenshots/AfterAlign1.png)
  /// 1. Click the drop down arrow under Align Junctions by Angle, then click Vertically Align Junctions by Angle or Horizontally Align Junctions by Angle.
  /// 1. Sketch a line on the diagram background by clicking the origin location of that line, then clicking its end location.
  /// ![UI](Screenshots/SketchedLineBeforeAlign2.png)
  /// RESULT: The selected junctions move so they are vertically/horizontally projected along the line that goes through the specified reference point and is paralleled to the line you’ve just sketched.
  /// ![UI](Screenshots/AfterAlign3.png)
  /// NOTE: When the 'Keep vertices along edges when aligning, swapping and mirroring a diagram feature set' option is checked, these four alignment commands keep any vertices along any diagram edges connecting the aligned junctions. When the option is unchecked, all vertices along these connected edges are removed. To set another diagram junction as the reference point, click the diagram junction.The gray star over the previous reference point is removed, and a new gray star overlays the newly clicked diagram junction. This junction is considered as the new reference point in the diagram.
  /// To remove the specified reference point, click anywhere on the diagram background. The gray star over the current reference point is removed.
  /// </remarks>
  internal class DiagramEditingModule : Module
  {
    private static DiagramEditingModule _this = null;

    public const string EnableSelection2 = "DiagramTools_2SelectState";

    public const string ErrorReferenceFlag = "This tool expects a reference point.";
    //public const string ErrorFlags = "This tool needs a Pivot Flag and eventually Barrier Flag";
    public const string ErrorFlagsNumber = "There are more than one reference point currently specified in the active diagram. This tool expects a single reference point to properly align the selected diagram junctions. Please review the specified reference points";

    public const string ExploreTool = "esri_mapping_exploreTool";

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static DiagramEditingModule Current => _this ??= (DiagramEditingModule)FrameworkApplication.FindModule("DiagramEditing_Module");
    public static bool KeepVertices = true;
    internal static int SelectionCount;

    internal static Map GlobalActiveMap { get; set; }
    internal static List<DiagramJunctionElement> GlobalDiagramJunctionElements;
    internal static List<DiagramEdgeElement> GlobalDiagramEdgeElements;
    internal static List<DiagramContainerElement> GlobalDiagramContainerElements;
    internal static List<DiagramJunctionElement> GlobalJunctionsToSave;
    internal static List<DiagramEdgeElement> GlobalEdgesToSave;
    internal static List<DiagramContainerElement> GlobalContainersToSave;
    internal static NetworkDiagram GlobalDiagram;
    internal static DiagramLayer GlobalDiagramLayer;
    internal static SelectionSet GlobalMapSelection { get; set; }
    internal static List<long> GlobalSelectedJunctionIDs { get; set; }
    internal static List<long> GlobalSelectedEdgeIDs { get; set; }
    internal static List<long> GlobalSelectedContainerIDs { get; set; }

    internal static System.Timers.Timer GlobalTimer;
    internal static bool IsSavingDiagram = false;

    #region Overrides
    /// <summary>
    /// Called by Framework when ArcGIS Pro is closing
    /// </summary>
    /// <returns>False to prevent Pro from closing, otherwise True</returns>
    protected override bool CanUnload()
    {
      //TODO - add your business logic
      //return false to ~cancel~ Application close
      return true;
    }

    protected override bool Initialize()
    {
      ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);
      MapSelectionChangedEvent.Subscribe(OnMapSelectionChanged);
      ProjectClosingEvent.Subscribe(OnProjectClosing);
      GlobalTimer = new(10000);
      GlobalTimer.Elapsed += GlobalTimer_Elapsed;

      return base.Initialize();
    }

    protected override void Uninitialize()
    {
      ActiveMapViewChangedEvent.Unsubscribe(OnActiveMapViewChanged);
      MapSelectionChangedEvent.Unsubscribe(OnMapSelectionChanged);
      ProjectClosingEvent.Unsubscribe(OnProjectClosing);
      GlobalTimer.Elapsed -= GlobalTimer_Elapsed;
      GlobalTimer = null;

      base.Uninitialize();
    }

    #endregion Overrides

    #region Events
    private Task OnProjectClosing(ProjectClosingEventArgs arg)
    {
      ActiveMapViewChangedEvent.Unsubscribe(OnActiveMapViewChanged);
      ProjectClosingEvent.Unsubscribe(OnProjectClosing);
      MapSelectionChangedEvent.Unsubscribe(OnMapSelectionChanged);

      return QueuedTask.Run(() =>
      {
        CleanModule();
      });
    }

    private void GlobalTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      GlobalTimer.Stop();
      LoadElements(MapView.Active?.Map);
    }

    private void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs obj)
    {
      if (obj == null || obj.IncomingView == null || obj.IncomingView.Map == null) return;

      Map incomingMap = obj.IncomingView.Map;

      LoadElements(incomingMap);
    }

    private static void LoadElements(Map ActiveMap)
    {
      if (ActiveMap == null)
      {
        CleanModule();
        return;
      }

      QueuedTask.Run(() =>
      {
        try
        {
          GetDiagramFromMap(ActiveMap);
          if (GlobalDiagram != null)
          {
            if (GlobalDiagram.DiagramManager?.GetNetwork<UtilityNetwork>()?.GetDatastore() is Geodatabase geodatabase)
            {
              if (geodatabase.HasEdits())
              {
                GlobalTimer.Start();
              }
              else
              {
                GetElements(UseSelection: true, ReferenceMap: ActiveMap);
              }
            }
          }
        }
        catch (Exception ex)
        { ShowException(ex); }
      });
    }

    private void OnMapSelectionChanged(MapSelectionChangedEventArgs obj)
    {
      if (IsSavingDiagram) return;

      QueuedTask.Run(() =>
      {
        if (obj == null || obj.Map == null)
        {
          CleanModule();
          return;
        }

        GetElements(UseSelection: true, ReferenceMap: obj.Map);
      });
    }
    #endregion Events

    internal static void CleanModule()
    {
      GlobalDiagramJunctionElements = null;
      GlobalDiagramEdgeElements = null;
      GlobalDiagramContainerElements = null;
      GlobalJunctionsToSave = null;
      GlobalEdgesToSave = null;
      GlobalContainersToSave = null;
      GlobalDiagram = null;
      GlobalMapSelection = null;
      GlobalSelectedJunctionIDs = null;
      GlobalSelectedEdgeIDs = null;
      GlobalSelectedContainerIDs = null;
      GlobalDiagramLayer = null;
      GlobalActiveMap = null;
      SelectionCount = 0;
    }

    /// <summary>
    /// Initialize the list of DiagramElement to save
    /// </summary>
    internal static void InitialiseEdit()
    {
      GlobalJunctionsToSave = new List<DiagramJunctionElement>();
      GlobalEdgesToSave = new List<DiagramEdgeElement>();
      GlobalContainersToSave = new List<DiagramContainerElement>();
    }

    /// <summary>
    /// Inititalize some global fields
    /// </summary>
    /// <param name="ReferenceMap">Map</param>
    internal static void GetDiagramFromMap(Map ReferenceMap)
    {
      if (ReferenceMap == null || ReferenceMap.MapType != MapType.NetworkDiagram)
      {
        CleanModule();
        return;
      }

      if (ReferenceMap == GlobalActiveMap) return;

      GlobalActiveMap = ReferenceMap;

      IReadOnlyList<Layer> myLayers = ReferenceMap.Layers;
      if (myLayers == null) return;

      foreach (Layer l in myLayers)
      {
        if (l.GetType() == typeof(DiagramLayer))
        {
          GlobalDiagramLayer = l as DiagramLayer;
          break;
        }
      }

      if (GlobalDiagramLayer == null)
      {
        CleanModule();
        return;
      }

      GlobalDiagram = GlobalDiagramLayer.GetNetworkDiagram();
      return;
    }

    /// <summary>
    /// Gets DiagramElements
    /// </summary>
    /// <param name="ReferenceMap">Map</param>
    /// <param name="UseSelection">Get the selection</param>
    /// <returns></returns>
    internal static string GetElements(Map ReferenceMap, bool UseSelection = true)
    {
      CleanModule();
      GetDiagramFromMap(ReferenceMap);

      if (GlobalDiagram == null) return string.Empty;

      GlobalSelectedJunctionIDs = new List<long>();
      GlobalSelectedContainerIDs = new List<long>();
      GlobalSelectedEdgeIDs = new List<long>();

      GlobalMapSelection = ReferenceMap.GetSelection();

      if (UseSelection)
      {
        foreach (var v in GlobalMapSelection.ToDictionary())
        {
          FeatureLayer layer = v.Key as FeatureLayer;
          if (layer.ShapeType == esriGeometryType.esriGeometryPoint)
          {
            foreach (var id in v.Value)
            {
              GlobalSelectedJunctionIDs.Add(id);
              SelectionCount++;
            }
          }
          else if (layer.ShapeType == esriGeometryType.esriGeometryPolygon)
          {
            foreach (var id in v.Value)
            {
              GlobalSelectedContainerIDs.Add(id);
              SelectionCount++;
            }
          }
          else
          {
            foreach (var id in v.Value)
            {
              GlobalSelectedEdgeIDs.Add(id);
              SelectionCount++;
            }
          }
        }

        if (SelectionCount == 0)
          FrameworkApplication.State.Deactivate(EnableSelection2);
        else
        {
          if (SelectionCount == 2)
            FrameworkApplication.State.Activate(EnableSelection2);
          else
            FrameworkApplication.State.Deactivate(EnableSelection2);
        }
      }

      DiagramElementQueryResult deqr;
      if (SelectionCount == 0)
      {
        DiagramElementQueryByElementTypes query = new()
        {
          QueryDiagramContainerElement = true,
          QueryDiagramEdgeElement = true,
          QueryDiagramJunctionElement = true
        };
        deqr = GlobalDiagram.QueryDiagramElements(query);
      }
      else
      {
        DiagramElementQueryByObjectIDs query1 = new()
        {
          AddConnected = true,
          AddContents = true,
          ContainerObjectIDs = GlobalSelectedContainerIDs,
          JunctionObjectIDs = GlobalSelectedJunctionIDs,
          EdgeObjectIDs = GlobalSelectedEdgeIDs
        };

        deqr = GlobalDiagram.QueryDiagramElements(query1);
      }

      GlobalDiagramJunctionElements = deqr.DiagramJunctionElements.ToList();
      GlobalDiagramEdgeElements = deqr.DiagramEdgeElements.ToList();
      GlobalDiagramContainerElements = deqr.DiagramContainerElements.ToList();

      return "";
    }

    /// <summary>
    /// Save diagram in geodatabase
    /// </summary>
    internal static void SaveDiagram()
    {
      if (GlobalJunctionsToSave.Count + GlobalEdgesToSave.Count + GlobalContainersToSave.Count > 0)
      {
        IsSavingDiagram = true;

        GlobalDiagram.SaveLayout(new NetworkDiagramSubset { DiagramContainerElements = GlobalContainersToSave, DiagramEdgeElements = GlobalEdgesToSave, DiagramJunctionElements = GlobalJunctionsToSave }, KeepVertices);

        GlobalJunctionsToSave = null;
        GlobalEdgesToSave = null;
        GlobalContainersToSave = null;

        var commandId = "esri_networkdiagrams_recalculateDiagramExtentButton";

        if (FrameworkApplication.GetPlugInWrapper(commandId) is ICommand command)
        {
          command.Execute(null);
        }

        MapView.Active.Redraw(true);

        if (GlobalMapSelection != null)
        {
          MapView.Active.Map.ClearSelection();
          IsSavingDiagram = false;
          MapView.Active.Map.SetSelection(GlobalMapSelection, SelectionCombinationMethod.New);
        }
        else
        {
          IsSavingDiagram = false;
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="exception"></param>
    internal static void ShowException(Exception exception)
    {
      string s = ExceptionFormat(exception);

      //#if (DEBUG)
      s += Environment.NewLine + Environment.NewLine + exception.StackTrace;// + Environment.NewLine + Environment.NewLine;
                                                                            //#endif

      MessageBox.Show(s);
    }

    /// <summary>
    /// Format exception and InnerException
    /// </summary>
    /// <param name="ex">Exception</param>
    /// <returns>Return exception and InnerException as string</returns>
    internal static string ExceptionFormat(Exception ex)
    {
      if (ex.InnerException != null)
      {
        string s = ExceptionFormat(ex.InnerException);

        if (s.Length > 0)
          return string.Format("{0}\n{1}", ex.Message, s);
      }

      return ex.Message;
    }

    /// <summary>
    /// Return the junction identified by ObjectID
    /// </summary>
    /// <param name="ObjectID">Identity</param>
    /// <returns>DiagramJunctionElement</returns>
    internal static DiagramJunctionElement GetJunctionByObjectID(long ObjectID)
    {
      return GlobalDiagramJunctionElements.FirstOrDefault(a => a.ObjectID == ObjectID);
    }

    /// <summary>
    /// Return the edge identified by ObjectID
    /// </summary>
    /// <param name="ObjectID">Identity</param>
    /// <returns>DiagramEdgeElement</returns>
    internal static DiagramEdgeElement GetEdgeByObjectID(long ObjectID)
    {
      return GlobalDiagramEdgeElements.FirstOrDefault(a => a.ObjectID == ObjectID);
    }

    /// <summary>
    /// Return the container identified by ObjectID
    /// </summary>
    /// <param name="ObjectID">Identity</param>
    /// <returns>DiagramContainerElement</returns>
    internal static DiagramContainerElement GetContainerByObjectID(long ObjectID)
    {
      return GlobalDiagramContainerElements.FirstOrDefault(a => a.ObjectID == ObjectID);
    }

    /// <summary>
    /// Return the junction identified by ID
    /// </summary>
    /// <param name="ObjectID">Identity</param>
    /// <returns>DiagramJunctionElement</returns>
    internal static DiagramJunctionElement GetJunctionByID(long ID)
    {
      return GlobalDiagramJunctionElements.FirstOrDefault(a => a.ObjectID == ID);
    }

    /// <summary>
    /// Return the edge identified by ObjectID
    /// </summary>
    /// <param name="ObjectID">Identity</param>
    /// <returns>DiagramEdgeElement</returns>
    internal static DiagramEdgeElement GetEdgeByID(long ID)
    {
      return GlobalDiagramEdgeElements.FirstOrDefault(a => a.ID == ID);
    }

    /// <summary>
    /// Move a junction
    /// </summary>
    /// <param name="Junction">DiagramJunctionElement</param>
    /// <param name="dX">X shift</param>
    /// <param name="dY">Y shift</param>
    internal static void MoveJunction(DiagramJunctionElement Junction, double dX, double dY)
    {
      if (Junction == null || (dX == 0.0 && dY == 0.0))
        return;

      if (GlobalJunctionsToSave.FirstOrDefault(a => a.ObjectID == Junction.ObjectID) != null)
        return;

      if (Junction.Shape.HasZ)
        Junction.Shape = GeometryEngine.Instance.Move(Junction.Shape, dX, dY, 0.0).Clone();
      else
        Junction.Shape = GeometryEngine.Instance.Move(Junction.Shape, dX, dY).Clone();

      GlobalJunctionsToSave.Add(Junction);
    }

    /// <summary>
    /// Move a container
    /// </summary>
    /// <param name="Container">DiagramContainerElement</param>
    /// <param name="dX">X shift</param>
    /// <param name="dY">Y shift</param>
    /// <remarks>Move all objects in container if it not empty, else move it</remarks>
    internal static void MoveContainer(DiagramContainerElement Container, double dX, double dY)
    {
      GetContainerCenter(Container, out double posXContainer, out double posYContainer, out double Width, out double Height);

      IEnumerable<DiagramContainerElement> lContainers = GlobalDiagramContainerElements.Where(a => a.ContainerID == Container.ID);
      foreach (var v in lContainers)
        MoveContainer(v, dX, dY);

      IEnumerable<DiagramEdgeElement> lEdges = GlobalDiagramEdgeElements.Where(a => a.ContainerID == Container.ID);
      foreach (var v in lEdges)
        MoveEdge(v, dX, dY);

      IEnumerable<DiagramJunctionElement> lJunctions = GlobalDiagramJunctionElements.Where(a => a.ContainerID == Container.ID);
      foreach (var v in lJunctions)
        MoveJunction(v, dX, dY);

      // The container is empty
      if (!lContainers.Any() && !lEdges.Any() && !lJunctions.Any())
      {
        if (Container.Shape.HasZ)
          Container.Shape = GeometryEngine.Instance.Move(Container.Shape, dX, dY, 0.0);
        else
          Container.Shape = GeometryEngine.Instance.Move(Container.Shape, dX, dY);

        GlobalContainersToSave.Add(Container);
      }
    }

    /// <summary>
    /// Move an edge
    /// </summary>
    /// <param name="Edge">DiagramEdgeElement</param>
    /// <param name="dX">X shift</param>
    /// <param name="dY">Y shift</param>
    /// <remarks>If KeepVertice is on, move the the edge and its extremities</remarks>
    internal static void MoveEdge(DiagramEdgeElement Edge, double dX, double dY)
    {
      if (Edge == null || (dX == 0.0 && dY == 0.0))
        return;
      if (GlobalEdgesToSave.FirstOrDefault(a => a.ObjectID == Edge.ObjectID) != null)
        return;

      DiagramJunctionElement from1 = GlobalDiagramJunctionElements.SingleOrDefault(a => a.ID == Edge.FromID);
      DiagramJunctionElement to1 = GlobalDiagramJunctionElements.SingleOrDefault(a => a.ID == Edge.ToID);

      if (!KeepVertices)
      {
        MoveJunction(from1, dX, dY);
        MoveJunction(to1, dX, dY);
        return;
      }

      Geometry fromShape = from1.Shape;
      Geometry toShape = to1.Shape;

      Edge.Shape = MovePolyline(Edge.Shape as Polyline, dX, dY, ref fromShape, ref toShape);

      if (!GlobalJunctionsToSave.Contains(from1))
        from1.Shape = fromShape;
      if (!GlobalJunctionsToSave.Contains(to1))
        to1.Shape = toShape;

      GlobalEdgesToSave.Add(Edge);
      AddUniqueJunctionToList(Junction: from1, listJunction: ref GlobalJunctionsToSave);
      AddUniqueJunctionToList(Junction: to1, listJunction: ref GlobalJunctionsToSave);
    }

    /// <summary>
    /// Move a Polyline
    /// </summary>
    /// <param name="Pol">Polyline</param>
    /// <param name="dX">X shift</param>
    /// <param name="dY">Y shift</param>
    /// <param name="from">Origine point</param>
    /// <param name="to">Extremity point</param>
    /// <returns>New polyline geometry</returns>
    /// <remarks>If KeepVertice is on, move the the edge and its extremities</remarks>
    private static Geometry MovePolyline(Polyline Pol, double dX, double dY, ref Geometry from, ref Geometry to)
    {
      Geometry geo;
      if (Pol.HasZ)
        geo = GeometryEngine.Instance.Move(Pol, dX, dY, 0.0);
      else
        geo = GeometryEngine.Instance.Move(Pol, dX, dY);

      Polyline newPol = geo as Polyline;

      from = newPol.Points[0];
      to = newPol.Points[newPol.PointCount - 1];

      return geo;
    }

    /// <summary>
    /// Add a junction to a list if not exists
    /// </summary>
    /// <param name="Junction">Junction to add</param>
    /// <param name="listJunction">List of junctions</param>
    internal static void AddUniqueJunctionToList(DiagramJunctionElement Junction, ref List<DiagramJunctionElement> listJunction)
    {
      if (Junction == null) return;
      if (listJunction.SingleOrDefault(a => a.ObjectID == Junction.ObjectID) == null)
        listJunction.Add(Junction);
    }

    /// <summary>
    /// Add an edge to a list if not exists
    /// </summary>
    /// <param name="Edge">Edge to add</param>
    /// <param name="listEdge">List of edges</param>
    internal static void AddUniqueEdgeToList(DiagramEdgeElement Edge, ref List<DiagramEdgeElement> listEdge)
    {
      if (Edge == null) return;
      if (listEdge.SingleOrDefault(a => a.ObjectID == Edge.ObjectID) != null)
        listEdge.Remove(Edge);

      listEdge.Add(Edge);
    }

    /// <summary>
    /// Return dimensions of DiagramContainerElement
    /// </summary>
    /// <param name="Container">DiagramContainerElement</param>
    /// <param name="X">X center position</param>
    /// <param name="Y">Y center position</param>
    /// <param name="Width">Width</param>
    /// <param name="Height">Height</param>
    internal static void GetContainerCenter(DiagramContainerElement Container, out double XCenter, out double YCenter, out double Width, out double Height)
    {
      Envelope env = Container.Shape.Extent;

      Width = env.XMax - env.XMin;
      Height = env.YMax - env.YMin;
      XCenter = env.XMin + Width / 2;
      YCenter = env.YMin + Height / 2;
    }

  }
}
