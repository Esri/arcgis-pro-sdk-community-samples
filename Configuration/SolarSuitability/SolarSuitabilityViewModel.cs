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
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Geoprocessing;
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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Xml.Linq;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;

namespace SolarSuitability
{
  internal class SolarSuitabilityViewModel : DockPane
  {
    private const string _dockPaneID = "SolarSuitability_SolarSuitability";

    protected SolarSuitabilityViewModel()
    {
      DistrictName = "Redlands Packing House district";

      // Steps 1-4 checked by default
      //Step1Checked = true;
      MaxRoofAngle = 45;
      //Step2Checked = true;
      FromDate = new DateTime(2026, 3, 1);
      ToDate = new DateTime(2026, 3, 31);
      //Step3Checked = true;
      SolarPotential = "Solar Potential Analysis complete";
      //Step4Checked = true;
      //Step7Checked = false; // needed for tri-state toggle button example
      SymbologySources = new ObservableCollection<string>
            {
                "Irradiance",
                "Roof Area",
                "Tilt",
                "Annual Output"
            };
      SelectedSource = "Roof Area";
      ShowReportCommand = new RelayCommand(() => ShowReport());
      ResetCommand = new RelayCommand(() => Reset());
      CancelCommand = new RelayCommand(() => Cancel());
      LegendItems = new ObservableCollection<LegendItem>();
      LegendItems.Add(new LegendItem { Color = "#77F5F500", Label = "< 11 MWh (Poor)" });
      LegendItems.Add(new LegendItem { Color = "#77FF5500", Label = "11 to 16 MWh (Good)" });
      LegendItems.Add(new LegendItem { Color = "#7700734C", Label = "> 16 MWh (Best)" });
    }
    private bool _reportToggle = false;
    protected override void OnShow(bool isVisible)
    {
      ////Is project name Solar Suitability Analysis?
      //var name = Project.Current?.Name;
      //if (string.Compare(name, "SolarEstimation", StringComparison.OrdinalIgnoreCase) != 0)
      //{
      //  MessageBox.Show("This dockpane is designed to work with the 'Solar Suitability Analysis' project. Please open that project to use this dockpane.");
      //  return;
      //}
      ////Is the RPHDBuildingFootprints layer and a DSM layer in the active map?
      //var layersNeeded = new List<string>() { "RPHDBuildingFootprints", "DSM" }; 
      //var layers = MapView.Active?.Map?.GetLayersAsFlattenedList();
      //var layersInMap = new List<string>();
      //foreach (var layer in layers)
      //{
      //  layersInMap.Add(layer.Name);
      //}
      ////are any of the "layersInMap" in the map named "RPHDBuildingFootprints" or "DSM" contained in the layersNeeded list?
      ////Please compare the two lists
      //  if (!layersInMap.Intersect(layersNeeded).Any())
      //  {
      //    MessageBox.Show($@"The active map needs the required data layers for this dockpane to work properly. Please add those layers and try again.");
      //    return;
      //}
    }
    private void ShowReport()
    {
      _reportToggle = !_reportToggle;

      if (_reportToggle)
        FrameworkApplication.SetCurrentToolAsync("SolarSuitability_MapToolTargetPropertyTrigger");
      else
      {
        FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");

        // find the BuildingFootprints feature layer and clear the selection under the clicked point, then update the TargetPropertyInformationViewModel
        var map = MapView.Active.Map;
        if (map != null)
        {
          var buildingFootprintsLayer = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(lyr => lyr.Name == "Suitable_Buildings");

          // clear the selection and hide the TargetPropertyInformationViewModel
          if (buildingFootprintsLayer != null)
          {
            QueuedTask.Run(() => buildingFootprintsLayer.ClearSelection());
          }
        }
      }
    }

    /// <summary>
    /// The following checked states change every time the togglebutton was pressed 
    /// </summary>


    private bool _Step1Checked = false;
    public bool Step1Checked
    {
      get => _Step1Checked;
      set
      {
        SetProperty(ref _Step1Checked, value);
        Step1Completed = value;

      }
    }
    private bool _Step2Checked = false;
    public bool Step2Checked
    {
      get => _Step2Checked;
      set
      {
        SetProperty(ref _Step2Checked, value);
        Step2Completed = value;
      }
    }
    private bool _Step3Checked = false;
    public bool Step3Checked
    {
      get => _Step3Checked;
      set
      {
        SetProperty(ref _Step3Checked, value);
        Step3Completed = value;
      }
    }
    private bool _Step4Checked = false;
    public bool Step4Checked
    {
      get => _Step4Checked;
      set
      {
        SetProperty(ref _Step4Checked, value);
        SolarPotential = value ? "Solar Potential Analysis complete" : "Calculate Solar Potential";
        Step4Completed = value;
      }
    }
    private RelayCommand _ShowLayerCommand;
    public RelayCommand ShowLayerCommand
    {
      get
      {
        return new RelayCommand(() =>
        {
          var layer = MapView.Active.Map.Layers.FirstOrDefault(l => l.Name == "Suitable_Buildings");
          if (layer != null)
          {
            QueuedTask.Run(() =>
            {
              layer.SetVisibility(true);
            });
          }
        });
      }
    }


