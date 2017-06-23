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
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace ConstructingGeometries
{
  /// <summary>
  /// This code sample shows how to build Multipoint objects. 
  /// 20 random points are generated in the extent of the map extent of the active view.
  /// </summary>
  internal class CreateMultiPoints : Button
  {
    protected override void OnClick()
    {
      // to work in the context of the active display retrieve the current map 
      Map activeMap = MapView.Active.Map;

      // retrieve the first multi-point layer in the map
      var multiPointFeatureLayer = activeMap.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(
          lyr => lyr.ShapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryMultipoint).FirstOrDefault();

      if (multiPointFeatureLayer == null)
        return;

      // construct multipoint
      ConstructSampleMultiPoints(multiPointFeatureLayer);
    }

    /// <summary>
    /// Create a single multi-point feature that is comprised of 20 points.
    /// </summary>
    /// <param name="multiPointLayer">Multi-point geometry feature layer used to add the multi-point feature.</param>
    /// <returns></returns>
    private Task ConstructSampleMultiPoints(FeatureLayer multiPointLayer)
    {
      // create a random number generator
      var randomGenerator = new Random();

      // the database and geometry interactions are considered fine-grained and need to be executed on
      // a separate thread
      return QueuedTask.Run(() =>
      {
              // get the feature class associated with the layer
              var featureClass = multiPointLayer.GetTable() as FeatureClass;
        var featureClassDefinition = featureClass.GetDefinition() as FeatureClassDefinition;

              // store the spatial reference as its own variable
              var spatialReference = featureClassDefinition.GetSpatialReference();

              // define an area of interest. Random points are generated in the allowed
              // confines of the allow extent range
              var areaOfInterest = MapView.Active.Extent;

        // start an edit operation to create new (random) multi-point feature
        var createOperation = new EditOperation()
        {
          Name = "Generate multipoints"
        };

        // retrieve the class definition of the point feature class
        var classDefinition = featureClass.GetDefinition() as FeatureClassDefinition;

        Multipoint newPoints = null;
              // generate either 2D or 3D geometries
              if (classDefinition.HasZ())
        {
                // 3D
                // create a list to hold the 20 coordinates of the multi-point feature
                IList<Coordinate3D> coordinateList = new List<Coordinate3D>(20);
          for (int i = 0; i < 20; i++)
          {
            coordinateList.Add(randomGenerator.NextCoordinate3D(areaOfInterest));
          }
          newPoints = MultipointBuilder.CreateMultipoint(coordinateList, classDefinition.GetSpatialReference());
        }
        else
        {
                // 2D
                // create a list to hold the 20 coordinates of the multi-point feature
                IList<Coordinate2D> coordinateList = new List<Coordinate2D>(20);
          for (int i = 0; i < 20; i++)
          {
            coordinateList.Add(randomGenerator.NextCoordinate2D(areaOfInterest));
          }
          newPoints = MultipointBuilder.CreateMultipoint(coordinateList, classDefinition.GetSpatialReference());
        }
              // create and execute the feature creation operation
              createOperation.Create(multiPointLayer, newPoints);

        return createOperation.ExecuteAsync();
      });
    }


  }
}
