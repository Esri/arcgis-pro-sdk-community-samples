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
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.DDL;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDLCreateFeatureClass
{
  internal class CreateTestFeatureClass : Button
  {
	protected override async void OnClick()
	{
	  var selectedLayer = MapView.Active.GetSelectedLayers().FirstOrDefault();
	  if (selectedLayer == null || !(selectedLayer is FeatureLayer))
	  {
		MessageBox.Show("You have to select a feature layer.  The selected layer's database connection is then used to create the new FeatureClass.");
		return;
	  }
	  var selectedFeatureLayer = selectedLayer as FeatureLayer;

	  await QueuedTask.Run(() =>
	  {
		var selectedLayerTable = selectedFeatureLayer.GetTable();

		var testName = $@"Point{DateTime.Now:HHmmss}";
		var hasZ = false;
		var hasM = false;
		// Create a ShapeDescription object
		var shapeDescription = new ShapeDescription(GeometryType.Point, SpatialReferences.WebMercator)
		{
		  HasM = hasM,
		  HasZ = hasZ
		};
		var objectIDFieldDescription = new ArcGIS.Core.Data.DDL.FieldDescription("ObjectID", FieldType.OID);
		var stringFieldDescription = new ArcGIS.Core.Data.DDL.FieldDescription("TheString", FieldType.String);
		var intFieldDescription = new ArcGIS.Core.Data.DDL.FieldDescription("TheInteger", FieldType.Integer);
		var dblFieldDescription = new ArcGIS.Core.Data.DDL.FieldDescription("TheDouble", FieldType.Double)
		{
		  Precision = 9,
		  Scale = 5
		};
		var dateFieldDescription = new ArcGIS.Core.Data.DDL.FieldDescription("TheDate", FieldType.Date);

		using (var geoDb = selectedLayerTable.GetDatastore() as Geodatabase)
		{
		  var fcName = $@"{testName}";
		  try
		  {
			// Assemble a list of all of our field descriptions
			var fieldDescriptions = new List<ArcGIS.Core.Data.DDL.FieldDescription>() {
										objectIDFieldDescription,
										stringFieldDescription,
										intFieldDescription,
										dblFieldDescription,
										dateFieldDescription
							  };
			// Create a FeatureClassDescription object to describe the feature class
			// that we want to create
			var fcDescription =
				new FeatureClassDescription(fcName, fieldDescriptions, shapeDescription);

			// Create a SchemaBuilder object
			SchemaBuilder schemaBuilder = new SchemaBuilder(geoDb);

			// Add the creation of the new feature class to our list of DDL tasks
			schemaBuilder.Create(fcDescription);

			// Execute the DDL
			bool success = schemaBuilder.Build();
		  }
		  catch (Exception ex)
		  {
			MessageBox.Show($@"Exception: {ex}");
		  }
		}
	  });
	}
  }
}
