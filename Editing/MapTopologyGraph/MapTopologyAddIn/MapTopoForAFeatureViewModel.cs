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
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.Topology;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Catalog;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace MapTopologyAddIn
{
  /// <summary>
  /// This custom dockpane allows you to find the topologically connected features of a particular feature you
  /// select on the map. It also highlights the connected features on the map and displays their feature class name 
  /// and object ID in a list box in the pane. If you click on any of the list box items on the pane, it will
  /// flash that particular feature on the map. The overlay will be cleared when you close the pane.
  /// </summary>
  internal class MapTopoForAFeatureViewModel : DockPane
  {
    #region Private Properties
    private const string _dockPaneID = "MapTopologyAddIn_MapTopoForAFeature";

    /// <summary>
    /// used to lock collections for use by multiple threads
    /// </summary>
    private readonly object _lockLinkedFeaturesCollections = new object();


    /// <summary>
    /// UI lists, readonly collections, and properties
    /// </summary>
    private readonly ObservableCollection<FeatureInfo> _listOfLinkedFeatures = new ObservableCollection<FeatureInfo>();

    private readonly ReadOnlyObservableCollection<FeatureInfo> _readOnlyListOfLinkedFeatures;

    private FeatureInfo _selectedLinkedFeature;

    List<IDisposable> snapshot = new List<IDisposable>();
    private static System.IDisposable _overlayObject = null;
    private CIMPointSymbol _symbol;
    private CIMLineSymbol _symbolLine;
    private CIMSymbolReference _symbolReference;
    private CIMSymbolReference _symbolReferenceLine;
    List<string> layerNames = new List<string>();
    IEnumerable<FeatureLayer> pLayers;

    #endregion

    #region Public Properties
    MapView mapView = MapView.Active;
    /// <summary>
    /// Our List of Linked Features which is bound to our Dockpane XAML
    /// </summary>
    public ReadOnlyObservableCollection<FeatureInfo> ListOfLinkedfeatures => _readOnlyListOfLinkedFeatures;
    /// <summary>
    /// This is where we store the feature selected from the grid on the pane
    /// </summary>
    public FeatureInfo SelectedLinkedFeature
    {
      get { return _selectedLinkedFeature; }
      set
      {
        Utils.StartOnUIThread(() =>
        {
          SetProperty(ref _selectedLinkedFeature, value, () => SelectedLinkedFeature);
          if (_selectedLinkedFeature != null)
          {
            //Flash the feature selected in the list box in the dock pane
            BasicFeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(l => l.Name.Equals(_selectedLinkedFeature.FeatureClassName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            MapView.Active.FlashFeature(layer, _selectedLinkedFeature.ObjectID);
          }
        });
      }
    }

    #endregion

    protected MapTopoForAFeatureViewModel()
    {
      //Setup the lists and sync between background and UI
      _readOnlyListOfLinkedFeatures = new ReadOnlyObservableCollection<FeatureInfo>(_listOfLinkedFeatures);
      BindingOperations.EnableCollectionSynchronization(_readOnlyListOfLinkedFeatures, _lockLinkedFeaturesCollections);
      pLayers = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(l => l.ShapeType == esriGeometryType.esriGeometryPolygon || l.ShapeType == esriGeometryType.esriGeometryPolyline || l.ShapeType == esriGeometryType.esriGeometryPoint);
      if (pLayers != null)
      {
        foreach (var l in pLayers)
          layerNames.Add(l.Name);
      }
    }

    #region Overrides
    /// <summary>
    /// Override to implement custom initialization code for this dockpane
    /// </summary>
    /// <returns></returns>
    protected override Task InitializeAsync()
    {
      //Subscribe to the selection changed event.
      MapSelectionChangedEvent.Subscribe(FindLinkedFeatures);
      return base.InitializeAsync();
    }

    protected override Task UninitializeAsync()
    {
      MapSelectionChangedEvent.Unsubscribe(FindLinkedFeatures);
      return base.UninitializeAsync();
    }
    #endregion

    /// <summary>
    /// Show the DockPane.
    /// </summary>
    internal static void Show()
    {
      DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
      if (pane == null)
        return;
      pane.Activate();
    }

    /// <summary>
    /// When the dock pane is hidden (when it's closed), the graphics overlay is cleared too.
    /// </summary>
    protected override void OnHidden()
    {
      foreach (var graphic in snapshot)
        graphic.Dispose();
      snapshot.Clear();
      UninitializeAsync();
    }

    /// <summary>
    /// Text shown near the top of the DockPane.
    /// </summary>
    private string _heading = "Topologically Connected Features";
    public string Heading
    {
      get { return _heading; }
      set
      {
        SetProperty(ref _heading, value, () => Heading);
      }
    }

    /// <summary>
    /// Called after the feature selection changed.
    /// Method for retrieving map items in the project.
    /// </summary>
    private async void FindLinkedFeatures(MapSelectionChangedEventArgs args)
    {
      await QueuedTask.Run(() =>
      {
        //Clear the collections
        _listOfLinkedFeatures.Clear();

        //Clearing the currently drawn overlay on the map
        foreach (var graphic in snapshot)
          graphic.Dispose();
        snapshot.Clear();

        Feature selectedFeature = null;
        //Gets the feature that is currently selected in the map
        var selectedFeat = MapView.Active.Map.GetSelection().ToDictionary().Where(kvp => layerNames.Contains(kvp.Key.ToString())).FirstOrDefault();
        if (selectedFeat.Value == null)
          return;
        var layer = pLayers.Where(l => l.Name.Equals(selectedFeat.Key.ToString(), StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

        QueryFilter queryFilter = new QueryFilter()
        {
          ObjectIDs = selectedFeat.Value
        };

        using (RowCursor cursor = layer.Search(queryFilter))
        {
          System.Diagnostics.Debug.Assert(cursor.MoveNext());
          selectedFeature = (Feature)cursor.Current;
        }

        if (selectedFeature != null)
        {
          //Add Overlay showing all the connected topology nodes and for that selected feature
          if (_symbol == null)
          {
            //Construct point and line symbols
            _symbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.RedRGB, 10.0, SimpleMarkerStyle.Circle);
            _symbolLine = SymbolFactory.Instance.ConstructLineSymbol(ColorFactory.Instance.RedRGB, 5.0);
            //Get symbol reference from the symbol 
            _symbolReference = _symbol.MakeSymbolReference();
            _symbolReferenceLine = _symbolLine.MakeSymbolReference();
          }

          MapView.Active.BuildMapTopologyGraph<TopologyDefinition>(topologyGraph =>
          {
            var nodes = topologyGraph.GetNodes(selectedFeature); //Topology nodes via that feature

            foreach (TopologyNode node in nodes)
            {
              var edges = node.GetEdges();

              var parent = node.GetParentFeatures();
              foreach (var p in parent)
              {
                //Skipping the currently selected feature so as not to list it in the pane
                if (p.FeatureClassName.Equals(selectedFeat.Key.ToString()) && p.ObjectID.Equals(selectedFeature.GetObjectID()))
                  continue;
                if (!_listOfLinkedFeatures.Contains(p))
                  _listOfLinkedFeatures.Add(p);
              }

              foreach (TopologyEdge edge in edges)
              {
                var shape = edge.GetShape();
                _overlayObject = MapView.Active.AddOverlay(edge.GetShape() as Polyline, _symbolReferenceLine);
                snapshot.Add(_overlayObject);
              }
              var nodeShape = node.GetShape();
              _overlayObject = MapView.Active.AddOverlay(node.GetShape() as MapPoint, _symbolReference);
              snapshot.Add(_overlayObject);
            }
          });
          System.Diagnostics.Debug.WriteLine($"Number of topologically connected features are:  {_listOfLinkedFeatures.Count}.");
        }
      });
    }
  }

  /// <summary>
  /// Button implementation to show the custom DockPane.
  /// </summary>
	internal class MapTopoForAFeature_ShowButton : Button
  {
    protected override void OnClick()
    {
      MapTopoForAFeatureViewModel.Show();
    }
  }
}
