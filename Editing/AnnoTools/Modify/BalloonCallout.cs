//   Copyright 2018 Esri

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
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Editing;
using ArcGIS.Core.Data;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Editing.Attributes;

namespace AnnoTools
{
  /// <summary>
  /// Illustrates how to add a balloon callout with a leader line to an annotation feature.  This involves interacting with the CIMTextGraphic of the 
  /// annotation feature directly. 
  /// </summary>
  /// <remarks>
  /// First use the <see cref="Inspector.GetAnnotationProperties"/> method to obtain the AnnotationProperties object.  Obtain the CIMTextGraphic via the 
  /// <see cref="AnnotationProperties.TextGraphic"/> property.  Add leader lines and callouts to the CIMTextGraphic then call 
  /// <see cref="AnnotationProperties.LoadFromTextGraphic(CIMTextGraphic)"/> to set the new CIMTextGraphic onto the annotation properties. Finally call
  /// <see cref="Inspector.SetAnnotationProperties(AnnotationProperties)"/> to update the Inspector object.
  /// <para></para>
  /// </remarks>
  internal class BalloonCallout : MapTool
  {
    public BalloonCallout()
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
        var insp = new Inspector();
        foreach (var annoLayer in features.Keys.OfType<AnnotationLayer>())
        {
          // are there features?
          var featOids = features[annoLayer];
          if (featOids.Count == 0)
            continue;

          // for each feature
          foreach (var oid in featOids)
          {
            // load an inspector
            insp.Load(annoLayer, oid);

            // get the annotation properties
            var annoProperties = insp.GetAnnotationProperties();

            // get the text graphic
            var cimTextGraphic = annoProperties.TextGraphic;
            if (cimTextGraphic != null)
                    {
                      //
                      // add a leader point to the text graphic
                      //

                      // get the feature shape
              var textExtent = insp.Shape;

                      // find the lower left of the text extent
                      var extent = textExtent.Extent;
                      var lowerLeft = MapPointBuilder.CreateMapPoint(extent.XMin, extent.YMin, textExtent.SpatialReference);
                      // move it a little to the left and down
                      var newPoint = GeometryEngine.Instance.Move(lowerLeft, -40, -40);

                      // create a leader point
                      CIMLeaderPoint leaderPt = new CIMLeaderPoint();
                      leaderPt.Point = newPoint as MapPoint;

                      // add to a list
                      List<CIMLeader> leaderArray = new List<CIMLeader>();
                      leaderArray.Add(leaderPt);

                      // assign to the textGraphic
              cimTextGraphic.Leaders = leaderArray.ToArray();


                      //
                      // add the balloon callout
                      //

                      // create the callout
                      CIMBalloonCallout balloon = new CIMBalloonCallout();
                      // yellow background
                      balloon.BackgroundSymbol = SymbolFactory.Instance.ConstructPolygonSymbol(ColorFactory.Instance.CreateRGBColor(255, 255, 0));
                      // set the balloon style
                      balloon.BalloonStyle = BalloonCalloutStyle.RoundedRectangle;
                      // add a text margin
                      CIMTextMargin textMargin = new CIMTextMargin() { Left = 4, Right = 4, Bottom = 4, Top = 4 };
                      balloon.Margin = textMargin;

                      // asign it to the textSymbol
              var symbol = cimTextGraphic.Symbol.Symbol;
                      var textSymbol = symbol as CIMTextSymbol;

                      textSymbol.Callout = balloon;

              // assign the text graphic back to the annotation properties
              annoProperties.LoadFromTextGraphic(cimTextGraphic);

              // assign the annotation properties back to the inspector
              insp.SetAnnotationProperties(annoProperties);
                      }

            // create the edit operation
            if (op == null)
                      {
              op = new EditOperation();
              op.Name = "Alter symbol to balloon callout";
              op.SelectModifiedFeatures = true;
              op.SelectNewFeatures = false;
                      }

            // modify the feature
            op.Modify(insp);
          }
        }

        if ((op != null) && !op.IsEmpty)
        {
          bool result = op.Execute();
          return result;
        }
        return
          false;
      });

    }
  }
}
