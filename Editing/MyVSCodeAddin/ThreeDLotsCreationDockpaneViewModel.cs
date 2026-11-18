using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Events;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyVSCodeAddin
{
  /// <summary>Supplies parcel and resolution state for the 3D Lots Creation dockpane.</summary>
  internal class ThreeDLotsCreationDockpaneViewModel : DockPane
  {
    // This lookup ID must exactly match the dockPane declaration in Config.daml.
    private const string DockpaneId = "MyVSCodeAddin_ThreeDLotsCreationDockpane";
    private const string LotsLayerName = "Lots";
    private const string LotMultipatchLayerName = "Lot MultiPatch";
    private const string NoParcelSelectedMessage = "select a parcel lot";
    private const int DefaultGridResolution = 10;
    private const double ExtrusionHeight = 10.0;

    private SubscriptionToken _mapSelectionChangedToken;
    private string _selectedParcelId = NoParcelSelectedMessage;
    private Polygon _selectedGeometry;
    private ResolutionOption _selectedResolution;

    protected ThreeDLotsCreationDockpaneViewModel()
    {
      ResolutionOptions = Enumerable.Range(0, 11)
        .Select(index =>
        {
          int value = index * 10;
          string label = value switch
          {
            0 => "0 - rough",
            100 => "100 - fine",
            _ => value.ToString()
          };

          return new ResolutionOption(value, label);
        })
        .ToArray();

      _selectedResolution = ResolutionOptions[1];
      Create3DLotCommand = new RelayCommand(Create3DLot);
    }

    internal static void Show()
    {
      DockPane pane = FrameworkApplication.DockPaneManager.Find(DockpaneId);
      pane?.Activate();
    }

    public string SelectedParcelId
    {
      get => _selectedParcelId;
      set => SetProperty(ref _selectedParcelId, value);
    }

    public Polygon SelectedGeometry
    {
      get => _selectedGeometry;
      private set => SetProperty(ref _selectedGeometry, value);
    }

    public IReadOnlyList<ResolutionOption> ResolutionOptions { get; }

    public ResolutionOption SelectedResolution
    {
      get => _selectedResolution;
      set => SetProperty(ref _selectedResolution, value);
    }

    public ICommand Create3DLotCommand { get; }

    protected override void OnShow(bool isVisible)
    {
      if (!isVisible || _mapSelectionChangedToken != null)
        return;

      _mapSelectionChangedToken = MapSelectionChangedEvent.Subscribe(OnMapSelectionChanged);
      _ = UpdateSelectedParcelAsync(MapView.Active?.Map);
    }

    protected override void OnHidden()
    {
      if (_mapSelectionChangedToken == null)
        return;

      MapSelectionChangedEvent.Unsubscribe(_mapSelectionChangedToken);
      _mapSelectionChangedToken = null;
    }

    private void OnMapSelectionChanged(MapSelectionChangedEventArgs args)
    {
      _ = UpdateSelectedParcelAsync(args.Map);
    }

    private async Task UpdateSelectedParcelAsync(Map map)
    {
      ParcelSelection selectedParcel = null;

      try
      {
        if (map != null)
        {
          // Layer selection, row access, and geometry cloning require the MCT.
          selectedParcel = await QueuedTask.Run(() => ReadFirstSelectedParcel(map));
        }
      }
      catch (Exception exception)
      {
        System.Diagnostics.Debug.WriteLine(exception);
      }

      System.Windows.Application.Current.Dispatcher.Invoke(() =>
      {
        SelectedGeometry = selectedParcel?.Geometry;
        SelectedParcelId = selectedParcel?.Name ?? NoParcelSelectedMessage;
      });
    }

    private static ParcelSelection ReadFirstSelectedParcel(Map map)
    {
      FeatureLayer lotsLayer = map.GetLayersAsFlattenedList()
        .OfType<FeatureLayer>()
        .FirstOrDefault(layer => string.Equals(layer.Name, LotsLayerName, StringComparison.Ordinal));

      if (lotsLayer == null)
        return null;

      using Selection selection = lotsLayer.GetSelection();
      IReadOnlyList<long> objectIds = selection.GetObjectIDs();
      if (objectIds.Count == 0)
        return null;

      QueryFilter queryFilter = new()
      {
        ObjectIDs = new[] { objectIds[0] },
        SubFields = "Name,Shape"
      };

      using RowCursor rows = lotsLayer.Search(queryFilter);
      if (!rows.MoveNext())
        return null;

      using Row row = rows.Current;
      string parcelName = Convert.ToString(row["Name"]);
      Polygon geometry = (row["Shape"] as Polygon)?.Clone() as Polygon;
      return new ParcelSelection(parcelName, geometry);
    }

    private async void Create3DLot()
    {
      Polygon parcelGeometry = SelectedGeometry?.Clone() as Polygon;
      if (parcelGeometry == null || parcelGeometry.IsEmpty)
      {
        MessageBox.Show("Select a parcel lot before creating a 3D lot.", "Create 3D Lot");
        return;
      }

      Map activeMap = MapView.Active?.Map;
      if (activeMap == null)
      {
        MessageBox.Show("An active map is required to create a 3D lot.", "Create 3D Lot");
        return;
      }

      int gridResolution = SelectedResolution?.Value ?? DefaultGridResolution;
      gridResolution = Math.Max(1, gridResolution);

      try
      {
        CreateLotResult result = await QueuedTask.Run(
          () => Create3DLotOnMctAsync(activeMap, parcelGeometry, gridResolution));

        if (!result.IsSuccess)
          MessageBox.Show(result.Message, "Create 3D Lot");
      }
      catch (Exception exception)
      {
        MessageBox.Show($"The 3D lot could not be created. {exception.Message}", "Create 3D Lot");
      }
    }

    private static async Task<CreateLotResult> Create3DLotOnMctAsync(
      Map map,
      Polygon parcelGeometry,
      int gridResolution)
    {
      FeatureLayer targetLayer = map.GetLayersAsFlattenedList()
        .OfType<FeatureLayer>()
        .FirstOrDefault(layer => string.Equals(
          layer.Name,
          LotMultipatchLayerName,
          StringComparison.Ordinal));

      if (targetLayer == null)
        return CreateLotResult.Failure("The 'Lot MultiPatch' feature layer was not found in the active map.");

      using FeatureClass targetFeatureClass = targetLayer.GetFeatureClass();
      using FeatureClassDefinition targetDefinition = targetFeatureClass.GetDefinition();
      if (targetDefinition.GetShapeType() != GeometryType.Multipatch)
      {
        return CreateLotResult.Failure(
          "The 'Lot MultiPatch' layer does not use multipatch geometry.");
      }

      Envelope parcelExtent = parcelGeometry.Extent;
      double cellWidth = parcelExtent.Width / gridResolution;
      double cellHeight = parcelExtent.Height / gridResolution;
      if (cellWidth <= 0 || cellHeight <= 0)
        return CreateLotResult.Failure("The selected parcel geometry has an invalid extent.");

      MultipatchBuilderEx multipatchBuilder = null;
      for (int xIndex = 0; xIndex < gridResolution; xIndex++)
      {
        double xMin = parcelExtent.XMin + (xIndex * cellWidth);
        double xMax = xIndex == gridResolution - 1 ? parcelExtent.XMax : xMin + cellWidth;

        for (int yIndex = 0; yIndex < gridResolution; yIndex++)
        {
          double yMin = parcelExtent.YMin + (yIndex * cellHeight);
          double yMax = yIndex == gridResolution - 1 ? parcelExtent.YMax : yMin + cellHeight;
          Envelope cellEnvelope = EnvelopeBuilderEx.CreateEnvelope(
            xMin,
            yMin,
            xMax,
            yMax,
            parcelGeometry.SpatialReference);

          Polygon clippedPolygon = GeometryEngine.Instance.Clip(parcelGeometry, cellEnvelope) as Polygon;
          if (clippedPolygon == null || clippedPolygon.IsEmpty)
            continue;

          SurfaceZsResult surfaceResult = await map.GetZsFromSurfaceAsync(clippedPolygon);
          Polygon surfacePolygon = surfaceResult.Geometry as Polygon;
          if (surfacePolygon == null || surfacePolygon.IsEmpty)
          {
            return CreateLotResult.Failure(
              $"Ground-surface Z values could not be calculated ({surfaceResult.Status}).");
          }

          Multipatch cellMultipatch = GeometryEngine.Instance.ConstructMultipatchExtrude(
            surfacePolygon,
            ExtrusionHeight);

          if (multipatchBuilder == null)
          {
            multipatchBuilder = new MultipatchBuilderEx(cellMultipatch);
            continue;
          }

          MultipatchBuilderEx cellBuilder = new(cellMultipatch);
          foreach (Patch patch in cellBuilder.Patches)
            multipatchBuilder.Patches.Add(patch);
        }
      }

      if (multipatchBuilder == null)
        return CreateLotResult.Failure("The selected parcel did not produce any 3D geometry.");

      multipatchBuilder.SynchronizeAttributeAwareness();
      Multipatch outputMultipatch = multipatchBuilder.ToGeometry();
      SpatialReference targetSpatialReference = targetDefinition.GetSpatialReference();
      if (targetSpatialReference != null)
      {
        outputMultipatch = GeometryEngine.Instance.Project(
          outputMultipatch,
          targetSpatialReference) as Multipatch;
      }

      if (outputMultipatch == null || outputMultipatch.IsEmpty)
        return CreateLotResult.Failure("The resulting multipatch geometry is empty.");

      EditOperation createOperation = new()
      {
        Name = "Create 3D Lot",
        SelectNewFeatures = true
      };
      createOperation.Create(targetLayer, outputMultipatch);

      bool wasCreated = await createOperation.ExecuteAsync();
      return wasCreated
        ? CreateLotResult.Success()
        : CreateLotResult.Failure(createOperation.ErrorMessage);
    }
  }

  internal sealed record CreateLotResult(bool IsSuccess, string Message)
  {
    internal static CreateLotResult Success() => new(true, string.Empty);

    internal static CreateLotResult Failure(string message) => new(false, message);
  }

  internal sealed record ParcelSelection(string Name, Polygon Geometry);

  internal sealed record ResolutionOption(int Value, string Label);

  /// <summary>Provides the ribbon entry point for the 3D Lots Creation dockpane.</summary>
  internal sealed class ThreeDLotsCreationDockpane_ShowButton : Button
  {
    protected override void OnClick() => ThreeDLotsCreationDockpaneViewModel.Show();
  }
}