    private bool _Step5Checked = false;
    public bool Step5Checked
    {
      get => _Step5Checked;
      set
      {
        SetProperty(ref _Step5Checked, value);
        Step5Completed = value;
      }
    }

    private static void ApplySymbologyFromTemplate(string layerXFile)
    {
      var featureLayerToSymbolize = MapView.Active?.Map?.Layers.FirstOrDefault(n => n.Name == "RPHDBuildingFootprints") as FeatureLayer;
      if (featureLayerToSymbolize == null)
        return;
      QueuedTask.Run(() =>
      {
        
        if (!File.Exists(layerXFile)) return;
        var lyrDocFromLyrxFile = new LayerDocument(layerXFile);
        var cimLyrDoc = lyrDocFromLyrxFile.GetCIMLayerDocument();

        //Get the renderer from the layer file
        var rendererFromLayerFile = ((CIMFeatureLayer)cimLyrDoc.LayerDefinitions[0]).Renderer as CIMClassBreaksRenderer;
        if (rendererFromLayerFile == null) return;
        //Apply the renderer to the feature layer
        //Note: If working with a raster layer, use the SetColorizer method.
        featureLayerToSymbolize?.SetRenderer(rendererFromLayerFile);
      });
    }

    private bool _Step6Checked = false;
    private uint _msgCnt = 0;
    public bool Step6Checked
    {
      get => _Step6Checked;
      set
      {
        SetProperty(ref _Step6Checked, value);
        Step6Completed = value;
        if (Step6Completed)
        {
          // project error notification with details
          string msg = @"Redlands Packing House District estimation complete - Building #2 at 110 Stuart St is a Go";
          string msgDetails = $@"Sales team, the Redlands Packing House District estimation is complete - Building #2 at 110 Stuart St and is a Go. \nPlease engage with client.";
          NotificationItem notification = new($@"id_project_error_details{_msgCnt++}", true,
                        $"Notification sent to Sales team with message: {msg}", NotificationType.Confirmation,
                        details: msgDetails);
          NotificationManager.AddNotification(notification);

          var plugin = FrameworkApplication.GetPlugInWrapper("esri_core_notificationButton");
          if ((ICommand)plugin != null)
          {
            ((ICommand)plugin).Execute(null);
          }
        }
      }
    }
    private bool? _Step7Checked = false;
    public bool? Step7Checked
    {
      get => _Step7Checked;
      set
      {
        SetProperty(ref _Step7Checked, value);
        Step7Completed = value;
        // the values of the tristate button are: false → true → null →
        //Show the spinner on true otherwise hide it.
        ShowSpinner = value == true ? Visibility.Visible : Visibility.Collapsed;
        //OpenBlenderWithGltf();

      }
    }
    private Visibility _showSpinner = Visibility.Collapsed;
    public Visibility ShowSpinner
    {
      get => _showSpinner;
      set { SetProperty(ref _showSpinner, value); }
    }


    private string _DistrictName;
    public string DistrictName
    {
      get => _DistrictName;
      set
      {
        SetProperty(ref _DistrictName, value);
      }
    }

    private string _createSolarRadiationLayer = "Create a solar radiation layer";
    public string CreateSolarRadiationLayer
    {
      get => _createSolarRadiationLayer;
      set { SetProperty(ref _createSolarRadiationLayer, value); }
    }

    private string _SolarPotential = "Calculate Solar Potential";
    public string SolarPotential
    {
      get => _SolarPotential;
      set { SetProperty(ref _SolarPotential, value); }
    }

    private decimal _radiation = 100;
    public decimal Radiation
    {
      get => _radiation;
      set { SetProperty(ref _radiation, value); }
    }
    private decimal _MaxRoofAngle;
    public decimal MaxRoofAngle
    {
      get => _MaxRoofAngle;
      set { SetProperty(ref _MaxRoofAngle, value); }
    }

    private string _Aspect;
    public string Aspect
    {
      get => _Aspect;
      set { SetProperty(ref _Aspect, value); }
    }

    private DateTime? _FromDate;
    public DateTime? FromDate
    {
      get => _FromDate;
      set { SetProperty(ref _FromDate, value); }
    }

    private DateTime? _ToDate;
    public DateTime? ToDate
    {
      get => _ToDate;
      set { SetProperty(ref _ToDate, value); }
    }

    private ObservableCollection<string> _SymbologySources;
    public ObservableCollection<string> SymbologySources
    {
      get => _SymbologySources;
      set { SetProperty(ref _SymbologySources, value); }
    }

    private string _SelectedSource;
    public string SelectedSource
    {
      get => _SelectedSource;
      set { SetProperty(ref _SelectedSource, value); }
    }


    private bool _Step1Completed;
    public bool Step1Completed
    {
      get => _Step1Completed;
      set { SetProperty(ref _Step1Completed, value); }
    }

    private bool _Step2Completed;
    public bool Step2Completed
    {
      get => _Step2Completed;
      set { SetProperty(ref _Step2Completed, value); }
    }

    private bool _Step3Completed;
    public bool Step3Completed
    {
      get => _Step3Completed;
      set { SetProperty(ref _Step3Completed, value); }
    }

    private bool _Step4Completed;
    public bool Step4Completed
    {
      get => _Step4Completed;
      set { SetProperty(ref _Step4Completed, value); }
    }

