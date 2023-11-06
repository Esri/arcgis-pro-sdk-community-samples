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

namespace AddRemoveFields
{
    internal class RemoveFieldTableButton : Button
    {
        protected override void OnClick()
        {
            QueuedTask.Run(() =>
            {
                using (Geodatabase geodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(@"C:\temp\mySampleGeoDatabase.gdb"))))
                {
                    RemoveFieldTableSnippet(geodatabase);
                }
            });
        }
        
        #region Removing fields from a Table
        public void RemoveFieldTableSnippet(Geodatabase geodatabase)
        {
            // Removing all fields from 'Parcels' table except following 
            // Tax_Code
            // The table to remove field
            string tableName = "Parcels"; TableDefinition tableDefinition = geodatabase.GetDefinition<TableDefinition>(tableName);
            IReadOnlyList<Field> fields = tableDefinition.GetFields();       // Existing fields from 'Parcels' table
            Field taxCodeField = fields.First(f => f.Name.Equals("Tax_Code"));
            FieldDescription taxFieldDescription = new FieldDescription(taxCodeField);
            
            // Fields to retain in modified table
            List<FieldDescription> fieldsToBeRetained = new List<FieldDescription>()
                {          
                    taxFieldDescription
                };      
            
            // New description of the 'Parcels' table with the 'Tax_Code' field
            TableDescription modifiedTableDescription = new TableDescription(tableName, fieldsToBeRetained); SchemaBuilder schemaBuilder = new SchemaBuilder(geodatabase);       // Remove all fields except the 'Tax_Code' and 'Parcel_Address' fields
            schemaBuilder.Modify(modifiedTableDescription);
            schemaBuilder.Build();
        }
        #endregion
    }
}
