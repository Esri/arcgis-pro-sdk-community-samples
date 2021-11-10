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

namespace DDLAddField2FeatureClass
{
  internal class AddFieldToFeatureClass : Button
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
		  var fcName = selectedLayerTable.GetName();
		  try
		  {
			FeatureClassDefinition originalFeatureClassDefinition = geoDb.GetDefinition<FeatureClassDefinition>(fcName);
			FeatureClassDescription originalFeatureClassDescription = new FeatureClassDescription(originalFeatureClassDefinition);

			// Assemble a list of all of new field descriptions
			var fieldDescriptions = new List<ArcGIS.Core.Data.DDL.FieldDescription>() {
										stringFieldDescription,
										intFieldDescription,
										dblFieldDescription,
										dateFieldDescription
							  };
				  // Create a FeatureClassDescription object to describe the feature class to create
				  var fcDescription =
					  new FeatureClassDescription(fcName, fieldDescriptions, originalFeatureClassDescription.ShapeDescription);

				  // Create a SchemaBuilder object
				  SchemaBuilder schemaBuilder = new SchemaBuilder(geoDb);

				  // Add the modification to the feature class to our list of DDL tasks
				  schemaBuilder.Modify(fcDescription);

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
