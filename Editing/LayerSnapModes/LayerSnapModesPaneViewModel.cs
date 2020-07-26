//Copyright 2020 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace LayerSnapModes
{
  internal class LayerSnapModesPaneViewModel : DockPane
  {
    private const string _dockPaneID = "LayerSnapModes_LayerSnapModesPane";
    private static ObservableCollection<SnapAgent> _snapCollection = new ObservableCollection<SnapAgent>();

    protected LayerSnapModesPaneViewModel()
    {
      //subscribe to events to populate snap layer list when the map changes, layers added/removed
      ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);
      LayersAddedEvent.Subscribe(onLayersAddRem);
      LayersRemovedEvent.Subscribe(onLayersAddRem);
    }

    private void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs obj)
    {
      if (obj.IncomingView == null)
        return;
      //populate the snap list with layers in the incoming active map
      PopulateSnapList();
    }

    private void onLayersAddRem(LayerEventsArgs obj)
    {
      //regenerate snaplist when layers are added or removed
      //run on UI Thread to sync layersadded event (which runs on background)
      System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() => { PopulateSnapList(); }));
    }
    /// <summary>
    /// Show the DockPane.
    /// </summary>
    internal static void Show()
    {
      DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
      if (pane == null)
        return;
      PopulateSnapList();
      pane.Activate();
    }

    /// <summary>
    /// Text shown near the top of the DockPane.
    /// </summary>
    private string _heading = "Layer Snap Modes";
    public string Heading
    {
      get => _heading;
      set => SetProperty(ref _heading, value);
    }
    /// <summary>
    /// Collection bound to datagrid
    /// </summary>
    public ObservableCollection<SnapAgent> SnapList
    {
      get => _snapCollection;
      set => SetProperty(ref _snapCollection, value);
    }

    public static void PopulateSnapList()
    {
      //populate the snaplist for the active map with feature layers
      _snapCollection.Clear();
      var layerlist = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>();
      foreach (var layer in layerlist)
      {
        var lsm = Snapping.GetLayerSnapModes(layer);
        _snapCollection.Add(new SnapAgent(layer, lsm.Vertex, lsm.Edge, lsm.End));
      }
    }

    public class SnapAgent : ViewModelBase
    {
      public SnapAgent(Layer layer, bool vertex, bool edge, bool end) { this.layer = layer; _vertex = vertex; _edge = edge; _end = end; }

      private bool _vertex;
      private bool _edge;
      private bool _end;
      public Layer layer { get; set; }
      public bool Vertex
      {
        get => _vertex;
        set
        {
          SetProperty(ref _vertex, value);
          Snapping.SetLayerSnapModes(layer, SnapMode.Vertex, value);

          //alternate method
          //var lsm = Snapping.GetLayerSnapModes(layer);
          //lsm.Vertex = value;
          //Snapping.SetLayerSnapModes(layer, lsm);
        }
      }
      public bool Edge
      {
        get => _edge;
        set
        {
          SetProperty(ref _edge, value);
          Snapping.SetLayerSnapModes(layer, SnapMode.Edge, value);
        }
      }
      public bool End
      {
        get => _end;
        set
        {
          SetProperty(ref _end, value);
          Snapping.SetLayerSnapModes(layer, SnapMode.End, value);
        }
      }
    }
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class LayerSnapModesPane_ShowButton : Button
  {
    protected override void OnClick()
    {
      LayerSnapModesPaneViewModel.Show();
    }
  }
}