    private bool _Step5Completed;
    public bool Step5Completed
    {
      get => _Step5Completed;
      set { SetProperty(ref _Step5Completed, value); }
    }

    private bool _Step6Completed;
    public bool Step6Completed
    {
      get => _Step6Completed;
      set { SetProperty(ref _Step6Completed, value); }
    }

    private bool? _Step7Completed;
    public bool? Step7Completed
    {
      get => _Step7Completed;
      set { SetProperty(ref _Step7Completed, value); }
    }

    private ObservableCollection<LegendItem> _legendItems;
    public ObservableCollection<LegendItem> LegendItems
    {
      get => _legendItems;
      set { SetProperty(ref _legendItems, value); }
    }

    public bool IsGo { get; set; }
    public bool IsNoGo { get; set; }

    public ICommand ShowResultsCommand { get; }
    public ICommand ShowReportCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand CancelCommand { get; }

    private void Reset() { }
    private void Cancel() { Hide(); }
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
    /// Text shown near the top of the DockPane.
    /// </summary>
    private string _heading = "My DockPane";
    public string Heading
    {
      get => _heading;
      set => SetProperty(ref _heading, value);
    }

    #region Step 1: Create Solar Radiation Layer
    private bool _step1HasExecuted;
    private async void ExecuteStep1()
    {
      if (_step1HasExecuted)
      {
        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Step 1 has already been executed. No need to run again for this example.", "Step 1", MessageBoxButton.OK, MessageBoxImage.Information);
        Step1Completed = true;
        return;
      }
      var activeMapView = MapView.Active;
      if (activeMapView == null)
      {
        return;
      }
      var map = MapView.Active.Map;
      if (map == null) return;
      //Get the RPHDBuildingFootprints layer's simple renderer to use in the ResetDemo function.
      var buildingFootprintsLayer = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(lyr => lyr.Name == "RPHDBuildingFootprints");
      await QueuedTask.Run(() =>
      {
        if (buildingFootprintsLayer != null)
        {

          Module1._buildingFootprintSimpleRenderer = buildingFootprintsLayer.GetRenderer() as CIMSimpleRenderer;
        }
      });
      string dataPath = @$"{Project.Current.HomeFolderPath}";

      string inRasterDS = System.IO.Path.Combine(dataPath, "RedlandsPackingHouseDistrictDSM.tiff");
      string inMaskDS = System.IO.Path.Combine(dataPath, "SolarEstimation.gdb\\RPHDBuildingFootprints");
      // Place parameters into an array
      var parameters = Geoprocessing.MakeValueArray(
          inRasterDS, //DSM input, in_surface_raster
          "Step1_Solar_Rad", //out_raster
          FromDate, //start date
          ToDate, //end date
                  //new DateTime(2025, 1, 1), //start date
                  //new DateTime(2025, 12, 31), //end date
          inMaskDS, //building footprint mask, in_analysis_mask
          null, //in_slope_raster
          null, //in_aspect_raster
          null, //out_direct_radiation_raster
          null, //out_diffuse_radiation_raster
          null, //out_duration_raster
          "Pacific_Standard_Time", //timezone
          "NOT_ADJUSTED_FOR_DST",//adjust_DST
          "NO_INTERVAL", //use_time_interval
          "DAY", //interval_unit
          14, //interval
          "0.14 Meters", //neighborhood_distance
          "FIXED_NEIGHBORHOOD", //use_adaptive_neighborhood
          "UNIFORM_SKY", //diffuse_model_type
          0.3, //diffuse_proportion
          0.5, //transmittivity
          "GPU then CPU", //GPU - CPU
          null //sunmap_grid_level
          );

      // Place environment settings in an array, in this case, OK to over-write
      var extent = MapView.Active?.Extent;
      var environments = Geoprocessing.MakeEnvironmentArray(overwriteoutput: true, extent: extent, cellSize: .5);
      var progDlg = new ProgressDialog($@"Step 1: Creating Solar Radiation Layer", "Canceled", false);
      var progsrc = new CancelableProgressorSource(progDlg);
      // Execute Raster Solar Radiation tool   
      var gpResult = await Geoprocessing.ExecuteToolAsync("RasterSolarRadiation_sa", parameters, environments, progsrc.Progressor);

      if (gpResult.IsFailed)
      {
        Step1Checked = false; //leave the toggle button in unchecked state if the tool execution failed
        // display error messages if the tool fails, otherwise shows the default messages
        if (gpResult.Messages.Count() != 0)
        {
          Geoprocessing.ShowMessageBox(gpResult.Messages, progsrc.Message,
                                          gpResult.IsFailed ?
                                          GPMessageBoxStyle.Error : GPMessageBoxStyle.Default);
        }
        else
        {
          Step1Checked = false; //leave the toggle button in unchecked state if the tool execution is canceled
          MessageBox.Show($@"{progsrc.Message} failed with errorcode, check parameters.");
        }
        return;
      }

      if (progsrc.CancellationTokenSource.IsCancellationRequested)
      {
        MessageBox.Show("The operation was canceled.");
        return;
      }

      if (!gpResult.IsFailed)
      {
        _step1HasExecuted = true;
        Step1Checked = true; //toggle button is checked
        //Turn off the DSM layer
        var layersToHide = new List<string>(){
        "DSM", "RPHDBuildingFootprints"
        };
        await QueuedTask.Run(() =>
        {
          foreach (var layerName in layersToHide)
          {
            var layer = MapView.Active.Map.Layers.FirstOrDefault(l => l.Name == layerName);
            if (layer != null)
              layer.SetVisibility(false);
          }
          var buildingFootprintLyr = MapView.Active.Map.Layers.FirstOrDefault(l => l.Name == "RPHDBuildingFootprints");
          if (buildingFootprintLyr != null)
            MapView.Active.ZoomTo(buildingFootprintLyr);
          //Get the layerDoc for Solar_Rad layer and save it in a LayerDoc to use later.
          var solarRadLayer = MapView.Active.Map.Layers.FirstOrDefault(l => l.Name == "Step1_Solar_Rad");
          if (solarRadLayer != null)
          {
             _layerDoc = new LayerDocument(solarRadLayer);
          }
        });
      }
    }

