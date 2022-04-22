using ArcGIS.Core.Data.UtilityNetwork.NetworkDiagrams;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using static CustomizeNetworkDiagramLayoutExecution.CustomizeNetworkDiagramLayoutExecutionModule;

namespace CustomizeNetworkDiagramLayoutExecution
{
  class PredefinedSmartTreeLayout
  {
    private readonly DiagramLayoutParameters layoutParameters;

    /// <summary>
    /// Constructor
    /// </summary>
    public PredefinedSmartTreeLayout()
    {
      layoutParameters = new SmartTreeDiagramLayoutParameters
      {
        Direction = SmartTreeDiagramLayoutParameters.TreeDirection.FromTopToBottom,
        DisjoinedGraphSpacing = 5.0,
        AbsoluteUnit = false,
        PreserveContainers = true,
        SubtreeSpacing = 10.0,
        AlongSpacing = 20.0,
        PerpendicularSpacing = 8.0
      };
    }

    /// <summary>
    /// Apply the layout to the diagram layer
    /// </summary>
    /// <param name="diagramLayer">Diagram Layer</param>
    public void Apply(DiagramLayer diagramLayer)
    {
      if (diagramLayer == null) return;
      try
      {
        QueuedTask.Run(() =>
        {
          NetworkDiagram Diagram = diagramLayer.GetNetworkDiagram();

          var selection = MapView.Active.Map.GetSelection();
          if (selection != null && selection.Count > 0)
          {
            List<long> JunctionObjectIDs = new List<long>();
            List<long> ContainerObjectIDs = new List<long>();
            List<long> EdgeObjectIDs = new List<long>();

            foreach (var v in selection)
            {
              if (v.Key is FeatureLayer layer)
              {
                if (layer.ShapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryPoint)
                  JunctionObjectIDs.AddRange(v.Value);
                else if (layer.ShapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolygon)
                  ContainerObjectIDs.AddRange(v.Value);
                else if (layer.ShapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolyline)
                  EdgeObjectIDs.AddRange(v.Value);
              }
            }

            Diagram.ApplyLayout(layoutParameters, new DiagramElementObjectIDs
            {
              ContainerObjectIDs = ContainerObjectIDs,
              JunctionObjectIDs = JunctionObjectIDs,
              EdgeObjectIDs = EdgeObjectIDs
            }, ArcGIS.Core.Data.InvocationTarget.SynchronousService);
          }
          else
            Diagram.ApplyLayout(layoutParameters, ArcGIS.Core.Data.InvocationTarget.SynchronousService);

          MapView.Active.Redraw(true);
          MapView.Active.ZoomTo(Diagram.GetDiagramInfo().DiagramExtent.Expand(1.05, 1.05, true));

          if (FrameworkApplication.CurrentTool.Contains("_networkdiagrams_"))
            FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");
        });
      }
      catch (Exception ex)
      {
        ShowException(ex);
      }
    }
  }
}
