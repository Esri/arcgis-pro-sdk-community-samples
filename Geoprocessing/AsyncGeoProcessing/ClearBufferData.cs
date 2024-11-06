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
using ArcGIS.Core.Data.DDL;
using ArcGIS.Core.Data.Exceptions;
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

namespace AsyncGeoProcessing
{
	internal class ClearBufferData : Button
	{
		protected override async void OnClick()
		{
			try
			{
				var map = MapView.Active.Map;
				if (map == null)
					return;
				var removeFeatureClasses = new List<(Geodatabase geoDatabase, FeatureClass fc)>() { };

				var bufferPrefix = "GPL0_Buffer";

				// remove all demo layers from map
				var removeLayers = MapView.Active.Map.GetLayersAsFlattenedList().
														OfType<FeatureLayer>().
														Where(fl => fl.Name.StartsWith(bufferPrefix));
				await QueuedTask.Run(() =>
				{
					map.RemoveLayers(removeLayers);
				});

				// remove all FeatureClasses  from project databases
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
								DeleteFeatureClass(geodatabase, geodatabase.OpenDataset<FeatureClass>(fcName));
							}
						}
						// refresh Pro UI
						gdbProjectItem.Refresh();
					}
				});
			}
			catch (GeodatabaseException ex)
			{
				if (ex.Message == "The table was not found.") return;

				MessageBox.Show(ex.Message);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		public bool DeleteFeatureClass(Geodatabase geoDatabase, FeatureClass featureClass)
		{
			#region Deleting a Feature Class

			// Create a FeatureClassDescription object
			FeatureClassDescription featureClassDescription = new FeatureClassDescription(featureClass.GetDefinition());

			// Create a SchemaBuilder object
			SchemaBuilder schemaBuilder = new SchemaBuilder(geoDatabase);

			// Add the deletion for the feature class to our list of DDL tasks
			schemaBuilder.Delete(featureClassDescription);

			// Execute the DDL
			bool success = schemaBuilder.Build();
			return success;
			#endregion
		}
	}
}
