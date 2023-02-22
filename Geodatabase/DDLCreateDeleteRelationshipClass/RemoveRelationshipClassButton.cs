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

namespace DDLCreateDeleteRelationshipClass
{
    internal class RemoveRelationshipClassButton : Button
    {
        protected override void OnClick()
        {
            QueuedTask.Run(() =>
            {
                using (Geodatabase geodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(@"C:\temp\mySampleGeoDatabase.gdb"))))
                using (RelationshipClassDefinition relationshipClassDefinition = geodatabase.GetDefinition<RelationshipClassDefinition>("BuildingToBuildingType"))
                using (FeatureDatasetDefinition featureDatasetDefinition = geodatabase.GetDefinition<FeatureDatasetDefinition>("Parcel_Information"))
                {
                    SchemaBuilder schemaBuilder = new SchemaBuilder(geodatabase);
                    Operations.RemoveRelationshipClass(schemaBuilder, featureDatasetDefinition, relationshipClassDefinition);
                }
            });
        }

    }
}
