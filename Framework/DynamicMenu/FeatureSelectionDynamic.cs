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

        /// <summary>
        /// Define the tool as a sketch tool that draws a rectangle in screen space on the view.
        /// </summary>
        public FeatureSelectionDynamic()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Rectangle;
            SketchOutputMode = SketchOutputMode.Screen;

            // binding sync: we need to lock this selection collection since it will be updated
            // asynchronousely from a worker thread
            BindingOperations.EnableCollectionSynchronization(Selection, LockSelection);
        }
        /// <summary>
        /// Called when a sketch is completed.
        /// </summary>
        protected async override Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            lock (LockSelection)
            {
                Selection.Clear();
            }
            POINT pt;
            GetCursorPos(out pt);
            _clickedPoint = new Point(pt.X, pt.Y);
            await QueuedTask.Run(() =>
            {
                var mapView = MapView.Active;

                //Get the features that intersect the sketch geometry.
                var features = mapView?.GetFeatures(geometry);

                if (features == null)
                    return false;

                var firstLyr = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(); //get the first layer in the map

                if (firstLyr == null)
                    return false;

                var oidList = features[firstLyr]; //gets the OIds of all the features selected for the first layer in the map.

                // add to the list asynchronously
                lock (LockSelection)
                {
                    Selection.Add(firstLyr, oidList); //adding the first layer selected and its OIDs
                }

                return true;

            });
            if (Selection.Count > 0)
                ShowContextMenu();

            return true;
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
    }
}
