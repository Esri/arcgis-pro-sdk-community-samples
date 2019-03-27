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
using ArcGIS.Core.CIM;
using ArcGIS.Core.Events;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Editing;

namespace SeqNum
{
  class SeqNumTool : MapTool
  {
    SubscriptionToken _mmpcToken = null;
    SeqNumControlViewModel _vm;

    public SeqNumTool()
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Line;
      SketchOutputMode = SketchOutputMode.Map;
      UseSnapping = true;
      ControlID = "SeqNum_SeqNumControl";
    }

    protected override Task OnToolActivateAsync(bool hasMapViewChanged)
    {    
      //set a ref to the embeded tool control
      _vm = EmbeddableControl as SeqNumControlViewModel;

      //populate layer list of visibible, editable polygon, line and point layers
      PopulateLayerList();

      //listen to any layer changes that could affect the layer list
      //use a token so we only have one event
      if (_mmpcToken == null)
        _mmpcToken = MapMemberPropertiesChangedEvent.Subscribe(OnMapMemberPropertiesChanged, false);

      return Task.FromResult(true);
    }

    protected override Task OnToolDeactivateAsync(bool hasMapViewChanged)
    {
      //stop listeneing to layer changed event
      MapMemberPropertiesChangedEvent.Unsubscribe(_mmpcToken);
      _mmpcToken = null;
      return Task.FromResult(true);
    }

    protected override void OnToolKeyDown(MapViewKeyEventArgs k)
    {
      //display options form to set help set start value
      if (k.Key == System.Windows.Input.Key.Multiply | k.Key == System.Windows.Input.Key.O)
      {
        var svf = new StartValue();
        svf.ShowDialog();

        if (svf.DialogResult.HasValue && svf.DialogResult.Value)
          _vm.startValue = svf._svalue;
      }
    }
    /// <summary>
    /// Sets sequential numbers with the sketch
    /// </summary>
    /// <param name="geometry">The sketch geometry as a line</param>
    /// <remarks>
    /// The goal is to create a list of feature oids in order along the sketch where
    /// the list is then itterated and a sequential number assiged to a field.
    /// 
    /// This done by creating a point for each feature along the sketch then
    /// adding the distance of the point along the original sketch to a sorted dictionary
    /// with the feature. The dictionary is then itterated by distance (the dictionary key)
    /// 
    /// For polygons the feature point is created by intersecting the sketch with each feature
    /// then taking the midpoint of that intersect.
    /// For lines the sketch is intersected with each line to create the intersecting point.
    /// For points the intersecting point is used.
    /// </remarks>
    protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      if (Module1._targetLayer == null)
        return Task.FromResult(true);

      //run on MCT
      return QueuedTask.Run(() =>
      {
        //get the selected layer
        var featLayer = MapView.Active.Map.FindLayers(Module1._targetLayer).First() as FeatureLayer;

        //find features under the sketch
        var features = MapView.Active.GetFeatures(geometry);
        //if no features then return
        if (features.Count == 0)
          return true;

        //get oids for featlayer, otherwise return
        var featOids = new List<long>();
        if (!features.TryGetValue(featLayer, out featOids))
          return true;

        //dictionary to hold list of feature oids along the sketch
        var distanceFromStart = new SortedDictionary<double, long>();
        var insp = new Inspector();
        MapPoint sketchPoint = null;
        var featLayerShapeType = featLayer.ShapeType;

        //loop through each feature under the sketch
        foreach (var oid in featOids)
        {
          //load the feature to get the feature geom
          insp.Load(featLayer, oid);
          var featShape = GeometryEngine.Instance.Project(insp.Shape, MapView.Active.Map.SpatialReference);

          //create sketchpoint based on layer geometry
          switch (featLayerShapeType)
          {
            case esriGeometryType.esriGeometryPolygon:
              //intersect poly with sketch
              var sketchInPoly = GeometryEngine.Instance.Intersection(geometry, featShape) as Polyline;

              //skip if sketchinpoly length is 0. Likely sketch started or ended on polygon edge.
              if (sketchInPoly.Length == 0)
                continue;

              //Find the midpoint of the intersected sketch
              sketchPoint = GeometryEngine.Instance.MovePointAlongLine(sketchInPoly, 0.5, true, 0, SegmentExtension.NoExtension);
              break;

            case esriGeometryType.esriGeometryPolyline:
              //get the first intersection point between the sketch and the lines
              var intersectionPoints = GeometryEngine.Instance.Intersection(geometry, featShape, GeometryDimension.esriGeometry0Dimension) as Multipoint;
              sketchPoint = intersectionPoints.Points[0];
              break;

            case esriGeometryType.esriGeometryPoint:
              //sketchpoint is the feature
              sketchPoint = featShape as MapPoint;
              break;
          }

          //find the distance of the sketchPoint along the original sketch and add to sorted dictionary for this feature oid
          double distanceAlongCurve, distanceFromCurve = 0;
          LeftOrRightSide whichSide;
          GeometryEngine.Instance.QueryPointAndDistance(geometry as Multipart, SegmentExtension.NoExtension, sketchPoint, AsRatioOrLength.AsLength, out distanceAlongCurve, out distanceFromCurve, out whichSide);
          distanceFromStart.Add(distanceAlongCurve, oid);
        }

        //get the current and increment values
        int currentValue;
        if (!int.TryParse(Module1._startValue, out currentValue))
          return false;
        var incValue = Convert.ToInt32(Module1._incValue);

        //if the target field is string then parse the format for easy number insertion
        string textFormatter = "";
        string dCount = "";
        object fieldValue;
        if (Module1._isTargetFieldString)
        {
          //rebuild the string formatter
          if (String.IsNullOrWhiteSpace(Module1._stringFormat))
            textFormatter = "#";
          else
          {
            //get the D count (the number of #)
            dCount = "D" + Module1._stringFormat.Count(c => c == '#').ToString();
            //reduce the # to one
            int hashFound = 0;
            textFormatter = new string(Module1._stringFormat.Where(c => c != '#' || ++hashFound <= 1).ToArray());
          }
        }

        //create edit operation
        var op = new EditOperation();
        op.Name = "Sequential numbering";
        op.SelectModifiedFeatures = false;
        op.SelectNewFeatures = false;

        //loop through each distance pair and increment field value
        foreach (var distancePair in distanceFromStart)
        {
          if (Module1._isTargetFieldString)
            fieldValue = textFormatter.Replace("#", currentValue.ToString(dCount));
          else
            fieldValue = currentValue;

          var modDict = new Dictionary<string, object> { { Module1._targetField, fieldValue } };
          op.Modify(featLayer, distancePair.Value, modDict);
          currentValue += incValue;
        }

        //set current start value back on the control
        _vm.startValue = Convert.ToString(currentValue);

        return op.Execute();
      });
    }


    private void OnMapMemberPropertiesChanged(MapMemberPropertiesChangedEventArgs obj)
    {
      //could try to work out if a relevant layer changed
      //but just reset the whole list
      PopulateLayerList();
    }

    private void PopulateLayerList()
    {
      //find visible, editible polygon, line and point layers
      var layerList = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>()
      .Where(lyr => (lyr.ShapeType == esriGeometryType.esriGeometryPolygon | lyr.ShapeType == esriGeometryType.esriGeometryPolyline 
        | lyr.ShapeType == esriGeometryType.esriGeometryPoint)
        && lyr.IsEditable && lyr.IsVisible)
      .Select(lyr => lyr.Name).ToList();

      //set the layer dropdown list if required
      if (!layerList.SequenceEqual(Module1._layerList))
        _vm.layerList = layerList;  
    }
  }
}
