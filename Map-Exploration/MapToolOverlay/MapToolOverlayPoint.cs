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
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MapToolOverlay
{
  internal class MapToolOverlayPoint : MapTool
  {
    enum TrackingState
    {
      NotTracking = 0,
      CanTrack,
      Tracking
    }

    private IDisposable _graphic = null;
    private CIMPointSymbol _pointSymbol = null;

    private System.Windows.Point? _lastLocation = null;
    private System.Windows.Point? _workingLocation = null;

    private TrackingState _trackingMouseMove = TrackingState.NotTracking;
    private static readonly object _lock = new object();

    public MapToolOverlayPoint()
    {
      IsSketchTool = false;//we are not using the sketch feedback in this tool
      SketchType = SketchGeometryType.None;
      //SketchOutputMode = SketchOutputMode.Map;
    }

    protected async override Task OnToolActivateAsync(bool active)
    {
      _trackingMouseMove = TrackingState.NotTracking;
      if (_pointSymbol == null)
      {
        await QueuedTask.Run(() =>
        {
          _pointSymbol = SymbolFactory.Instance.ConstructPointSymbol(CIMColor.CreateRGBColor(100, 255, 40), 10, SimpleMarkerStyle.Circle);
        });
      }
      _lastLocation = null;
      _workingLocation = null;
    }

    protected override Task OnToolDeactivateAsync(bool hasMapViewChanged)
    {
      _pointSymbol = null;
      lock (_lock)
      {
        if (_graphic != null)
        {
          _graphic.Dispose();
          _graphic = null;
        }
      }
      return base.OnToolDeactivateAsync(hasMapViewChanged);
    }

    protected override void OnToolMouseDown(MapViewMouseButtonEventArgs e)
    {
      if (e.ChangedButton == MouseButton.Left)
        e.Handled = true;
    }

    protected override void OnToolMouseUp(MapViewMouseButtonEventArgs e)
    {
      lock (_lock)
      {
        _trackingMouseMove = TrackingState.NotTracking;
      }
    }

    private double _r = 0.0;
    private double _g = 80.0;
    private double _b = 160.0;

    protected override async void OnToolMouseMove(MapViewMouseEventArgs e)
    {
      //All of this logic is to avoid unnecessarily updating the graphic position
      //for ~every~ mouse move. We skip any "intermediate" points in-between rapid
      //mouse moves.
      lock (_lock)
      {
        if (_trackingMouseMove == TrackingState.NotTracking)
          return;
        else
        {
          if (_workingLocation.HasValue)
          {
            _lastLocation = e.ClientPoint;
            return;
          }
          else
          {
            _lastLocation = e.ClientPoint;
            _workingLocation = e.ClientPoint;
          }
        }
        _trackingMouseMove = TrackingState.Tracking;
      }
      //The code "inside" the QTR will execute for all points that
      //get "buffered" or "queued". This avoids having to spin up a QTR
      //for ~every~ point of ~every mouse move.

      await QueuedTask.Run(() =>
      {
        while (true)
        {
          System.Windows.Point? point;
          IDisposable graphic = null;
          MapPoint mapPoint = null;
          lock (_lock)
          {
            point = _lastLocation;
            _lastLocation = null;
            _workingLocation = point;
            if (point == null || !point.HasValue)
            {
              //No new points came in while we updated the overlay
              _workingLocation = null;
              break;
            }
            else if (_graphic == null)
            {
              //conflict with the mouse down,
              //If this happens then we are done. A new line and point will be
              //forthcoming from the SketchCompleted callback
              _trackingMouseMove = TrackingState.NotTracking;
              break;
            }
            graphic = _graphic;
            if (point.HasValue)
              mapPoint = this.ActiveMapView.ClientToMap(point.Value);
          }
          if (mapPoint != null)
          {
            _r += 10; if (_r > 255) _r = 0;
            _g += 10; if (_g > 255) _g = 80;
            _b += 10; if (_b > 255) _b = 160;
            _pointSymbol.SetColor(CIMColor.CreateRGBColor(_r, _g, _b));
            //update the graphic overlay
            this.UpdateOverlay(graphic, mapPoint, _pointSymbol.MakeSymbolReference());
          }
        }
      });
    }

    protected override Task HandleMouseDownAsync(MapViewMouseButtonEventArgs e)
    {
      //Select a line feature and place the initial graphic. Clear out any
      //previously placed graphic
      return QueuedTask.Run(() =>
      {
        var mapPoint = this.ActiveMapView.ClientToMap(e.ClientPoint);
        lock (_lock)
        {
          _workingLocation = null;
          if (_graphic != null)
            _graphic.Dispose();
          _graphic = this.AddOverlay(mapPoint, _pointSymbol.MakeSymbolReference());
          _trackingMouseMove = TrackingState.CanTrack;
        }
      });
    }
  }
}
