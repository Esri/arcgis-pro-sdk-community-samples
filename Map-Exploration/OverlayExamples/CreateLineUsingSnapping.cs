/*

   Copyright 2016 Esri

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
using System.Windows;
using System.Windows.Input;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;

namespace ForSteffen.Tools {

    public enum MapToolState {
        Selecting = 0,
        Digitizing
    }
    class CreateLineUsingSnapping : MapTool {

        private CIMSymbol _sketchSymbol = null;
        private FeatureLayer _wiroWisRoute = null;
        private MapToolState _toolState = MapToolState.Selecting;
        private bool _ignoreEvents = false;
        private long _oid = -1;
        private Point _lastClickedPoint;
        public CreateLineUsingSnapping() {
            IsSketchTool = true;
            UseSnapping = true;
            SketchType = SketchGeometryType.Line;
            SketchOutputMode = SketchOutputMode.Map;
        }

        /// <summary>
        /// Occurs when the tool is activated.
        /// </summary>
        /// <param name="hasMapViewChanged">A value indicating if the active <see cref="T:ArcGIS.Desktop.Mapping.MapView"/> has changed.</param>
        /// <returns>
        /// A Task that represents a tool activation event.
        /// </returns>
        protected override async Task OnToolActivateAsync(bool hasMapViewChanged) {
            //Sketch symbol - only need to set this once per session if we
            //are not changing it.
            if (_sketchSymbol == null) {
                _sketchSymbol = await Module1.CreateLineSymbolAsync();
                this.SketchSymbol = _sketchSymbol.MakeSymbolReference();
            }

            //Snapping on the Feature Layer
            _wiroWisRoute = await Module1.ConfigureWiroWisRouteLayerForToolAsync();
        }

        protected override Task OnToolDeactivateAsync(bool hasMapViewChanged) {
            _wiroWisRoute = null;
            _toolState = MapToolState.Selecting;
            return base.OnToolDeactivateAsync(hasMapViewChanged);
        }

        /// <summary>
        /// Occurs when a mouse button is pressed on the view.
        /// </summary>
        /// <remarks>
        /// This method is intended to perform synchronous operations associated with a mouse down event. To perform any asynchronous operations set the handled property
        ///             on the <see cref="T:ArcGIS.Desktop.Mapping.MapViewMouseButtonEventArgs"/> to true and override the <see cref="M:ArcGIS.Desktop.Mapping.MapTool.HandleMouseDownAsync(ArcGIS.Desktop.Mapping.MapViewMouseButtonEventArgs)"/> virtual method.
        /// </remarks>
        /// <param name="e">A <see cref="T:ArcGIS.Desktop.Mapping.MapViewMouseButtonEventArgs"/> that contains the event data.</param>
        /// <example>
        /// <code title="Get Map Coordinates" description="Create a tool that allows you to click in the view and return the point in map coordinates that was clicked." source="..\..\ArcGIS\SharedArcGIS\SDK\Examples\ArcGIS.Desktop.Mapping\MapExploration\GetMapCoordinates.cs" lang="CS"/>
        /// </example>
        protected override void OnToolMouseDown(MapViewMouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left)
                _lastClickedPoint = e.ClientPoint;
        }

        /// <summary>
        /// Occurs when a sketch is modified.
        /// </summary>
        /// <returns>
        /// True if the sketch modified event was handled.
        /// </returns>
        protected override async Task<bool> OnSketchModifiedAsync() {
            if (_ignoreEvents)
                return true;
            _ignoreEvents = true;

            var geometry = await ActiveMapView.GetCurrentSketchAsync();
            try {
                var sketchLine = geometry as Polyline;
                //So this is a special case. Even though the sketch is a line, we currently
                //have only digitized one point so the sketch line is not actually a valid line
                //until two points have been digitized
                if (_toolState == MapToolState.Selecting) {
                    MapPoint mapPoint = null;
                    if (sketchLine.PointCount == 0) {
                        //we have only digitized one point and do not have a valid line in the sketch
                        //use the last clicked point instead
                        mapPoint = await QueuedTask.Run(() => ActiveMapView.ClientToMap(_lastClickedPoint));
                    }
                    else {
                        mapPoint = sketchLine.Points[0];
                    }
                    _oid = await Module1.SelectLayerFeatureAsync(_wiroWisRoute, mapPoint);
                    if (_oid >= 0) _toolState = MapToolState.Digitizing;
                    else await this.ClearSketchAsync(); //Only continue sketching if a feature is selected
                }
                else {
                    //Force a two point line
                    await this.FinishSketchAsync();
                }
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine("Exception ex: {0}", ex.ToString());
                await this.ClearSketchAsync();
                _toolState = MapToolState.Selecting;
            }
            
            _ignoreEvents = false;
            return true;
        }

        protected override Task<bool> OnSketchCompleteAsync(Geometry geometry) {
            _toolState = MapToolState.Selecting;//reset

            if (ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                "Soll das Schadensereignis gespeichert werden ?",
                "Schadensereignis erfassen",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information,
                MessageBoxResult.Yes) != MessageBoxResult.Yes) {
                //no point going further
                _wiroWisRoute.ClearSelection();
                this.ClearSketchAsync();//we do not await for this (because we don't need to)
                return Task.FromResult(true);
            }

            return QueuedTask.Run(() => {
                var inspector = new Inspector(true); //Argument must be = true
                inspector.Load(_wiroWisRoute, _oid);
                int rt_id = (int)(inspector["RT_ID"] is DBNull ? -1 : inspector["RT_ID"]);

                var sketchLine = geometry as Polyline;

                //How far along the line?
                GeometryEngine.LeftOrRightSide whichSide;
                double dOnCurve1, dOnCurve2, dFromCurve;
                GeometryEngine.QueryPointAndDistance(inspector["SHAPE"] as Multipart,
                    GeometryEngine.SegmentExtension.NoExtension, sketchLine.Points[0],
                    GeometryEngine.AsRatioOrLength.AsLength, out dOnCurve1, out dFromCurve,
                    out whichSide);
                GeometryEngine.QueryPointAndDistance(inspector["SHAPE"] as Multipart,
                    GeometryEngine.SegmentExtension.NoExtension, sketchLine.Points[sketchLine.PointCount - 1],
                    GeometryEngine.AsRatioOrLength.AsLength, out dOnCurve2, out dFromCurve,
                    out whichSide);

                inspector.Clear();

                //Set up operation to create the line feature
                var op = new EditOperation();
                op.Name = string.Format("Create Line Event for {0}", rt_id);
                var attrib = new Dictionary<string, object>();
                attrib.Add("RT_ID", rt_id);
                attrib.Add("VON", dOnCurve1);
                attrib.Add("METER", dOnCurve2);
                attrib.Add("SCHADEN", 0);
                attrib.Add("WER", Environment.UserName);
                attrib.Add("DATUM", DateTime.Now.Date);

                op.Create(ActiveMapView.Map.StandaloneTables.First((tbl) => tbl.Name == "SCHADEN"), attrib);
                _wiroWisRoute.ClearSelection();//no need to do it on the callback as we are already on 
                                               //the background thread of Pro
                                               
                return op.Execute();
            });
        }
    }
}
