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
using ArcGIS.Core.Data.Knowledge;
using ArcGIS.Core.Data.Knowledge.Extensions;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using FilteredFindPathAnalysis.Helpers;
using System;

namespace FilteredFindPathAnalysis.Ribbon
{
	internal class KG_Run_FFP_Analysis_Button_Base : Button
	{
		protected FFP_Analysis_Type _ffp_type = FFP_Analysis_Type.FFP;

		protected async void RunFFP(bool appendResults = true)
		{
			var mv = MapView.Active;

			var map = mv?.Map;
			if (map == null || !map.IsLinkChart)
				return;
			Module1.Current.FFP_Error_Msg = "";

			await QueuedTask.Run(() =>
			{
				//see comments in the module1.cs regarding the KG service used here
				//and for usr/pwd if a login dialog is popped
				using (var kg = KGUtils.Instance.InitKnowledgeGraph(
											Module1.KG_URL))
				{
					RunIt(map, kg, appendResults);
				}
			});

			Module1.Current.ShowAnyError(_ffp_type.ToString());
		}

		private void RunIt(Map linkChart, KnowledgeGraph kg, bool append)
		{

			try
			{
				//Check environment/context is valid
				var env_ok = KGUtils.Instance.CanRunFFPAnalysisType(linkChart, _ffp_type);
				if (!env_ok.isValid)
				{
					Module1.Current.FFP_Error_Msg = env_ok.errorMsg;
					return;
				}
				var ffp_config = KGUtils.Instance.GetConfiguration(linkChart, _ffp_type);
				var results = kg.RunFilteredFindPaths(ffp_config);
				if (results == null)
				{
					Module1.Current.FFP_Error_Msg =
						$"{_ffp_type} analysis returned no results.";
					return;
				}

				//Only FFPConfig supports creating a new link chart in the Pro UI -
				//but simply pass in "false" for the append parameter for the other
				//FFPConfig analysis types to create a new link chart with the results
				if (append)
					KGUtils.Instance.UpdateLinkChartWithResults(
						linkChart, results, _ffp_type);
				else
					//typically this is just FFPConfig
					_ = KGUtils.Instance.CreateLinkChartWithResultsAsync(kg, results);

				//Print out the results in addition (to updating the link chart)
				var result_string = results.PrintAsString(_ffp_type);
				System.Diagnostics.Debug.WriteLine(result_string);
			}
			catch (Exception ex)
			{
				Module1.Current.FFP_Error_Msg = ex.ToString();
			}
		}
	}


	internal class KG_Run_FFP : KG_Run_FFP_Analysis_Button_Base
	{
		protected override void OnClick()
		{
			_ffp_type = FFP_Analysis_Type.FFP;
			//Are we appending results or not?...
			//For FFPConfig we can create either a new link chart or append to
			//the existing...
			var append = new Random().Next(1, 100) % 2 == 0;
			RunFFP(append);
		}
	}

	internal class KG_Run_Expand : KG_Run_FFP_Analysis_Button_Base
	{
		protected override void OnClick()
		{
			_ffp_type = FFP_Analysis_Type.Expand;
			RunFFP(true);
		}
	}

	internal class KG_Run_FilteredExpand : KG_Run_FFP_Analysis_Button_Base
	{
		protected override void OnClick()
		{
			_ffp_type = FFP_Analysis_Type.FilteredExpand;
			RunFFP(true);
		}
	}

	internal class KG_Run_Connect : KG_Run_FFP_Analysis_Button_Base
	{
		protected override void OnClick()
		{
			_ffp_type = FFP_Analysis_Type.Connect;
			RunFFP(true);
		}
	}

	internal class KG_Run_FindBetween : KG_Run_FFP_Analysis_Button_Base
	{
		protected override void OnClick()
		{
			_ffp_type = FFP_Analysis_Type.FindBetween;
			RunFFP(true);
		}
	}
}
