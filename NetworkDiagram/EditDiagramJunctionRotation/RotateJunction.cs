using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork.NetworkDiagrams;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EditDiagramJunctionRotation.EditDiagramJunctionRotationModule;

namespace EditDiagramJunctionRotation
{
  internal class RotateJunction : Button
  {
    private bool _isRelative;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="IsRelative">Flag indicated the running mode</param>
    public RotateJunction(bool IsRelative)
    {
      _isRelative = IsRelative;
    }

    protected override void OnClick()
    {
      RotateSelectedJunctions(Rotation);
    }

    /// <summary>
    /// Run the rotate selected junctions
    /// </summary>
    /// <param name="rotation">Rotation Angle</param>
    private void RotateSelectedJunctions(double rotation)
    {
      if (MapView.Active != null)
      {
        // Get the Network Diagram Layer
        DiagramLayer diagramLayer = GetDiagramLayerFromMap(MapView.Active.Map);
        if (diagramLayer != null)
        {
          QueuedTask.Run(() =>
          {
            // Get the Network Diagram
            NetworkDiagram diagram = diagramLayer.GetNetworkDiagram();
            if (diagram != null)
            {
              try
              {
                List<long> junctionObjectIDs = new List<long>();

                // get the selection by Layer
                Dictionary<MapMember, List<long>> selection = MapView.Active.Map.GetSelection();

                // Get the selection only for junctions
                foreach (var v in selection)
                {
                  FeatureLayer featureLayer = v.Key as FeatureLayer;
                  if (featureLayer != null)
                  {
                    if (featureLayer.ShapeType != esriGeometryType.esriGeometryPoint)
                      continue;

                    junctionObjectIDs.AddRange(v.Value);
                  }
                }

                // if no junction selected, work on all diagram junctions
                DiagramElementQueryResult result;
                if (junctionObjectIDs.Count == 0)
                {
                  DiagramElementQueryByElementTypes query = new DiagramElementQueryByElementTypes
                  {
                    QueryDiagramContainerElement = false,
                    QueryDiagramEdgeElement = false,
                    QueryDiagramJunctionElement = true
                  };

                  result = diagram.QueryDiagramElements(query);
                }
                else
                {
                  DiagramElementQueryByObjectIDs query = new DiagramElementQueryByObjectIDs
                  {
                    AddConnected = false,
                    AddContents = false,
                    JunctionObjectIDs = junctionObjectIDs
                  };

                  result = diagram.QueryDiagramElements(query);
                }

                List<DiagramJunctionElement> junctionsToSave = new List<DiagramJunctionElement>();

                // Set the new Rotation Value
                foreach (var junction in result.DiagramJunctionElements)
                {
                  if (_isRelative)
                    junction.Rotation += rotation;
                  else
                    junction.Rotation = rotation;

                  junctionsToSave.Add(junction);
                }

                // Save junctions if needed
                if (junctionsToSave.Count() > 0)
                {
                  NetworkDiagramSubset nds = new NetworkDiagramSubset
                  {
                    DiagramEdgeElements = null,
                    DiagramContainerElements = null,
                    DiagramJunctionElements = junctionsToSave
                  };

                  diagram.SaveLayout(nds, true);

                  MapView.Active.Redraw(true);

                  // re set the selection
                  if (selection.Count > 0)
                    MapView.Active.Map.SetSelection(selection, SelectionCombinationMethod.New);
                }
              }
              catch (GeodatabaseException e)
              {
                MessageBox.Show(e.Message, "Failed to Rotate Junctions ");
              }
            }
          });
        }
      }
    }
  }

  /// <summary>
  /// Class for Absolute Rotation
  /// </summary>
  internal class RotateJunctionAbsolute : RotateJunction
  {
    public RotateJunctionAbsolute()
      : base(false)
    { }
  }

  /// <summary>
  /// Class for Relative Rotation
  /// </summary>
  internal class RotateJunctionRelative : RotateJunction
  {
    public RotateJunctionRelative()
      : base(true)
    { }
  }
}
