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
    internal class AddFieldsInFeatureClassButton : Button
    {
        protected override void OnClick()
        {
            QueuedTask.Run(() =>
            {
                using (Geodatabase geodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(@"C:\temp\mySampleGeoDatabase.gdb"))))
                {
                    AddFieldsInFeatureClassSnippet(geodatabase);
                }
            });
        }

        
        #region Adding fields to a FeatureClass
        public void AddFieldsInFeatureClassSnippet(Geodatabase geodatabase)
        {
            // Adding following fields to the 'Parcels' FeatureClass
            // Global ID
            // Parcel_ID
            // Tax_Code
            // Parcel_Address       // The FeatureClass to add fields
            string featureClassName = "Parcels"; 
            FeatureClassDefinition originalFeatureClassDefinition = geodatabase.GetDefinition<FeatureClassDefinition>(featureClassName);
            FeatureClassDescription originalFeatureClassDescription = new FeatureClassDescription(originalFeatureClassDefinition); // The four new fields to add on the 'Parcels' FeatureClass
            
            FieldDescription globalIdField = FieldDescription.CreateGlobalIDField();
            FieldDescription parcelIdDescription = new FieldDescription("Parcel_ID", FieldType.GUID);
            FieldDescription taxCodeDescription = FieldDescription.CreateIntegerField("Tax_Code");
            FieldDescription addressDescription = FieldDescription.CreateStringField("Parcel_Address", 150); 
            
            List<FieldDescription> fieldsToAdd = new List<FieldDescription> { globalIdField, parcelIdDescription, taxCodeDescription, addressDescription }; // Add new fields on the new FieldDescription list
            
            List<FieldDescription> modifiedFieldDescriptions = new List<FieldDescription>(originalFeatureClassDescription.FieldDescriptions);
            modifiedFieldDescriptions.AddRange(fieldsToAdd);       // The new FeatureClassDescription with additional fields
            
            FeatureClassDescription modifiedFeatureClassDescription = new FeatureClassDescription(originalFeatureClassDescription.Name,
                modifiedFieldDescriptions, originalFeatureClassDescription.ShapeDescription); SchemaBuilder schemaBuilder = new SchemaBuilder(geodatabase);  // Update the 'Parcels' FeatureClass with newly added fields
            
            schemaBuilder.Modify(modifiedFeatureClassDescription);
            bool modifyStatus = schemaBuilder.Build(); if (!modifyStatus)
            {
                IReadOnlyList<string> errors = schemaBuilder.ErrorMessages;
            }
        }
        #endregion
    }
}
