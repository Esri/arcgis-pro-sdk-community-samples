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
using ArcGIS.Core.Data.Exceptions;
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
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Animation;

namespace AsyncGeoProcessing
{
	internal class RunGPToolsWoInterface : Button
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
				var lastGPBufferName = await GetLastGPName();
				_ = RunGPToolAsync(Module1.Current.GPCycles, polyGeometry, lastGPBufferName);
				
			}
			catch (Exception ex)
			{
				MessageBox.Show($@"Error: {ex.Message}");
			}
		}

		private async Task AddToMapViewAsync((string GdbPath, string FcName) lastGPBufferName, uint gPCycles)
		{
			var bufferPrefix = "GPL0_Buffer";
			uint idxStartOffset = 0;
			if (string.IsNullOrEmpty(lastGPBufferName.GdbPath)) throw (new Exception("Default GeoDatabase path is missing."));
			if (string.IsNullOrEmpty(lastGPBufferName.FcName))
			{
				lastGPBufferName = (lastGPBufferName.GdbPath, bufferPrefix);
			}
			else
			{
				if (uint.TryParse(lastGPBufferName.FcName.Replace(bufferPrefix, string.Empty), out uint lastBufferIdx))
				{
					idxStartOffset = lastBufferIdx;
				}
			}
			await QueuedTask.Run(() =>
			{
				for (uint iBuffer = 1; iBuffer <= gPCycles; iBuffer++)
				{
					Uri fcUri = null;
					if (iBuffer == 1 && lastGPBufferName.FcName == bufferPrefix)
					{
						fcUri = new Uri($@"{lastGPBufferName.GdbPath}\{bufferPrefix}");
					}
					else
					{
						fcUri = new Uri($@"{lastGPBufferName.GdbPath}\{bufferPrefix}{iBuffer + idxStartOffset}");
					}
					//Define the Feature Layer's parameters.
					var layerParams = new FeatureLayerCreationParams(fcUri);
					//Create the layer with the feature layer parameters and add it to the active map
					LayerFactory.Instance.CreateLayer<FeatureLayer>(layerParams, MapView.Active.Map);
				}
			});
		}

		private async Task RunGPToolAsync(uint numBuffers, Geometry geometry, (string GdbPath, string FcName) lastGPBufferName)
		{
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
				var gpResult = await Geoprocessing.ExecuteToolAsync("analysis.Buffer", valueArray,
																														null, CancelableProgressor.None, GPExecuteToolFlags.None);
				if (gpResult.IsFailed)
				{
					// display error messages if the tool fails, otherwise shows the default messages
					if (gpResult.Messages.Count() != 0)
					{
						Geoprocessing.ShowMessageBox(gpResult.Messages, $@"GP tool error: [{gpResult.ErrorCode}]",
														gpResult.IsFailed ?
														GPMessageBoxStyle.Error : GPMessageBoxStyle.Default);
					}
					else
					{
						MessageBox.Show($@"GP tool error: [{gpResult.ErrorCode}], check parameters.");
					}
					break;
				}
			}
			await AddToMapViewAsync(lastGPBufferName, Module1.Current.GPCycles);
		}

		private async Task<(string GdbPath, string FcName)> GetLastGPName()
		{
			(string GdbPath, string FcName) lastGPName = (string.Empty, string.Empty);
			try
			{
				var bufferPrefix = "GPL0_Buffer";
				
				// check all project databases for the buffer feature classes
				IEnumerable<GDBProjectItem> gdbProjectItems = Project.Current.GetItems<GDBProjectItem>();
				await QueuedTask.Run(() =>
				{
					foreach (GDBProjectItem gdbProjectItem in gdbProjectItems)
					{
						using Datastore datastore = gdbProjectItem.GetDatastore();
						// Unsupported DataStores (non File GDB and non Enterprise GDB)
						// will be of type UnknownDatastore
						if (datastore is UnknownDatastore)
							continue;

						using Geodatabase geodatabase = datastore as Geodatabase;
						// Use the GeoDatabase.
						IReadOnlyList<FeatureClassDefinition> fcDefinitions = geodatabase.GetDefinitions<FeatureClassDefinition>();
						foreach (FeatureClassDefinition fcDef in fcDefinitions)
						{
							var fcName = fcDef.GetName();
							if (fcName.StartsWith(bufferPrefix))
							{
								var path = gdbProjectItem.Path;
								lastGPName = (path, fcName);
							}
						}
						// refresh Pro UI
						gdbProjectItem.Refresh();
					}
				});
			}
			catch (GeodatabaseException ex)
			{
				if (ex.Message == "The table was not found.") return (string.Empty, string.Empty);

				MessageBox.Show(ex.Message);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			return lastGPName;
		}
	}
}
