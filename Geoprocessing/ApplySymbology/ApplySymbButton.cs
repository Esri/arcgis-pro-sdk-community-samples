/*

   Copyright 2022 Esri

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Catalog;

namespace ApplySymbology
{
	internal class ApplySymbButton : Button
	{
		protected override async void OnClick()
		{
			try
			{
				IGPResult toolResult = await Execute_ApplySymbologyFromFeatureLayer();
				Geoprocessing.ShowMessageBox(toolResult.Messages, "GP Messages", 
					toolResult.IsFailed ? GPMessageBoxStyle.Error : GPMessageBoxStyle.Default);
			}
			catch (Exception ex)
			{
				MessageBox.Show($@"Error: {ex}");
			}
		}

		public async Task<IGPResult> Execute_ApplySymbologyFromFeatureLayer()
		{
			var environments = Geoprocessing.MakeEnvironmentArray(overwriteoutput: true);

			var prj = Project.Current;
			var map = MapView.Active;

			var featLayers = map.Map.Layers.OfType<FeatureLayer>().Where(l => l.Name.StartsWith ("Crimes", StringComparison.OrdinalIgnoreCase));
			if (featLayers.Count() < 1)
			{
				MessageBox.Show("Unable to find Crimes layer: the topmost Crime layer is the target layer for the symbology, the symbology source is a lyrx file");
				return null;
			}

			// the first layer in TOC is without any symbology
			FeatureLayer targetLayer = featLayers.ElementAt(0);

			// the second layer has the symbology
			// symbology from the 2nd layer will be applied to the first layer
			//Create instance of BrowseProjectFilter using the id for Pro's Lyrx files filter
			BrowseProjectFilter bf = new BrowseProjectFilter("esri_browseDialogFilters_layers_lyrx");
			//Display the filter in an Open Item dialog
			OpenItemDialog lyrxOpenDlg = new OpenItemDialog
			{
				Title = "Symbology Input Layer",
				MultiSelect = false,
				BrowseFilter = bf
			};
			bool? ok = lyrxOpenDlg.ShowDialog();
			if (!ok.HasValue || !ok.Value) return null;
			// Full path for the lyrx file
			var lryxItem = lyrxOpenDlg.Items.FirstOrDefault().Path;

			var sourceField = "Major_Offense_Type";
			var targetField = "Major_Offense_Type";
			var fieldType = "VALUE_FIELD";
			String fieldInfo = String.Format("{0} {1} {2}", fieldType, sourceField, targetField); // VALUE_FIELD NAME NAME")
			MessageBox.Show("Field Info for symbology: " + fieldInfo);

			var parameters = Geoprocessing.MakeValueArray(targetLayer, lryxItem, fieldInfo); //, symbologyFields, updateSymbology);

			// Does not Add outputs to Map as GPExecuteToolFlags.AddOutputsToMap is not used
			GPExecuteToolFlags executeFlags = GPExecuteToolFlags.RefreshProjectItems | GPExecuteToolFlags.GPThread | GPExecuteToolFlags.AddToHistory;

			IGPResult result = await Geoprocessing.ExecuteToolAsync("ApplySymbologyFromLayer_management", parameters, null, null, null, executeFlags);
			return result;
		}


	}
}