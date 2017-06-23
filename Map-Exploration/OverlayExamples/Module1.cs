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
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;
using System.Windows;
using ArcGIS.Core;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace OverlayExamples
{
    /// <summary>
    /// This sample contains three different examples of working with Pro's graphic overlay
    /// </summary>
    /// <remarks>
    /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
    /// 1. Make sure that the Sample data is unzipped in c:\data
    /// 1. Before you run the sample verify that the project C:\data\SDK\SDK 1.1.aprx"C:\Data\FeatureTest\FeatureTest.aprx" is present since this is required to run the sample.
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open. 
    /// 1. Open the "C:\Data\FeatureTest\FeatureTest.aprx" project.
    /// 1. Click on the Add-In tab on the ribbon.
    /// Playing with the add-in:  
    /// There are 3 examples of working with the graphic overlay:  
    /// 1. "Add Overlay:" Sketch a line anywhere. Each time you sketch, the previous graphic is erased
    /// ![UI](Screenshots/Screen1.png)
    /// 1. "Add Overlay With Snapping:" Sketch a line anywhere but use snapping. The graphic will snap to existing line features
    /// ![UI](Screenshots/Screen2.png)
    /// 1. "Add Overlay Track Mouse:" Digitize a point on top of a line. You have to click on a line feature. (2D Only)
    /// For the third example, hold the mouse down to drag the graphic back and forth along the 2D line.
    /// ![UI](Screenshots/Screen3.png)
    /// Each mouse click will place a new graphic (and erase the previous one).
    /// ![UI](Screenshots/Screen4.png)
    /// </remarks>
    internal class Module1 : Module {
        private static Module1 _this = null;
        private static bool _messageShown = false;
        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current
        {
            get
            {
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("OverlayExamples_Module"));
            }
        }

        public static bool MessageShown {
            get {
                return _messageShown;
            }
            set {
                _messageShown = value;
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
        /// This method must be called on the MCT
        /// </summary>
        /// <remarks>If multiple features are selected just the ObjectID of the first feature
        /// in the selected set is returned</remarks>
        /// <param name="point"></param>
        /// <returns>The object id of the selected feature or -1</returns>
        internal static Polyline SelectLineFeature(MapPoint point) {
            if (OnUIThread)
                throw new CalledOnWrongThreadException();

            var pt = MapView.Active.MapToClient(point);

            double llx = pt.X - 5;
            double lly = pt.Y - 5;
            double urx = pt.X + 5;
            double ury = pt.Y + 5;

            EnvelopeBuilder envBuilder = new EnvelopeBuilder(MapView.Active.ClientToMap(new Point(llx, lly)),
                                                             MapView.Active.ClientToMap(new Point(urx, ury)));

            //Just get feature layers that are line types
            var selection = MapView.Active.SelectFeatures(envBuilder.ToGeometry()).Where(
                k => k.Key.ShapeType == esriGeometryType.esriGeometryPolyline
            ).ToList();

            Polyline selectedLine = null;
            if (selection.Count() > 0) {
                //return the first of the selected features
                var flayer = selection.First().Key;
                var oid = selection.First().Value[0];
                var inspector = new Inspector();
                inspector.Load(flayer, oid);
                selectedLine = inspector["SHAPE"] as Polyline;
            }
            return selectedLine;
        }

        internal static bool AreThereAnyLineLayers() {
            return MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Count(
                l => l.ShapeType == esriGeometryType.esriGeometryPolyline) > 0;
        }

        internal static async Task ConfigureSnappingAsync() {
            //General Snapping
            Snapping.IsEnabled = true;
            Snapping.SetSnapMode(SnapMode.Edge, true);
            Snapping.SetSnapMode(SnapMode.End, true);
            Snapping.SetSnapMode(SnapMode.Intersection, true);

            //Snapping on any line Feature Layers
            var flayers = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(
                l => l.ShapeType == esriGeometryType.esriGeometryPolyline).ToList();

            if (flayers.Count() > 0) {
                //GetDefinition and SetDefinition must be called inside QueuedTask
                await QueuedTask.Run(() => {
                    foreach (var fl in flayers) {
                        var layerDef = fl.GetDefinition() as CIMGeoFeatureLayerBase;
                        if (!layerDef.Snappable) {
                            layerDef.Snappable = true;
                            fl.SetDefinition(layerDef);
                        }
                    }
                });
            }
        }

        /// <summary>Create a linesymbol with circle markers on the ends</summary>
        internal static Task<CIMLineSymbol> CreateLineSymbolAsync() {
            return QueuedTask.Run(() => {
                var lineStroke = SymbolFactory.Instance.ConstructStroke(ColorFactory.Instance.RedRGB, 4.0);
                var marker = SymbolFactory.Instance.ConstructMarker(ColorFactory.Instance.RedRGB, 12, SimpleMarkerStyle.Circle);
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

        /// <summary>
        /// Create a point symbol
        /// </summary>
        /// <returns></returns>
        internal static Task<CIMPointSymbol> CreatePointSymbolAsync() {
            return QueuedTask.Run(() => {
                return SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.RedRGB, 14, SimpleMarkerStyle.Circle);
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
