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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDLCreateDeleteRelationshipClass
{
    public static class Operations
    {
    
        #region Creating attributed relationship class
        public static void CreateAttributedRelationship(SchemaBuilder schemaBuilder)
        {
            // Creating the 'BuildingType' table with two fields - BuildingType and BuildingTypeDescription
            FieldDescription buildingType = FieldDescription.CreateIntegerField("BuildingType");
            FieldDescription buildingTypeeDescription = FieldDescription.CreateStringField("BuildingTypeDescription", 100);
            TableDescription buildingTypeDescription = new TableDescription("BuildingType", new List<FieldDescription>() { buildingType, buildingTypeeDescription });
            TableToken buildingtypeToken = schemaBuilder.Create(buildingTypeDescription);

            // Creating the 'Building' feature class with three fields - BuildingId, Address, and BuildingType
            FieldDescription buildingId = FieldDescription.CreateIntegerField("BuildingId");
            FieldDescription buildingAddress = FieldDescription.CreateStringField("Address", 100);
            FeatureClassDescription featureClassDescription = new FeatureClassDescription("Building", new List<FieldDescription> { buildingId, buildingAddress, buildingType }, new ShapeDescription(GeometryType.Polygon, SpatialReferences.WGS84));
            FeatureClassToken buildingToken = schemaBuilder.Create(featureClassDescription);

            // Creating M:M relationship between the 'Building' feature class and 'BuildingType' table
            AttributedRelationshipClassDescription attributedRelationshipClassDescription = new AttributedRelationshipClassDescription("BuildingToBuildingType", new FeatureClassDescription(buildingToken),
                new TableDescription(buildingtypeToken), RelationshipCardinality.ManyToMany, "OBJECTID", "BuildingID", "OBJECTID", "BuildingTypeID");

            // Adding optional attribute field in the intermediate table - 'OwnershipPercentage' field
            attributedRelationshipClassDescription.FieldDescriptions.Add(FieldDescription.CreateIntegerField("OwnershipPercentage"));

            schemaBuilder.Create(attributedRelationshipClassDescription);
            schemaBuilder.Build();
        }
        #endregion
        
        #region Add relationship rules to a relationship class
        public static void ModifyRelationshipClass(SchemaBuilder schemaBuilder, AttributedRelationshipClassDefinition attributedRelationshipClassDefinition)
        {
            AttributedRelationshipClassDescription attributedRelationshipClassDescription = new AttributedRelationshipClassDescription(attributedRelationshipClassDefinition);

            // Update relationship split policy
            attributedRelationshipClassDescription.RelationshipSplitPolicy = RelationshipSplitPolicy.UseDefault;

            // Add field in the intermediate table
            attributedRelationshipClassDescription.FieldDescriptions.Add(FieldDescription.CreateIntegerField("RelationshipStatus"));

            // Add relationship rules based on subtypes,if available
            // Assuming origin class has subtype with code 1
            attributedRelationshipClassDescription.RelationshipRuleDescriptions.Add(new RelationshipRuleDescription(null, null) { OriginMaximumCardinality = 2, OriginMinimumCardinality = 1 });

            schemaBuilder.Modify(attributedRelationshipClassDescription);
            schemaBuilder.Build();
        }
        #endregion
        
        #region Deleting a relationship class
        public static void DeleteRelationshipClass(SchemaBuilder schemaBuilder, AttributedRelationshipClassDefinition AttributedRelationshipClassDefinition)
        {
            schemaBuilder.Delete(new AttributedRelationshipClassDescription(AttributedRelationshipClassDefinition));
            schemaBuilder.Build();
        }
        #endregion

      
        #region Adding/Removing Relationship class in/out of a feature dataset
        public static void RemoveRelationshipClass(SchemaBuilder schemaBuilder, FeatureDatasetDefinition featureDatasetDefinition, RelationshipClassDefinition relationshipClassDefinition)
        {
            FeatureDatasetDescription featureDatasetDescription = new FeatureDatasetDescription(featureDatasetDefinition);
            RelationshipClassDescription relationshipClassDescription = new RelationshipClassDescription(relationshipClassDefinition);

            // Remove relationship class from the feature dataset
            schemaBuilder.RemoveRelationshipClass(featureDatasetDescription, relationshipClassDescription);

            // Add relationship class in to the feature dataset
            // schemaBuilder.AddRelationshipClass(featureDatasetDescription, relationshipClassDescription);

            schemaBuilder.Build();
        }
        #endregion

 
        #region Creating relationship class
        public static void CreateRelationshipWithRelationshipRules(SchemaBuilder schemaBuilder)
        {
            // Creating the 'BuildingType' table with two fields - BuildingType and BuildingTypeDescription
            FieldDescription buildingType = FieldDescription.CreateIntegerField("BuildingType");
            FieldDescription buildingTypeeDescription = FieldDescription.CreateStringField("BuildingTypeDescription", 100);
            TableDescription buildingTypeDescription = new TableDescription("BuildingType", new List<FieldDescription>() { buildingType, buildingTypeeDescription });
            TableToken buildingtypeToken = schemaBuilder.Create(buildingTypeDescription);

         
            // Creating the 'Building' feature class with three fields - BuildingId, Address, and BuildingType
            FieldDescription buildingId = FieldDescription.CreateIntegerField("BuildingId");
            FieldDescription buildingAddress = FieldDescription.CreateStringField("Address", 100);
            FieldDescription usageSubType = FieldDescription.CreateIntegerField("UsageSubtype");
            FeatureClassDescription featureClassDescription = new FeatureClassDescription("Building", new List<FieldDescription> { buildingId, buildingAddress, buildingType, usageSubType }, new ShapeDescription(GeometryType.Polygon, SpatialReferences.WGS84));

            // Adding subtype (optional)
            featureClassDescription.SubtypeFieldDescription = new SubtypeFieldDescription(usageSubType.Name, new Dictionary<int, string> { { 1, "Marketing" }, { 2, "Utility" } });


            // Create a FeatureDataset token
            FeatureDatasetDescription featureDatasetDescription = new FeatureDatasetDescription("Parcel_Information", SpatialReferences.WGS84);
            FeatureDatasetToken featureDatasetToken = schemaBuilder.Create(featureDatasetDescription);

            FeatureClassToken buildingToken = schemaBuilder.Create(new FeatureDatasetDescription(featureDatasetToken),featureClassDescription);

            // Creating a 1:M relationship between the 'Building' feature class and 'BuildingType' table
            RelationshipClassDescription relationshipClassDescription = new RelationshipClassDescription("BuildingToBuildingType", new FeatureClassDescription(buildingToken), new TableDescription(buildingtypeToken),
              RelationshipCardinality.OneToMany, buildingType.Name, buildingType.Name)
            {
                RelationshipType = RelationshipType.Composite
            };

            // Adding relationship rules for a usage subtypes
            relationshipClassDescription.RelationshipRuleDescriptions.Add(new RelationshipRuleDescription(1, null));
            // Creating a FeatureDataset named as 'Parcel_Information' and a FeatureClass with name 'Parcels' in one operation            

            // Create a RelationshipClass inside a FeatureDataset
            RelationshipClassToken relationshipClassToken = schemaBuilder.Create(new FeatureDatasetDescription(featureDatasetToken), relationshipClassDescription);

            // Build status
            bool buildStatus = schemaBuilder.Build();

            // Build errors
            if (!buildStatus)
            {
                IReadOnlyList<string> errors = schemaBuilder.ErrorMessages;
            }
            schemaBuilder.Create(relationshipClassDescription);
          
            schemaBuilder.Build();
        }
        #endregion
    }
}
