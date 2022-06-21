//   Copyright 2019 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

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
  /// Illustrates how to modify the text attributes and symbol of an annotation feature using the 
  /// GetAnnotationProperties and SetAnnotationProperties methods on the inspector, followed by the EditOperation.Modify method.
  /// <para></para>
  /// The text attributes of an annotation feature are represented as a CIMTextGraphic.The CIMTextGraphic 
  /// contains the text string, text formatting attributes (such as alignment, angle, font, color, etc), and other information (such as 
  /// callouts and leader lines). It also has a shape which represents the baseline geometry that the annotation text string sits upon.
  /// </summary>
  /// <remarks>
  /// Use the GetAnnotationProperties method on the <see cref="ArcGIS.Desktop.Editing.Attributes.Inspector"/> object to retrieve the 
  /// AnnotationProperties class which allows you to directly access and set the majority of the CIMTextGraphic properties including it's geometry.  
  /// If you need to access the CIMTextGraphic itself use the TextGraphic property.  Use the LoadFromTextGraphic to set a new CIMTextGraphic.  After modifying the 
  /// annotation properties, don't forget to use the SetAnnotationProperties method on the inspector to write the properties. Then pass the inspector to 
  /// EditOperation.Modify.  This tool illustrates this pattern.
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
        foreach (var annoLayer in features.ToDictionary().Keys.OfType<AnnotationLayer>())
        {
          // are there features?
          var featOids = features[annoLayer];
          if (featOids.Count == 0)
            continue;

            // create the edit operation
            if (op == null)
            {
              op = new EditOperation();
              op.Name = "Update annotation symbol";
              op.SelectModifiedFeatures = true;
              op.SelectNewFeatures = false;
            }

          // load an inspector with all the features
          var insp = new Inspector();
          insp.Load(annoLayer, featOids);

          // get the annotation properties
          var annoProperties = insp.GetAnnotationProperties();

                        // change the text 
          annoProperties.TextString = "Hello World";
          // you can use a textstring with embedded formatting information 
          //annoProperties.TextString = "My <CLR red = \"255\" green = \"0\" blue = \"0\" >Special</CLR> Text";

          // change font color to red
          annoProperties.Color = ColorFactory.Instance.RedRGB;

          // change the horizontal alignment
          annoProperties.HorizontalAlignment = HorizontalAlignment.Center;

          // set the annotation properties back on the inspector
          insp.SetAnnotationProperties(annoProperties);

          // call modify
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


