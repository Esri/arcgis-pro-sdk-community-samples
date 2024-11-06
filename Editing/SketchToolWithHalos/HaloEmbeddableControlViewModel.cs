// Copyright 2024 Esri 
//
// 
//   Licensed under the Apache License, Version 2.0 (the "License"); 
//   you may not use this file except in compliance with the License. 
//   You may obtain a copy of the License at 
//
//       https://www.apache.org/licenses/LICENSE-2.0 
//
//   Unless required by applicable law or agreed to in writing, software 
//   distributed under the License is distributed on an "AS IS" BASIS, 
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//   See the License for the specific language governing permissions and 
//   limitations under the License. 

using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Xml.Linq;

namespace SketchToolWithHalos
{
  internal class HaloEmbeddableControlViewModel : EmbeddableControl
  {
    private readonly object _lockCollection = new object();

    private readonly ObservableCollection<HaloItem> _halos = new ObservableCollection<HaloItem>();
    private readonly ReadOnlyObservableCollection<HaloItem> _readOnlyHalos;

    public ReadOnlyObservableCollection<HaloItem> HaloItems => _readOnlyHalos;

    private double _canvasHeight = 0;
    public double CanvasHeight
    {
      get => _canvasHeight;
      set => SetProperty(ref _canvasHeight, value);
    }


    // Use _showAllHalos to indicate if the halos should be visible at all scales. 
    // As the halo size (in pixels) becomes smaller it becomes less useful on the screen. 
    // It may be useful to filter these smaller halos out. 
    private bool _showAllHalos = false;
    // indicates a custom cut-off scale (in pixels) as to when a halo will no longer be visible
    private double _haloMinimumVisibleSize = 10;


    public HaloEmbeddableControlViewModel(XElement options, bool canChangeOptions) : base(options, canChangeOptions)
    {
      _readOnlyHalos = new ReadOnlyObservableCollection<HaloItem>(_halos);
      BindingOperations.EnableCollectionSynchronization(_readOnlyHalos, _lockCollection);

      // subscribe to MapViewCameraChanged
      ArcGIS.Desktop.Mapping.Events.MapViewCameraChangedEvent.Subscribe(OnMapViewCameraChanged);
    }

    public override async Task OpenAsync()
    {
      if (_halos.Count == 0)
        await AssignHalos();
    }

    // when the camera / scale changes; recalculate the halo screen sizes
    internal async void OnMapViewCameraChanged(ArcGIS.Desktop.Mapping.Events.MapViewCameraChangedEventArgs args)
    {
      if (args == null)
        return;

      await UpdateHalos();
    }

    internal async Task AssignHalos()
    {
      // assign a list of radii in m
      List<double> radii = new List<double>() { 200, 60, 500, 2500 };   // in m

      var map = MapView.Active?.Map;
      if (map == null)
        return;

      double maxDiameter = 0;

      var count = radii.Count();
      // for each radius
      for (int idx = 0; idx < count; idx++)
      {
        // randomize color and stroke thickness
        var val = idx % 3;
        CIMColor color = null;
        double thickness = val + 1;
        switch (val)
        {
          case 0:
            color = CIMRGBColor.CreateRGBColor(255, 0, 0);
            break;
          case 1:
            color = CIMRGBColor.CreateRGBColor(255, 0, 255);
            break;
          case 2:
            color = CIMRGBColor.CreateRGBColor(0, 255, 255);
            break;
        }

        // create the halo item
        var halo = new HaloItem(radii[idx], color, thickness);
        // do the calculation / conversions
        await halo.DoConversions();
        // update visibility
        halo.CalculateVisibility(_haloMinimumVisibleSize, _showAllHalos);

        // add to the collection
        _halos.Add(halo);

        // keep track of the max screen diameter
        var diameter = halo.ScreenDiameter;
        if (diameter > maxDiameter)
          maxDiameter = diameter;
      }

      // update the canvas dimensions to the maximum screen diameter
      CanvasHeight = maxDiameter;

      // calculate the positions of all the halos on the canvas
      foreach (var halo in _halos)
        halo.CalculatePosition(CanvasHeight);

      // notify the UI that the collection has changed
      NotifyPropertyChanged(nameof(HaloItems));
    }

    internal async Task UpdateHalos()
    {
      var map = MapView.Active?.Map;
      if (map == null)
        return;

      double maxDiameter = 0;

      foreach (var halo in _halos)
      {
        // do the radius conversion based on screen scale
        await halo.DoRadiusConversion();

        // update visibility
        halo.CalculateVisibility(_haloMinimumVisibleSize, _showAllHalos);

        // keep track of the max screen diameter
        var diameter = halo.ScreenDiameter;
        if (diameter > maxDiameter)
          maxDiameter = diameter;
      }

      // update the canvas dimensions to the maximum screen diameter
      CanvasHeight = maxDiameter;

      // calculate the positions of all the halos on the canvas
      foreach (var halo in _halos)
        halo.CalculatePosition(CanvasHeight);

      // notify the UI that the collection has changed
      NotifyPropertyChanged(nameof(HaloItems));
    }

  }
}
