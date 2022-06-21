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

            foreach (var v in selection.ToDictionary())
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
            }, ArcGIS.Core.Data.ServiceSynchronizationType.Synchronous);
          }
          else
            Diagram.ApplyLayout(layoutParameters, ArcGIS.Core.Data.ServiceSynchronizationType.Synchronous);

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
