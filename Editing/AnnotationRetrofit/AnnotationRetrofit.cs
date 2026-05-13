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
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
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
using System.Text;
using System.Threading.Tasks;

namespace AnnotationRetrofit
{
	internal class AnnotationRetrofit : Button
	{
		private const string annotationLayerName = "SampleAnno";
		private const string annotationTemplateName = "Annotation Class 1";
		private const string toolContentGUID = "e2096d13-b437-4bc1-94ea-4494c3260f72"; // Example GUID, replace with actual GUID from DAML

		protected override async void OnClick()
		{
			try 
			{
				// This is the entry point for the Annotation Retrofit tool
				// It will activate the SelectToRetrofit tool which allows users to select features for annotation retrofit
				// select the first point feature layer in the active map
				var annotationLayer = MapView.Active?.Map?.GetLayersAsFlattenedList().OfType<AnnotationLayer>().
						Where(lyr => lyr.Name == annotationLayerName).FirstOrDefault();
				if (annotationLayer == null)
				{
					MessageBox.Show($@"Unable to find the annotation layer [{annotationLayerName}] in the active map", "Annotation Retrofit Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
					return;
				}
				var result = await ChangeTemplateDefaultToolAsync(annotationLayer, toolContentGUID, annotationTemplateName);
				if (!result)
				{
					MessageBox.Show("Failed to change the template default tool.", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
					return;
				}
				await FrameworkApplication.SetCurrentToolAsync("AnnotationRetrofit_SelectToRetrofit");
			}
			catch (Exception ex)
			{
				MessageBox.Show($"An error occurred while checking tool availability: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
				return;
			}
		}

		public async Task<bool> ChangeTemplateDefaultToolAsync(AnnotationLayer flayer,
									string toolContentGUID,
									string templateName)
		{
			return await QueuedTask.Run<bool>(() =>
			{
				var lst = flayer?.GetTemplatesAsFlattenedList();
				foreach (var tmpl in lst)
				{
					System.Diagnostics.Trace.WriteLine($@"{tmpl.Name}: {tmpl.DefaultToolID}");
				}
				// retrieve the edit template form the layer by name
				// get the definition of the layer
				if ((flayer?.GetTemplate(templateName) is not ArcGIS.Desktop.Editing.Templates.EditingTemplate template) || (flayer?.GetDefinition() is not ArcGIS.Core.CIM.CIMAnnotationLayer layerDef))
					return false;

				if (template.DefaultToolID != this.ID)
				{
					//if (layerDef.AutoGenerateFeatureTemplates)
					//{
					//	layerDef.AutoGenerateFeatureTemplates = false;
					//}
					// retrieve the CIM edit template definition
					var templateDef = template.GetDefinition();

					// assign the GUID from the tool DAML definition, for example
					// <tool id="TestConstructionTool_SampleSDKTool" categoryRefID="esri_editing_construction_polyline" ….>
					//   <tooltip heading="">Tooltip text<disabledText /></tooltip>
					//   <content guid="e2096d13-b437-4bc1-94ea-4494c3260f72" />
					// </tool>
					// then the toolContentGUID would be "e2096d13-b437-4bc1-94ea-4494c3260f72"
					templateDef.DefaultToolGUID = toolContentGUID;

					// set the definition back to 
					template.SetDefinition(templateDef);

					//// update the layer definition too
					//flayer.SetDefinition(layerDef);
					return true;
				}
				return true;
			});
		}
	}
}
