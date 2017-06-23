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
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Core;

namespace ConstructingGeometries
{
    /// <summary>
    /// This code sample shows how to build MapPoint objects. 
    /// 20 random points are generated in the extent of the map extent of the active view.
    /// </summary>
    internal class CreatePoints : Button
    {
        protected override async void OnClick()
        {
            // to work in the context of the active display retrieve the current map 
            Map activeMap = MapView.Active.Map;

            // retrieve the first point layer in the map
            var pointFeatureLayer = activeMap.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(
                lyr => lyr.ShapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryPoint).FirstOrDefault();

            if (pointFeatureLayer == null)
                return;

            // first generate some random points
            await ConstructSamplePoints(pointFeatureLayer);

            // activate the button completed state to enable the polyline button
            FrameworkApplication.State.Activate("geometry_points_constructed");
        }

        /// <summary>
        /// Create random sample points in the extent of the spatial reference
        /// </summary>
        /// <param name="pointFeatureLayer">Point geometry feature layer used to the generate the points.</param>
        /// <returns>Task{bool}</returns>
        private Task<bool> ConstructSamplePoints(FeatureLayer pointFeatureLayer)
        {
            // create a random number generator
            var randomGenerator = new Random();

            // the database and geometry interactions are considered fine-grained and must be executed on
            // the main CIM thread
            return QueuedTask.Run(() =>
            {
              // start an edit operation to create new (random) point features
              var createOperation = new EditOperation()
              {
                Name = "Generate points",
                SelectNewFeatures = false
              };

              // get the feature class associated with the layer
              var featureClass = pointFeatureLayer.GetTable() as FeatureClass;

                // define an area of interest. Random points are generated in the allowed
                // confines of the allow extent range
                var areaOfInterest = MapView.Active.Extent;

                MapPoint newMapPoint = null;

                // retrieve the class definition of the point feature class
                var classDefinition = featureClass.GetDefinition() as FeatureClassDefinition;

                // store the spatial reference as its own variable
                var spatialReference = classDefinition.GetSpatialReference();

                // create 20 new point geometries and queue them for creation
                for (int i = 0; i < 20; i++)
                {
                    // generate either 2D or 3D geometries
                    if (classDefinition.HasZ())
                        newMapPoint = MapPointBuilder.CreateMapPoint(randomGenerator.NextCoordinate3D(areaOfInterest), spatialReference);
                    else
                        newMapPoint = MapPointBuilder.CreateMapPoint(randomGenerator.NextCoordinate2D(areaOfInterest), spatialReference);
                    // queue feature creation
                    createOperation.Create(pointFeatureLayer, newMapPoint);
                }

                // execute the edit (feature creation) operation
                return createOperation.ExecuteAsync();
            });

        }
    }
}
