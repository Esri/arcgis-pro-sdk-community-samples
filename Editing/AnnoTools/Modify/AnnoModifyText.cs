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

using System.Linq;
using System.Threading.Tasks;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Editing;

namespace AnnoTools
{
  /// <summary>
  /// Illustrates the recommended way to modify the text of an annotation feature at ArcGIS Pro 2.2 using the 
  /// GetAnnotationProperties and SetAnnotationProperties methods on the inspector, followed by the EditOperation.Modify method. 
  /// </summary>
  /// <remarks>
  /// The only guaranteed fields in an annotation feature class schema are AnnotationClassID, SymbolID, Element, FeatureID, ZOrder, 
  /// Status and Shape.  All other fields which store text formatting attributes (such as TextString, FontName,  VerticalAlignment etc) 
  /// are not guaranteed to exist in the physical schema.   This is different from the annotation schema in ArcGIS 10x where all fields 
  /// existed and were unable to be deleted. In ArcGIS Pro, these text formatting fields are able to be deleted by the user if they exist; 
  /// they are no longer designated as protected or system fields. 
  /// <para></para>
  /// Take care when writing  or porting tools that create or modify annotation features, it is essential to take this important concept into account. 
  /// Do not use the inspector[fieldName] methodology for text formatting attributes as the field may not exist.  Use the GetAnnotationProperties 
  /// and SetAnnotationProperties methods on the inspector. 
  /// <para></para>
  /// </remarks>
  internal class AnnoModifyText : MapTool
  {
    public AnnoModifyText()
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
        foreach (var annoLayer in features.Keys.OfType<AnnotationLayer>())
        {
          // are there features?
          var featOids = features[annoLayer];
          if (featOids.Count == 0)
            continue;

          // use the inspector methodology - load multiple features at once
          var insp = new Inspector(true);
          insp.Load(annoLayer, featOids);

          // get the annotation proeprties
          var annoProperties = insp.GetAnnotationProperties();
          // assign the text string
          annoProperties.TextString = "Hello World";
          // you could also assign a text string such as the following which incorporates formatting tags
          // annoProperties.TextString = "My <CLR red = \"255\" green = \"0\" blue = \"0\" >Special</CLR> Text";

          // set the annotation propertes back on the inspector
          insp.SetAnnotationProperties(annoProperties);

          // create the edit operation
          if (op == null)
          {
            op = new EditOperation();
            op.Name = "Update annotation text";
            op.SelectModifiedFeatures = true;
            op.SelectNewFeatures = false;
          }

          op.Modify(insp);
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



