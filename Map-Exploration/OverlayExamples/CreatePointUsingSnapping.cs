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
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace ForSteffen.Tools {
    class CreatePointUsingSnapping : MapTool {

        private CIMSymbol _sketchSymbol = null;
        private FeatureLayer _wiroWisRoute = null;
        public CreatePointUsingSnapping() {
            IsSketchTool = true;
            UseSnapping = true;
            SketchType = SketchGeometryType.Point;
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
                _sketchSymbol = await Module1.CreatePointSymbolAsync();
                this.SketchSymbol = _sketchSymbol.MakeSymbolReference();
            }

            //Snapping on the Feature Layer
            _wiroWisRoute = await Module1.ConfigureWiroWisRouteLayerForToolAsync();
        }

        /// <summary>
        /// Occurs when the tool is deactivated.
        /// </summary>
        /// <param name="hasMapViewChanged">A value indicating if the active <see cref="T:ArcGIS.Desktop.Mapping.MapView"/> has changed.</param>
        /// <returns>
        /// A Task that represents a tool deactivation event.
        /// </returns>
        protected override Task OnToolDeactivateAsync(bool hasMapViewChanged) {
            _wiroWisRoute = null;
            return base.OnToolDeactivateAsync(hasMapViewChanged);
        }

        /// <summary>
        /// OnSketchCompleteAsync will be called after the sketch has been completed.
        /// </summary>
        /// <remarks>We assume that the user has digitized the point event on top of a WIRO_WIS_ROUTE
        /// feature because of the snapping settings</remarks>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry) {
            
            var op = new EditOperation();

            var selectedOid = await QueuedTask.Run(() => {

                var oid = Module1.SelectLayerFeature(_wiroWisRoute, geometry as MapPoint);
                
                if (oid >= 0) {
                    var inspector = new Inspector(true);//Argument must be = true
                    inspector.Load(_wiroWisRoute, oid);
                    int rt_id = (int) (inspector["RT_ID"] is DBNull ? -1 : inspector["RT_ID"]);

                    //How far along the line?
                    GeometryEngine.LeftOrRightSide whichSide;
                    double dOnCurve, dFromCurve;
                    GeometryEngine.QueryPointAndDistance(inspector["SHAPE"] as Multipart,
                        GeometryEngine.SegmentExtension.NoExtension, geometry as MapPoint,
                        GeometryEngine.AsRatioOrLength.AsLength, out dOnCurve, out dFromCurve,
                        out whichSide);

                    inspector.Clear();

                    //Set up the operation to create the point event
                    op.Name = string.Format("Create Point Event for {0}", rt_id);
                    var attrib = new Dictionary<string, object>();
                    attrib.Add("RT_ID", rt_id);
                    attrib.Add("METER", dOnCurve);
                    attrib.Add("SCHADEN", 0);
                    attrib.Add("ERFASSER", Environment.UserName);
                    attrib.Add("DATUM", DateTime.Now.Date);

                    //Let's use the "callback" of Create to clear the selection after the
                    //Edit Operation is executed
                    op.Create(ActiveMapView.Map.StandaloneTables.First((tbl) => tbl.Name == "SCHADEN"), attrib,
                        (id) => QueuedTask.Run(() => _wiroWisRoute.ClearSelection()));
                    
                }
                return oid ;
            });

            var addThePointEvent = MessageBoxResult.No;
            if (selectedOid >= 0) {
                addThePointEvent = ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                    "Soll das Punktereignis gespeichert werden ?",
                    "Punktereignis erfassen",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information,
                    MessageBoxResult.Yes);
            }
            //we only actually execute the operation if the user clicks "yes"....
            //otherwise it simply goes out of scope
            return addThePointEvent != MessageBoxResult.Yes || await op.ExecuteAsync();

        }
    }
}
