//   Copyright 2026 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using ArcGIS.Core.CIM;
using ArcGIS.Core.Data.DDL;
using ArcGIS.Core.Data.Mapping;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace AnnoTools
{
  /// <summary>
  /// A more complicated annotation construction tool.  Specify a symbol, text string and geometry for the annotation feature rather than using the default template attributes. 
  /// </summary>
  /// <remarks>
  /// Annotation construction tools work as per other construction tools.  Set the categoryRefID in the daml file to be "esri_editing_construction_annotation".
  /// <para></para>
  /// Annotation feature classes store polygon geometry.  This polygon is the bounding box of the text of an annotation feature. The bounding box 
  /// is calculated from the text string, font, font size, angle orientation and other text formatting attributes of the feature. It is automatically 
  /// updated by the application each time the annotation attributes are modified. You should never need to access or modify an annotation features 
  /// polygon shape.  
  /// <para></para>
  /// The text attributes of an annotation feature are represented by a CIMTextGraphic. The CIMTextGraphic consists of the text string, text attributes 
  /// (such as verticalAlignment, horizontalAlignment, fontFamily, fontSize etc), callouts, leader lines and the CIMTextGraphic geometry. This geometry 
  /// can be a point, straight line, bezier curve, multipoint geometry or GeometryBag and represents the baseline geometry that the text string sits upon. 
  /// <para></para>
  /// When creating an annotation feature the geometry passed to the Create method is the CIMTextGraphic geometry.  
  /// <para></para>
  /// <para></para>
  /// Another annotation consideration is the annotation schema.  The only guaranteed fields to exist are AnnotationClassID, SymbolID, Status, Element, Shape. 
  /// All other fields (such as textString, FontName,  VerticalAlignment etc) are optional and may not exist in the physical schema. 
  ///
  /// If you are wishing to alter attributes from the template defaults, the recommended way is to use the AnnotationProperties object on the template's inspector obtained 
  /// via the GetAnnotationProperties method.  The AnnotationProperties object allows you to directly access and set the majority of the CIMTextGraphic properties.  
  /// If you need to access the CIMTextGraphic itself use the TextGraphic property.  Use the LoadFromTextGraphic to set a new CIMTextGraphic.  After modifying the 
  /// annotation properties, don't forget to use the SetAnnotationProperties method on the inspector to write the properties. Then pass the inspector to 
  /// EditOperation.Create.  This construction tool illustrates this methodology.
  /// </remarks>
  internal class AnnoAdvancedConstructionTool : MapTool
  {
    public AnnoAdvancedConstructionTool()
    {
      IsSketchTool = true;
      UseSnapping = true;
      // set the sketch type to line
      SketchType = SketchGeometryType.Line;
    }

    /// <summary>
    /// Restrict the sketch to a two point line
    /// </summary>
    /// <returns></returns>
    protected override async Task<bool> OnSketchModifiedAsync()
    {
      // restrict the sketch to a 2 point line
      bool finish = await QueuedTask.Run(async () =>
      {
        // get the current sketch
        var geometry = await base.GetCurrentSketchAsync();
        // cast to a polyline - the geometry will always be a polyline because the SketchType (set in the constructor) is set to Line.
        var geom = geometry as ArcGIS.Core.Geometry.Polyline;
        // check the point count
        return geom?.PointCount >= 2;
      });

      // call FinishSketchAsync if we have 2 points
      if (finish)
        finish = await base.FinishSketchAsync();

      return finish;
    }

    /// <summary>
    /// Called when the sketch finishes. This is where we will create the edit operation and then execute it.
    /// </summary>
    /// <param name="geometry">The geometry created by the sketch.</param>
    /// <returns>A Task returning a Boolean indicating if the sketch complete event was successfully handled.</returns>
    protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      if (CurrentTemplate == null || geometry == null)
        return false;
			(bool ok, Exception ex) result = await QueuedTask.Run<(bool ok, Exception ex)>(() =>
			{
				try
				{
					// get the annotation layer
					if (CurrentTemplate.Layer is not AnnotationLayer annoLayer)
						return (false, new Exception ($@"Current Template: [{CurrentTemplate.Name}] is not for an Annotation Layer"));

					// get the annotation feature class
					if (annoLayer.GetFeatureClass() is not ArcGIS.Core.Data.Mapping.AnnotationFeatureClass annotationFc)
						return (false, new Exception($@"Can't get F/C for annotation layer: [{annoLayer.Name}]"));

					// get the feature class CIM definition which contains the labels, symbols
					var cimDefinition = annotationFc.GetDefinition() as ArcGIS.Core.Data.Mapping.AnnotationFeatureClassDefinition;
					var labels = cimDefinition.GetLabelClassCollection();
					var symbols = cimDefinition.GetSymbolCollection();

					// make sure there are labels, symbols
					if ((labels.Count == 0) || (symbols.Count == 0))
						return (false, new Exception($@"Can't get labels for F/C: [{annotationFc.GetName()}]"));

					// find the label class required
					//   typically you would use a subtype name or some other characteristic
					// but here we use the first label class or match the label class name to the template name
					var label = GetLabel(labels);

					// each label has a textSymbol
					// the symbolName *should* be the symbolID to be used
					var symbolName = label.TextSymbol.SymbolName;
					int symbolID = GetSymbolIDForLabel(annotationFc, symbolName, symbols);

					// no symbol?
					if (symbolID == -1)
						return (false, new Exception($@"Unable to get Symbol for: [{symbolName}]"));

					// use the template's inspector object
					var inspector = CurrentTemplate.Inspector;
					// get the annotation properties
					var annoProperties = inspector.GetAnnotationProperties();

					// AnnotationClassID, SymbolID and Shape are the bare minimum for an annotation feature

					// use the inspector[fieldName] to set the annotationClassid - this is allowed since annotationClassID is a guaranteed field in the annotation schema
					inspector["AnnotationClassID"] = label.ID;
					// set the symbolID too
					inspector["SymbolID"] = symbolID;

					// use the annotation properties to set the other attributes
					annoProperties.TextString = "My test annotation feature";
					annoProperties.Color = ColorFactory.Instance.GreenRGB;
					annoProperties.VerticalAlignment = ArcGIS.Core.CIM.VerticalAlignment.Top;
					annoProperties.Underline = true;

					// set the geometry to be the sketched line
					// when creating annotation features the shape to be passed in the create operation is the CIMTextGraphic shape
					annoProperties.Shape = geometry;

					// set the annotation properties back on the inspector
					inspector.SetAnnotationProperties(annoProperties);

					// Create an edit operation
					var createOperation = new EditOperation
					{
						Name = $"Create {CurrentTemplate.Layer.Name}",
						SelectNewFeatures = true
					};

					// create and execute using the inspector
					var rowToken = createOperation.Create(CurrentTemplate.Layer, inspector);
					// rowToken content is populated after .Execute is called
					var result = createOperation.Execute();
					if (result)
					{
						return (true, null);
					}
					return (false, new Exception ($@"Create operation failed: {createOperation.ErrorMessage}"));
				}
				catch (Exception ex)
				{
					return (false, ex);
				}
			});
			if (!result.ok)
				MessageBox.Show (result.ex.ToString ());
			return result.ok;
    }

		private CIMLabelClass GetLabel(IReadOnlyList<CIMLabelClass> labels)
		{
			// find the label class required
			//   typically you would use a subtype name or some other characteristic

			// use the first label class
			var label = labels[0];
			if (labels.Count > 1)
			{
				// find a label class based on template name 
				foreach (var labelClass in labels)
				{
					if (labelClass.Name == CurrentTemplate.Name)
					{
						label = labelClass;
						break;
					}
				}
			}
			return label;
		}

		private int GetSymbolIDForLabel(AnnotationFeatureClass fcAnno,
			string symbolName,
			IReadOnlyList<CIMSymbolIdentifier> symbols)
		{
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
			return symbolID;
		}
	}
}
