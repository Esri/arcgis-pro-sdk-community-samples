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
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace CustomAnimation
{
  /// <summary>
  /// This sample shows how to create custom animations such as flying along a 3D line feature and rotating around a point of interest.
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 2. Click Start button to open ArcGIS Pro.
  /// 3. ArcGIS Pro will open. 
  /// 4. With a 3D scene view active go to the Add-In tab.
  /// 5. Select a 3D line features in the scene.
  /// 6. Click the Follow Path button. This will create new keyframes that you can use to animate along the path. 
  /// 7. On the Animation tab click the play button to fly along the line. Additional options are available in the Path group on the Add-In tab to configure how the keyframes are created.
  /// 8. Click the Center At tool and with the tool active click a point of interest in the view.
  /// 9. New keyframes will be constructed that will fly around the point keeping the point you clicked at the center of the view.
  /// </remarks>
  internal class Animation : Module
  {
    private static Animation _this = null;
    private static AnimationSettings _settings = AnimationSettings.Default;

    ~Animation()
    {
      _settings.Save();
    }

    /// <summary>
    /// Get the singleton instance to this module.
    /// </summary>
    public static Animation Current
    {
      get
      {
        return _this ?? (_this = (Animation)FrameworkApplication.FindModule("CustomAnimation_Module"));
      }
    }

    /// <summary>
    /// Get the settings persisted with the user profile for the add-in.
    /// </summary>
    public static AnimationSettings Settings
    {
      get { return _settings; }
    }

    #region Internal Methods

    /// <summary>
    /// Creates keyframes along the path using the user defined settings.
    /// </summary>
    /// <param name="line">The geometry of the line to fly along.</param>
    /// <param name="verticalUnit">The elevation unit of the 3D layer</param>
    internal Task CreateKeyframesAlongPath(Polyline line, Unit verticalUnit)
    {
      return QueuedTask.Run(() =>
      {
        var mapView = MapView.Active;
        if (mapView == null)
          return;

        //Get the camera track from the active map's animation.
        //There will always be only one camera track in the animation.
        var cameraTrack = mapView.Map.Animation.Tracks.OfType<CameraTrack>().First();
        
        //Get some of the user settings for constructing the keyframes alone the path.
        var densifyDistance = Animation.Settings.KeyEvery;
        var verticalOffset = Animation.Settings.HeightAbove / ((mapView.Map.SpatialReference.IsGeographic) ? 1.0 : mapView.Map.SpatialReference.Unit.ConversionFactor); //1 meter
        double currentTimeSeconds = GetInsertTime(mapView.Map.Animation);

        //We need to project the line to a projected coordinate system to calculate the line's length in 3D 
        //as well as more accurately calculated heading and pitch along the path. 
        if (line.SpatialReference.IsGeographic)
        {
          if (mapView.Map.SpatialReference.IsGeographic)
          {
            var transformation = ProjectionTransformation.Create(line.SpatialReference, SpatialReferences.WebMercator, line.Extent);
            line = GeometryEngine.Instance.ProjectEx(line, transformation) as Polyline;
          }    
          else
          {
            var transformation = ProjectionTransformation.Create(line.SpatialReference, mapView.Map.SpatialReference, line.Extent);
            line = GeometryEngine.Instance.ProjectEx(line, transformation) as Polyline;
          }
        }
        
        //If the user has specified to create keyframes at additional locations than just the vertices 
        //we will densify the line by the distance the user specified. 
        if (!Animation.Settings.VerticesOnly)
          line = GeometryEngine.Instance.DensifyByLength(line, densifyDistance / line.SpatialReference.Unit.ConversionFactor) as Polyline;

        //To maintain a constant speed we need to divide the total time we want the animation to take by the length of the line.
        var duration = Animation.Settings.Duration;
        var secondsPerUnit = duration / line.Length3D;
        Camera prevCamera = null;

        //Loop over each vertex in the line and create a new keyframe at each.
        for (int i = 0; i < line.PointCount; i++)
        {
          #region Camera

          MapPoint cameraPoint = line.Points[i];
          
          //If the point is not in the same spatial reference of the map we need to project it.
          if (cameraPoint.SpatialReference.Wkid != mapView.Map.SpatialReference.Wkid)
          {
            var transformation = ProjectionTransformation.Create(cameraPoint.SpatialReference, mapView.Map.SpatialReference);
            cameraPoint = GeometryEngine.Instance.Project(cameraPoint, mapView.Map.SpatialReference) as MapPoint;
          }       

          //Construct a new camera from the point.
          var camera = new Camera(cameraPoint.X, cameraPoint.Y, cameraPoint.Z,
            Animation.Settings.Pitch, 0.0, cameraPoint.SpatialReference, CameraViewpoint.LookFrom);

          //Convert the Z unit to meters if the camera is not in a geographic coordinate system.
          if (!camera.SpatialReference.IsGeographic)
            camera.Z /= camera.SpatialReference.Unit.ConversionFactor;

          //Convert the Z to the unit of the layer's elevation unit and then add the user defined offset from the line.
          camera.Z *= verticalUnit.ConversionFactor;
          camera.Z += verticalOffset;

          //If this is the last point in the collection use the same heading and pitch from the previous camera.
          if (i + 1 == line.Points.Count)
          {
            camera.Heading = prevCamera.Heading;
            camera.Pitch = prevCamera.Pitch;
          }
          else
          {
            var currentPoint = line.Points[i];
            var nextPoint = line.Points[i + 1];

            #region Heading

            //Calculate the heading from the current point to the next point in the path.
            var difX = nextPoint.X - currentPoint.X;
            var difY = nextPoint.Y - currentPoint.Y;
            var radian = Math.Atan2(difX, difY);
            var heading = radian * -180 / Math.PI;
            camera.Heading = heading;

            #endregion

            #region Pitch

            //If the user doesn't want to hardcode the pitch, calculate the pitch based on the current point to the next point.
            if (Animation.Settings.UseLinePitch)
            {
              var hypotenuse = Math.Sqrt(Math.Pow(difX, 2) + Math.Pow(difY, 2));
              var difZ = nextPoint.Z - currentPoint.Z;
              //If the line's unit is not the same as the elevation unit of the layer we need to convert the Z so they are in the same unit.
              if (line.SpatialReference.Unit.ConversionFactor != verticalUnit.ConversionFactor)
                difZ *= (verticalUnit.ConversionFactor / line.SpatialReference.Unit.ConversionFactor);
              radian = Math.Atan2(difZ, hypotenuse);
              var pitch = radian * 180 / Math.PI;
              camera.Pitch = pitch;
            }
            else
            {
              camera.Pitch = Animation.Settings.Pitch;
            }

            #endregion
          }

          #endregion

          #region Time

          //The first point will have a time of 0 seconds, after that we need to set the time based on the 3D distance between the points.
          if (i > 0)
          {
            var lineSegment = PolylineBuilder.CreatePolyline(new List<MapPoint>() { line.Points[i - 1], line.Points[i] }, 
              line.SpatialReference);
            var length = lineSegment.Length3D;
            currentTimeSeconds += length * secondsPerUnit;
          }

          #endregion

          //Create a new keyframe using the camera and the time.
          cameraTrack.CreateKeyframe(camera, TimeSpan.FromSeconds(currentTimeSeconds), AnimationTransition.Linear);
          prevCamera = camera;
        }
      });
    }

    /// <summary>
    /// Create keyframes centered around a point. 
    /// </summary>
    /// <param name="point">The center point around which the keyframes are created.</param>
    internal Task CreateKeyframesAroundPoint(MapPoint point)
    {
      return QueuedTask.Run(() =>
      {
        var mapView = MapView.Active;
        var degrees = Animation.Settings.Degrees;
        if (mapView == null || degrees == 0)
          return;

        //Get the camera track from the active map's animation.
        //There will always be only one camera track in the animation.
        var cameraTrack = mapView.Map.Animation.Tracks.OfType<CameraTrack>().First();
        var camera = mapView.Camera;

        //Calculate the number of keys to create.
        var keyEvery = (degrees < 0) ? -10 : 10; //10 degrees
        var numOfKeys = Math.Floor(degrees / keyEvery);
        var remainder = degrees % keyEvery;

        //To maintain a constant speed we need to divide the total time we want the animation to take by the number of degrees of rotation.
        var duration = Animation.Settings.Duration;
        double timeInterval = duration / Math.Abs(degrees);
        double currentTimeSeconds = GetInsertTime(mapView.Map.Animation);

        //Get the distance from the current location to the point we want to rotate around to get the radius.
        var cameraPoint = MapPointBuilder.CreateMapPoint(camera.X, camera.Y, camera.SpatialReference);
        var radius = GeometryEngine.Instance.GeodesicDistance(cameraPoint, point);
        var radian = ((camera.Heading - 90) / 180.0) * Math.PI;

        //If the spatial reference of the point is projected and the unit is not in meters we need to convert the Z values to meters.
        if (!point.SpatialReference.IsGeographic && point.SpatialReference.Unit.ConversionFactor != 1.0)
          point = MapPointBuilder.CreateMapPoint(point.X, point.Y, 
            point.Z * point.SpatialReference.Unit.ConversionFactor, point.SpatialReference);

        //For all geodesic calculations we will use WGS84 so we will project the point if it is not already.
        if (point.SpatialReference.Wkid != SpatialReferences.WGS84.Wkid)
        {
          var transformation = ProjectionTransformation.Create(point.SpatialReference, SpatialReferences.WGS84);
          point = GeometryEngine.Instance.ProjectEx(point, transformation) as MapPoint;
        }

        //Create an ellipse around the center point.
        var parameter = new GeodesicEllipseParameter()
        {
          Center = point.Coordinate2D,
          SemiAxis1Length = radius,
          SemiAxis2Length = radius,
          AxisDirection = radian,
          LinearUnit = LinearUnit.Meters,
          OutGeometryType = GeometryType.Polyline,
          VertexCount = 36
        };
        var ellipse = GeometryEngine.Instance.GeodesicEllipse(parameter, point.SpatialReference) as Polyline;

        //For each key we will progressively rotate around the ellipse and calculate the camera position at each.
        for (int i = 0; i <= numOfKeys; i++)
        {
          var percentAlong = ((Math.Abs(keyEvery) * i) % 360) / 360.0;
          if (keyEvery > 0)
            percentAlong = 1 - percentAlong;

          //Get the camera at the position around the ellipse.
          camera = OffsetCamera(camera, ellipse, point, percentAlong);

          //Increment the time by the amount of time per key.
          if (i != 0)
            currentTimeSeconds += (timeInterval * Math.Abs(keyEvery));

          //Create a new keyframe for the camera.
          cameraTrack.CreateKeyframe(camera, TimeSpan.FromSeconds(currentTimeSeconds), AnimationTransition.FixedArc);
        }

        //For any degree rotation left over create a keyframe. For example 155, would have a keyframe every 10 degrees and then one for the final 5 degrees.
        if (remainder != 0.0)
        {
          var percentAlong = ((Math.Abs(keyEvery) * numOfKeys + Math.Abs(remainder)) % 360) / 360.0;
          if (remainder > 0)
            percentAlong = 1 - percentAlong;

          OffsetCamera(camera, ellipse, point, percentAlong);

          //Increment the time and create the keyframe.
          currentTimeSeconds += (timeInterval * Math.Abs(remainder));
          cameraTrack.CreateKeyframe(camera, TimeSpan.FromSeconds(currentTimeSeconds), AnimationTransition.FixedArc);
        }
      });
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Creates a new camera offset from the provided camera around an ellipse.
    /// </summary>
    /// <param name="camera">The starting camera.</param>
    /// <param name="ellipse">The ellipse around which the camera will rotate.</param>
    /// <param name="centerPoint">The center point of the ellipse.</param>
    /// <param name="percentAlong">The percentage around the ellipse to create the camera.</param>
    private Camera OffsetCamera(Camera camera, Polyline ellipse, MapPoint centerPoint, double percentAlong)
    {
      camera = CloneCamera(camera);

      var fromPoint = GeometryEngine.Instance.MovePointAlongLine(ellipse, percentAlong, true, 0, SegmentExtension.NoExtension);

      var segment = LineBuilder.CreateLineSegment(new Coordinate2D(centerPoint.X, centerPoint.Y), new Coordinate2D(fromPoint.X, centerPoint.Y), centerPoint.SpatialReference);
      var difX = GeometryEngine.Instance.GeodesicLength(PolylineBuilder.CreatePolyline(segment, segment.SpatialReference));
      if (centerPoint.X - fromPoint.X < 0)
        difX *= -1;

      segment = LineBuilder.CreateLineSegment(new Coordinate2D(centerPoint.X, centerPoint.Y), new Coordinate2D(centerPoint.X, fromPoint.Y), centerPoint.SpatialReference);
      var difY = GeometryEngine.Instance.GeodesicLength(PolylineBuilder.CreatePolyline(segment, segment.SpatialReference));
      if (centerPoint.Y - fromPoint.Y < 0)
        difY *= -1;

      var radian = Math.Atan2(difX, difY);
      var heading = radian * -180 / Math.PI;
      camera.Heading = heading;

      var difZ = centerPoint.Z - (camera.Z * ((camera.SpatialReference.IsGeographic) ? 1.0 : camera.SpatialReference.Unit.ConversionFactor));
      var hypotenuse = GeometryEngine.Instance.GeodesicDistance(fromPoint, centerPoint);
      radian = Math.Atan2(difZ, hypotenuse);
      var pitch = radian * 180 / Math.PI;
      camera.Pitch = pitch;

      if (fromPoint.SpatialReference.Wkid != camera.SpatialReference.Wkid)
      {
        var transformation = ProjectionTransformation.Create(fromPoint.SpatialReference, camera.SpatialReference);
        fromPoint = GeometryEngine.Instance.ProjectEx(fromPoint, transformation) as MapPoint;
      }

      camera.X = fromPoint.X;
      camera.Y = fromPoint.Y;
      return camera;
    }

    /// <summary>
    /// Creates a clone of an existing camera.
    /// </summary>
    private Camera CloneCamera(Camera camera)
    {
      return new Camera()
      {
        X = camera.X,
        Y = camera.Y,
        Z = camera.Z,
        Scale = camera.Scale,
        Heading = camera.Heading,
        Pitch = camera.Pitch,
        Roll = camera.Roll,
        SpatialReference = camera.SpatialReference,
        Viewpoint = camera.Viewpoint
      };
    }

    /// <summary>
    /// Get the time time to begin inserting new keyframes and shift any existing keyframes if necessary.
    /// </summary>
    /// <param name="animation">The animation to be modified.</param>
    private double GetInsertTime(ArcGIS.Desktop.Mapping.Animation animation)
    {
      var duration = Animation.Settings.Duration;
      double currentTimeSeconds = 0;
      if (animation.Duration > TimeSpan.Zero)
      {
        if (Animation.Settings.IsAfterTime)
        {
          currentTimeSeconds = (animation.Duration + TimeSpan.FromSeconds(Animation.Settings.AfterTime)).TotalSeconds;
        }
        else
        {
          currentTimeSeconds = Animation.Settings.AtTime;
          ShiftKeyframes(currentTimeSeconds, duration);
        }
      }
      return currentTimeSeconds;
    }

    /// <summary>
    /// Shift the existing keyframes from the provided time by the provided duration.
    /// </summary>
    /// <param name="insertTime">The time at which all keyframes after should be shifted.</param>
    /// <param name="duration">The amount of time to shift each keyframe.</param>
    private void ShiftKeyframes(double insertTime, double duration)
    {
      var mapView = MapView.Active;
      if (mapView == null)
        return;

      var animation = mapView.Map.Animation;
      foreach (var track in animation.Tracks)
      {
        var keyframes = track.Keyframes.Where(k => k.TrackTime > TimeSpan.FromSeconds(insertTime)).OrderByDescending(k => k.TrackTime);
        foreach (var keyframe in keyframes)
        {
          keyframe.TrackTime = (keyframe.TrackTime + TimeSpan.FromSeconds(duration));
        }
      }
    }

    #endregion
  }
}
