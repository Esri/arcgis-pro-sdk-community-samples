/*

   Copyright 2024 Esri

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
using ArcGIS.Desktop.Core.Geoprocessing;
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

namespace AsyncGeoProcessing
{
	internal class RunGPTools : Button
	{
		protected override async void OnClick()
		{
			// get the first selected polygon layer
			// and then get the first selected feature
			// for use in the GeoProcessing tool
			if (MapView.Active?.Map == null) return;
			var polyGeometry = await QueuedTask.Run<Geometry>(() =>
			{
				var selectionSet = MapView.Active.Map.GetSelection().ToDictionary();
				// For each map member with a selection on it.
				foreach (var selectedKeyValue in selectionSet)
				{
					var mapMember = selectedKeyValue.Key;
					if (!(mapMember is BasicFeatureLayer layer))
						continue;
					if (layer.ShapeType != esriGeometryType.esriGeometryPolygon)
						continue;
					using var cursor = layer.Search(new QueryFilter() { ObjectIDs = [selectedKeyValue.Value[0]] });
					if (cursor.MoveNext())
					{
						using var feature = cursor.Current as Feature;
						if (feature == null) continue;
						return feature.GetShape().Clone();
					}
				}
				return null;
			});
			if (polyGeometry == null)
			{
				MessageBox.Show("Couldn't find a selected polygon feature.");
				return;
			}
			try
			{
				_ = RunGPToolAsync(Module1.Current.GPCycles, polyGeometry);
			}
			catch (Exception ex)
			{
				MessageBox.Show($@"Error: {ex.Message}");
			}
		}

		private async Task RunGPToolAsync(uint numBuffers, Geometry geometry)
		{
			// create and initialize the progress dialog
			// Note: Progress dialogs are not displayed when debugging in Visual Studio
			var progDlg = new ProgressDialog($@"Creating {numBuffers} buffers", "Canceled", false);
			var progsrc = new CancelableProgressorSource(progDlg);
			for (uint iBuffer = 1; iBuffer <= numBuffers; iBuffer++)
			{
				var valueArray = await QueuedTask.Run<IReadOnlyList<string>>(() =>
				{
					var geometries = new List<object>() { geometry };
					// Creates a 100-meter buffer around the geometry object
					// null indicates a default output name is used
					var valueArray = Geoprocessing.MakeValueArray(geometries, null, $@"{iBuffer * 100} Meters");
					return valueArray;
				});
				progsrc.ExtendedStatus = $@"Creating buffer #: {iBuffer} of {numBuffers}";
				progsrc.Value = 100 * (iBuffer - 1);
				progsrc.Max = 100 * numBuffers + 1;
				var gpResult = await Geoprocessing.ExecuteToolAsync("analysis.Buffer", valueArray, null, progsrc.Progressor);
				if (gpResult.IsFailed)
				{
					// display error messages if the tool fails, otherwise shows the default messages
					if (gpResult.Messages.Count() != 0)
					{
						Geoprocessing.ShowMessageBox(gpResult.Messages, progsrc.Message,
														gpResult.IsFailed ?
														GPMessageBoxStyle.Error : GPMessageBoxStyle.Default);
					}
					else
					{
						MessageBox.Show($@"{progsrc.Message} failed with errorcode, check parameters.");
					}
					break;
				}
				// check if the operator canceled
				if (progsrc.CancellationTokenSource.IsCancellationRequested) break;
			}
			if (progsrc.CancellationTokenSource.IsCancellationRequested)
			{
				MessageBox.Show("The operation was canceled.");
			}
		}
	}
	}
