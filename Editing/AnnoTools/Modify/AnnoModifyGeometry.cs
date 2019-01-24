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

using System.Threading.Tasks;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Editing;
using ArcGIS.Core.Data;
using ArcGIS.Core.CIM;
using System.Linq;
using ArcGIS.Desktop.Editing.Attributes;

namespace AnnoTools
{
  /// <summary>
  /// Illustrates how to modify the baseline geometry of an annotation feature using the 
  /// GetAnnotationProperties and SetAnnotationProperties methods on the inspector. Then use the EditOperation.Modify method. 
  /// <para></para>
  /// Annotation feature classes store polygon geometry.  This polygon is the bounding box of the text of an annotation feature. The bounding box 
  /// is calculated from the text string, font, font size, angle orientation and other text formatting attributes of the feature. It is automatically 
  /// updated by the application each time the annotation attributes are modified. You should never need to access or modify an annotation features 
  /// polygon shape.  
  /// <para></para>
  /// The text attributes of an annotation feature are represented by a CIMTextGraphic. The CIMTextGraphic consists of the text string, text attributes 
  /// (such as verticalAlignment, horizontalAlignment, fontFamily, fontSize etc), callouts, leader lines and the CIMTextGraphic geometry. This geometry 
  /// can be a point, straight line, bezier curve or multipoint geometry and represents the baseline geometry that the text string sits upon. This
  /// geometry is the significant shape of an annotation feature that you will typically interact with in a custom Editing tool.
  /// <para></para>
  /// </summary>
  /// <remarks>
  /// Using the <see cref="ArcGIS.Desktop.Editing.Attributes.Inspector.Shape"/> method on an annotation feature will return the polygon shape.  Use the 
  /// GetAnnotationProperties method on the <see cref="ArcGIS.Desktop.Editing.Attributes.Inspector"/> object to retrieve the 
  /// AnnotationProperties class which allows you to directly access and set the majority of the CIMTextGraphic properties including it's geometry.  
  /// If you need to access the CIMTextGraphic itself use the TextGraphic property.  Use the LoadFromTextGraphic to set a new CIMTextGraphic.  After modifying the 
  /// annotation properties, don't forget to use the SetAnnotationProperties method on the inspector to write the properties. Then pass the inspector to 
  /// EditOperation.Modify.  This tool illustrates this pattern.
  /// </remarks>
  internal class AnnoModifyGeometry : MapTool
  {
    public AnnoModifyGeometry()
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Rectangle;
      SketchOutputMode = SketchOutputMode.Map;
    }

    protected override Task OnToolActivateAsync(bool active)
    {
      return base.OnToolActivateAsync(active);
    }

    /// <summary>
    /// Called when the sketch finishes. This is where we will create the edit operation and then execute it.
    /// </summary>
    /// <param name="geometry">The geometry created by the sketch.</param>
    /// <returns>A Task returning a Boolean indicating if the sketch complete event was successfully handled.</returns>
    protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      // execute on the MCT
      return QueuedTask.Run(() =>
      {
        // find features under the sketch 
        var features = MapView.Active.GetFeatures(geometry);
        if (features.Count == 0)
          return false;

        EditOperation op = null;
        // for each layer in the features retrieved
        foreach (var annoLayer in features.Keys.OfType<AnnotationLayer>())
        {
          // are there features?
          var featOids = features[annoLayer];
          if (featOids.Count == 0)
            continue;

          // for each feature
          foreach (var oid in featOids)
          {
            // Remember - the shape of an annotation feature is a polygon - the bounding box of the annotation text. 
            // We need to update the cimTextGraphic geometry. 

            // load the inspector with the feature
            var insp = new Inspector();
            insp.Load(annoLayer, oid);

            // get the annotation properties
            var annoProperties = insp.GetAnnotationProperties();
            // get the cimTextGraphic geometry
            Geometry textGraphicGeometry = annoProperties.Shape;

            // if cimTextGraphic geometry is not a polyline, ignore
            Polyline baseLine = textGraphicGeometry as Polyline;
            if (baseLine == null)
              continue;

            // rotate the baseline 90 degrees
            var origin = GeometryEngine.Instance.Centroid(baseLine);
            Geometry rotatedBaseline = GeometryEngine.Instance.Rotate(baseLine, origin, System.Math.PI / 2);

            // set the updated geometry back to the annotation properties
            annoProperties.Shape = rotatedBaseline;
            // assign the annotation properties back to the inspector
            insp.SetAnnotationProperties(annoProperties);

            // create the edit operation
            if (op == null)
            {
              op = new EditOperation();
              op.Name = "Update annotation baseline";
              op.SelectModifiedFeatures = true;
              op.SelectNewFeatures = false;
            }

            op.Modify(insp);

            // OR 
            // pass the updated geometry directly
            // op.Modify(layerKey, oid, rotatedBaseline);

            // OR 
            // use the Dictionary methodology

            //Dictionary<string, object> newAtts = new Dictionary<string, object>();
            //newAtts.Add("SHAPE", rotatedBaseline);
            //op.Modify(layer, oid, newAtts);

          }
        }

        // execute the operation
        if ((op != null) && !op.IsEmpty)
          return op.Execute();
        return
          false;

      });
    }
  }
}
