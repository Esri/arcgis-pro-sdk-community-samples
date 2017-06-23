//Copyright 2017 Esri

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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Editing;
using ArcGIS.Core.Geometry;

namespace ConstructingGeometries
{
    /// <summary>
    /// This code sample shows how to build Polygon objects. 
    /// The code will take line geometries from the line feature layer and construct a polygon from a convex hull for all lines.
    /// </summary>
    internal class CreatePolygons : Button
    {
        protected override async void OnClick()
        {
            // to work in the context of the active display retrieve the current map 
            Map activeMap = MapView.Active.Map;

            // retrieve the first line layer in the map
            var lineFeatureLayer = activeMap.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(
                lyr => lyr.ShapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolyline).FirstOrDefault();

            if (lineFeatureLayer == null)
                return;

            // retrieve the first polygon feature layer in the map
            var polygonFeatureLayer = activeMap.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(
                lyr => lyr.ShapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolygon).FirstOrDefault();

            if (polygonFeatureLayer == null)
                return;

            //construct the polyline based of the convex hull of all polylines
            await ConstructSamplePolygon(polygonFeatureLayer, lineFeatureLayer);
        }

        /// <summary>
        /// Create sample polygon feature using the point geometries from the multi-point feature using the 
        /// ConvexHull method provided by the GeometryEngine.
        /// </summary>
        /// <param name="polygonLayer">Polygon geometry feature layer used to add the new feature.</param>
        /// <param name="lineLayer">The polyline feature layer containing the features used to construct the polygon.</param>
        /// <returns></returns>
        private Task<bool> ConstructSamplePolygon(FeatureLayer polygonLayer, FeatureLayer lineLayer)
        {

            // execute the fine grained API calls on the CIM main thread
            return QueuedTask.Run(() =>
            {
                // get the underlying feature class for each layer
                var polygonFeatureClass = polygonLayer.GetTable() as FeatureClass;
                var polygonDefinition = polygonFeatureClass.GetDefinition() as FeatureClassDefinition;
                var lineFeatureClass = lineLayer.GetTable() as FeatureClass;

                // construct a cursor to retrieve the line features
                var lineCursor = lineFeatureClass.Search(null, false);

                // set up the edit operation for the feature creation
                var createOperation = new EditOperation()
                {
                    Name = "Create polygons",
                    SelectNewFeatures = false
                };

                PolylineBuilder polylineBuilder = new PolylineBuilder(polygonDefinition.GetSpatialReference());

                while (lineCursor.MoveNext())
                {
                    // retrieve the first feature
                    var lineFeature = lineCursor.Current as Feature;

                    // add the coordinate collection of the current geometry into our overall list of collections
                    var polylineGeometry = lineFeature.GetShape() as Polyline;
                    polylineBuilder.AddParts(polylineGeometry.Parts);
                }

                // use the ConvexHull method from the GeometryEngine to construct the polygon geometry
                var newPolygon = GeometryEngine.Instance.ConvexHull(polylineBuilder.ToGeometry()) as Polygon;

                // queue the polygon creation
                createOperation.Create(polygonLayer, newPolygon);

                // execute the edit (polygon create) operation
                return createOperation.ExecuteAsync();
            });
        }
    }
}
