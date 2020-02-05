//   Copyright 2019 Esri
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
using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Framework.Dialogs;

namespace AnimationFromPath
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
  public static class CreateAnimationFromPath
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
  {
    #region Constants

    //determines where intermediate keyframes are created along a segment
    private const double LINE_CONSTRAINT_FACTOR = 0.15;
    private const double ARC_CONSTRAINT_FACTOR = 0.15;

    //minimum length of a straight line segment needed to create intermediate keyframes
    //and use additional logic for ignoring rotation on end points. If a straight line segment
    //is smaller than this threshold then intermediate keyframes are not created and ignore-rotation-at-end-points
    //logic is not used
    private const double STRAIGHT_SEGMENT_LENGTH_THRESHOLD = 30;

    private const double ANIMATION_APPEND_TIME = 3; //seconds

    #endregion

    #region Fields/Properties

    private static double _keyframeHeading = 0;

    private static double _keyframePitch = 0;

    private static double Z_CONVERSION_FACTOR = 1;

    private static string _selectedMethod = "Keyframes along path";
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static string SelectedMethod
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
      get { return _selectedMethod; }

      set { _selectedMethod = value; }
    }

    private static string _selectedCameraView = "Top down";
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static string SelectedCameraView
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
      get { return _selectedCameraView; }

      set { _selectedCameraView = value; }
    }


    private static double _customPitch = -90;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static double CustomPitch
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
      get { return _customPitch; }

      set { _customPitch = value; }
    }

    private static double _cameraZOffset = 1000;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static double CameraZOffset
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
      get { return _cameraZOffset; }

      set { _cameraZOffset = value; }
    }

    private static double _totalDuration = 0;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static double TotalDuration
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
      get { return _totalDuration; }

      set { _totalDuration = value; }
    }

    private static double _keyEveryNSecond = 1;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static double KeyEveryNSecond
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
      get { return _keyEveryNSecond; }

      set { _keyEveryNSecond = value; }
    }

    private static MapPoint _targetPoint = null;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static MapPoint TargetPoint
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
      get { return _targetPoint; }
      set { _targetPoint = value; }
    }

    #endregion

    #region Functions

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static async Task CreateKeyframes()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
      FeatureLayer ftrLayer = null;

      MapView mapView = MapView.Active;
      if (mapView == null)
        return;

      var mapSelection = await QueuedTask.Run(() => MapView.Active.Map.GetSelection());

      if (mapSelection.Count == 1)
      {
        var layer = mapSelection.First().Key;
        if (layer is FeatureLayer)
        {
          ftrLayer = (FeatureLayer)layer;
          if (ftrLayer.ShapeType != ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolyline)
          {
            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Select a polyline feature.");
            return;
          }

          int numFtrsSelected = await QueuedTask.Run(() => ftrLayer.GetSelection().GetCount());

          if (numFtrsSelected != 1)
          {
            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Select only one polyline feature.");
            return;
          }
        }
        else
        {
          ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Select a polyline feature.");
          return;
        }
      }
      else
      {
        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Select a polyline feature.");
        return;
      }

      if (SelectedCameraView == "Face target" && TargetPoint == null)
      {
        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Selected view type is - Face target - but a target point is not set.");
        return;
      }

      string oid_fieldName = await QueuedTask.Run(() => ftrLayer.GetTable().GetDefinition().GetObjectIDField());

      //get selected polyline
      Polyline lineGeom = await QueuedTask.Run<Polyline>(() =>
      {
        var selectedFtrOID = MapView.Active.Map.GetSelection()[ftrLayer][0];
        QueryFilter qf = new QueryFilter();
        qf.WhereClause = oid_fieldName + " = " + selectedFtrOID.ToString();
        RowCursor result = ftrLayer.GetFeatureClass().Search(qf);
        if (result != null && result.MoveNext())
        {
              Polyline plyLine = null;
              using (Feature selectedFtr = result.Current as Feature)
              {
                  plyLine = selectedFtr.GetShape().Clone() as Polyline;
              }
              return plyLine;
        }
        return null;
      });

      //couldn't get the selected feature
      if (lineGeom == null)
        return;

      ProjectionTransformation transformation = await QueuedTask.Run(() => ProjectionTransformation.Create(ftrLayer.GetSpatialReference(), mapView.Map.SpatialReference));
      SpatialReference layerSpatRef = await QueuedTask.Run(() => ftrLayer.GetSpatialReference());

      if (layerSpatRef.Unit.Name != "Degree")
        Z_CONVERSION_FACTOR = layerSpatRef.Unit.ConversionFactor;

      //Project target point if method is Face target
      if (SelectedCameraView == "Face target")
      {
        if (TargetPoint != null && TargetPoint.SpatialReference != layerSpatRef)
        {
          ProjectionTransformation transf_forTarget = await QueuedTask.Run(() => ProjectionTransformation.Create(TargetPoint.SpatialReference, layerSpatRef));
          MapPoint projected_targetPoint = (MapPoint)GeometryEngine.Instance.ProjectEx(TargetPoint, transf_forTarget);
          TargetPoint = null;
          TargetPoint = projected_targetPoint;
        }
      }

      var animation = mapView.Map.Animation;
      var cameraTrack = animation.Tracks.OfType<CameraTrack>().First();
      var keyframes = cameraTrack.Keyframes;

      //Get segment list for line
      ReadOnlyPartCollection polylineParts = lineGeom.Parts;

      //get total segment count and determine path length
      double pathLength = 0;
      int segmentCount = 0;
      IEnumerator<ReadOnlySegmentCollection> segments = polylineParts.GetEnumerator();

      while (segments.MoveNext())
      {
        ReadOnlySegmentCollection seg = segments.Current;
        foreach (Segment s in seg)
        {
          //pathLength += s.Length;//s.Length returns 2D length

          double length3D = Math.Sqrt((s.EndPoint.X - s.StartPoint.X) * (s.EndPoint.X - s.StartPoint.X) +
                                        (s.EndPoint.Y - s.StartPoint.Y) * (s.EndPoint.Y - s.StartPoint.Y) +
                                          (s.EndPoint.Z - s.StartPoint.Z) * (s.EndPoint.Z - s.StartPoint.Z));

          pathLength += length3D;
          segmentCount += 1;
        }
      }

      //reset heading and pitch
      _keyframeHeading = 0;
      _keyframePitch = 0;

      // Create keyframes based on chosen method
      if (SelectedMethod == "Keyframes along path")
      {
        await CreateKeyframes_AlongPath(mapView, layerSpatRef, transformation, cameraTrack, segments, segmentCount, pathLength);
      }
      else if (SelectedMethod == "Keyframes every N seconds")
      {
        await CreateKeyframes_EveryNSeconds(mapView, layerSpatRef, transformation, cameraTrack, segments, segmentCount, pathLength, KeyEveryNSecond);
      }
      else if (SelectedMethod == "Keyframes only at vertices")
      {
        await CreateKeyframes_AtVertices(mapView, layerSpatRef, transformation, cameraTrack, lineGeom, segments, segmentCount, pathLength);
      }
    }

    //Use this method for smoother turns at corners. Additionally this method processes straight line segments and arc segments separately
    //For arc segments a keyframe is created at every second. However a minimum of 5 keyframes are created for arcs.
    //So if arc segment length is less than 5 then we default to at least 5 keyframes. This is an attempt to stick to the path as much as possible.
    //For straight line segments, rotation is ignored at end point of each segment except for the end point of the path itself. Two keyframes with rotation
    //are created at certain distance (determined by LINE_CONSTRAINT_FACTOR) before and after the end point of each segment. This is an attempt to avoid
    //sharp turns at corners along the path.
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static async Task CreateKeyframes_AlongPath(MapView mapView, SpatialReference layerSpatRef, ProjectionTransformation transformation,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
                                                    CameraTrack cameraTrack, IEnumerator<ReadOnlySegmentCollection> segments,
                                                    int segmentCount, double pathLength)
    {
      double segmentLength = 0;
      int num_iterations = 0;
      segments.Reset();

      //process each segment depending upon its type - straight line or arc
      while (segments.MoveNext())
      {
        ReadOnlySegmentCollection seg = segments.Current;
        double accumulatedDuration = mapView.Map.Animation.Duration.TotalSeconds + ((mapView.Map.Animation.Duration.TotalSeconds > 0) ? ANIMATION_APPEND_TIME : 0); // 0;

        foreach (Segment s in seg)
        {
          double length3D = Math.Sqrt((s.EndPoint.X - s.StartPoint.X) * (s.EndPoint.X - s.StartPoint.X) +
                                        (s.EndPoint.Y - s.StartPoint.Y) * (s.EndPoint.Y - s.StartPoint.Y) +
                                          (s.EndPoint.Z - s.StartPoint.Z) * (s.EndPoint.Z - s.StartPoint.Z));

          
          double segmentDuration = (TotalDuration / pathLength) * length3D;
          segmentLength = length3D;

          //straight line segments
          if (s.SegmentType == SegmentType.Line)
          {
            MapPoint startPt = await QueuedTask.Run(() => MapPointBuilder.CreateMapPoint(s.StartPoint.X, s.StartPoint.Y, s.StartPoint.Z * Z_CONVERSION_FACTOR, layerSpatRef));
            MapPoint endPt = await QueuedTask.Run(() => MapPointBuilder.CreateMapPoint(s.EndPoint.X, s.EndPoint.Y, s.EndPoint.Z * Z_CONVERSION_FACTOR, layerSpatRef));

            //we will be creating three intermediate keyframes for staright segments only if segment length is more than a set threshold
            //the threshold is just a guess and might have to be altered depending upon the path geometry. Should work for most cases though
            MapPoint firstIntPoint = null;
            MapPoint midIntPoint = null;
            MapPoint lastIntPoint = null;

            if (segmentLength >= STRAIGHT_SEGMENT_LENGTH_THRESHOLD)
            {
              //first intermediate point
              firstIntPoint = await CreatePointAlongSegment(startPt, endPt, LINE_CONSTRAINT_FACTOR * segmentLength, layerSpatRef);

              //mid point
              midIntPoint = await CreatePointAlongSegment(startPt, endPt, 0.5 * segmentLength, layerSpatRef);

              //last intermediate point
              lastIntPoint = await CreatePointAlongSegment(startPt, endPt, (1 - LINE_CONSTRAINT_FACTOR) * segmentLength, layerSpatRef);
            }

            //create keyframe at start vertex of path in map space
            double timeSpanValue = accumulatedDuration;
            TimeSpan keyframeTimespan = TimeSpan.FromSeconds(timeSpanValue);

            if (segmentLength >= STRAIGHT_SEGMENT_LENGTH_THRESHOLD)
            {
              SetPitchAndHeadingForLine(startPt, firstIntPoint);
            }
            else
            {
              SetPitchAndHeadingForLine(startPt, endPt);
            }

            //ignore rotation for all start vertices (which would also be end vertices of previous segments) EXCEPT for the first vertex of path
            if (num_iterations == 0 || segmentLength < STRAIGHT_SEGMENT_LENGTH_THRESHOLD)
            {
              await CreateCameraKeyframe(mapView, startPt, transformation, cameraTrack, keyframeTimespan, _keyframePitch, _keyframeHeading);
            }
            else
            {
              await CreateCameraKeyframe(mapView, startPt, transformation, cameraTrack, keyframeTimespan, _keyframePitch, _keyframeHeading, true, false);
            }

            if (segmentLength > STRAIGHT_SEGMENT_LENGTH_THRESHOLD)
            {
              //Create a keyframe at PATH_CONSTRAINT_FACTOR distance along the segment from start point
              double distanceAlong = LINE_CONSTRAINT_FACTOR * segmentLength;
              timeSpanValue = accumulatedDuration + LINE_CONSTRAINT_FACTOR * segmentDuration;
              keyframeTimespan = TimeSpan.FromSeconds(timeSpanValue);
              SetPitchAndHeadingForLine(firstIntPoint, midIntPoint);
              await CreateCameraKeyframe(mapView, firstIntPoint, transformation, cameraTrack, keyframeTimespan, _keyframePitch, _keyframeHeading);

              //Create a keyframe at middle of segment
              distanceAlong = 0.5 * segmentLength;
              timeSpanValue = accumulatedDuration + 0.5 * segmentDuration;
              keyframeTimespan = TimeSpan.FromSeconds(timeSpanValue);
              SetPitchAndHeadingForLine(midIntPoint, lastIntPoint);
              //await CreateCameraKeyframe(mapView, midIntPoint, transformation, cameraTrack, keyframeTimespan, _keyframePitch, _keyframeHeading);

              //Create a keyframe at (1 - PATH_CONSTRAINT_FACTOR) distance along the segment from start point
              distanceAlong = (1 - LINE_CONSTRAINT_FACTOR) * segmentLength;
              timeSpanValue = accumulatedDuration + (1 - LINE_CONSTRAINT_FACTOR) * segmentDuration;
              keyframeTimespan = TimeSpan.FromSeconds(timeSpanValue);
              SetPitchAndHeadingForLine(lastIntPoint, endPt);
              await CreateCameraKeyframe(mapView, lastIntPoint, transformation, cameraTrack, keyframeTimespan, _keyframePitch, _keyframeHeading);
            }

            //Create a keyframe at end point of segment only for the end point of last segment
            //Otherwise we will get duplicate keyframes at end of one segment and start of the next one
            if (num_iterations == segmentCount - 1)
            {
              timeSpanValue = accumulatedDuration + segmentDuration;
              keyframeTimespan = TimeSpan.FromSeconds(timeSpanValue);

              if (SelectedCameraView == "Face target")
              {
                SetPitchAndHeadingForLine(endPt, TargetPoint);
              }

              await CreateCameraKeyframe(mapView, endPt, transformation, cameraTrack, keyframeTimespan, _keyframePitch, _keyframeHeading);
            }
          }
          //processing for arcs - create a keyframe every second for arcs
          //we will create a minimum of 5 keyframes along the arc
          else if (s.SegmentType == SegmentType.EllipticArc && segmentDuration > 5)
          {
            EllipticArcSegment ellipArc = s as EllipticArcSegment;
            MapPoint startPt = await QueuedTask.Run(() => MapPointBuilder.CreateMapPoint(s.StartPoint.X, s.StartPoint.Y, s.StartPoint.Z, layerSpatRef));
            MapPoint endPt = await QueuedTask.Run(() => MapPointBuilder.CreateMapPoint(s.EndPoint.X, s.EndPoint.Y, s.EndPoint.Z, layerSpatRef));

            double radius = Math.Sqrt((ellipArc.CenterPoint.X - startPt.X) * (ellipArc.CenterPoint.X - startPt.X) + (ellipArc.CenterPoint.Y - startPt.Y) * (ellipArc.CenterPoint.Y - startPt.Y));
            double angle = ellipArc.CentralAngle;
            MapPoint centerPt = await QueuedTask.Run(() => MapPointBuilder.CreateMapPoint(ellipArc.CenterPoint.X, ellipArc.CenterPoint.Y, (s.StartPoint.Z + s.EndPoint.Z) / 2, layerSpatRef));

            int num_keys = (int)segmentDuration;

            MapPoint firstIntPoint = null;

            //first intermediate keyframe for arc - needed for setting heading for start vertex
            // >2 to account for start and end
            if (num_keys > 2)
            {
              firstIntPoint = await CreatePointAlongArc(startPt, endPt, centerPt, angle / (num_keys - 1), radius, layerSpatRef, ellipArc.IsMinor, ellipArc.IsCounterClockwise);
            }

            //Create keyframe at start vertex of path in map space
            double timeSpanValue = accumulatedDuration;
            TimeSpan keyframeTimespan = TimeSpan.FromSeconds(timeSpanValue);

            if (firstIntPoint != null)
            {
              SetPitchAndHeadingForLine(startPt, firstIntPoint);
            }
            else
            {
              SetPitchAndHeadingForLine(startPt, endPt);
            }

            //Ignore rotation for all start vertices EXCEPT for the first vertex of path
            if (num_iterations == 0)
            {
              await CreateCameraKeyframe(mapView, startPt, transformation, cameraTrack, keyframeTimespan, _keyframePitch, _keyframeHeading);
            }
            else
            {
              await CreateCameraKeyframe(mapView, startPt, transformation, cameraTrack, keyframeTimespan, _keyframePitch, _keyframeHeading, true, false);
            }

            //Loop to create intermediate keyframes at each second
            for (int i = 0; i < num_keys - 2; i++)
            {
              MapPoint currentIntPoint = null;
              MapPoint nextIntPoint = null;

              currentIntPoint = await CreatePointAlongArc(startPt, endPt, centerPt, (angle / (num_keys - 1)) * (i + 1), radius, layerSpatRef, ellipArc.IsMinor, ellipArc.IsCounterClockwise);
              if (i < num_keys - 3)
              {
                nextIntPoint = await CreatePointAlongArc(startPt, endPt, centerPt, (angle / (num_keys - 1)) * (i + 2), radius, layerSpatRef, ellipArc.IsMinor, ellipArc.IsCounterClockwise);
              }
              else //for the last intermediate keyframe, heading/pitch has to be determined relative to the end point fo segment
              {
                nextIntPoint = endPt;
              }
              //timeSpanValue = accumulatedDuration + (i + 1) * 1; //at each second
              timeSpanValue = accumulatedDuration + (i + 1) * (segmentDuration / (num_keys - 1));

              keyframeTimespan = TimeSpan.FromSeconds(timeSpanValue);
              SetPitchAndHeadingForLine(currentIntPoint, nextIntPoint);
              await CreateCameraKeyframe(mapView, currentIntPoint, transformation, cameraTrack, keyframeTimespan, _keyframePitch, _keyframeHeading);
            }

            //Create a keyframe at end point of segment only for the end point of last segment
            if (num_iterations == segmentCount - 1)
            {
              timeSpanValue = accumulatedDuration + segmentDuration;
              keyframeTimespan = TimeSpan.FromSeconds(timeSpanValue);

              if (SelectedCameraView == "Face target")
              {
                SetPitchAndHeadingForLine(endPt, TargetPoint);
              }

              await CreateCameraKeyframe(mapView, endPt, transformation, cameraTrack, keyframeTimespan, _keyframePitch, _keyframeHeading);
            }
          }
          //create a minimum of 5 keyframes along the arc
          else if (s.SegmentType == SegmentType.EllipticArc)
          {
            EllipticArcSegment ellipArc = s as EllipticArcSegment;
            MapPoint startPt = await QueuedTask.Run(() => MapPointBuilder.CreateMapPoint(s.StartPoint.X, s.StartPoint.Y, s.StartPoint.Z, layerSpatRef));
            MapPoint endPt = await QueuedTask.Run(() => MapPointBuilder.CreateMapPoint(s.EndPoint.X, s.EndPoint.Y, s.EndPoint.Z, layerSpatRef));

            double radius = Math.Sqrt((ellipArc.CenterPoint.X - startPt.X) * (ellipArc.CenterPoint.X - startPt.X) + (ellipArc.CenterPoint.Y - startPt.Y) * (ellipArc.CenterPoint.Y - startPt.Y));
            double angle = ellipArc.CentralAngle;
            MapPoint centerPt = await QueuedTask.Run(() => MapPointBuilder.CreateMapPoint(ellipArc.CenterPoint.X, ellipArc.CenterPoint.Y, (s.StartPoint.Z + s.EndPoint.Z) / 2, layerSpatRef));

            //we are creating five intermediate keyframes for arcs
            MapPoint firstIntPoint = await CreatePointAlongArc(startPt, endPt, centerPt, angle * ARC_CONSTRAINT_FACTOR, radius, layerSpatRef, ellipArc.IsMinor, ellipArc.IsCounterClockwise);

            MapPoint secondIntPoint = await CreatePointAlongArc(startPt, endPt, centerPt, angle * ARC_CONSTRAINT_FACTOR * 2, radius, layerSpatRef, ellipArc.IsMinor, ellipArc.IsCounterClockwise);

            MapPoint midIntPoint = await CreatePointAlongArc(startPt, endPt, centerPt, angle * 0.5, radius, layerSpatRef, ellipArc.IsMinor, ellipArc.IsCounterClockwise);

            MapPoint secondLastIntPoint = await CreatePointAlongArc(startPt, endPt, centerPt, angle * (1 - ARC_CONSTRAINT_FACTOR * 2), radius, layerSpatRef, ellipArc.IsMinor, ellipArc.IsCounterClockwise);

            MapPoint lastIntPoint = await CreatePointAlongArc(startPt, endPt, centerPt, angle * (1 - ARC_CONSTRAINT_FACTOR), radius, layerSpatRef, ellipArc.IsMinor, ellipArc.IsCounterClockwise);

            //Create keyframe at start vertex of path in map space
            double timeSpanValue = accumulatedDuration;
            TimeSpan keyframeTimespan = TimeSpan.FromSeconds(timeSpanValue);
            SetPitchAndHeadingForLine(startPt, firstIntPoint);

            //Ignore rotation for all start vertices EXCEPT for the first vertex of path
            if (num_iterations == 0)
            {
              await CreateCameraKeyframe(mapView, startPt, transformation, cameraTrack, keyframeTimespan, _keyframePitch, _keyframeHeading);
            }
            else
            {
              await CreateCameraKeyframe(mapView, startPt, transformation, cameraTrack, keyframeTimespan, _keyframePitch, _keyframeHeading, true, false);
            }

            //await CreateCameraKeyframe(mapView, startPt, transformation, cameraTrack, keyframeTimespan, _keyframePitch, _keyframeHeading);

            //Create a keyframe at PATH_CONSTRAINT_FACTOR distance along the segment from start point
            timeSpanValue = accumulatedDuration + ARC_CONSTRAINT_FACTOR * segmentDuration;
            keyframeTimespan = TimeSpan.FromSeconds(timeSpanValue);
            SetPitchAndHeadingForLine(firstIntPoint, secondIntPoint);
            await CreateCameraKeyframe(mapView, firstIntPoint, transformation, cameraTrack, keyframeTimespan, _keyframePitch, _keyframeHeading);

            //Create a keyframe at 2* PATH_CONSTRAINT_FACTOR distance along the segment from start point
            timeSpanValue = accumulatedDuration + ARC_CONSTRAINT_FACTOR * 2 * segmentDuration;
            keyframeTimespan = TimeSpan.FromSeconds(timeSpanValue);
            SetPitchAndHeadingForLine(secondIntPoint, midIntPoint);
            await CreateCameraKeyframe(mapView, secondIntPoint, transformation, cameraTrack, keyframeTimespan, _keyframePitch, _keyframeHeading);

            //Create a keyframe at middle of segment
            timeSpanValue = accumulatedDuration + 0.5 * segmentDuration;
            keyframeTimespan = TimeSpan.FromSeconds(timeSpanValue);
            SetPitchAndHeadingForLine(midIntPoint, secondLastIntPoint);
            await CreateCameraKeyframe(mapView, midIntPoint, transformation, cameraTrack, keyframeTimespan, _keyframePitch, _keyframeHeading);

            //Create a keyframe at (1 - PATH_CONSTRAINT_FACTOR * 2) distance along the segment from start point
            timeSpanValue = accumulatedDuration + (1 - ARC_CONSTRAINT_FACTOR * 2) * segmentDuration;
            keyframeTimespan = TimeSpan.FromSeconds(timeSpanValue);
            SetPitchAndHeadingForLine(secondLastIntPoint, lastIntPoint);
            await CreateCameraKeyframe(mapView, secondLastIntPoint, transformation, cameraTrack, keyframeTimespan, _keyframePitch, _keyframeHeading);

            //Create a keyframe at (1 - PATH_CONSTRAINT_FACTOR) distance along the segment from start point
            timeSpanValue = accumulatedDuration + (1 - ARC_CONSTRAINT_FACTOR) * segmentDuration;
            keyframeTimespan = TimeSpan.FromSeconds(timeSpanValue);
            SetPitchAndHeadingForLine(lastIntPoint, endPt);
            await CreateCameraKeyframe(mapView, lastIntPoint, transformation, cameraTrack, keyframeTimespan, _keyframePitch, _keyframeHeading);

            //Create a keyframe at end point of segment only for the end point of last segment
            if (num_iterations == segmentCount - 1)
            {
              timeSpanValue = accumulatedDuration + segmentDuration;
              keyframeTimespan = TimeSpan.FromSeconds(timeSpanValue);

              if (SelectedCameraView == "Face target")
              {
                SetPitchAndHeadingForLine(endPt, TargetPoint);
              }

              await CreateCameraKeyframe(mapView, endPt, transformation, cameraTrack, keyframeTimespan, _keyframePitch, _keyframeHeading);
            }
          }

          accumulatedDuration += segmentDuration;
          num_iterations++;
        }
      }
    }

    //Use this method to create a keyframe at every n-second of the specified animation duration
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static async Task CreateKeyframes_EveryNSeconds(MapView mapView, SpatialReference layerSpatRef, ProjectionTransformation transformation,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
                                                    CameraTrack cameraTrack, IEnumerator<ReadOnlySegmentCollection> segments,
                                                    int segmentCount, double pathLength, double keyEveryNSecond = 1)
    {
      double segmentLength = 0;
      int numKeysToCreate = (int)(TotalDuration / keyEveryNSecond); //approximately
      double createKeyAtDist = pathLength / numKeysToCreate;
            
      double skippedDistance = 0;
      double accumulatedDuration = mapView.Map.Animation.Duration.TotalSeconds + ((mapView.Map.Animation.Duration.TotalSeconds > 0) ? ANIMATION_APPEND_TIME : 0); // 0;

      int num_iterations = 0;
      segments.Reset();

      List<MapPoint> pointsForKeyframes = new List<MapPoint>();

      MapPoint pathEndPt = null;

      //process each segment depending upon its type - straight line or arc
      while (segments.MoveNext())
      {
        ReadOnlySegmentCollection seg = segments.Current;

        foreach (Segment s in seg)
        {
          segmentLength = Math.Sqrt((s.EndPoint.X - s.StartPoint.X) * (s.EndPoint.X - s.StartPoint.X) +
                                        (s.EndPoint.Y - s.StartPoint.Y) * (s.EndPoint.Y - s.StartPoint.Y) +
                                          (s.EndPoint.Z - s.StartPoint.Z) * (s.EndPoint.Z - s.StartPoint.Z));
                    
          double segmentDuration = (TotalDuration / pathLength) * segmentLength;

          //straight line segments
          if (s.SegmentType == SegmentType.Line)
          {
            MapPoint startPt = await QueuedTask.Run(() => MapPointBuilder.CreateMapPoint(s.StartPoint.X, s.StartPoint.Y, s.StartPoint.Z, layerSpatRef));
            MapPoint endPt = await QueuedTask.Run(() => MapPointBuilder.CreateMapPoint(s.EndPoint.X, s.EndPoint.Y, s.EndPoint.Z, layerSpatRef));

            //add start of path to points collection
            if (num_iterations == 0)
            {
              pointsForKeyframes.Add(startPt);
            }

            if (num_iterations == segmentCount - 1 || segmentCount == 1)
            {
              pathEndPt = endPt; //store path end pt. This will be the last keyframe.
            }

            double distCoveredAlongSeg = Math.Abs(createKeyAtDist - skippedDistance); //we are accouunting for skipped distances from previous segments

            if (distCoveredAlongSeg < segmentLength)
            {
              MapPoint keyPt = await CreatePointAlongSegment(startPt, endPt, distCoveredAlongSeg, layerSpatRef);
              //add point to collection
              pointsForKeyframes.Add(keyPt);

              //skipped distance is used now, reset to zero
              skippedDistance = 0;

              //are more keyframes possible for this segment
              bool moreKeysPossible = ((segmentLength - distCoveredAlongSeg) >= createKeyAtDist);

              while (moreKeysPossible)
              {
                double keyAtDistAlongSeg = distCoveredAlongSeg + createKeyAtDist;

                keyPt = await CreatePointAlongSegment(startPt, endPt, keyAtDistAlongSeg, layerSpatRef);
                //add point to collection
                pointsForKeyframes.Add(keyPt);

                distCoveredAlongSeg += createKeyAtDist;

                moreKeysPossible = ((segmentLength - distCoveredAlongSeg) > createKeyAtDist);
              }

              //if any segment length left then add to skipped distance
              skippedDistance += (segmentLength - distCoveredAlongSeg);
            }
            else
            {
              //add this segment's length to skipped distance as no keyframe could be created along it
              skippedDistance += segmentLength;
            }
          }
          else if (s.SegmentType == SegmentType.EllipticArc)
          {
            EllipticArcSegment ellipArc = s as EllipticArcSegment;
            MapPoint startPt = await QueuedTask.Run(() => MapPointBuilder.CreateMapPoint(s.StartPoint.X, s.StartPoint.Y, s.StartPoint.Z, layerSpatRef));
            MapPoint endPt = await QueuedTask.Run(() => MapPointBuilder.CreateMapPoint(s.EndPoint.X, s.EndPoint.Y, s.EndPoint.Z, layerSpatRef));

            double radius = Math.Sqrt((ellipArc.CenterPoint.X - startPt.X) * (ellipArc.CenterPoint.X - startPt.X) + (ellipArc.CenterPoint.Y - startPt.Y) * (ellipArc.CenterPoint.Y - startPt.Y));
            double angle = ellipArc.CentralAngle;
            MapPoint centerPt = await QueuedTask.Run(() => MapPointBuilder.CreateMapPoint(ellipArc.CenterPoint.X, ellipArc.CenterPoint.Y, (s.StartPoint.Z + s.EndPoint.Z) / 2, layerSpatRef));

            //add start of path to points collection
            if (num_iterations == 0)
            {
              pointsForKeyframes.Add(startPt);
            }

            if (num_iterations == segmentCount - 1 || segmentCount == 1)
            {
              pathEndPt = endPt; //store path end pt. This will be the last keyframe.
            }

            double distCoveredAlongSeg = Math.Abs(createKeyAtDist - skippedDistance); //we are accouunting for skipped distances from previous segments

            if (distCoveredAlongSeg < segmentLength)
            {
              MapPoint keyPt = await CreatePointAlongArc(startPt, endPt, centerPt, angle * distCoveredAlongSeg / segmentLength, radius, layerSpatRef, ellipArc.IsMinor, ellipArc.IsCounterClockwise);
              //add point to collection
              pointsForKeyframes.Add(keyPt);

              //skipped distance is used now, reset to zero
              skippedDistance = 0;

              //are more keyframes possible for this segment
              bool moreKeysPossible = ((segmentLength - distCoveredAlongSeg) >= createKeyAtDist);

              while (moreKeysPossible)
              {
                double keyAtDistAlongSeg = distCoveredAlongSeg + createKeyAtDist;

                keyPt = await CreatePointAlongArc(startPt, endPt, centerPt, angle * keyAtDistAlongSeg / segmentLength, radius, layerSpatRef, ellipArc.IsMinor, ellipArc.IsCounterClockwise);
                //add point to collection
                pointsForKeyframes.Add(keyPt);

                distCoveredAlongSeg += createKeyAtDist;

                moreKeysPossible = ((segmentLength - distCoveredAlongSeg) > createKeyAtDist);
              }

              //if any segment length left then add to skipped distance
              skippedDistance += (segmentLength - distCoveredAlongSeg);
            }
            else
            {
              //add this segment's length to skipped distance as no keyframe could be created along it
              skippedDistance += segmentLength;
            }

          }

          num_iterations++;
        }
      }

      //now iterate over the points list and create keyframes

      double timeSpanValue = accumulatedDuration;
      TimeSpan keyframeTimespan = TimeSpan.FromSeconds(timeSpanValue);

      for (int i = 0; i < pointsForKeyframes.Count; i++)
      {
        MapPoint currentPt = pointsForKeyframes[i];
        MapPoint nextPt = null;

        if (i + 1 < pointsForKeyframes.Count)
        {
          nextPt = pointsForKeyframes[i + 1];
        }
        else
        {
          nextPt = pathEndPt;
        }

        timeSpanValue = i * keyEveryNSecond + accumulatedDuration;
        keyframeTimespan = TimeSpan.FromSeconds(timeSpanValue);

        SetPitchAndHeadingForLine(currentPt, nextPt);
        await CreateCameraKeyframe(mapView, currentPt, transformation, cameraTrack, keyframeTimespan, _keyframePitch, _keyframeHeading);

        if (i == pointsForKeyframes.Count - 1 && skippedDistance > 0)
        {
          keyframeTimespan = TimeSpan.FromSeconds(TotalDuration + accumulatedDuration);
          await CreateCameraKeyframe(mapView, pathEndPt, transformation, cameraTrack, keyframeTimespan, _keyframePitch, _keyframeHeading);
        }
      }
    }

    //Use this method if you want keyframes ONLY at line vertices. This is good if the line is highly densified.
    //However, you will get sharp turns at corners because there is no attempt to smooth the animation
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static async Task CreateKeyframes_AtVertices(MapView mapView, SpatialReference layerSpatRef, ProjectionTransformation transformation,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
                                                    CameraTrack cameraTrack, Polyline lineGeom, IEnumerator<ReadOnlySegmentCollection> segments,
                                                    int segmentCount, double pathLength)
    {
      double segmentLength = 0;
      int num_iterations = 0;
      segments.Reset();

      //process each segment depending upon its type - straight line or arc
      while (segments.MoveNext())
      {
        ReadOnlySegmentCollection seg = segments.Current;
        double accumulatedDuration = mapView.Map.Animation.Duration.TotalSeconds + ((mapView.Map.Animation.Duration.TotalSeconds > 0) ? ANIMATION_APPEND_TIME : 0); // 0;

        foreach (Segment s in seg)
        {
          segmentLength = Math.Sqrt((s.EndPoint.X - s.StartPoint.X) * (s.EndPoint.X - s.StartPoint.X) +
                                        (s.EndPoint.Y - s.StartPoint.Y) * (s.EndPoint.Y - s.StartPoint.Y) +
                                          (s.EndPoint.Z - s.StartPoint.Z) * (s.EndPoint.Z - s.StartPoint.Z));

          double segmentDuration = (TotalDuration / pathLength) * segmentLength;

          MapPoint startPt = await QueuedTask.Run(() => MapPointBuilder.CreateMapPoint(s.StartPoint.X, s.StartPoint.Y, s.StartPoint.Z, layerSpatRef));
          MapPoint endPt = await QueuedTask.Run(() => MapPointBuilder.CreateMapPoint(s.EndPoint.X, s.EndPoint.Y, s.EndPoint.Z, layerSpatRef));

          //create keyframe at start vertex of path in map space
          double timeSpanValue = accumulatedDuration;
          TimeSpan keyframeTimespan = TimeSpan.FromSeconds(timeSpanValue);
          SetPitchAndHeadingForLine(startPt, endPt);
          await CreateCameraKeyframe(mapView, startPt, transformation, cameraTrack, keyframeTimespan, _keyframePitch, _keyframeHeading);

          //Create a keyframe at end point of segment only for the end point of last segment
          //Otherwise we will get duplicate keyframes at end of one segment and start of the next one
          if (num_iterations == segmentCount - 1)
          {
            timeSpanValue = accumulatedDuration + segmentDuration;
            keyframeTimespan = TimeSpan.FromSeconds(timeSpanValue);

            if (SelectedCameraView == "Face target")
            {
              SetPitchAndHeadingForLine(endPt, TargetPoint);
            }

            await CreateCameraKeyframe(mapView, endPt, transformation, cameraTrack, keyframeTimespan, _keyframePitch, _keyframeHeading);
          }

          accumulatedDuration += segmentDuration;
          num_iterations++;
        }
      }
    }

    private static async Task<MapPoint> CreatePointAlongSegment(MapPoint startPt, MapPoint endPt, double distanceFromStartPoint, SpatialReference spatRef)
    {
      System.Windows.Media.Media3D.Point3D fromPt = new System.Windows.Media.Media3D.Point3D(startPt.X, startPt.Y, startPt.Z);
      System.Windows.Media.Media3D.Point3D toPt = new System.Windows.Media.Media3D.Point3D(endPt.X, endPt.Y, endPt.Z);
      System.Windows.Media.Media3D.Vector3D lineVec = new System.Windows.Media.Media3D.Vector3D(toPt.X - fromPt.X, toPt.Y - fromPt.Y, toPt.Z - fromPt.Z);

      lineVec.Normalize();

      System.Windows.Media.Media3D.Point3D ptAlong = new System.Windows.Media.Media3D.Point3D(fromPt.X + distanceFromStartPoint * (lineVec.X),
                                                          fromPt.Y + distanceFromStartPoint * (lineVec.Y),
                                                          fromPt.Z + distanceFromStartPoint * (lineVec.Z));

      MapPoint intermediateKeyframePoint = await QueuedTask.Run(() => MapPointBuilder.CreateMapPoint(ptAlong.X, ptAlong.Y, ptAlong.Z, spatRef));
      return intermediateKeyframePoint;
    }
    private static async Task<MapPoint> CreatePointAlongArc(MapPoint startPt, MapPoint endPt, MapPoint centerPt, double angle, double radius, SpatialReference spatRef, bool arcIsMinor, bool arcIsCounterClockwise)
    {
      System.Windows.Media.Media3D.Vector3D start = new System.Windows.Media.Media3D.Vector3D(startPt.X - centerPt.X, startPt.Y - centerPt.Y, startPt.Z - centerPt.Z);
      System.Windows.Media.Media3D.Vector3D end = new System.Windows.Media.Media3D.Vector3D(endPt.X - centerPt.X, endPt.Y - centerPt.Y, endPt.Z - centerPt.Z);

      System.Windows.Media.Media3D.Vector3D normalOfPlane = new System.Windows.Media.Media3D.Vector3D();
      normalOfPlane = System.Windows.Media.Media3D.Vector3D.CrossProduct(start, end);

      //Two ortho vectors: orthoVec and start
      System.Windows.Media.Media3D.Vector3D orthoVec = new System.Windows.Media.Media3D.Vector3D();
      orthoVec = System.Windows.Media.Media3D.Vector3D.CrossProduct(normalOfPlane, start);

      //If this is not done then half of the keyframes for S-shaped curve are not on the curve
      if (arcIsMinor && !arcIsCounterClockwise)
        orthoVec.Negate();

      //Normalize
      start.Normalize();
      orthoVec.Normalize();

      System.Windows.Media.Media3D.Vector3D ptAlong = new System.Windows.Media.Media3D.Vector3D();
      ptAlong = radius * Math.Cos(angle) * start + radius * Math.Sin(angle) * orthoVec;

      MapPoint intermediateKeyframePoint = await QueuedTask.Run(() => MapPointBuilder.CreateMapPoint(ptAlong.X + centerPt.X, ptAlong.Y + centerPt.Y, ptAlong.Z + centerPt.Z, spatRef));
      return intermediateKeyframePoint;
    }
    private static async Task CreateCameraKeyframe(MapView mapView, MapPoint orig_cameraPoint, ProjectionTransformation transformation,
                                                             CameraTrack cameraTrack, TimeSpan currentTimespanValue, double pitch, double heading, bool ignoreRotation = false, bool ignoreTranslation = false)
    {
      await QueuedTask.Run(() =>
      {
        Keyframe keyFrame = null;
        MapPoint projected_cameraPoint = (MapPoint)GeometryEngine.Instance.ProjectEx(orig_cameraPoint, transformation);

        if (mapView.ViewingMode == MapViewingMode.Map)
        {
          var camera = new Camera(projected_cameraPoint.X, projected_cameraPoint.Y, CameraZOffset, heading, null, CameraViewpoint.LookAt);
          keyFrame = cameraTrack.CreateKeyframe(camera, currentTimespanValue, AnimationTransition.FixedArc, .5);
        }
        else
        {
          var camera = new Camera(projected_cameraPoint.X, projected_cameraPoint.Y, (projected_cameraPoint.Z + CameraZOffset), pitch, heading, null, CameraViewpoint.LookAt);
          keyFrame = cameraTrack.CreateKeyframe(camera, currentTimespanValue, AnimationTransition.FixedArc, .5);
        }

        if (ignoreRotation)
        {
          CameraKeyframe camKey = keyFrame as CameraKeyframe;
          camKey.HeadingTransition = AnimationTransition.None;
          camKey.RollTransition = AnimationTransition.None;
          camKey.PitchTransition = AnimationTransition.None;
        }
        if (ignoreTranslation)
        {
          CameraKeyframe camKey = keyFrame as CameraKeyframe;
          camKey.XTransition = AnimationTransition.None;
          camKey.YTransition = AnimationTransition.None;
          camKey.ZTransition = AnimationTransition.None;
        }
      });
    }

    private static void SetPitchAndHeadingForLine(MapPoint startPt, MapPoint endPt)
    {
      if (SelectedCameraView == "Top down")
      {
        _keyframeHeading = CalculateHeading(startPt, endPt);
        _keyframePitch = -90;
      }
      else if (SelectedCameraView == "Top down - face north")
      {
        _keyframeHeading = 0;
        _keyframePitch = -90;
      }
      else if (SelectedCameraView == "Custom pitch")
      {
        _keyframeHeading = CalculateHeading(startPt, endPt);
        _keyframePitch = CustomPitch;
      }
      else if (SelectedCameraView == "View along" || SelectedCameraView == "Face backward")
      {
        _keyframeHeading = CalculateHeading(startPt, endPt);
        _keyframePitch = CalculatePitch(startPt, endPt);
      }
      else if (SelectedCameraView == "Face target")
      {
        _keyframeHeading = CalculateHeading(startPt, TargetPoint);
        _keyframePitch = CalculatePitch(startPt, TargetPoint);
      }
    }
    private static double CalculateHeading(MapPoint startPt, MapPoint endPt)
    {
      double dx, dy, dz, angle, heading;

      if (SelectedCameraView == "Top down - face north")
      {
        heading = 0;
      }
      else
      {
        dx = endPt.X - startPt.X;
        dy = endPt.Y - startPt.Y;

        //need to apply z-conversion factor to target Z
        dz = (SelectedCameraView == "Face target") ? endPt.Z * Z_CONVERSION_FACTOR - startPt.Z : endPt.Z - startPt.Z;

        angle = Math.Atan2(dy, dx);
        heading = 180 + (90 + angle * 180 / Math.PI);

        if (SelectedCameraView == "Face backward") { heading = heading - 180; }
      }

      return heading;
    }
    private static double CalculatePitch(MapPoint startPt, MapPoint endPt)
    {
      double dx, dy, dz, pitchForLookingAtTarget;
      dx = startPt.X - endPt.X;
      dy = startPt.Y - endPt.Y;
      //dz = startPt.Z - endPt.Z;

      //need to apply z-conversion factor to target Z
      dz = (SelectedCameraView == "Face target") ? startPt.Z - endPt.Z * Z_CONVERSION_FACTOR : startPt.Z - endPt.Z;

      //try dividing by 0.00017 unit conversion factor if units = degree
      SpatialReference spatRef = startPt.SpatialReference;
      if (spatRef.Unit.Name == "Degree")
      {
        dx = dx * 111000;
        dy = dy * 111000;
      }

      double test = dz / Math.Sqrt(dx * dx + dy * dy + dz * dz);
      pitchForLookingAtTarget = -(90 - Math.Acos(test) * 180 / Math.PI);
      return pitchForLookingAtTarget;
    }

    #endregion
  }
}

