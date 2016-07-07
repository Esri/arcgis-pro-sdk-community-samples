//   Copyright 2016 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Framework.Win32;
using ArcGIS.Desktop.Mapping;

namespace FeatureDynamicMenu
{
    /// <summary>
    /// cursor position
    /// </summary>
    public struct POINT
    {
        /// <summary>
        /// cursor X
        /// </summary>
        public int X;
        /// <summary>
        /// cursor Y
        /// </summary>
        public int Y;
    }
    /// <summary>
    /// Implementation of custom Map tool.
    /// </summary>
    class FeatureSelectionDynamic : MapTool
    {

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT pt);

        private System.Windows.Point _clickedPoint;

        private static readonly IDictionary<BasicFeatureLayer, List<long>> Selection = new Dictionary<BasicFeatureLayer, List<long>>();
        private static readonly object LockSelection = new object();

        private bool _showingContextMenu = false;
        private double _tolerance = 3;

        /// <summary>
        /// Define the tool as a sketch tool that draws a rectangle in screen space on the view.
        /// </summary>
        public FeatureSelectionDynamic()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Point;
            SketchOutputMode = SketchOutputMode.Screen;

            // binding sync: we need to lock this selection collection since it will be updated
            // asynchronously from a worker thread
            BindingOperations.EnableCollectionSynchronization(Selection, LockSelection);
        }
       

        internal static IDictionary<BasicFeatureLayer, List<long>> FeatureSelection => Selection;

        private void ShowContextMenu()
        {
            var contextMenu = FrameworkApplication.CreateContextMenu("DynamicMenu_DynamicFeatureSelection", () => MouseLocation);
            contextMenu.DataContext = this;
            contextMenu.Closed += (o, e) =>
            {
                this._showingContextMenu = false;
                // clear the list asynchronously
                lock (LockSelection)
                {
                    Selection.Clear();
                }
            };
            contextMenu.IsOpen = true;
        }
        public System.Windows.Point MouseLocation => _clickedPoint;
        public double PixelTolerance => _tolerance;

        private  async Task<List<long>>  GetFeaturesWithinGeometryAsync(FeatureLayer featureLayer)
        {
            return await QueuedTask.Run(() =>
            {
                List<long> oids = new List<long>();
                //Get the client point edges
                Point topLeft = new Point(_toolClickedPoint.X - PixelTolerance, _toolClickedPoint.Y + PixelTolerance);
                Point bottomRight = new Point(_toolClickedPoint.X + PixelTolerance, _toolClickedPoint.Y - PixelTolerance);

                //convert the client points to Map points
                MapPoint mapTopLeft = MapView.Active.ClientToMap(topLeft);
                MapPoint mapBottomRight = MapView.Active.ClientToMap(bottomRight);

                //create a geometry using these points
                Geometry envelopeGeometry = EnvelopeBuilder.CreateEnvelope(mapTopLeft, mapBottomRight);


                if (envelopeGeometry == null)
                    return null;

                //Spatial query to gather the OIDs of the features within the geometry
                SpatialQueryFilter spatialQueryFilter = new SpatialQueryFilter
                {
                    FilterGeometry = envelopeGeometry,
                    SpatialRelationship = SpatialRelationship.Intersects,

                };

                using (RowCursor rowCursor = featureLayer.Search(spatialQueryFilter))
                {

                    while (rowCursor.MoveNext())
                    {
                        using (Row row = rowCursor.Current)
                        {
                            var id = row.GetObjectID();
                            oids.Add(id);

                        }
                    }
                }

                return oids;
            });
        }
        /// <summary>
        /// Occurs when a mouse button is pressed on the view.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnToolMouseDown(MapViewMouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                e.Handled = true; //Handle the event args to get the call to the corresponding async method
        }

        private Point _toolClickedPoint;
        /// <summary>
        /// Occurs when the OnToolMouseDown event is handled.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected async override Task HandleMouseDownAsync(MapViewMouseButtonEventArgs e)
        {
            lock (LockSelection)
            {
                Selection.Clear();
            }
            POINT pt;
            GetCursorPos(out pt);
            _clickedPoint = new Point(pt.X, pt.Y); //Point on screen to show the context menu

            await QueuedTask.Run(async () =>
            {
                //Convert the clicked point in client coordinates to the corresponding map coordinates.
                _toolClickedPoint = e.ClientPoint;

                var allLyrs = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>(); //get all the layers in teh TOC for that map

                if (allLyrs == null)
                    return;

                List<long> oidList = new List<long>();                

                foreach (var lyr in allLyrs)
                {
                    oidList = await GetFeaturesWithinGeometryAsync(lyr); //gets the oids of all the features within 3 pixels of the point clicked
                    if (oidList != null && oidList.Count > 0)
                    {
                        lock (LockSelection)
                        {
                            Selection.Add(lyr, oidList);
                        }
                    }
                }                
            });

           // if (Selection.Count > 0)
           ShowContextMenu();
        }
    }
}
