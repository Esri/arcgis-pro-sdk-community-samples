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
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;
using System.Windows;
using ArcGIS.Core;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace ForSteffen {
    internal class Module1 : Module {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current {
            get {
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("ForSteffen_Module"));
            }
        }

        /// <summary>
        /// Are we on the UI thread?
        /// </summary>
        internal static bool OnUIThread {
            get {
                return System.Windows.Application.Current.Dispatcher.CheckAccess();
            }
        }

        /// <summary>
        /// This method can be safely called from the UI.
        /// </summary>
        /// <remarks>If multiple features are selected just the ObjectID of the first feature
        /// in the selected set is returned</remarks>
        /// <param name="point"></param>
        /// <returns>Task of object id of the selected feature or -1</returns>
        internal static Task<long> SelectLayerFeatureAsync(FeatureLayer featureLayer, MapPoint point) {
            return QueuedTask.Run(() => SelectLayerFeature(featureLayer, point));
        }

        /// <summary>
        /// This method must be called on the MCT
        /// </summary>
        /// <remarks>If multiple features are selected just the ObjectID of the first feature
        /// in the selected set is returned</remarks>
        /// <param name="point"></param>
        /// <returns>The object id of the selected feature or -1</returns>
        internal static long SelectLayerFeature(FeatureLayer featureLayer, MapPoint point) {
            if (Module1.OnUIThread)
                throw new CalledOnWrongThreadException();

            var pt = MapView.Active.MapToClient(point);

            double llx = pt.X - 5;
            double lly = pt.Y - 5;
            double urx = pt.X + 5;
            double ury = pt.Y + 5;

            EnvelopeBuilder envBuilder = new EnvelopeBuilder(MapView.Active.ClientToMap(new Point(llx, lly)),
                                                             MapView.Active.ClientToMap(new Point(urx, ury)));

            //Select the feature
            var select = featureLayer.Select(new SpatialQueryFilter() {
                FilterGeometry = envBuilder.ToGeometry(),
                SpatialRelationship = SpatialRelationship.Intersects
            });
            return select.GetCount() > 0 ? select.GetObjectIDs()[0] : -1;
        }

        internal static Task<FeatureLayer> ConfigureWiroWisRouteLayerForToolAsync() {
            //General Snapping
            Snapping.IsEnabled = true;
            Snapping.SetSnapMode(SnapMode.Edge, true);
            Snapping.SetSnapMode(SnapMode.End, true);
            Snapping.SetSnapMode(SnapMode.Intersection, true);

            //Snapping on the Feature Layer
            var wiroWisRoute = MapView.Active.Map.GetLayersAsFlattenedList().FirstOrDefault(
                                    (layer) => layer.Name == "WIRO_WIS_ROUTE") as FeatureLayer;

            //GetDefinition and SetDefinition must be called inside QueuedTask
            return QueuedTask.Run(() => {
                var layerDef = wiroWisRoute.GetDefinition() as CIMGeoFeatureLayerBase;
                if (!layerDef.Snappable) {
                    layerDef.Snappable = true;
                    wiroWisRoute.SetDefinition(layerDef);
                }
                return wiroWisRoute;
            });
        }

        internal static Task<CIMLineSymbol> CreateLineSymbolAsync() {
            return QueuedTask.Run(() => {

                //marker.MarkerPlacement = new CIMMarkerPlacementOnLine() {
                //    AngleToLine = true,
                //    RelativeTo = PlacementOnLineRelativeTo.LineEnd
                //};

                var lineStroke = SymbolFactory.ConstructStroke(ColorFactory.Red, 4.0);
                var marker = SymbolFactory.ConstructMarker(ColorFactory.Red, 12, SimpleMarkerStyle.Circle);
                marker.MarkerPlacement = new CIMMarkerPlacementOnVertices() {
                    AngleToLine = true,
                    PlaceOnEndPoints = true,
                    Offset = 0
                };
                return new CIMLineSymbol() {
                    SymbolLayers = new CIMSymbolLayer[2] { marker, lineStroke }
                };
            });
        }

        internal static Task<CIMPointSymbol> CreatePointSymbolAsync() {
            return QueuedTask.Run(() => {
                return SymbolFactory.ConstructPointSymbol(ColorFactory.Red, 14, SimpleMarkerStyle.Pushpin);
            });
        }

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload() {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

        #endregion Overrides

    }
}
