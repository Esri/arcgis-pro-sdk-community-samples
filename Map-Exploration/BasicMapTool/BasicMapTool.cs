/*

   Copyright 2018 Esri

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
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace BasicMapTool {
    internal class BasicMapTool : MapTool {

        private bool _zMode = false;

        public BasicMapTool() : base() {
            this.OverlayControlID = "BasicMapTool_BasicEmbeddableControl";
            //Embeddable control can be resized
            OverlayControlCanResize = true;
            //Specify ratio of 0 to 1 to place the control
            OverlayControlPositionRatio = new System.Windows.Point(0, 0); //top left
        }
        protected override Task OnToolActivateAsync(bool active) {
            return base.OnToolActivateAsync(active);
        }

        protected override void OnToolKeyDown(MapViewKeyEventArgs k) {
            //We will do some basic key handling to allow panning
            //from the arrow keys
            if (k.Key == Key.Left ||
                k.Key == Key.Right ||
                k.Key == Key.Up ||
                k.Key == Key.Down)
                k.Handled = true;
            base.OnToolKeyDown(k);
        }

        protected override Task HandleKeyDownAsync(MapViewKeyEventArgs k) {
            var camera = MapView.Active.Camera;

            double dx = MapView.Active.Extent.Width / 20;
            double dy = MapView.Active.Extent.Height / 20;
            double dz = 20.0;//20 meters vertical change per key stroke

            bool shiftKey = (Keyboard.Modifiers & ModifierKeys.Shift)
                                     == ModifierKeys.Shift;

            //When in 3D mode use the Shift key to switch from "Y" position change
            //which is 'Up','Down' in 2D but is 'forward','back' in 3D to
            //"Z" position change which is 'Up','Down' in 3D
            if (!_zMode && ActiveMapView.ViewingMode != MapViewingMode.Map)
                _zMode = shiftKey;

            switch (k.Key) {
                case Key.Left:
                    camera.X -= dx;
                    break;
                case Key.Right:
                    camera.X += dx;
                    break;
                case Key.Up:
                    if (_zMode) {
                        camera.Z += dz;
                    }
                    else {
                        camera.Y += dy;
                    }
                    break;
                case Key.Down:
                    if (_zMode) {
                        camera.Z -= dz;
                    }
                    else {
                        camera.Y -= dy;
                    }
                    break;
            }
            return MapView.Active.ZoomToAsync(camera, new TimeSpan(0, 0, 0, 0, 250));
        }

        protected override void OnToolKeyUp(MapViewKeyEventArgs k) {
            _zMode = false;
            base.OnToolKeyUp(k);
        }
        protected override void OnToolMouseDown(MapViewMouseButtonEventArgs e) {
            //On mouse down check if the mouse button pressed is the left mouse button. If it is handle the event.
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                e.Handled = true;
        }

        protected override Task HandleMouseDownAsync(MapViewMouseButtonEventArgs e) {
            //Get the instance of the ViewModel
            var vm = OverlayEmbeddableControl as BasicEmbeddableControlViewModel;
            if (vm == null)
                return Task.FromResult(0);

            //Get the map coordinates from the click point and set the property on the ViewModel.
            return QueuedTask.Run(() =>
            {
                var mapPoint = MapView.Active.ClientToMap(e.ClientPoint);
                var sb = new StringBuilder();
                sb.AppendLine(string.Format("X: {0}", mapPoint.X));
                sb.Append(string.Format("Y: {0}", mapPoint.Y));
                if (mapPoint.HasZ) {
                    sb.AppendLine();
                    sb.Append(string.Format("Z: {0}", mapPoint.Z));
                }
                vm.Text = sb.ToString();
            });
        }
    }
}
