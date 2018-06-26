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
using System.Windows.Data;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System.Windows;
using ArcGIS.Core.CIM;

namespace MapToolWithDynamicMenu
{
    internal class MapToolShowMenu : MapTool
    {
        public MapToolShowMenu()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Point;
            SketchOutputMode = SketchOutputMode.Screen;
        }

        protected override Task OnToolActivateAsync(bool active)
        {
            return base.OnToolActivateAsync(active);
        }

        protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            var bottomRight = new Point();
            IList<Tuple<string, string, long>> tripleTuplePoints = new List<Tuple<string, string, long>>();
            var hasSelection = await QueuedTask.Run(() =>
            {
                // geometry is a point
                var clickedPnt = geometry as MapPoint;
                if (clickedPnt == null) return false;
                // pixel tolerance
                var tolerance = 3;
                //Get the client point edges
                var topLeft = new Point(clickedPnt.X - tolerance, clickedPnt.Y + tolerance);
                bottomRight = new Point(clickedPnt.X + tolerance, clickedPnt.Y - tolerance);
                //convert the client points to Map points
                var mapTopLeft = MapView.Active.ClientToMap(topLeft);
                var mapBottomRight = MapView.Active.ClientToMap(bottomRight);
                //create a geometry using these points
                Geometry envelopeGeometry = EnvelopeBuilder.CreateEnvelope(mapTopLeft, mapBottomRight);
                if (envelopeGeometry == null) return false;
                //Get the features that intersect the sketch geometry.
                var result = ActiveMapView.GetFeatures(geometry);
                foreach (var kvp in result)
                {
                    var bfl = kvp.Key;
                    // only look at points
                    if (kvp.Key.ShapeType != esriGeometryType.esriGeometryPoint) continue;
                    var layerName = bfl.Name;
                    var oidName = bfl.GetTable().GetDefinition().GetObjectIDField();
                    foreach (var oid in kvp.Value)
                    {
                        tripleTuplePoints.Add(new Tuple<string, string, long>(layerName, oidName, oid));
                    }
                }
                return true;
            });
            if (hasSelection)
            {
                ShowContextMenu(bottomRight, tripleTuplePoints);
            }
            return true;
        }

        private void ShowContextMenu(System.Windows.Point screenLocation, IList<Tuple<string, string, long>> tripleTuplePoints)
        {
            var contextMenu = FrameworkApplication.CreateContextMenu("DynamicMenu_SelectPoint", () => screenLocation);
            if (contextMenu == null) return;
            DynamicSelectPointMenu.SetMenuPoints(tripleTuplePoints);
            contextMenu.Closed += (o, e) =>
            {
                // nothing to do
                System.Diagnostics.Debug.WriteLine(e);
            };
            contextMenu.IsOpen = true;
        }
    }
}
