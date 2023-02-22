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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDLCreateDeleteFeatureClassWithSubtypes
{
    internal class CreateFeatureClassWithSubtypes : Button
    {
        protected override void OnClick()
        {
            QueuedTask.Run(() =>
            {
                using (Geodatabase geodatabase = SchemaBuilder.CreateGeodatabase(new FileGeodatabaseConnectionPath(new Uri(@"C:\temp\mySampleGeoDatabase.gdb"))))
                {
                    SchemaBuilder schemaBuilder = new SchemaBuilder(geodatabase);
                    CreateTableWithSubtypes(schemaBuilder);
                }
            });

        }
        #region Creating table with subtypes
        public void CreateTableWithSubtypes(SchemaBuilder schemaBuilder)
        {
            // Creating a 'Building' table with the subtype field 'BuildingType'
            FieldDescription buildingType = new FieldDescription("BuildingType", FieldType.Integer);
            FieldDescription buildingName = new FieldDescription("Name", FieldType.String);



            FeatureClassDescription featureClassDescription = new FeatureClassDescription("Building", new List<FieldDescription> { buildingName, buildingType }, new ShapeDescription(GeometryType.Polygon, SpatialReferences.WGS84));



            // Building types with three subtypes - Business, Marketing, Security
            featureClassDescription.SubtypeFieldDescription = new SubtypeFieldDescription(buildingType.Name, new Dictionary<int, string> { { 1, "Business" }, { 2, "Marketing" }, { 3, "Security" } })
            {
                DefaultSubtypeCode = 3 // Assigning 'Security' building type as the default subtype
            };

            schemaBuilder.Create(featureClassDescription);
            schemaBuilder.Build();
        }

        #endregion
    }
}
