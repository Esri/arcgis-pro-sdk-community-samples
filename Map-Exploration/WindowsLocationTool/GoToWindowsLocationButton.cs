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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Device.Location;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.CIM;

namespace WindowsLocationTool
{
    internal class GoToWindowsLocationButton : Button
    {
        private IDisposable _graphic = null;
        private CIMPointSymbol _pointSymbol = null;

        protected override async void OnClick()
        {
            if (_pointSymbol == null)
                _pointSymbol = await Module1.CreatePointSymbolAsync();
            GeoCoordinateWatcher watcher = new GeoCoordinateWatcher();
            watcher.PositionChanged += Watcher_PositionChanged;
            // Do not suppress prompt, and wait 1000 milliseconds to start.
            bool bStarted = watcher.TryStart(false, TimeSpan.FromMilliseconds(2000));
            if (!bStarted)
            {
                MessageBox.Show ("GeoCoordinateWatcher timed out on start.");
            }
        }

        private void Watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            GeoCoordinate coord = e.Position.Location;
            var mapView = MapView.Active;
            if (mapView == null) return;
            if (coord.IsUnknown != true)
            {
                System.Diagnostics.Debug.WriteLine("Lat: {0}, Long: {1}",
                    coord.Latitude,
                    coord.Longitude);
                ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
                {
                    MapPoint zoomToPnt = MapPointBuilder.CreateMapPoint(coord.Longitude, coord.Latitude, SpatialReferences.WGS84);
                    var geoProject = GeometryEngine.Instance.Project (zoomToPnt, SpatialReferences.WebMercator) as MapPoint;
                    var expandSize = 200.0;
                    var minPoint = MapPointBuilder.CreateMapPoint(geoProject.X - expandSize, geoProject.Y - expandSize, SpatialReferences.WebMercator);
                    var maxPoint = MapPointBuilder.CreateMapPoint(geoProject.X + expandSize, geoProject.Y + expandSize, SpatialReferences.WebMercator);
                    Envelope env = EnvelopeBuilder.CreateEnvelope(minPoint, maxPoint);
                    mapView.ZoomTo(env, new TimeSpan(0, 0, 3));
                    _graphic = MapView.Active.AddOverlay(zoomToPnt, _pointSymbol.MakeSymbolReference());
                });
            }
            else
            {
                MessageBox.Show("Unknown latitude and longitude.  Check your control panel for Location Privacy settings.");
            }
        }
    }
}
