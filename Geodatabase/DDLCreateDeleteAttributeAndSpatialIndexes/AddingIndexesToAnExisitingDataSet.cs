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
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;


namespace DDLCreateDeleteAttributeAndSpatialIndexes
{
    internal class AddingIndexesToAnExisitingDataSet : Button
    {
        protected override void OnClick()
        {
            QueuedTask.Run(() =>
            {
                using (Geodatabase geodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(@"C:\temp\mySampleGeoDatabase.gdb"))))
                {
                    SchemaBuilder schemaBuilder = new SchemaBuilder(geodatabase);
                    using (FeatureClass feature = geodatabase.OpenDataset<FeatureClass>("Buildings"))
                    {
                        FeatureClassDefinition featureClassDefinition = feature.GetDefinition();
                        AddingIndexes(schemaBuilder, featureClassDefinition);
                    }
                }
            });
        }
        #region Adding indexes in pre-existing dataset
        public void AddingIndexes(SchemaBuilder schemaBuilder, FeatureClassDefinition featureClassDefinition)
        {
            // Field names to add in the attribute index
            string buildingUsage = featureClassDefinition.GetFields().First(f => f.AliasName.Contains("buildingUsage")).Name;
            string buildingColor = featureClassDefinition.GetFields().First(f => f.AliasName.Contains("buildingColor")).Name;       // Creating an attribute index with index name 'Idx' and two participating fields' name
            AttributeIndexDescription attributeIndexDescription = new AttributeIndexDescription("Idx2", new TableDescription(featureClassDefinition), new List<string> { buildingUsage, buildingColor });       // Enqueue DDL operation for attribute index creation 
            schemaBuilder.Create(attributeIndexDescription);       // Creating the spatial index 
            SpatialIndexDescription spatialIndexDescription = new SpatialIndexDescription(new FeatureClassDescription(featureClassDefinition));       // Enqueue DDL operation for spatial index creation
            schemaBuilder.Create(spatialIndexDescription);       // Execute build indexes operation
            bool isBuildSuccess = schemaBuilder.Build(); if (!isBuildSuccess)
            {
                IReadOnlyList<string> errors = schemaBuilder.ErrorMessages;
                // Iterate and handle errors 
            }
        }
        #endregion
    }

}
