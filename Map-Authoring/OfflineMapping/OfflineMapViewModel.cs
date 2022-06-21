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
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
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
using ArcGIS.Desktop.Mapping.Offline;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace OfflineMapping
{
  internal class OfflineMapViewModel : CustomControl
  {
    private bool _hasBasemap;
    public bool HasBasemap
    {
      get { 
        return (_hasRasterTileCacheLayers || _hasVectorTileCacheLayers);
      }
      set { SetProperty(ref _hasBasemap, value, () => HasBasemap); }
    }
    private bool _includeBasemap;
    public bool IncludeBasemap
    {
      get { return _includeBasemap; }
      set {
        SetProperty(ref _includeBasemap, value, () => IncludeBasemap);
        if (!_canGenerateReplica) //No FS to replicate, so only basemaps
          _canDownloadMap = _includeBasemap ? true : false;
      }
    }

    private bool _hasVectorTileCacheLayers;
    public bool HasVectorTileCacheLayers
    {
      get {
        return _hasVectorTileCacheLayers;
      }
      set {
        SetProperty(ref _hasVectorTileCacheLayers, value, () => HasVectorTileCacheLayers);
      }
    }
    private bool _hasRasterTileCacheLayers;
    public bool HasRasterTileCacheLayers
    {
      get
      {
        return _hasRasterTileCacheLayers;
      }
      set
      {
        SetProperty(ref _hasRasterTileCacheLayers, value, () => HasRasterTileCacheLayers);
      }
    }
    private ObservableCollection<string> _vectorScales = new ObservableCollection<string>();
    public ObservableCollection<string> VectorScales
    {
      get {       
        return _vectorScales; }
      set {                 
        SetProperty(ref _vectorScales, value, () => VectorScales);
        SelectedVectorScale = VectorScales[0];
      }
    }

    private string _selectedVectorScale;
    public string SelectedVectorScale
    {
      get { return _selectedVectorScale; }
      set
      {
        SetProperty(ref _selectedVectorScale, value, () => SelectedVectorScale);
      }
    }

    private ObservableCollection<string> _rasterScales = new ObservableCollection<string>();
    public ObservableCollection<string> RasterScales
    {
      get { return _rasterScales; }
      set
      {
        SetProperty(ref _rasterScales, value, () => RasterScales);
      }
    }
    private string _selectedRasterScale;
    public  string SelectedRasterScale
    {
      get { return _selectedRasterScale; }
      set
      {
        SetProperty(ref _selectedRasterScale, value, () => SelectedRasterScale);
      }
    }
    private Visibility _displayRasterTileScales = Visibility.Collapsed;
    public Visibility DisplayRasterTileScales
    {
      get { return _displayRasterTileScales; }
      set
      {
        SetProperty(ref _displayRasterTileScales, value, () => DisplayRasterTileScales);
      }
    }

    private Visibility _displayVectorTileScales = Visibility.Collapsed;
    public Visibility DisplayVectorTileScales
    {
      get { return _displayVectorTileScales; }
      set
      {
        SetProperty(ref _displayVectorTileScales, value, () => DisplayVectorTileScales);
      }
    }
    public ICommand DownloadMapCmd
    {
      get
      {
        return new RelayCommand(new Action(() => InvokeDownloadMap()), () => _canDownloadMap);
      }
    }
    private bool _canDownloadMap;
    private bool _canGenerateReplica;
    public bool CanDownloadMap { get; set; }
   

    private void InvokeDownloadMap()
    {
      var activeMap = MapView.Active;
      if (activeMap == null) return;
      Map map = activeMap.Map;
      var extent = MapView.Active?.Extent;
      QueuedTask.Run(() => {
        var canGenerteReplica = GenerateOfflineMap.Instance.GetCanGenerateReplicas(map);
        if (canGenerteReplica)
        {
          var replicaParams = new GenerateReplicaParams { Extent = extent};
          try {
            GenerateOfflineMap.Instance.GenerateReplicas(map, replicaParams);
          }
          catch (Exception ex)
          {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
          }         
        }
        if (IncludeBasemap) //export basemaps
        {
          //Vector Tile Layers
          var canExportVectorTile = GenerateOfflineMap.Instance.GetCanExportVectorTileCache(map);
          if (canExportVectorTile)
          {
            GenerateOfflineMap.Instance.ExportVectorTileCache(map, new ExportTileCacheParams()
            {
              Extent = extent,
              MaximumUserDefinedScale = DisplayScaleToDouble(SelectedVectorScale)
            });
          }
          //Raster Tile Layers
          var canExportRasterTile = GenerateOfflineMap.Instance.GetCanExportRasterTileCache(map);
          if (canExportRasterTile)
          {
            //Export
            GenerateOfflineMap.Instance.ExportRasterTileCache(map, new ExportTileCacheParams()
            {
              Extent = extent,
              MaximumUserDefinedScale = DisplayScaleToDouble(SelectedRasterScale)
            });
          }
        }
      });
    }
    private List<string> vectorMapScaleStrings = new List<string>();
    private List<string> rasterMapScaleStrings = new List<string>();
    private async Task Refresh()
    {
      //Check if the map has vector tiles cache
      Map map = MapView.Active.Map;
      var extent = MapView.Active.Extent;
      if (map == null)
        return;
      await QueuedTask.Run( () => {
        _canGenerateReplica = GenerateOfflineMap.Instance.GetCanGenerateReplicas(map); //Map has a Feature service that can replica
        _canDownloadMap = _canGenerateReplica ? true : false; //Enable download button
        //Determine if the map has a vector tile layer
        _hasVectorTileCacheLayers = GenerateOfflineMap.Instance.GetCanExportVectorTileCache(map);
        //Notify property changed for UI Controls to refresh
        Visibility vectorScaleDisplay = _hasVectorTileCacheLayers ? Visibility.Visible : Visibility.Collapsed;
        SetProperty(ref _displayVectorTileScales, vectorScaleDisplay, () => DisplayVectorTileScales);

        //Determine if the map has a raster tile layer
        _hasRasterTileCacheLayers = GenerateOfflineMap.Instance.GetCanExportRasterTileCache(map);
        //Notify property changed for UI Controls to refresh
        Visibility rasterScaleDisplay = _hasRasterTileCacheLayers ? Visibility.Visible : Visibility.Collapsed;
        SetProperty(ref _displayRasterTileScales, rasterScaleDisplay, () => DisplayRasterTileScales);

        //Map contains a vector or raster tile layer
        //Notify property changed for UI Controls to refresh
        bool hasBasemaps = _hasRasterTileCacheLayers || _hasVectorTileCacheLayers;
        SetProperty(ref _hasBasemap, hasBasemaps, () => HasBasemap);

        //Get the vector tile layer's scales 
        if (_hasVectorTileCacheLayers == true)
        {
          if (!_canGenerateReplica) //No FS to replicate
          {
            _canDownloadMap = IncludeBasemap ? true : false; //enable the Download button
          }
          List<double> vectorscales = new List<double> { };
          vectorscales = GenerateOfflineMap.Instance.GetExportVectorTileCacheScales(map, extent);
          //Get a list of strings of the vector map scales
          vectorMapScaleStrings.Clear();
          foreach (var vectorScale in vectorscales)
          {
            vectorMapScaleStrings.Add(ScaleToString(vectorScale));
          }
          //Notify Property changed to update the vector tile layer scales in the UI
          var scales = new ObservableCollection<string>(vectorMapScaleStrings);
          SetProperty(ref _vectorScales, scales, () => VectorScales);
          SetProperty(ref _selectedVectorScale, VectorScales[0], () => SelectedVectorScale);
        }
        //Get the raster tile layer's scales
        if (_hasRasterTileCacheLayers == true)
        {
          if (!_canGenerateReplica) //No FS to replicate
          {
            _canDownloadMap = IncludeBasemap ? true : false; //enable the Download button
          }
          List<double> rasterscales = new List<double> { };
          rasterscales = GenerateOfflineMap.Instance.GetExportRasterTileCacheScales(map, extent);
          //Get a list of the scales as strings
          rasterMapScaleStrings.Clear();
          foreach (var rasterScale in rasterscales)
          {
            rasterMapScaleStrings.Add(ScaleToString(rasterScale));
          }
          //Notify Property changed to update the raster tile layer scales in the UI
          var rasterScalesCollection = new ObservableCollection<string>(rasterMapScaleStrings);
          SetProperty(ref _rasterScales, rasterScalesCollection, () => RasterScales);
          SetProperty(ref _selectedRasterScale, RasterScales[0], () => SelectedRasterScale);
        }
        
      });
    }
    protected async override void OnDropDownOpened()
    {
      await Refresh();
    }
    private string ScaleToString(double scale)
    {
      var doubleToInt = Convert.ToInt32(scale);
      var scaleString = $"1:{string.Format("{0:#,0}", doubleToInt)}";
      return scaleString;
    }
    private double DisplayScaleToDouble(string displayScale)
    {
      var position = displayScale.IndexOf(":");
      var scale = displayScale.Substring(position + 1).Trim();
      return Convert.ToDouble(scale);
    }
  }
}
