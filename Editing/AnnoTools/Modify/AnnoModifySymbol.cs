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

using System.Threading.Tasks;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Editing;
using ArcGIS.Core.Data;
using ArcGIS.Core.CIM;

namespace AnnoTools
{
  /// <summary>
  /// Illustrates how to modify the text attributes and symbol of an annotation feature using the EditOperation.Callback method.  
  /// <para></para>
  /// The text attributes of an annotation feature are represented as a CIMTextGraphic.The CIMTextGraphic 
  /// contains the text string, text formatting attributes (such as alignment, angle, font, color, etc), and other information (such as 
  /// callouts and leader lines). It also has a shape which represents the baseline geometry that the annotation text string sits upon.
  /// <para></para>
  /// At ArcGIS Pro 2.1 the only way to retrieve the CIMTextGraphic is with the GetGraphic method on the AnnotationFeature object.
  /// This tool illustrates this pattern.
  /// </summary>
  /// <remarks>
  /// This is the only way to update an annotation symbol in ArcGIS Pro 2.1. We will be providing additional patterns at ArcGIS Pro 2.2.
  /// </remarks>
  internal class AnnoModifySymbol : MapTool
  {
    public AnnoModifySymbol()
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Rectangle;
      SketchOutputMode = SketchOutputMode.Map;
    }

     protected override Task OnToolActivateAsync(bool active)
    {
      return base.OnToolActivateAsync(active);
    }

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
        foreach (var annoLayer in features.Keys)
        {
          // is it an anno layer?
          if (!(annoLayer is AnnotationLayer))
            continue;

          // are there features?
          var featOids = features[annoLayer];
          if (featOids.Count == 0)
            continue;

          // for each feature
          foreach (var oid in featOids)
          {
            // create the edit operation
            if (op == null)
            {
              op = new EditOperation();
              op.Name = "Update annotation symbol";
              op.SelectModifiedFeatures = true;
              op.SelectNewFeatures = false;
            }

            // use the callback method
            op.Callback(context =>
            {
              // find the feature
              QueryFilter qf = new QueryFilter();
              qf.WhereClause = "OBJECTID = " + oid.ToString();

              // use the table
              using (var table = annoLayer.GetTable())
              {
                // make sure you use a non-recycling cursor
                using (var rowCursor = table.Search(qf, false))
                {
                  rowCursor.MoveNext();
                  if (rowCursor.Current != null)
                  {
                    ArcGIS.Core.Data.Mapping.AnnotationFeature annoFeature = rowCursor.Current as ArcGIS.Core.Data.Mapping.AnnotationFeature;
                    if (annoFeature != null)
                    {
                      // get the CIMTextGraphic
                      var textGraphic = annoFeature.GetGraphic() as CIMTextGraphic;
                      if (textGraphic != null)
                      {
                        // change the text 
                        textGraphic.Text = "Hello World";
                                                 
                        // get the symbol reference
                        var cimSymbolReference = textGraphic.Symbol;
                        // get the symbol
                        var cimSymbol = cimSymbolReference.Symbol;

                        // change the color to red
                        cimSymbol.SetColor(ColorFactory.Instance.RedRGB);

                        //// change the horizontal alignment
                        //var cimTextSymbol = cimSymbol as CIMTextSymbol;
                        //cimTextSymbol.HorizontalAlignment = HorizontalAlignment.Center;

                        // update the symbol
                        textGraphic.Symbol = cimSymbol.MakeSymbolReference();

                        // update the graphic
                        annoFeature.SetGraphic(textGraphic);

                        // store
                        annoFeature.Store();

                        // refresh the cache
                        context.Invalidate(annoFeature);
                      }
                    }
                  }
                }
              }
            }, annoLayer.GetTable());
             
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


