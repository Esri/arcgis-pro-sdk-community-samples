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
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Core;

namespace MagnifierWindow
{
    internal class Magnifier_MapTool : MapTool
    {
        private MapControlWindow mapControlWindow = null;
        
        public Magnifier_MapTool()
        {
          //Only enable for 2D view
          if (MapView.Active.ViewingMode != ArcGIS.Core.CIM.MapViewingMode.Map)
            return;
          mapControlWindow = new MapControlWindow();
          mapControlWindow.Show();
        }
        protected override Task OnToolActivateAsync(bool active)
        {
          if (mapControlWindow == null && MapView.Active.ViewingMode == ArcGIS.Core.CIM.MapViewingMode.Map)
          {
            mapControlWindow = new MapControlWindow();
            mapControlWindow.Show();
          }
          return base.OnToolActivateAsync(active);
        }
        protected override Task OnToolDeactivateAsync(bool hasMapViewChanged)
        {
          if (mapControlWindow != null)
          {
            mapControlWindow.Close();
            mapControlWindow = null;
          }
          return base.OnToolDeactivateAsync(hasMapViewChanged);
        }

        protected override void OnToolKeyDown(MapViewKeyEventArgs k)
        {
          base.OnToolKeyDown(k);
          if (k.Key == System.Windows.Input.Key.Escape)
          {
            this.OnToolDeactivateAsync(false);
          }
        }

        protected override void OnToolMouseDown(MapViewMouseButtonEventArgs e)
        {
        }

        protected override void OnToolMouseMove(MapViewMouseEventArgs e)
        {
          //Only do this for 2D view
          if (MapView.Active.ViewingMode != ArcGIS.Core.CIM.MapViewingMode.Map)
            return;
          base.OnToolMouseMove(e);
          //Offset the map control window so right edge of the window is to the left and slightly offset from the mouse position in the main map view
          mapControlWindow.Left = MapView.Active.ClientToScreen(e.ClientPoint).X - mapControlWindow.Width - 30;
          mapControlWindow.Top = e.ClientPoint.Y;            
          mapControlWindow.viewModel.UpdateMapControlCamera(e.ClientPoint);
        }
    }
}
