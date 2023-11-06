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
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Xml.Linq;
using System.Windows;
using System.Collections.ObjectModel;
using ArcGIS.Desktop.Mapping.Events;
using ArcGIS.Desktop.Internal.Mapping.CommonControls;
using System.Security.Policy;

namespace GetLineOfSight
{
  internal class LoSToolOptionsViewViewModel : ToolOptionsEmbeddableControl
  {
    public LoSToolOptionsViewViewModel(XElement options, bool canChangeOptions) : base(options, canChangeOptions)
    {
      System.Windows.Data.BindingOperations.EnableCollectionSynchronization(_tinLayersInMap, _lock);
      LayersAddedEvent.Subscribe(OnLayersAdded);
      LayersRemovedEvent.Subscribe(OnLayersRemoved);
    }
    ~LoSToolOptionsViewViewModel()
    {
      LayersAddedEvent.Unsubscribe(OnLayersAdded);
      LayersRemovedEvent.Unsubscribe(OnLayersRemoved);
    }

    private void OnLayersRemoved(LayerEventsArgs args)
    {
      foreach (var layer in args.Layers)
      {

        if (TinLayersInMap.Contains(layer))
          TinLayersInMap.Remove((TinLayer)layer);

      }

      if (SelectedTinLayer == null)
      {
        SelectedTinLayer = TinLayersInMap.FirstOrDefault();
      }
    }

    private void OnLayersAdded(LayerEventsArgs args)
    {
      foreach (var layer in args.Layers)
      {
        if (layer is TinLayer)
        {
          TinLayersInMap.Add((TinLayer)layer);
          if (SelectedTinLayer == null)
            SelectedTinLayer = (TinLayer)layer;
        }
      }
    }
    internal const string SelectedTinLayerName = nameof(SelectedTinLayer);
    internal const string ObserverHeightOptionName = nameof(ObserverHeight);
    internal const string TargetHeightOptionName = nameof(TargetHeight);
    internal const double DefaultObserverHeight = 25.0;
    internal const double DefaultTargetHeight = 25.0;
    internal static TinLayer DefaultTinLayer = null;
    internal const string ApplyCurvatureName = nameof(ApplyCurvature);
    internal const string ApplyRefractionName = nameof(ApplyRefraction);
    internal const string RefractionFactorName = nameof(RefractionFactor);
    private double _observerHeight;
    private string _observerHeightLabel;
    private string _targetHeightLabel;
    private TinLayer _originalSelectedTinLayer;
    private double _originalObserverHeight;
    private bool _originalApplyCurvature;
    private bool _originalApplyRefraction;
    private double _originalRefractionFactor;


    #region XAML Bindings
    private ObservableCollection<TinLayer> _tinLayersInMap = new ObservableCollection<TinLayer>();
    private object _lock = new object();
    public ObservableCollection<TinLayer> TinLayersInMap
    {
      get => _tinLayersInMap;
    }
    private TinLayer _selectedTinLayer;
    public TinLayer SelectedTinLayer
    {
      get => _selectedTinLayer;
      set
      {
        if (SetProperty(ref _selectedTinLayer, value))
        {
          SetToolOption(SelectedTinLayerName, value);
          _ = IsCurvatureSupportedAsync(SelectedTinLayer);
          IsDirty = _selectedTinLayer != _originalSelectedTinLayer;
        }
      }
    }

    public string ObserverHeightLabel
    {
      get { return _observerHeightLabel; }
      set => SetProperty(ref _observerHeightLabel, value);
    }
    public double ObserverHeight
    {
      get { return _observerHeight; }
      set
      {
        if (SetProperty(ref _observerHeight, value))
        {
          SetToolOption(ObserverHeightOptionName, value);
          IsDirty = _observerHeight != _originalObserverHeight;
        }
      }
    }

    private double _targetHeight;
    private double _originalTargetHeight;
    public double TargetHeight
    {
      get { return _targetHeight; }
      set
      {
        if (SetProperty(ref _targetHeight, value))
        {
          SetToolOption(TargetHeightOptionName, value);
          IsDirty = _targetHeight != _originalTargetHeight;
        }
      }
    }
    
    public string TargetHeightLabel
    {
      get { return _targetHeightLabel; }
      set
      {
        SetProperty(ref _targetHeightLabel, value);
      }
    }

    private bool _applyCurvature;
    public bool ApplyCurvature
    {
      get => _applyCurvature;
      set
      {
        if (SetProperty(ref _applyCurvature, value))
        {
          SetToolOption(ApplyCurvatureName, value);
          IsDirty = _applyCurvature != _originalApplyCurvature;
        }
      }
    }

    private bool _applyRefraction;
    public bool ApplyRefraction
    {
      get => _applyRefraction;
      set
      {
        if (SetProperty(ref _applyRefraction, value))
        {
          SetToolOption(ApplyRefractionName, value);
          IsDirty = _applyRefraction != _originalApplyRefraction;
          IsRefractionEnabled = ApplyRefraction ? true : false;
        }
      }
    }

    private double _refractionFactor = LineOfSightParams.DefaultRefractionFactor;
    public double RefractionFactor
    {
      get => _refractionFactor;
      set
      {
        if (SetProperty(ref _refractionFactor, value))
        {
          SetToolOption(RefractionFactorName, value);
          IsDirty = _refractionFactor != _originalRefractionFactor;
        }
      }
    }

    private Boolean _refractionEnabled = false;
    public Boolean IsRefractionEnabled
    {
      get => _refractionEnabled;
      set => SetProperty(ref _refractionEnabled, value);
    }

    private Boolean _isCurvatureApplicable = false;
    public Boolean IsCurvatureApplicable
    {
      get => _isCurvatureApplicable;
      set => SetProperty(ref _isCurvatureApplicable, value);
    }
    #endregion

