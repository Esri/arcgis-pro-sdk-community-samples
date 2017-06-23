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
using System.Linq;
using System.Threading.Tasks;

using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace ExtendTool
{
    internal class Extend : MapTool
    {
        public Extend() : base()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Point;
            SketchOutputMode = ArcGIS.Desktop.Mapping.SketchOutputMode.Map;
            UseSnapping = true;
        }

        protected override Task<bool> OnSketchCompleteAsync(ArcGIS.Core.Geometry.Geometry geometry)
        {
            //Check we only have one feature to extend to
            if (MapView.Active.Map.SelectionCount != 1)
            {
                MessageBox.Show("Please select one polyline or polygon feature to extend to", "Extend");
                return Task.FromResult(true);
            }

            //Run on MCT
            return QueuedTask.Run(() =>
            {
                //get selected feature geometry
                var selectedFeatures = MapView.Active.Map.GetSelection();
                var insp = new Inspector();
                insp.Load(selectedFeatures.Keys.First(), selectedFeatures.Values.First());
                var selGeom = insp.Shape;
                if (!(selGeom.GeometryType == GeometryType.Polygon
                    || selGeom.GeometryType == GeometryType.Polyline))
                {
                    MessageBox.Show("Please choose as the selected feature either a polyline or polygon feature to extend to");
                    return false;
                }

                //find feature at the click
                var clickFeatures = MapView.Active.GetFeatures(geometry);
                var featuresOids = clickFeatures[clickFeatures.Keys.First()]; 
                insp.Load(clickFeatures.First().Key, featuresOids.First());
                var clickGeom = insp.Shape as Polyline;
                if (clickGeom == null)
                {
                    MessageBox.Show("Please select a polyline feature to extend");
                    return false;
                }

                //extend the line to the poly?
                ArcGIS.Core.Geometry.Polyline extPolyline;
                extPolyline = GeometryEngine.Instance.Extend(clickGeom, (selGeom.GeometryType == GeometryType.Polygon ? GeometryEngine.Instance.Boundary(selGeom) as Polyline : selGeom as Polyline), ExtendFlags.Default);
                if (extPolyline == null)
                {
                    MessageBox.Show(string.Format("Unable to extend the clicked {0} to the selected {1}",
                        clickGeom.GeometryType, selGeom.GeometryType));
                    return false;
                }

                //set the new geometry back on the feature
                insp.Shape = extPolyline;

              //create and execute the edit operation
              var op = new EditOperation()
              {
                Name = "Extend",
                SelectModifiedFeatures = false,
                SelectNewFeatures = false
              };
              op.Modify(insp);
                return op.Execute();
            });
        }
    }
}
