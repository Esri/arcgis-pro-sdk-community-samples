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
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Templates;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Core;

namespace ConstructingGeometries
{
  /// <summary>
  /// This sample provide four buttons showing the construction of geometry types of type MapPoint, Multipoint, Polyline, and Polygon and shows how to:
  /// * Construct and manipulate geometries
  /// * Use GeometryEngine functionality
  /// * Search and retrieve features
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
  /// 1. Make sure that the Sample data is unzipped in c:\data 
  /// 1. The project used for this sample is 'C:\Data\FeatureTest\FeatureTest.aprx'
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open, select the FeatureTest.aprx project
  /// 1. Click on the ADD-IN tab and make sure that your active map contains Setup/point/multipoint/line/polygon features buttons as shown below.
  /// ![UI](Screenshots/ScreenPoints.png)
  /// 1. Click on Setup button to enable the create point and create multipoint buttons 
  /// ![UI](Screenshots/ScreenPoint1.png)
  /// 1. Click the createPoints button to create random points over the current extent of the map
  /// 1. The map extent shows the random created points and also enables create polylines button
  /// ![UI](Screenshots/ScreenPoint2.png)
  /// 1. Click the createPolylines button to create random lines the current extent of the map
  /// 1. The map extent shows the random lines and also enables create polygons button
  /// ![UI](Screenshots/ScreenPoint3.png)
  /// 1. Click the createPolygons button to create random polygon over the current extent of the map
  /// ![UI](Screenshots/ScreenPoint4.png)
  /// </remarks>
  internal class ConstructingGeometriesModule : Module
  {
    private static ConstructingGeometriesModule _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static ConstructingGeometriesModule Current
    {
      get
      {
        return _this ?? (_this = (ConstructingGeometriesModule)FrameworkApplication.FindModule("ConstructingGeometries_Module"));
      }
    }

    #region Overrides
    /// <summary>
    /// Called by Framework when ArcGIS Pro is closing
    /// </summary>
    /// <returns>False to prevent Pro from closing, otherwise True</returns>
    protected override bool CanUnload()
    {
      //TODO - add your business logic
      //return false to ~cancel~ Application close
      return true;
    }

    /// <summary>
    /// Generic implementation of ExecuteCommand to allow calls to
    /// <see cref="FrameworkApplication.ExecuteCommand"/> to execute commands in
    /// your Module.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    protected override Func<Task> ExecuteCommand(string id)
    {

      //TODO: replace generic implementation with custom logic
      //etc as needed for your Module
      var command = FrameworkApplication.GetPlugInWrapper(id) as ICommand;
      if (command == null)
        return () => Task.FromResult(0);
      if (!command.CanExecute(null))
        return () => Task.FromResult(0);

      return () =>
      {
        command.Execute(null); // if it is a tool, execute will set current tool
              return Task.FromResult(0);
      };
    }
    #endregion Overrides

    /// <summary>
    /// The method ensures that there are point, multipoint, line, and polygon layers in the map of the active view.
    /// In case the layer is missing, then a default feature class will be created in the default geodatabase of the project.
    /// </summary>
    public static async void PrepareTheSample()
    {
      var pointLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(lyr => lyr.ShapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryPoint).FirstOrDefault();

      if (pointLayer == null)
      {
        await CreateLayer("sdk_points", "POINT");
      }

      var multiPointLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(lyr => lyr.ShapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryMultipoint).FirstOrDefault();

      if (multiPointLayer == null)
      {
        await CreateLayer("sdk_multipoints", "MULTIPOINT");
      }

      var polylineLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(lyr => lyr.ShapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolyline).FirstOrDefault();

      if (polylineLayer == null)
      {
        await CreateLayer("sdk_polyline", "POLYLINE");
      }

      var polygonLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(lyr => lyr.ShapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolygon).FirstOrDefault();

      if (polygonLayer == null)
      {
        await CreateLayer("sdk_polygon", "POLYGON");
      }
    }

    /// <summary>
    /// Create a feature class in the default geodatabase of the project.
    /// </summary>
    /// <param name="featureclassName">Name of the feature class to be created.</param>
    /// <param name="featureclassType">Type of feature class to be created. Options are:
    /// <list type="bullet">
    /// <item>POINT</item>
    /// <item>MULTIPOINT</item>
    /// <item>POLYLINE</item>
    /// <item>POLYGON</item></list></param>
    /// <returns></returns>
    private static async Task CreateLayer(string featureclassName, string featureclassType)
    {
      List<object> arguments = new List<object>
      {
        // store the results in the default geodatabase
        CoreModule.CurrentProject.DefaultGeodatabasePath,
        // name of the feature class
        featureclassName,
        // type of geometry
        featureclassType,
        // no template
        "",
        // no z values
        "DISABLED",
        // no m values
        "DISABLED"
      };
      await QueuedTask.Run(() =>
      {
              // spatial reference
              arguments.Add(SpatialReferenceBuilder.CreateSpatialReference(3857));
      });

      IGPResult result = await Geoprocessing.ExecuteToolAsync("CreateFeatureclass_management", Geoprocessing.MakeValueArray(arguments.ToArray()));
    }
  }

  /// <summary>
  /// Extension methods to generate random coordinates within a given extent.
  /// </summary>
  public static class RandomExtension
  {
    /// <summary>
    /// Generate a random double number between the min and max values.
    /// </summary>
    /// <param name="random">Instance of a random class.</param>
    /// <param name="minValue">The min value for the potential range.</param>
    /// <param name="maxValue">The max value for the potential range.</param>
    /// <returns>Random number between min and max</returns>
    /// <remarks>The random result number will always be less than the max number.</remarks>
    public static double NextDouble(this Random random, double minValue, double maxValue)
    {
      return random.NextDouble() * (maxValue - minValue) + minValue;
    }

    /// <summary>
    /// /Generate a random coordinate (only x,y values)  within the provided envelope.
    /// </summary>
    /// <param name="random">Instance of a random class.</param>
    /// <param name="withinThisExtent">Area of interest in which the random coordinate will be created.</param>
    /// <returns>A coordinate with random values (only x,y values) within the extent.</returns>
    public static Coordinate2D NextCoordinate2D(this Random random, Envelope withinThisExtent)
    {
      return new Coordinate2D(random.NextDouble(withinThisExtent.XMin, withinThisExtent.XMax),
                          random.NextDouble(withinThisExtent.YMin, withinThisExtent.YMax));
    }

    /// <summary>
    /// /Generate a random coordinate 3D (containing x,y,z values) within the provided envelope.
    /// </summary>
    /// <param name="random">Instance of a random class.</param>
    /// <param name="withinThisExtent">Area of interest in which the random coordinate will be created.</param>
    /// <returns>A coordinate with random values 3D (containing x,y,z values) within the extent.</returns>
    public static Coordinate3D NextCoordinate3D(this Random random, Envelope withinThisExtent)
    {
      return new Coordinate3D(random.NextDouble(withinThisExtent.XMin, withinThisExtent.XMax),
          random.NextDouble(withinThisExtent.YMin, withinThisExtent.YMax), 0);
    }
  }
}