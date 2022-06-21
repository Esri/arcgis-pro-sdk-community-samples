//Copyright 2020 Esri

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
using ArcGIS.Core.Events;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Editing;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;

namespace CreatePointsAlongLine3D
{
  internal class CreatePointsAlongLine3DControlViewModel : EmbeddableControl
  {
    public CreatePointsAlongLine3DControlViewModel(XElement options, bool canChangeOptions) : base(options, canChangeOptions) { }
    private static Polyline _selLineGeom;
    private static SubscriptionToken _mscToken = null;
    private static IDisposable _mapOverlay = null;

    public override Task OpenAsync()
    {
      QueuedTask.Run(() =>
      {
        _okRelay = new RelayCommand(() => ConstructPoints(IsNumberOfPoints, Value), CanCreatePoints);
        NotifyPropertyChanged("OKCommand");

        if (_mscToken == null)
          _mscToken = MapSelectionChangedEvent.Subscribe(OnSelectionChangedAsync);
        SetState(MapView.Active.Map.GetSelection().ToDictionary());
      });
      return Task.FromResult(true);
    }

    public override Task CloseAsync()
    {
      MapSelectionChangedEvent.Unsubscribe(_mscToken);
      _mscToken = null;
      if (_mapOverlay != null)
        _mapOverlay.Dispose();
      return Task.FromResult(true);
    }

    #region Bindable Properties
    private static double _2DLength = 0;
    private static double _3DLength = 0;
    public string Length2D => _2DLength.ToString("0.000");
    public string Length3D => _3DLength.ToString("0.000");

    private RelayCommand _okRelay;
    public ICommand OKCommand => _okRelay;

    private static bool _isNumberOfPoints = true;
    public bool IsNumberOfPoints
    {
      get { return _isNumberOfPoints; }
      set
      {
        if (SetProperty(ref _isNumberOfPoints, value))
          NotifyPropertyChanged("IsDistance");
      }
    }

    public bool IsDistance
    {
      get { return !_isNumberOfPoints; }
      set { IsNumberOfPoints = !value; }
    }

    private static double _value = 2;
    public double Value
    {
      get { return _value; }
      set { SetProperty(ref _value, value); }
    }

    private static bool _isEndsChecked = false;
    public bool IsEndsChecked
    {
      get { return _isEndsChecked; }
      set { SetProperty(ref _isEndsChecked, value); }
    }

    public bool EnableToolTip => !CanCreatePoints();
    public string ToolTipCreate => "Please select a 3D non curved line";
    #endregion binding

    private void OnSelectionChangedAsync(MapSelectionChangedEventArgs obj)
    {
      if (obj.Map != MapView.Active.Map) return;
      if (_mapOverlay != null)
        _mapOverlay.Dispose();
      SetState(obj.Selection.ToDictionary());
    }

    private async void SetState(Dictionary<MapMember, List<long>> sel)
    {
      _cachedValue = false;
      _cachedValue = await CheckSelection(sel);
      SetProperty(ref _2DLength, _cachedValue ? _selLineGeom.Length : 0, "Length2D");
      SetProperty(ref _3DLength, _cachedValue ? _selLineGeom.Length3D : 0, "Length3D");
      NotifyPropertyChanged("EnableToolTip");
    }

    private bool _cachedValue = false;
    private bool CanCreatePoints()
    {
      return _cachedValue;
    }

    private Task<bool> CheckSelection(Dictionary<MapMember, List<long>> sel)
    {
      //work around for possible bug where pane can become detached when changing maps
      if (EditingModuleInternal.TemplateManager.CurrentTemplate == null) return Task.FromResult(false);

      //Enable only if we have one selected polyline Z feature
        if (sel == null || sel.Values.Sum(List => List.Count) != 1) return Task.FromResult(false);

      var selMember = sel.Keys.FirstOrDefault();
      var selOID = sel[selMember].First();

      var flayer = selMember as FeatureLayer;
      if (flayer == null) return Task.FromResult(false);
      if (flayer.ShapeType != esriGeometryType.esriGeometryPolyline) return Task.FromResult(false);

      return QueuedTask.Run(() =>
      {
      var insp = new Inspector();
      insp.Load(selMember, selOID);
      _selLineGeom = insp.Shape as Polyline;

      //draw the line direction
      if (MapView.Active.Map.MapType == MapType.Map)
      {
          
          var symbol = SymbolFactory.Instance.ConstructPointSymbol(CIMColor.CreateRGBColor(155, 75, 75), 12, SimpleMarkerStyle.Triangle);
          _mapOverlay = MapView.Active.AddOverlay(_selLineGeom, symbol.MakeSymbolReference());
        }

        //draw the line direction in 3D. Not fully supported
        //if (MapView.Active.Map.MapType == MapType.Scene)
        //{
        //  string symbolXml = Settings1.Default.lineSymbol3D;
        //  var symbol = CIMSymbolReference.FromXml(symbolXml);
        //  _mapOverlay = MapView.Active.AddOverlay(_selLineGeom, symbol);
        //}

        return (!_selLineGeom.HasCurves & _selLineGeom.HasZ);
      });
    }

    private static Task ConstructPoints(bool IsNumberOfPoints, double value)
    {
      //Run on MCT
      return QueuedTask.Run(() =>
      {
        //calc distance between points
        double dbp = 0;
        if (IsNumberOfPoints)
          dbp = _3DLength / (_value + 1);
        else
          dbp = _value;

        var editOp = new EditOperation();
        editOp.Name = "Construct points along a 3D line";

        var currTemp = EditingModuleInternal.TemplateManager.CurrentTemplate;

        //create points at distance between points up to total length
        for (double d = dbp; d < _3DLength; d += dbp)
        {
          //get subcurve from 3D line and endpoint
          var subCurve3D = GeometryEngine.Instance.GetSubCurve3D(_selLineGeom, 0, d, AsRatioOrLength.AsLength);
          var scEndPoint = subCurve3D.Points.Last();

          editOp.Create(currTemp, scEndPoint);
        }

        //create points at start and end of line?
        if (_isEndsChecked)
        {
          editOp.Create(currTemp, _selLineGeom.Points.First());
          editOp.Create(currTemp, _selLineGeom.Points.Last());
        }
        return editOp.Execute();
      });
    }
  }
}
