/*

   Copyright 2017 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Mapping;
using System.Windows;
using System.Windows.Input;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace OverlayExamples {

    enum TrackingState {
        NotTracking = 0,
        CanTrack,
        Tracking
    }
    /// <summary>
    /// Add a point graphic to the overlay on any line and track the mouse move. 
    /// </summary>
    /// <remarks>Clear the point graphic each time we click the mouse. This tool
    /// does NOT use a sketch feedback</remarks>
    internal class AddOverlayTrackMouseTool : MapTool {

        private IDisposable _graphic = null;
        private CIMPointSymbol _pointSymbol = null;
        //private Queue<Point> _lastLocations = new Queue<Point>();
        private Point? _lastLocation = null;
        private Point? _workingLocation = null;

        private TrackingState _trackingMouseMove = TrackingState.NotTracking;
        private Polyline _lineFeature = null;
        private static readonly object _lock = new object();

        public AddOverlayTrackMouseTool() {
            IsSketchTool = false;//we are not using the sketch feedback in this tool
            SketchType = SketchGeometryType.Point;
            SketchOutputMode = SketchOutputMode.Map;
        }

        protected async override Task OnToolActivateAsync(bool active) {
            _trackingMouseMove = TrackingState.NotTracking;
            if (_pointSymbol == null)
                _pointSymbol = await Module1.CreatePointSymbolAsync();
            //_lastLocations.Clear();
            _lastLocation = null;
            _workingLocation = null;

            if (!Module1.MessageShown) {
                if (!Module1.AreThereAnyLineLayers()) {
                    MessageBox.Show("You need to add at least one line featurelayer to the map.",
                        this.Caption);
                }
                else {
                    MessageBox.Show("Click on any line feature and hold the mouse down to move the graphic. " +
                                "The graphic will track the mouse along the selected line feature.",
                    this.Caption);
                    Module1.MessageShown = true;
                }
            }
        }

        protected override Task OnToolDeactivateAsync(bool hasMapViewChanged) {
            lock (_lock) {
                if (_graphic != null)
                    _graphic.Dispose();
                _graphic = null;
            }
            return base.OnToolDeactivateAsync(hasMapViewChanged);
        }

        protected override void OnToolMouseDown(MapViewMouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left)
                e.Handled = true;
        }

        protected override void OnToolMouseUp(MapViewMouseButtonEventArgs e) {
            lock (_lock) {
                _trackingMouseMove = TrackingState.NotTracking;
                _lineFeature = null;
            }
        }

        protected override async void OnToolMouseMove(MapViewMouseEventArgs e) {
            //All of this logic is to avoid unnecessarily updating the graphic position
            //for ~every~ mouse move. We skip any "intermediate" points in-between rapid
            //mouse moves.
            lock (_lock) {
                if (_trackingMouseMove == TrackingState.NotTracking)
                    return;
                else {
                    if (_workingLocation.HasValue) {
                        _lastLocation = e.ClientPoint;
                        return;
                    }
                    else {
                        _lastLocation = e.ClientPoint;
                        _workingLocation = e.ClientPoint;
                    }
                }
                _trackingMouseMove = TrackingState.Tracking;
            }

            //The code "inside" the QTR will execute for all points that
            //get "buffered" or "queued". This avoids having to spin up a QTR
            //for ~every~ point of ~every mouse move.
            await QueuedTask.Run(() => {

                var symReference = _pointSymbol.MakeSymbolReference();
                while (true) {
                    System.Windows.Point? point;
                    Polyline lineFeature = null;
                    IDisposable graphic = null;
                    lock (_lock) {
                        point = _lastLocation;
                        _lastLocation = null;
                        _workingLocation = point;

                        if (point == null || !point.HasValue) {//No new points came in while we updated the overlay
                            _workingLocation = null;
                            break;
                        }
                        else if (_lineFeature == null || _graphic == null) {//conflict with the mouse down,
                            //If this happens then we are done. A new line and point will be
                            //forthcoming from the SketchCompleted callback
                            _trackingMouseMove = TrackingState.NotTracking;
                            break;
                        }
                        lineFeature = _lineFeature;
                        graphic = _graphic;
                    }
                    //update the graphic overlay
                    var nearest = GeometryEngine.Instance.NearestPoint(lineFeature, MapView.Active.ClientToMap(point.Value));
                    this.UpdateOverlay(graphic, nearest.Point, symReference);
                }
            });
        }

        protected override Task HandleMouseDownAsync(MapViewMouseButtonEventArgs e) {
            //Select a line feature and place the initial graphic. Clear out any
            //previously placed graphic
            return QueuedTask.Run(() => {
                var mapPoint = this.ActiveMapView.ClientToMap(e.ClientPoint);
                var lineFeature = Module1.SelectLineFeature(mapPoint);

                if (lineFeature != null) {
                    var nearest = GeometryEngine.Instance.NearestPoint(lineFeature, mapPoint);
                    lock (_lock) {
                        _workingLocation = null;
                        _lineFeature = lineFeature;
                        if (_graphic != null)
                            _graphic.Dispose();
                        _graphic = this.AddOverlay(nearest.Point, _pointSymbol.MakeSymbolReference());
                        _trackingMouseMove = TrackingState.CanTrack;
                    }
                }
                else {
                    lock (_lock) {
                        _workingLocation = null;
                        _lineFeature = null;
                        if (_graphic != null)
                            _graphic.Dispose();
                        _graphic = null;
                        _trackingMouseMove = TrackingState.NotTracking;
                    }
                }
            });
        }

    }
}
