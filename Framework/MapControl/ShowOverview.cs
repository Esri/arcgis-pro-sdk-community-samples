/*

   Copyright 2019 Esri

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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Controls;

namespace MapControl {
    internal class ShowOverview : Button {

        private bool _isOpen = false;
        private OverviewWindow _overview = null;
        private static readonly object _lock = new object();

        public ShowOverview() {
            RegisterForActiveViewChanged();
        }
        protected override void OnClick() {
            if (_isOpen)
                return;
            _overview = new OverviewWindow();
            _overview.ViewContent = MapControlContentFactory.Create(
                MapView.Active.Map, MapView.Active.Extent, MapView.Active.ViewingMode);
            _overview.Closed += (s, e) => {
                _isOpen = false;
                lock (_lock) {
                    _overview = null;
                }
            };
            _overview.Show();
            _isOpen = true;
        }

        private void RegisterForActiveViewChanged() {
            ArcGIS.Desktop.Mapping.Events.ActiveMapViewChangedEvent.Subscribe((args) => {
                if (args.IncomingView == null)
                    return;
                lock (_lock) {
                    if (_overview == null)
                        return;
                    _overview.ViewContent = MapControlContentFactory.Create(
                        MapView.Active.Map, MapView.Active.Extent, MapView.Active.ViewingMode);
                }
            });
        }

    }
}