    public ICommand CmdStep1 => new RelayCommand(() => ExecuteStep1(), () => true);
    #endregion

    #region Step 2: Create Slope and Aspect layers from the DSM
    private bool _step2HasExecuted;
    public ICommand CmdStep2 => new RelayCommand(() => ExecuteStep2(), () => true);
    private async void ExecuteStep2()
    {
      if (_step2HasExecuted)
      {
        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Step 2 has already been executed. No need to run again for this example.", "Step 2", MessageBoxButton.OK, MessageBoxImage.Information);
        Step2Completed = true;
        return;
      }
      var activeMapView = MapView.Active;
      if (activeMapView == null)
      {
        return;
      }
      var map = MapView.Active.Map;
      if (map == null) return;

      var extent = MapView.Active?.Extent;
      string dataPath = @$"{Project.Current.HomeFolderPath}";

      string in_raster = System.IO.Path.Combine(dataPath, "RedlandsPackingHouseDistrictDSM.tiff");
      // Running tool: Surface Parameters
      var toolName = "sa.SurfaceParameters";
      // Place environment settings in an array, in this case, OK to over-write
      var environments = Geoprocessing.MakeEnvironmentArray(overwriteoutput: true, extent: extent, cellSize: .5);

      #region Calculate the Slope layer from the DSM
      // Parameters Surface Parameters
      var parametersSlope = Geoprocessing.MakeValueArray(
                in_raster,
                "Step2_Slope_DSM", //out_raster
                "Slope", //parameter_type 
                "Quadratic", //local_surface_type
                "1 Meters", //neighborhood_distance
                null, //use_adaptive_neighborhood
                null, //z_unit = 
                "Degree", //output_slope_measurement
                null, //project_geodesic_azimuths
                null, //use_equatorial_aspect 
                in_raster //in_analysis_mask
                );

      var progDlgSlope = new ProgressDialog($@"Step 2: Creating Slope Layer from DSM", "Canceled", false);
      var progsrcSlope = new CancelableProgressorSource(progDlgSlope);
      // Execute Raster Solar Radiation tool   
      var gpResultSlope = await Geoprocessing.ExecuteToolAsync(toolName, parametersSlope, environments, progsrcSlope.Progressor);

      if (gpResultSlope.IsFailed)
      {
        Step2Checked = false; //leave the toggle button in unchecked state if the tool execution failed
        // display error messages if the tool fails, otherwise shows the default messages
        if (gpResultSlope.Messages.Count() != 0)
        {
          Geoprocessing.ShowMessageBox(gpResultSlope.Messages, progsrcSlope.Message,
                                          gpResultSlope.IsFailed ?
                                          GPMessageBoxStyle.Error : GPMessageBoxStyle.Default);
        }
        else
        {
          MessageBox.Show($@"Step 2: {progsrcSlope.Message} failed with errorcode, check parameters.");
        }
        return;
      }

      if (progsrcSlope.CancellationTokenSource.IsCancellationRequested)
      {
        Step2Checked = false; //leave the toggle button in unchecked state if the tool execution is canceled
        MessageBox.Show("The operation to create Slope layer from DSM was canceled.");
        return;
      }
      #endregion Calculate the Slope layer from the DSM
      #region Calculate the Aspect layer from the DSM
      var parametersAspect = Geoprocessing.MakeValueArray(
               in_raster,
               "Step2_Aspect_DSM", //out_raster
               "Aspect", //parameter_type 
               "Quadratic", //local_surface_type
               null, //neighborhood_distance
               null, //use_adaptive_neighborhood
               null, //z_unit = 
               null, //output_slope_measurement
               null, //project_geodesic_azimuths
               null, //use_equatorial_aspect 
               in_raster //in_analysis_mask
               );
      var progDlgAspect = new ProgressDialog($@"Step 2: Creating Aspect Layer from DSM", "Canceled", false);
      var progsrcAspect = new CancelableProgressorSource(progDlgAspect);
      // Execute Raster Solar Radiation tool   
      var gpResultAspect = await Geoprocessing.ExecuteToolAsync(toolName, parametersAspect, environments, progsrcAspect.Progressor);
      if (gpResultAspect.IsFailed)
      {
        Step2Checked = false; //leave the toggle button in unchecked state if the tool execution failed
        // display error messages if the tool fails, otherwise shows the default messages
        if (gpResultAspect.Messages.Count() != 0)
        {
          Geoprocessing.ShowMessageBox(gpResultAspect.Messages, progsrcAspect.Message,
                                          gpResultAspect.IsFailed ?
                                          GPMessageBoxStyle.Error : GPMessageBoxStyle.Default);
        }
        else
        {
          MessageBox.Show($@"Step 2: {progsrcAspect.Message} failed with errorcode, check parameters.");
        }
        return;
      }

      if (progsrcAspect.CancellationTokenSource.IsCancellationRequested)
      {
        Step2Checked = false; //leave the toggle button in unchecked state if the tool execution is canceled
        MessageBox.Show("The operation to create Aspect layer from DSM was canceled.");
        return;
      }
     
      #endregion Calculate the Aspect layer from the DSM
      if (!gpResultSlope.IsFailed && !gpResultAspect.IsFailed)
      {
        _step2HasExecuted = true;
        await MapView.Active.ZoomOutFixedAsync();
        Step2Checked = true; //toggle button is checked
      }
    }
    #endregion Step 2: Create Slope and Aspect layers from the DSM

