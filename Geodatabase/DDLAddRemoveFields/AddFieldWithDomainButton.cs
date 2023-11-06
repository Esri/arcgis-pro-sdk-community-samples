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
    internal class AddFieldWithDomainButton : Button
    {
        protected override void OnClick()
        {
            QueuedTask.Run(() =>
            {
                using (Geodatabase geodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(@"C:\temp\mySampleGeoDatabase.gdb"))))
                {
                    AddFieldWithDomainSnippet(geodatabase);
                }
            });
        }
        
        #region Adding a Field that uses a domain
        public void AddFieldWithDomainSnippet(Geodatabase geodatabase)
        {
            // Adding a field,'PipeType', which uses the coded value domain to the 'Pipes' FeatureClass       //The FeatureClass to add field
            string featureClassName = "Pipes"; SchemaBuilder schemaBuilder = new SchemaBuilder(geodatabase);       // Create a CodedValueDomain description for water pipes
            CodedValueDomainDescription pipeDomainDescription =
       new CodedValueDomainDescription("WaterPipeTypes", FieldType.String,
       new SortedList<object, string> { { "C_1", "Copper" },
 { "S_2", "Steel" } })
       {
           SplitPolicy = SplitPolicy.Duplicate,
           MergePolicy = MergePolicy.DefaultValue
       };       // Create a coded value domain token
            CodedValueDomainToken codedValueDomainToken = schemaBuilder.Create(pipeDomainDescription);       // Create a new description from domain token
            CodedValueDomainDescription codedValueDomainDescription = new CodedValueDomainDescription(codedValueDomainToken);       // Create a field named as 'PipeType' using a domain description
            FieldDescription domainFieldDescription = new FieldDescription("PipeType", FieldType.String)
            { DomainDescription = codedValueDomainDescription };       //Retrieve existing information for 'Pipes' FeatureClass
            FeatureClassDefinition originalFeatureClassDefinition = geodatabase.GetDefinition<FeatureClassDefinition>(featureClassName);
            FeatureClassDescription originalFeatureClassDescription =
            new FeatureClassDescription(originalFeatureClassDefinition);       // Add domain field on existing fields
            List<FieldDescription> modifiedFieldDescriptions = new List<FieldDescription>(originalFeatureClassDescription.FieldDescriptions) { domainFieldDescription };       // Create a new description with updated fields for 'Pipes' FeatureClass 
            FeatureClassDescription featureClassDescription =
       new FeatureClassDescription(originalFeatureClassDescription.Name, modifiedFieldDescriptions,
       originalFeatureClassDescription.ShapeDescription);       // Update the 'Pipes' FeatureClass with domain field
            schemaBuilder.Modify(featureClassDescription);       // Build status
            bool buildStatus = schemaBuilder.Build();       // Build errors
            if (!buildStatus)
            {
                IReadOnlyList<string> errors = schemaBuilder.ErrorMessages;
            }
        }
        #endregion
    }
}