    public override bool IsAutoOpen(string toolID)
    {
      return true;
    }
    protected override Task LoadFromToolOptions()
    {
      //Get all the values in the ToolOptions (template values) and save them in the "_originalXXX properties"
      //The current values are compared to the _originalXX values in the property setters so that the "IsDirty" property can be set.
      double? observerHeight = GetToolOption<double?>(ObserverHeightOptionName, DefaultObserverHeight, null);
      double? targetHeight = GetToolOption<double?>(TargetHeightOptionName, DefaultTargetHeight, null);
      bool? applyCurvature = GetToolOption<bool?>(ApplyCurvatureName, false, null);
      bool? applyRefraction = GetToolOption<bool?>(RefractionFactorName, false, null);
      double? refractionFactor = GetToolOption<double?>(RefractionFactorName, LineOfSightParams.DefaultRefractionFactor, null);
      DefaultTinLayer = MapView.Active?.Map.GetLayersAsFlattenedList().OfType<TinLayer>().FirstOrDefault();
      TinLayer selectedTinLayer = GetToolOption<TinLayer>(SelectedTinLayerName, DefaultTinLayer, null);
      _originalObserverHeight = observerHeight.GetValueOrDefault();
      _originalTargetHeight = targetHeight.GetValueOrDefault();
      _originalApplyCurvature = applyCurvature.GetValueOrDefault();
      _originalApplyRefraction = applyRefraction.GetValueOrDefault();
      _originalRefractionFactor = refractionFactor.GetValueOrDefault();

      //Set ViewModel properties to match the values obtained from the ToolOption template values
      if (observerHeight.HasValue)
        _observerHeight = observerHeight.Value;
      else
        _observerHeight = 0;

      if (targetHeight.HasValue)
        _targetHeight = targetHeight.Value;
      else
        _targetHeight = 0;

      if (applyCurvature.HasValue)
        _applyCurvature = applyCurvature.Value;
      else
        _applyCurvature = false;

      if (applyRefraction.HasValue)
        _applyRefraction = applyRefraction.Value;
      else
        _applyRefraction = false;

      if (refractionFactor.HasValue)
        _refractionFactor = refractionFactor.Value;
      else
        _refractionFactor = LineOfSightParams.DefaultRefractionFactor;

      if (selectedTinLayer != null)
        _selectedTinLayer = selectedTinLayer;
      else
        _selectedTinLayer = DefaultTinLayer;

      return Task.CompletedTask;
    }
    public override Task OpenAsync()
    {
      return base.OpenAsync();
    }

    public override Task CloseAsync()
    {
      return base.CloseAsync();
    }

    public override async void OnInitialize(IEnumerable<ToolOptions> optionsCollection, bool hostIsActiveTmplatePane)
    {
      base.OnInitialize(optionsCollection, hostIsActiveTmplatePane);
      //Populate Tin Layer collection
      if (MapView.Active == null) return;
      var tinLayers = MapView.Active.Map?.GetLayersAsFlattenedList().OfType<TinLayer>();
      if (tinLayers.Count() == 0) return;
      //Map height units
      string units = string.Empty;
      await QueuedTask.Run( () => {
        units = MapView.Active.Map.GetElevationUnitFormat().Abbreviation;
        System.Diagnostics.Debug.WriteLine($"Map Z Unit: {units}");
      });
      ObserverHeightLabel = $"Observer Height ({units}):";
      TargetHeightLabel = $"Target Height ({units}):";
      
      lock (_lock)
      {
        foreach (var layer in tinLayers)
        {
          TinLayersInMap.Add(layer);
        }
      }
      SelectedTinLayer = TinLayersInMap[0];

      double val = double.NaN;
      double firstVal = double.NaN;
      foreach (var option in optionsCollection)
      {
        val = option.GetProperty(ObserverHeightOptionName, DefaultObserverHeight);

        if (double.IsNaN(firstVal))
          firstVal = val;
        else
        {
          if (firstVal != val)
            IsDifferentValue = true;
        }
      }
    }
    private bool _isDifferentValue;
    public bool IsDifferentValue
    {
      get => _isDifferentValue;
      internal set => SetProperty(ref _isDifferentValue, value);
    }
    private BitmapImage _img = null;
    public override ImageSource SelectorIcon
    {
      get
      {
        if (_img == null)
          _img = new BitmapImage(new Uri(
            "pack://application:,,,/ArcGIS.Desktop.Resources;component/Images/AddFilter16.png", UriKind.Absolute));
        return _img;
      }
    }

    private async  Task IsCurvatureSupportedAsync(TinLayer tinLayer)
    {
      bool isCurvatureSupported = false;
      if (tinLayer == null)
      {
        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("No TIN layer in map.", "No TIN layer in map", MessageBoxButton.OK, MessageBoxImage.Information);
        return;
      }
      isCurvatureSupported = await QueuedTask.Run(() =>
      {
        using (var tinDataset = tinLayer.GetTinDataset())
        {
          if (!tinLayer.GetSpatialReference().IsProjected) //Projected coord
          {
            return false;
          }
          var zUnit = tinLayer.GetSpatialReference().ZUnit; //Should not be null
          if (zUnit != null && !double.IsNaN(zUnit.ConversionFactor))
          {
            return true;
          }
          else
            return false;
        }
      });
      IsCurvatureApplicable = isCurvatureSupported;
      //return isCurvatureSupported;
    }

    private bool IsOptionValid()
    {
     return MapView.Active?.Map.GetLayersAsFlattenedList().OfType<TinLayer>().Any() ?? false;
    }
  }
}
