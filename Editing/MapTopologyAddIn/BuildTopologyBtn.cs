//   Copyright 2022 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using ArcGIS.Core.CIM;
using ArcGIS.Core.Data.Topology;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MapTopologyAddIn
{
  /// <summary>
  /// This toggle button allows you to build the map topology graph for the current map view extent.
  /// It also highlights all the nodes and edges that are part of that graph and report the number of nodes
  /// and edges present in the graph.
  /// </summary>
  internal class BuildTopologyBtn : Button
  {
    List<IDisposable> snapshot = new List<IDisposable>();
    private static System.IDisposable _overlayObject = null;
    private CIMPointSymbol _symbol;
    private CIMLineSymbol _symbolLine;
    private CIMSymbolReference _symbolReference;
    private CIMSymbolReference _symbolReferenceLine;
    private IReadOnlyList<TopologyNode> topologyGraphNodes;
    private IReadOnlyList<TopologyEdge> topologyGraphEdges;

    protected override void OnClick()
    {
      //To act like a toggle button. Every alternate time, it either builds the graph
      //for the current view or it clears the overlay.
      this.IsChecked = this.IsChecked ? false : true;
      bool btnChecked = this.IsChecked;
      if (btnChecked)
        BuildGraphWithActiveView();
      else
        ClearOverlay();
    }

    /// <summary>
    /// Function to clear the current overlay on the map.
    /// </summary>
    private void ClearOverlay()
    {
      //Clearing the currently drawn overlay on the map
      foreach (var graphic in snapshot)
        graphic.Dispose();
      snapshot.Clear();
    }

    /// <summary>
    /// This function builds the map topology graph of the current map view extent.
    /// </summary>
    /// <returns></returns>
    private async Task BuildGraphWithActiveView()
    {
      await QueuedTask.Run(() =>
      {
        ClearOverlay();

        //Build the map topology graph
        MapView.Active.BuildMapTopologyGraph<TopologyDefinition>(topologyGraph =>
        {
          //Getting the nodes and edges present in the graph
          topologyGraphNodes = topologyGraph.GetNodes();
          topologyGraphEdges = topologyGraph.GetEdges();

          if (_symbol == null)
          {
            //Construct point and line symbols
            _symbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.BlueRGB, 6.0, SimpleMarkerStyle.Circle);
            _symbolLine = SymbolFactory.Instance.ConstructLineSymbol(ColorFactory.Instance.BlueRGB, 3.0);
            //Get symbol references from the symbols 
            _symbolReference = _symbol.MakeSymbolReference();
            _symbolReferenceLine = _symbolLine.MakeSymbolReference();
          }

          //Draw the nodes and edges on the overlay to highlight them
          foreach (var node in topologyGraphNodes)
          {
            _overlayObject = MapView.Active.AddOverlay(node.GetShape() as MapPoint, _symbolReference);
            snapshot.Add(_overlayObject);
          }
          foreach (var edge in topologyGraphEdges)
          {
            _overlayObject = MapView.Active.AddOverlay(edge.GetShape() as Polyline, _symbolReferenceLine);
            snapshot.Add(_overlayObject);
          }

          MessageBox.Show($"Number of topo graph nodes are:  {topologyGraphNodes.Count}.\n Number of topo graph edges are {topologyGraphEdges.Count}.", "Map Topology Info");
        });
      });
    }
  }
}
