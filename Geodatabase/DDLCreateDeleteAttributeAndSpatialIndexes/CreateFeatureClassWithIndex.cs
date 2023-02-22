/*

   Copyright 2023 Esri

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
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.DDL;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using FieldDescription = ArcGIS.Core.Data.DDL.FieldDescription;

namespace DDLCreateDeleteAttributeAndSpatialIndexes
{
    internal class CreateFeatureClassWithIndex : Button
    {
        protected override void OnClick()
        {
            QueuedTask.Run(() =>
            {
                using (Geodatabase geodatabase = SchemaBuilder.CreateGeodatabase(new FileGeodatabaseConnectionPath(new Uri(@"C:\temp\mySampleGeoDatabase.gdb"))))
                {
                    SchemaBuilder schemaBuilder = new SchemaBuilder(geodatabase);
                    CreatingFeatureClassWithIndex(schemaBuilder);
                }
            });
        }
        #region Creating a Feature with index from scratch
        public void CreatingFeatureClassWithIndex(SchemaBuilder schemaBuilder)
        {
            FieldDescription nameFieldDescription = FieldDescription.CreateStringField("Name", 50);
            FieldDescription buildingUsageFieldDescription = FieldDescription.CreateStringField("buildingUsage", 50);
            FieldDescription buildingColorFieldDescription = FieldDescription.CreateStringField("buildingColor", 50);
            FieldDescription addressFieldDescription = FieldDescription.CreateStringField("Address", 200);       // Creating a feature class, 'Buildings' with four fields
            FeatureClassDescription featureClassDescription = new FeatureClassDescription("Buildings", new List<FieldDescription>() { nameFieldDescription, addressFieldDescription , buildingUsageFieldDescription, buildingColorFieldDescription }, new ShapeDescription(GeometryType.Polygon, SpatialReferences.WGS84));       // Enqueue DDL operation to create a table
            FeatureClassToken featureToken = schemaBuilder.Create(featureClassDescription);       // Creating an attribute index named as 'Idx'
            AttributeIndexDescription attributeIndexDescription = new AttributeIndexDescription("Idx", new TableDescription(featureToken),
       new List<string> { nameFieldDescription.Name, addressFieldDescription.Name });       // Enqueue DDL operation to create attribute index
            schemaBuilder.Create(attributeIndexDescription);       // Execute build indexes operation
            bool isBuildSuccess = schemaBuilder.Build();
        }
        #endregion
    }
}
