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
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using FilteredFindPathAnalysis.Helpers;
using System.Collections.Generic;

namespace FilteredFindPathAnalysis.Utilities
{
	internal class CreateLinkChart : Button
	{
		protected async override void OnClick()
		{
			//see comments in the module1.cs regarding the KG service used here
			//and for usr/pwd if a login dialog is popped
			QueuedTask.Run(() =>
			{
				using (var kg = KGUtils.Instance.InitKnowledgeGraph(Module1.KG_URL))
				{
					//Preselect nodes for use in the initial link chart
					var dict_initial_sel = new Dictionary<string, List<long>>();
					dict_initial_sel["Species"] = new List<long>() { 1 };
					dict_initial_sel["Observation"] = new List<long>() { 1, 45, 31, 42 };
					dict_initial_sel["ObservedIn"] = new List<long>() { 1, 45, 31, 42 };

					var kgIdSet = KnowledgeGraphLayerIDSet.FromDictionary(kg, dict_initial_sel);
					var linkChart = MapFactory.Instance.CreateLinkChart("FFP Sample", kg, kgIdSet);
					//Open a map pane
					_ = FrameworkApplication.Panes.CreateMapPaneAsync(linkChart);
				}
			});
		}
	}
}
