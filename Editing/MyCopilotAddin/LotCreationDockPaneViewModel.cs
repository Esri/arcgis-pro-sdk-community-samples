/*

   Copyright 2026 Esri

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
// file: E:\Repos\WolfOfKauai\CopilotAddInTest\MyCopilotAddin\LotCreationDockPaneViewModel.cs
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyCopilotAddin
{
  internal class LotCreationDockPaneViewModel : DockPane
  {
    private const string _dockPaneID = "MyCopilotAddin_3DLotCreationDockPane";

    private string _parcelId;
    public string ParcelId
    {
      get => _parcelId;
      set => SetProperty(ref _parcelId, value, () => ParcelId);
    }

    // New private property for selected geometry
    private Polygon _selectedGeometry;
    private Polygon SelectedGeometry
    {
      get => _selectedGeometry;
      set => _selectedGeometry = value;
    }

    public ObservableCollection<int> Resolutions { get; } =
      new ObservableCollection<int> { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };

    private int _selectedResolution = 50;
    public int SelectedResolution
    {
      get => _selectedResolution;
      set => SetProperty(ref _selectedResolution, value, () => SelectedResolution);
    }

    public ICommand Create3DLotCommand { get; }

    public LotCreationDockPaneViewModel()
    {
      Create3DLotCommand = new RelayCommand(() => Create3DLot(), () => !string.IsNullOrEmpty(ParcelId));
      MapSelectionChangedEvent.Subscribe(OnMapSelectionChanged);
    }

    /// <summary>
    /// Creates a 3D representation of the selected parcel geometry and adds it as a multipatch feature to the "Lot
    /// MultiPatch" layer.
    /// </summary>
    /// <remarks>This method divides the selected parcel geometry into a grid of envelopes, clips the parcel
    /// geometry with each envelope,  and extrudes the resulting polygons to create a 3D multipatch. The multipatch is
    /// then added to the "Lot MultiPatch" feature layer  in the active map. If the "Lot MultiPatch" layer is not found,
    /// the operation is aborted.</remarks>
    private async void Create3DLot()
    {
      await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(async () =>
      {
        if (SelectedGeometry == null)
        {
          MessageBox.Show("No parcel geometry selected.");
          return;
        }

        // Build grid and create rings
        var cutEnvelopes = new List<Envelope>();
        var env = SelectedGeometry.Extent;
        int res = SelectedResolution > 0 ? SelectedResolution : 10; // Avoid division by zero
        double dx = (env.XMax - env.XMin) / res;
        double dy = (env.YMax - env.YMin) / res;
        var sr = SelectedGeometry.SpatialReference;

        for (int i = 0; i < res; i++)
        {
          for (int j = 0; j < res; j++)
          {
            var x0 = env.XMin + i * dx;
            var y0 = env.YMin + j * dy;
            var x1 = x0 + dx;
            var y1 = y0 + dy;

            var cutEnvelope = EnvelopeBuilderEx.CreateEnvelope(x0, y0, x1, y1, sr);

            // Only add ring if it intersects the parcel
            if (GeometryEngine.Instance.Intersects(cutEnvelope, SelectedGeometry))
              cutEnvelopes.Add(cutEnvelope);
          }
        }
        // Material for the multipatch
        var materialBlue = new BasicMaterial
        {
          Color = System.Windows.Media.Colors.LightSteelBlue,
          EdgeColor = System.Windows.Media.Colors.LightSteelBlue,
          EdgeWidth = 0
        };
        var extructionHeight = 5;
        // Create multi-patch from clipped rings
        var multipatchBuilder = new MultipatchBuilderEx();
        List<Patch> patchList = new List<Patch>();
        bool isFirst = true;
        var mapView = MapView.Active;
        foreach (var cutEnvelope in cutEnvelopes)
        {
          // Clip the parcel geometry with the ring
          var clipped = GeometryEngine.Instance.Intersection(SelectedGeometry, cutEnvelope) as Polygon;
          if (clipped == null || clipped.IsEmpty)
            continue;

          // Set Z values from the surface using the active map view
          var clippedWithZ = await mapView.Map.GetZsFromSurfaceAsync(clipped);
          if (clippedWithZ.Status == SurfaceZsResultStatus.Ok)
          {
            clipped = GeometryEngine.Instance.Move(clippedWithZ.Geometry, 0, 0, extructionHeight) as Polygon;
          }
          var patchType = isFirst ? PatchType.FirstRing : PatchType.Ring;
          isFirst = false;
          var patch = multipatchBuilder.MakePatch(patchType);
          patch.InsertPoints(0, clipped.Points);
          patch.Material = materialBlue;
          patchList.Add(patch);
        }
        multipatchBuilder.Patches = patchList;

        // Get the existing "Lot MultiPatch" feature layer
        var map = mapView?.Map;
        var lotMultiPatchLayer = map?.Layers.OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "Lot MultiPatch");
        if (lotMultiPatchLayer == null)
        {
          MessageBox.Show("'Lot MultiPatch' layer not found. Please add it to the map.");
          return;
        }
        var lotMultiPatchFeatureClass = lotMultiPatchLayer.GetTable() as FeatureClass;

        // Add the multi-patch feature, setting "Shape" and "Name" fields
        var editOp = new EditOperation
        {
          Name = "Create 3D Lot"
        };
        var attributes = new Dictionary<string, object>
        {
          { lotMultiPatchFeatureClass.GetDefinition().GetShapeField(), multipatchBuilder.ToGeometry() },
          { "Name", ParcelId }
        };
        editOp.Create(lotMultiPatchFeatureClass, attributes);
        if (!editOp.Execute())
        {
          MessageBox.Show("Failed to create 3D lot feature.");
        }
        else
        {
          MessageBox.Show("3D lot created successfully.");
        }
      });
    }

    private void OnMapSelectionChanged(MapSelectionChangedEventArgs args)
    {
      _ = UpdateParcelIdAsync();
    }

    private async Task UpdateParcelIdAsync()
    {
      var mapView = MapView.Active;
      if (mapView == null)
      {
        ParcelId = string.Empty;
        SelectedGeometry = null;
        return;
      }

      await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
      {
        var lotLayer = mapView.Map?.Layers.OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "Lots");
        if (lotLayer == null)
        {
          ParcelId = string.Empty;
          SelectedGeometry = null;
          return;
        }

        var selection = lotLayer.GetSelection();
        var selectedOids = selection?.GetObjectIDs();
        if (selectedOids != null && selectedOids.Count > 0)
        {
          var objectId = selectedOids.First();
          var table = lotLayer.GetTable();
          var queryFilter = new QueryFilter
          {
            ObjectIDs = new List<long> { objectId },
            SubFields = "*"
          };
          using (var rowCursor = table.Search(queryFilter, false))
          {
            if (rowCursor.MoveNext())
            {
              using (var featureRow = rowCursor.Current as Feature)
              {
                if (featureRow != null)
                {
                  var nameValue = featureRow["Name"]?.ToString();
                  ParcelId = nameValue ?? string.Empty;

                  // Store a clone of the geometry in SelectedGeometry
                  var shape = featureRow["Shape"] as Geometry;
                  if (shape is Polygon polygon)
                    SelectedGeometry = polygon.Clone() as Polygon;
                  else
                    SelectedGeometry = null;
                  return;
                }
              }
            }
          }
        }
        ParcelId = string.Empty;
        SelectedGeometry = null;
      });
    }

    /// <summary>
    /// Call this to show the DockPane
    /// </summary>
    internal static void Show()
    {
      var pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
      if (pane != null)
        pane.Activate();
    }
  }
}