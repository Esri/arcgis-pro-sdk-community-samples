//   Copyright 2017 Esri
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
using System.Windows.Threading;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Events;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
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

        

        private static readonly IDictionary<BasicFeatureLayer, List<long>> Selection = new Dictionary<BasicFeatureLayer, List<long>>();
        private static readonly object LockSelection = new object();

        private bool _showingContextMenu = false;

        private System.Windows.Point _clickedPoint;
        public System.Windows.Point MouseLocation => _clickedPoint;



        /// <summary>
        /// Define the tool as a sketch tool that draws a point in screen space on the view.
        /// </summary>
        public FeatureSelectionDynamic()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Rectangle;
            SketchOutputMode = SketchOutputMode.Map;

            // binding sync: we need to lock this selection collection since it will be updated
            // asynchronously from a worker thread
            BindingOperations.EnableCollectionSynchronization(Selection, LockSelection);

           OverlayControlID = "FeatureDynamicMenu_EmbeddedControl";

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


        private SubscriptionToken _token = null;
        protected override Task OnToolActivateAsync(bool hasMapViewChanged)
        {
            GetLayersFromActiveMap();        
            return base.OnToolActivateAsync(hasMapViewChanged);
        }

        protected override Task OnToolDeactivateAsync(bool hasMapViewChanged)
        {
            return base.OnToolDeactivateAsync(hasMapViewChanged);
        }


        protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            List<long> oids = new List<long>();
            var vm = OverlayEmbeddableControl as EmbeddedControlViewModel;

            //POINT pt;
            //GetCursorPos(out pt);
            //_clickedPoint = new Point(pt.X, pt.Y); //Point on screen to show the context menu

            return QueuedTask.Run(() =>
            {
                SpatialQueryFilter spatialQueryFilter = new SpatialQueryFilter
                {
                    FilterGeometry = geometry,
                    SpatialRelationship = SpatialRelationship.Intersects,

                };
                var selection = vm?.SelectedLayer.Select(spatialQueryFilter);
                if (selection == null)
                    return false;
                oids.AddRange(selection.GetObjectIDs());

                lock (LockSelection)
                {
                    Selection.Add(vm?.SelectedLayer, oids);
                }

                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() => ShowContextMenu()));

                return true;
            });
        }

        private void GetLayersFromActiveMap()
        {
            var vm = OverlayEmbeddableControl as EmbeddedControlViewModel;
            vm?.Layers.Clear();
            Map map = MapView.Active.Map;
            if (map == null)
                return;
            vm.MapName = map.Name;
            var layers = map.GetLayersAsFlattenedList().OfType<BasicFeatureLayer>();
            lock (LockSelection)
            {
                foreach (var layer in layers)
                {
                    vm.Layers.Add(layer);
                }

                if (vm.Layers.Count > 0)
                    vm.SelectedLayer = vm.Layers[0];
            }
        }
    }
}