    #region Step 3: Identify suitable rooftops
    private bool _step3HasExecuted;
    public ICommand CmdStep3 => new RelayCommand(() => ExecuteStep3(), () => true);
    private async void ExecuteStep3()
    {
      if (_step3HasExecuted)
      {
        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Step 3 has already been executed. No need to run again for this example.", "Step 3", MessageBoxButton.OK, MessageBoxImage.Information);
        Step3Completed = true;
        return;
      }
      var activeMapView = MapView.Active;
      if (activeMapView == null)
      {
        return;
      }
      var map = MapView.Active.Map;
      if (map == null) return;
      var extent = MapView.Active?.Extent;
      string dataPath = @$"{Project.Current.HomeFolderPath}";
      var toolName = "sa.Con";
      // Place environment settings in an array, in this case, OK to over-write
      var environments = Geoprocessing.MakeEnvironmentArray(overwriteoutput: true, extent: extent, cellSize: .5);
      #region Remove High slopes
      var parametersRemoveHighSlopes = Geoprocessing.MakeValueArray(
                "Step2_Slope_DSM", // Input conditional raster
                "Step1_Solar_Rad", // Input true raster or constant value
                "Step3_Solar_Rad_S", // Output raster
                null,
                $"VALUE <= {MaxRoofAngle}"
                );
      var progDlgRemoveHighSlopes = new ProgressDialog($@"Step 3: Remove areas with high slopes ({MaxRoofAngle}) +", "Canceled", false);
      var progsrcRemoveHighSlopes = new CancelableProgressorSource(progDlgRemoveHighSlopes);
      // Execute tool   
      var gpResultRemoveHighSlopes = await Geoprocessing.ExecuteToolAsync(toolName, parametersRemoveHighSlopes, environments, progsrcRemoveHighSlopes.Progressor);
      if (gpResultRemoveHighSlopes.IsFailed)
      {
        Step3Checked = false; //leave the toggle button in unchecked state if the tool execution failed
        // display error messages if the tool fails, otherwise shows the default messages
        if (gpResultRemoveHighSlopes.Messages.Count() != 0)
        {
          Geoprocessing.ShowMessageBox(gpResultRemoveHighSlopes.Messages, progsrcRemoveHighSlopes.Message,
                                          gpResultRemoveHighSlopes.IsFailed ?
                                          GPMessageBoxStyle.Error : GPMessageBoxStyle.Default);
        }
        else
        {
          MessageBox.Show($@"Step 3: {progsrcRemoveHighSlopes.Message} failed with errorcode, check parameters.");
        }
        return;
      }

      if (progsrcRemoveHighSlopes.CancellationTokenSource.IsCancellationRequested)
      {
        Step3Checked = false; //leave the toggle button in unchecked state if the tool execution is canceled
        MessageBox.Show("The operation to remove areas with high slopes was canceled.");
        return;
      }
      
      #endregion Remove High slopes
      #region Remove areas with low solar radiation
      var progDlgRemoveLowSolarRadiation = new ProgressDialog($@"Step 3: Remove areas with low solar radiation", "Canceled", false);
      var progsrcRemoveLowSolarRadiation = new CancelableProgressorSource(progDlgRemoveLowSolarRadiation);
      var parametersRemoveLowSolarRadiationAreas = Geoprocessing.MakeValueArray(
          "Step3_Solar_Rad_S", // Input conditional raster
          "Step3_Solar_Rad_S", // Input true raster or constant value
          "Step3_Solar_Rad_S_HS", // Output raster
          null,
          $"VALUE >= {Radiation}"
          );
      // Execute  tool   
      var gpResultRemoveLowSolarRadiation = await Geoprocessing.ExecuteToolAsync(toolName, parametersRemoveLowSolarRadiationAreas, environments, progsrcRemoveLowSolarRadiation.Progressor);
      if (gpResultRemoveLowSolarRadiation.IsFailed)
      {
        Step2Checked = false; //leave the toggle button in unchecked state if the tool execution failed
        // display error messages if the tool fails, otherwise shows the default messages
        if (gpResultRemoveLowSolarRadiation.Messages.Count() != 0)
        {
          Geoprocessing.ShowMessageBox(gpResultRemoveLowSolarRadiation.Messages, progsrcRemoveLowSolarRadiation.Message,
                                          gpResultRemoveLowSolarRadiation.IsFailed ?
                                          GPMessageBoxStyle.Error : GPMessageBoxStyle.Default);
        }
        else
        {
          MessageBox.Show($@"Step 3: {progsrcRemoveLowSolarRadiation.Message} failed with errorcode, check parameters.");
        }
        return;
      }

      if (progsrcRemoveLowSolarRadiation.CancellationTokenSource.IsCancellationRequested)
      {
        Step3Checked = false; //leave the toggle button in unchecked state if the tool execution is canceled
        MessageBox.Show("The operation to remove areas with low solar radiation was canceled.");
        return;
      }
      
      #endregion Remove areas with low solar radiation

      #region Keep slopes that are almost flat
      var progDlgLeaveFlatSlopes = new ProgressDialog($@"Step 3: Keep slopes that are almost flat", "Canceled", false);
      var progsrcLeaveFlatSlopes = new CancelableProgressorSource(progDlgLeaveFlatSlopes);
      var parametersLeaveFlatSlopes = Geoprocessing.MakeValueArray(
                "Step2_Slope_DSM", // Input conditional raster
                "Step3_Solar_Rad_S_HS", // Input true raster or constant value
                "Step3_Solar_Rad_Low_Slope", // Output raster
                null,
                "VALUE <= 10"
                );
      var gpResultLeaveFlatSlopes = await Geoprocessing.ExecuteToolAsync(toolName, parametersLeaveFlatSlopes, environments, progsrcLeaveFlatSlopes.Progressor);
      if (gpResultLeaveFlatSlopes.IsFailed)
      {
        Step3Checked = false; //leave the toggle button in unchecked state if the tool execution is failed
        // display error messages if the tool fails, otherwise shows the default messages
        if (gpResultLeaveFlatSlopes.Messages.Count() != 0)
        {
          Geoprocessing.ShowMessageBox(gpResultLeaveFlatSlopes.Messages, progsrcLeaveFlatSlopes.Message,
                                          gpResultLeaveFlatSlopes.IsFailed ?
                                          GPMessageBoxStyle.Error : GPMessageBoxStyle.Default);
        }
        else
        {
          MessageBox.Show($@"Step 3: {progsrcLeaveFlatSlopes.Message} failed with errorcode, check parameters.");
        }
        return;
      }

      if (progsrcLeaveFlatSlopes.CancellationTokenSource.IsCancellationRequested)
      {
        Step3Checked = false; //leave the toggle button in unchecked state if the tool execution is canceled
        MessageBox.Show("The operation to keep slopes that are almost flat was canceled.");
        return;
      }
    
      #endregion Keep slopes that are almost flat

      #region Remove areas that face north
      var progDlgRemoveNorthRoofs = new ProgressDialog($@"Step 3: Remove areas that face north", "Canceled", false);
      var progsrcRemoveNorthRoofs = new CancelableProgressorSource(progDlgRemoveNorthRoofs);
      var parametersRemoveNorthRoofs = Geoprocessing.MakeValueArray(
        "Step2_Aspect_DSM", // Input conditional raster
        "Step3_Solar_Rad_S_HS ", // Input true raster or constant value
        "Step3_Solar_Rad_S_HS_NN", // Output raster
        "Step3_Solar_Rad_Low_Slope",//Input false raster or constant value
        "VALUE > 22.5 AND VALUE <= 337.5"
      );
      var gpResultRemoveNorthRoofs = await Geoprocessing.ExecuteToolAsync(toolName, parametersRemoveNorthRoofs, environments, progsrcRemoveNorthRoofs.Progressor);
      if (gpResultRemoveNorthRoofs.IsFailed)
      {
        // display error messages if the tool fails, otherwise shows the default messages
        if (gpResultRemoveNorthRoofs.Messages.Count() != 0)
        {
          Step3Checked = false; //leave the toggle button in unchecked state if the tool execution is failed
          Geoprocessing.ShowMessageBox(gpResultRemoveNorthRoofs.Messages, progsrcRemoveNorthRoofs.Message,
                                          gpResultRemoveNorthRoofs.IsFailed ?
                                          GPMessageBoxStyle.Error : GPMessageBoxStyle.Default);
        }
        else
        {
          MessageBox.Show($@"Step 3: {progsrcRemoveNorthRoofs.Message} failed with errorcode, check parameters.");
        }
        return;
      }

      if (progsrcRemoveNorthRoofs.CancellationTokenSource.IsCancellationRequested)
      {
        Step3Checked = false; //leave the toggle button in unchecked state if the tool execution is canceled
        MessageBox.Show("The operation to remove areas that face north was canceled.");
        return;
      }

      #endregion Remove areas that face north
      #region Step 3 Completed - toggle button is checked and map is updated to show suitable rooftop areas
      if (!gpResultRemoveHighSlopes.IsFailed && !gpResultRemoveLowSolarRadiation.IsFailed && !gpResultLeaveFlatSlopes.IsFailed && !gpResultRemoveNorthRoofs.IsFailed)
      {
        _step3HasExecuted = true;
        Step3Checked = true; //toggle button is checked
                             //Hide Solar_Rad_S
        await QueuedTask.Run(async () =>
        {
          var layer_Solar_Rad_S = MapView.Active.Map.Layers.FirstOrDefault(l => l.Name == "Step3_Solar_Rad_S");
          if (layer_Solar_Rad_S != null)
            layer_Solar_Rad_S.SetVisibility(false);
       
        //Hide Solar_Rad_S_HS
          var layer_Solar_Rad_S_HS = MapView.Active.Map.Layers.FirstOrDefault(l => l.Name == "Step3_Solar_Rad_S_HS");
          if (layer_Solar_Rad_S_HS != null)
            layer_Solar_Rad_S_HS.SetVisibility(false);

        //Hide Solar_Rad_Low_Slope
          var layer_Solar_Rad_Low_Slope = MapView.Active.Map.Layers.FirstOrDefault(l => l.Name == "Step3_Solar_Rad_Low_Slope");
          if (layer_Solar_Rad_Low_Slope != null)
            layer_Solar_Rad_Low_Slope.SetVisibility(false);
      
        //Hide two layers: Slope_DSM and Aspect_DSM
        var layersToHide = new List<string> { "Step2_Slope_DSM", "Step2_Aspect_DSM" };
        foreach (var layer in layersToHide)
        {
            var layerToHide = MapView.Active.Map.Layers.FirstOrDefault(l => l.Name == layer);
            layerToHide.SetVisibility(false);
        }
        await MapView.Active.ZoomInFixedAsync();
        //Apply the symbology from Solar_Doc layer document to the new layer Solar_Rad_S_HS_NN
        
          var solarRad_S_HS_NN_Layer = MapView.Active.Map.Layers.FirstOrDefault(l => l.Name == "Step3_Solar_Rad_S_HS_NN") as RasterLayer;
          if (solarRad_S_HS_NN_Layer != null && _layerDoc != null)
          {
            //Get the renderer form the layer document we saved in Step 1 and apply it to the new layer
            var cimLyrDoc = _layerDoc.GetCIMLayerDocument();
            //Get the renderer from the layer file
            var colorizerFromRaster = ((CIMRasterLayer)cimLyrDoc.LayerDefinitions[0]).Colorizer as CIMRasterStretchColorizer;
            if (colorizerFromRaster == null) return;
            //Apply the renderer to the feature layer
            //Note: If working with a raster layer, use the SetColorizer method.
            solarRad_S_HS_NN_Layer?.SetColorizer(colorizerFromRaster);
          }
          //Select the Solar_Rad layer in the TOC so that we can use the swipe tool to compare the original solar radiation layer with the final suitable rooftop raster layer.
          var solarRadLayer = map.Layers.FirstOrDefault(l => l.Name == "Step1_Solar_Rad");
          if (solarRadLayer != null)
          {
            var layersToSelect = new List<Layer>() { solarRadLayer };
            MapView.Active.SelectLayers(layersToSelect);
          }
          //Zoom to the RPHDBuildingFootprints layer
          var layer_buildingFootprint = MapView.Active.Map.Layers.FirstOrDefault(l => l.Name == "RPHDBuildingFootprints");
          MapView.Active.ZoomTo(layer_buildingFootprint);
        });
      }
      #endregion Step 3 Completed - toggle button is checked and map is updated to show suitable rooftop areas

    }
    #endregion Step 3: Identify suitable rooftops

