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
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Editing.Templates;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace AnnoTools
{
  /// <summary>
  /// Demonstrates how to create a new annotation template from an existing template.  Use the CreateTemplate extension method after setting the AnnotationProperties on an 
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
  internal class AnnoNewTemplateFromExistingTemplate : Button
  {
    protected override async void OnClick()
    {
      // get an anno layer
      AnnotationLayer annoLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<AnnotationLayer>().FirstOrDefault();
      if (annoLayer == null)
        return;

      bool result = await QueuedTask.Run(async () =>
      {

        // get an existing template
        EditingTemplate template = annoLayer.GetTemplates().FirstOrDefault();
        if (template == null)
          return false;

        // make sure it is active before you get the inspector
        await template.ActivateDefaultToolAsync();

        var insp = template.Inspector;
        if (insp == null)
          return false;

        // set up some properties
        AnnotationProperties annoProperties = insp.GetAnnotationProperties();
        annoProperties.TextString = "special text";
        annoProperties.Color = ColorFactory.Instance.GreenRGB;
        annoProperties.SmallCaps = true;
        insp.SetAnnotationProperties(annoProperties);

        // set up default tool - use daml-id rather than guid
        string defaultTool = "esri_editing_SketchStraightAnnoTool";

        // dont alter the filter or tags

        // create a new CIM template  - new extension method
        var newTemplate = annoLayer.CreateTemplate("template from existing template", "sample template description", insp, defaultTool);
        return (newTemplate != null);
      });

    }
  }
}
