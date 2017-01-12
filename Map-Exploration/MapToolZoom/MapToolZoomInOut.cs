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
using System.Windows.Input;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace MapToolZoom
{
    internal class MapToolZoomInOut : MapTool
    {
        public MapToolZoomInOut()
        {
        }

        protected override Task OnToolActivateAsync(bool active)
        {
            return base.OnToolActivateAsync(active);
        }

        protected override void OnToolMouseDown(MapViewMouseButtonEventArgs e)
        {
            // On mouse down check if the mouse button pressed is:
            // the left mouse button to handle zoom in
            // or the right mouse button to handle zoom out
            // If it is handle the event.
            switch (e.ChangedButton)
            {
                case MouseButton.Right:
                    e.Handled = true;
                    break;
                case MouseButton.Left:
                    e.Handled = true;
                    break;
            }
        }

        protected override Task HandleMouseDownAsync(MapViewMouseButtonEventArgs e)
        {
            // Get the map coordinates from the click point and change the Camera to zoom in or out.
            return QueuedTask.Run(() =>
            {
                var mapClickPnt = MapView.Active.ClientToMap(e.ClientPoint);                
                ActiveMapView.LookAt(mapClickPnt, TimeSpan.FromSeconds(1));
                // zoom out
                if (e.ChangedButton == MouseButton.Right)
                {
                    ActiveMapView.ZoomOutFixed(TimeSpan.FromSeconds(1));
                }
                // zoom in
                else if (e.ChangedButton == MouseButton.Left)
                {
                    ActiveMapView.ZoomInFixed(TimeSpan.FromSeconds(1));
                }
            });
        }

        protected override void OnToolKeyDown(MapViewKeyEventArgs k)
        {
            // using key up and down in order to zoom out and in
            // if those keys are used we need to mark them as handled
            if (k.Key == Key.Up || k.Key == Key.Down)
                k.Handled = true;
            base.OnToolKeyDown(k);
        }

        protected override Task HandleKeyDownAsync(MapViewKeyEventArgs k)
        {
            // only called when 'handled' in OnToolKeyDown
            if (k.Key == Key.Up)
            {
                // Key.Up => zoom out
                return ActiveMapView.ZoomOutFixedAsync(TimeSpan.FromSeconds(1));
            }
            // Key.Down => zoom in
            // else if (k.Key == Key.Down)
            {
                return ActiveMapView.ZoomInFixedAsync(TimeSpan.FromSeconds(1));
            }
        }
    }
}
