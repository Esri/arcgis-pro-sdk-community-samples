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
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace AnnoTools
{
  /// <summary>
  /// Demonstrates how to create a new annotation template.  Use the CreateTemplate extension method after setting the AnnotationProperties on an 
  /// Inspector object. 
  /// </summary>
  /// <remarks>
  /// The only guaranteed fields in an annotation feature class schema are AnnotationClassID, SymbolID, Element, FeatureID, ZOrder, 
  /// Status and Shape.  All other fields which store text formatting attributes (such as TextString, FontName,  VerticalAlignment etc) 
  /// are not guaranteed to exist in the physical schema.  This is different from the annotation schema in ArcGIS 10x where all fields 
  /// existed and were unable to be deleted. In ArcGIS Pro, these text formatting fields are able to be deleted by the user if they exist; 
  /// they are no longer designated as protected or system fields. 
  /// <para></para>
  /// Use the GetAnnotationProperties and SetAnnotationProperties on an Inspector object to access these text formatting attributes. Do not 
  /// use the inspector[fieldName] methodology as the fields may not exist. 
  /// </remarks>
  internal class AnnoNewTemplate : Button
  {
    protected override async void OnClick()
    {
      // get an anno layer
      AnnotationLayer annoLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<AnnotationLayer>().FirstOrDefault();
      if (annoLayer == null)
        return;

      Inspector insp = null;
      bool result = await QueuedTask.Run(() =>
      {
        // get the anno feature class
        var fc = annoLayer.GetFeatureClass() as ArcGIS.Core.Data.Mapping.AnnotationFeatureClass;

        // get the featureclass CIM definition which contains the labels, symbols
        var cimDefinition = fc.GetDefinition() as ArcGIS.Core.Data.Mapping.AnnotationFeatureClassDefinition;
        var labels = cimDefinition.GetLabelClassCollection();
        var symbols = cimDefinition.GetSymbolCollection();

        // make sure there are labels, symbols
        if ((labels.Count == 0) || (symbols.Count == 0))
          return false;

        // find the label class required
        //   typically you would use a subtype name or some other characteristic
        // in this case lets just use the first one

        // var label = labels[0];

        // find the label class required
        //   typically you would use a subtype name or some other characteristic

        // use the first label class
        var label = labels[0];
        if (labels.Count > 1)
        {
          // find a label class based on template name 
          foreach (var LabelClass in labels)
          {
            if (LabelClass.Name == "Basic")
            {
              label = LabelClass;
              break;
            }
          }
        }

        // each label has a textSymbol
        // the symbolName *should* be the symbolID to be used
        var symbolName = label.TextSymbol.SymbolName;
        int symbolID = -1;
        if (!int.TryParse(symbolName, out symbolID))
        {
          // int.TryParse fails - attempt to find the symbolName in the symbol collection
          foreach (var symbol in symbols)
          {
            if (symbol.Name == symbolName)
            {
              symbolID = symbol.ID;
              break;
            }
          }
        }
        // no symbol?
        if (symbolID == -1)
          return false;

        // load the schema
        insp = new Inspector();
        insp.LoadSchema(annoLayer);

        // ok to access AnnotationClassID, SymbolID this way - it is guaranteed to exist
        insp["AnnotationClassID"] = label.ID;
        insp["SymbolID"] = symbolID;

        // set up some text properties
        AnnotationProperties annoProperties = insp.GetAnnotationProperties();
        annoProperties.FontSize = 36;
        annoProperties.TextString = "My Annotation feature";
        annoProperties.VerticalAlignment = VerticalAlignment.Top;
        annoProperties.HorizontalAlignment = HorizontalAlignment.Justify;

        // assign the properties back to the inspector
        insp.SetAnnotationProperties(annoProperties);

        // set up tags
        var tags = new[] { "Annotation", "tag1", "tag2" };

        // set up default tool - use daml-id rather than guid
        string defaultTool = "esri_editing_SketchStraightAnnoTool";

        // tool filter is the tools to filter OUT
        var toolFilter = new[] { "esri_editing_SketchCurvedAnnoTool" };

        // create a new CIM template  - new extension method
        var newTemplate = annoLayer.CreateTemplate("My new template", "sample template description", insp, defaultTool, tags, toolFilter);

        return (newTemplate != null);
      });
    }
  }
}
