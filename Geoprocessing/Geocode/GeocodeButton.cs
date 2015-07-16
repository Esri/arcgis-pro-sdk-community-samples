//Copyright 2015 Esri

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
using System.Windows;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Mapping;
using ArcGIS.Desktop.Mapping;

namespace Geocode
{
    internal class GeocodeButton : Button
    {
        GeocodeWindow _dlg = null;

        protected override void OnClick()
        {
            if (_dlg != null)
                return;//already shown
            _dlg = new GeocodeWindow();
            _dlg.Closed += dlg_Closed;
            _dlg.Topmost = true;
            _dlg.Show();
        }

        void dlg_Closed(object sender, EventArgs e)
        {
            //try and clean up
            GeocodeUtils.RemoveFromMapOverlay(MapView.Active);
            //geocode dialog was closed
            _dlg.Closed -= dlg_Closed;
            _dlg = null;
        }
    }
}