    #region Step 4: Calculate power per building
    private bool _step4HasExecuted;
    public ICommand CmdStep4 => new RelayCommand(() => ExecuteStep4(), () => true);
    private async void ExecuteStep4()
    {
      if (_step4HasExecuted)
      {
        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Step 4 has already been executed. No need to run again for this example.", "Step 4", MessageBoxButton.OK, MessageBoxImage.Information);
        Step4Completed = true;
        return;
      }
      var activeMapView = MapView.Active;
      if (activeMapView == null)
      {
        return;
      }
      var map = MapView.Active.Map;
      if (map == null) return;

      string dataPath = @$"{Project.Current.HomeFolderPath}";
      string buildingFootprint = System.IO.Path.Combine(dataPath, "SolarEstimation.gdb\\RPHDBuildingFootprints");
      #region Aggregate cells per building
      var parametersAggregateCells = Geoprocessing.MakeValueArray(
          buildingFootprint, // Input Raster or Feature Zone Data
          "FacilityID", // Zone Field
          "Step3_Solar_Rad_S_HS_NN", //  Input Value Raster
          "Step4_Solar_Rad_Table",//Output Table
          "Mean" //Statistics Type
          );
      var progDlgAggregateCells = new ProgressDialog($@"Step 4: Aggregate cells per Building", "Canceled", false);
      var progsrcAggregateCells = new CancelableProgressorSource(progDlgAggregateCells);

      // Place environment settings in an array, in this case, OK to over-write
      var environments = Geoprocessing.MakeEnvironmentArray(overwriteoutput: true);
      // Execute Zonal Statistics as Table tool   
      var gpResultAggregateCells = await Geoprocessing.ExecuteToolAsync("ia.ZonalStatisticsAsTable", parametersAggregateCells, environments, progsrcAggregateCells.Progressor);
      if (gpResultAggregateCells.IsFailed)
      {
        Step4Checked = false; //leave the toggle button in unchecked state if the tool execution is failed
        // display error messages if the tool fails, otherwise shows the default messages
        if (gpResultAggregateCells.Messages.Count() != 0)
        {
          Geoprocessing.ShowMessageBox(gpResultAggregateCells.Messages, progsrcAggregateCells.Message,
                                          gpResultAggregateCells.IsFailed ?
                                          GPMessageBoxStyle.Error : GPMessageBoxStyle.Default);
        }
        else
        {
          MessageBox.Show($@"Step 4: {progsrcAggregateCells.Message} failed with errorcode, check parameters.");
        }
        return;
      }
      if (progsrcAggregateCells.CancellationTokenSource.IsCancellationRequested)
      {
        Step4Checked = false; //leave the toggle button in unchecked state if the tool execution is canceled
        MessageBox.Show("The operation was canceled.");
        return;
      }
      #endregion Aggregate cells per building

      #region Update Building footprint with solar radiation information
      var parametersBuildingFootprintUpdate = Geoprocessing.MakeValueArray(
                buildingFootprint, // Input Table
                "FacilityID", // Input Field
                "Step4_Solar_Rad_Table", //  Join Table
                "FacilityID",//Join Field
                "MEAN" //Transfer Fields
                );
      var progDlgBuildingFootprintUpdate = new ProgressDialog($@"Step 4: Update Building footprint with solar radiation information", "Canceled", false);
      var progsrcBuildingFootprintUpdate = new CancelableProgressorSource(progDlgBuildingFootprintUpdate);

      // Execute Raster Solar Radiation tool   
      var gpResultBuildingFootprintUpdate = await Geoprocessing.ExecuteToolAsync("management.JoinField", parametersBuildingFootprintUpdate, environments, progsrcBuildingFootprintUpdate.Progressor);

      if (gpResultBuildingFootprintUpdate.IsFailed)
      {
        Step4Checked = false; //leave the toggle button in unchecked state if the tool execution is failed
        // display error messages if the tool fails, otherwise shows the default messages
        if (gpResultBuildingFootprintUpdate.Messages.Count() != 0)
        {
          Geoprocessing.ShowMessageBox(gpResultBuildingFootprintUpdate.Messages, progsrcBuildingFootprintUpdate.Message,
                                          gpResultBuildingFootprintUpdate.IsFailed ?
                                          GPMessageBoxStyle.Error : GPMessageBoxStyle.Default);
        }
        else
        {
          MessageBox.Show($@"Step 4: {progsrcBuildingFootprintUpdate.Message} failed with errorcode, check parameters.");
        }
        return;
      }
      if (progsrcBuildingFootprintUpdate.CancellationTokenSource.IsCancellationRequested)
      {
        Step4Checked = false; //leave the toggle button in unchecked state if the tool execution is canceled
        MessageBox.Show("The operation was canceled.");
        return;
      }
      #endregion Update Building footprint with solar radiation information
      #region Step 4 Completed - toggle button is checked and map is updated to show suitable rooftop areas
      if (!gpResultAggregateCells.IsFailed && !gpResultBuildingFootprintUpdate.IsFailed)
      {
        _step4HasExecuted = true;
        var layers = MapView.Active.Map.GetLayersAsFlattenedList();
        List<string> layersToIgnore = new List<string>()
      {
        "RPHDBuildingFootprints", "Step3_Solar_Rad_S_HS_NN", "World Topographic Map", "World Hillshade"
      };
        foreach (var layer in layers)
        {
          if (!layersToIgnore.Contains(layer.Name))
          {
            await QueuedTask.Run(() => layer.SetVisibility(false));
          }
        }
        //open the RPHDBuildingFootprints 
        //Select the table in the TOC
        var buildingFootprintLayer = map.Layers.FirstOrDefault(l => l.Name == "RPHDBuildingFootprints");
        if (buildingFootprintLayer != null)
        {
          var layersToSelect = new List<Layer>() { buildingFootprintLayer };
          MapView.Active.SelectLayers(layersToSelect);
        }

        #endregion Step 4 Completed - toggle button is checked and map is updated to show suitable rooftop areas
      }
    }
    #endregion Step 4: Calculate power per building


    #region Step 5: Apply Symbology to Suitable Buildings Layer
    private bool _step5HasExecuted;
    private LayerDocument _layerDoc;

    public ICommand CmdStep5 => new RelayCommand(() => ExecuteStep5(), () => true);
    private async void ExecuteStep5()
    {
      if (_step5HasExecuted)
      {
        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Step 5 has already been executed. No need to run again for this example.", "Step 5", MessageBoxButton.OK, MessageBoxImage.Information);
        Step5Completed = true;
        return;
      }

      // Step 5 implementation goes here
      //Apply symbology to the map based on the SelectedSource
      var pathToLyrx = Path.Combine(Project.Current.HomeFolderPath, @"Suitable_Buildings.lyrx");
      ApplySymbologyFromTemplate(pathToLyrx);
      _step5HasExecuted = true;
      //ApplySymbologyFromTemplate();
    }

    #endregion Step 5: Apply Symbology to Suitable Buildings Layer
    internal class LegendItem
    {
      public string Color { get; set; }
      public string Label { get; set; }
    }
  }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class SolarSuitability_ShowButton : Button
    {
      protected override void OnClick()
      {
        SolarSuitabilityViewModel.Show();
      }
    }
  }
