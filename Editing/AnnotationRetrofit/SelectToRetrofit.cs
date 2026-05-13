/*

   Copyright 2026 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       https://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.Mapping;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Editing.Templates;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;

namespace AnnotationRetrofit
{
	internal class SelectToRetrofit : MapTool
	{
		private const string pointLayerName = "U.S. Cities";
		private const string pointLayerTextFieldName = "CITY_NAME";

		public SelectToRetrofit()
		{
			IsSketchTool = true;
			SketchType = SketchGeometryType.Rectangle;
		}

		protected override Task OnToolActivateAsync(bool active)
		{
			return base.OnToolActivateAsync(active);
		}

		protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
		{
			try
			{
				// select the first point feature layer in the active map
				var pointLayer = ActiveMapView.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().
						Where(lyr =>
						lyr.ShapeType == esriGeometryType.esriGeometryPoint
						&& lyr.Name == pointLayerName).FirstOrDefault();
				if (pointLayer == null)
				{
					MessageBox.Show($@"Unable to find point layer [{pointLayerName}] in the active map", "Annotation Retrofit Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
					return false;
				}
				var theTemplate = CurrentTemplate;
				if (theTemplate == null)
				{
					MessageBox.Show($@"Unable to find a current template for [{TooltipHeading}]");
					return false;
				}
				if (theTemplate.Layer is not AnnotationLayer annotationLayer)
				{
					MessageBox.Show($@"Unable to find annotation layer in the active map", "Annotation Retrofit Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
					return false;
				}
				//// make sure it is active before you get the inspector
				//await theTemplate.ActivateToolAsync(this.ID);
				// execute the select on the MCT
				var pntQueryResult = QueuedTask.Run<List<(Geometry geometry, string text)>>(() =>
				{
					List<(Geometry geometry, string text)> lstPointData = [];
					// define the spatial query filter
					var spatialQuery = new SpatialQueryFilter() { FilterGeometry = geometry, SpatialRelationship = SpatialRelationship.Contains };

					// gather the selection
					using var rowCursor = pointLayer.Search(spatialQuery);
					if (!rowCursor.MoveNext()) return lstPointData;
					do
					{
						using var row = rowCursor.Current as Feature;
						if (row == null)
							continue;
						// get the geometry of the feature
						var pointGeometry = row.GetShape().Clone();
						if (pointGeometry == null)
							continue;
						// get the text from the feature's attributes
						var text = new string(row[pointLayerTextFieldName] as string);
						// add to the list
						lstPointData.Add((pointGeometry, text));
					} while (rowCursor.MoveNext());
					return lstPointData;
				});
				var (ok, errorMsg) = await QueuedTask.Run<(bool ok, string errorMsg)>(async () =>
				{
					var createOperation = new EditOperation
					{
						Name = "Retrofit annotation",
						SelectNewFeatures = true
					};
					foreach (var pointData in pntQueryResult.Result)
					{
						// retrofit the annotation for each point feature
						(bool ok, string errorMsg) retrofitResult = RetrofitAnnotationForPointLayer(annotationLayer, pointData.geometry, pointData.text, createOperation);
						if (!retrofitResult.ok)
						{
							return retrofitResult;
						}
					}
					if (!createOperation.IsEmpty)
					{
						var result = await createOperation.ExecuteAsync();
						return (result, result ? string.Empty : createOperation.ErrorMessage);
					}
					return (false, "No points where found to create annotation for");
				});
				if (!ok) throw new Exception(errorMsg);
			}
			catch (Exception ex)
			{
				MessageBox.Show($@"Error retrofitting annotation: {ex}", "Annotation Retrofit Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
			}
			return true;
		}

		private (bool ok, string errorMsg) RetrofitAnnotationForPointLayer(AnnotationLayer annotationLayer, Geometry geometry, string text, EditOperation createOperation)
		{
			try
			{
				// get the annotation feature class from the annotation layer
				if (annotationLayer.GetFeatureClass() is not AnnotationFeatureClass annotationFeatureClass)
					return (false, $@"Unable to find feature class for annotation layer [{annotationLayer.Name}]");

				// get the feature class CIM definition which contains the labels, symbols
				var cimDefinition = annotationFeatureClass.GetDefinition();
				var labels = cimDefinition.GetLabelClassCollection();
				var symbols = cimDefinition.GetSymbolCollection();

				// make sure there are labels, symbols
				if ((labels.Count == 0) || (symbols.Count == 0))
					return (false, $@"Problem with labels/symbols: found {labels.Count} labels and {symbols.Count} symbols");

				// find the label class required
				//   typically you would use a subtype name or some other characteristic
				// use the first label class
				var label = GetLabel(labels);

				// each label has a textSymbol
				// the symbolName *should* point to the symbolID to be used
				var symbolName = label.TextSymbol.SymbolName;
				var symbolID = GetSymbolIDForLabel(annotationFeatureClass, symbolName, symbols);

				// no symbol?
				if (symbolID == -1)
					return (false, $@"Unable to get Symbol ID for symbol: {symbolName}");

				// use the template's inspector object
				var inspector = CurrentTemplate.Inspector;
				if (inspector == null)
					return (false, $@"Unable to get Inspector for template: {CurrentTemplate.Name}");
				// get the annotation properties
				var annoProperties = inspector.GetAnnotationProperties();

				// AnnotationClassID, SymbolID and Shape are the bare minimum for an annotation feature

				// use the inspector[fieldName] to set the annotationClassid - this is allowed since annotationClassID is a guaranteed field in the annotation schema
				inspector["AnnotationClassID"] = label.ID;
				// set the symbolID too
				inspector["SymbolID"] = symbolID;

				// use the annotation properties to set the other attributes
				annoProperties.TextString = text;
				annoProperties.Color = ColorFactory.Instance.RedRGB;
				annoProperties.VerticalAlignment = VerticalAlignment.Top;
				annoProperties.HorizontalAlignment = HorizontalAlignment.Left;
				annoProperties.Underline = true;

				// set the geometry to be the sketched line
				// when creating annotation features the shape to be passed in the create operation is the CIMTextGraphic shape
				var geometryProjected = GeometryEngine.Instance.Project(geometry, CurrentTemplate.Layer.GetSpatialReference());
				annoProperties.Shape = geometryProjected;

				inspector.SetAnnotationProperties(annoProperties);
				var rowToken = createOperation.Create(CurrentTemplate.Layer, inspector);
				// rowToken content is populated after .Execute is called

				return (true, string.Empty);
			}
			catch (Exception ex)
			{
				return (false, ex.ToString());
			}
		}

		private CIMLabelClass GetLabel (IReadOnlyList<CIMLabelClass> labels)
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
