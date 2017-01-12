//   Copyright 2017 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace CustomIdentify
{
    /// <summary>
    /// Class to managed the related information for a given feature layer
    /// </summary>
    public class RelateInfo {

        private string _displayFieldName = "";
        /// <summary>
        /// Finds all related children
        /// </summary>
        public HierarchyRow GetRelationshipChildren(Layer member, Geodatabase gdb,
                                 string featureClassName, long objectID, string rcException = "") {

            if (!QueuedTask.OnWorker) {
                throw new CalledOnWrongThreadException();
            }

            string displayValue = "";
            if (member != null) {
                if (string.IsNullOrEmpty(_displayFieldName)) {
                    CIMFeatureLayer currentCIMFeatureLayer = member.GetDefinition() as CIMFeatureLayer;
                    _displayFieldName = currentCIMFeatureLayer?.FeatureTable.DisplayField ?? "";
                }

                if (!string.IsNullOrEmpty(_displayFieldName)) {
                    var inspector = new Inspector();
                    inspector.Load(member, objectID);
                    displayValue =  $"{inspector[_displayFieldName]?.ToString() ?? ""}";
                }
            }
            //Did the display value get set?
            if (string.IsNullOrEmpty(displayValue))
                displayValue = $"OBJECTID: {objectID.ToString()}";

            var newHRow = new HierarchyRow()
            {
                name = displayValue,
                type = $"{featureClassName}" 
            };

            //Check the layer for any relationships
            var children = GetRelationshipChildrenFromLayer(member as BasicFeatureLayer, objectID);
            if (children.Count > 0) {
                newHRow.children = children;
                return newHRow;//Give layer related precedence over GDB related
            }

            //If we are here we do not have any relates on the layer
            //See if we have relates in the GDB
            var relationshipClassDefinitions = GetRelationshipClassDefinitionsFromFeatureClass(gdb, featureClassName);
            foreach (var relationshipClassDefinition in relationshipClassDefinitions)
            {
                var rcName = relationshipClassDefinition.GetName(); //get the name
                if (rcException == rcName) //exception so we don't go in circles
                    continue;
                //Alternate way of getting the features classes in the relationship (new at 1.3):  
                //IReadOnlyList<Definition> definitions = GetRelatedDefinitions(relationshipClassDefinition, DefinitionRelationshipType.DatasetsRelatedThrough);  
                var relationshipClass = gdb.OpenDataset<RelationshipClass>(rcName); //open the relationship class

                var origin = relationshipClassDefinition.GetOriginClass(); //get the origin of the relationship class
                var destination = relationshipClassDefinition.GetDestinationClass(); //get the destination of the relationship class
                string displayName = "";

                IReadOnlyList<Row> relatedRows = null;
                if (origin == featureClassName)
                {
                    relatedRows = relationshipClass.GetRowsRelatedToOriginRows(new List<long> { objectID }); //the feature class is the origin. So we need the rows in the destination related to the origin
                    if (relatedRows.Count > 0)

                    {
                        displayName = string.Format("{0}: {1}", rcName, destination);
                        var childHRow = new HierarchyRow()
                        {
                            name = displayName,
                            type = rcName
                        };
                        foreach (var row in relatedRows)
                        {

                            childHRow.children.Add(GetRelationshipChildren(null, gdb, destination, row.GetObjectID(), rcName)); //recursive: to get the attributes of the related feature
                        }
                        newHRow.children.Add(childHRow);
                        continue;
                    }
                }
                relatedRows = relationshipClass.GetRowsRelatedToDestinationRows(new List<long> { objectID }); //Feature class is the destination so get the rows related to it
                if (relatedRows.Count > 0)

                {
                    displayName = string.Format("{0}: {1}", rcName, origin);
                    var childHRow = new HierarchyRow()
                    {
                        name = displayName,
                        type = rcName
                    };
                    foreach (var row in relatedRows)
                    {

                        childHRow.children.Add(GetRelationshipChildren(null, gdb, origin, row.GetObjectID(), rcName));
                    }
                    newHRow.children.Add(childHRow);
                }

            }
            return newHRow;
        }

        internal List<HierarchyRow> GetRelationshipChildrenFromLayer(BasicFeatureLayer member, long objectID) {

            var children = new List<HierarchyRow>();

            CIMBasicFeatureLayer bfl = member.GetDefinition() as CIMBasicFeatureLayer;
            var relates = bfl.FeatureTable.Relates;
            if (relates == null || relates.Length == 0)
                return children;

            foreach (var relate in relates) {
                if (!(relate.DataConnection is CIMStandardDataConnection) &&
                    !(relate.DataConnection is CIMFeatureDatasetDataConnection))
                    continue;//Not supported in this sample

                var sdc = relate.DataConnection as CIMStandardDataConnection;
                var fdc = relate.DataConnection as CIMFeatureDatasetDataConnection;
                var factory = sdc?.WorkspaceFactory ?? fdc.WorkspaceFactory;
                var path = sdc?.WorkspaceConnectionString ?? fdc.WorkspaceConnectionString;
                if (string.IsNullOrEmpty(path))
                    continue;//No connection information we can use
                path = path.Replace("DATABASE=", "");
                var dstype = sdc?.DatasetType ?? fdc.DatasetType;

                if (dstype != esriDatasetType.esriDTFeatureClass &&
                    dstype != esriDatasetType.esriDTTable) {
                    continue;//Not supported in the sample
                }

                var dsname = sdc?.Dataset ?? fdc.Dataset;
                var featDatasetName = fdc?.FeatureDataset ?? "";

                Geodatabase gdb = null;
                if (factory == WorkspaceFactory.FileGDB) {
                    gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(path, UriKind.Absolute)));
                }
                else if (factory == WorkspaceFactory.SDE) {
                    gdb = new Geodatabase(new DatabaseConnectionFile(new Uri(path, UriKind.Absolute)));
                }

                Table table = null;
                //We have to open a type specific dataset - FeatureClass or Table
                //We cannot simply use 'Table' for both
                if (dstype == esriDatasetType.esriDTFeatureClass) {
                    table = GetDatasetFromGeodatabase<FeatureClass>(gdb, dsname, featDatasetName);
                }
                else {
                    table = GetDatasetFromGeodatabase<Table>(gdb, dsname, featDatasetName);
                }
                if (table == null)
                    continue;//Related dataset not found

                //Get any related rows
                var qry_fld = table.GetDefinition().GetFields().FirstOrDefault(f => f.Name == relate.ForeignKey);
                if (qry_fld == null)
                    continue;//We cannot find the designated foreign key

                //Load relevant values
                var inspector = new Inspector();
                inspector.Load(member, objectID);

                var need_quotes = qry_fld.FieldType == FieldType.String;
                var quote = need_quotes ? "'" : "";
                var where = $"{relate.ForeignKey} = {quote}{inspector[relate.PrimaryKey]}{quote}";
                var qf = new QueryFilter() {
                    WhereClause = where,
                    SubFields = $"{table.GetDefinition().GetObjectIDField()}, {relate.ForeignKey}" 
                };

                var childHRow = new HierarchyRow() {
                    name = dsname,
                    type = $"{inspector[relate.PrimaryKey]}" 
                };

                using (var rc = table.Search(qf)) {
                    while (rc.MoveNext()) {
                        using (var row = rc.Current) {
                            var id = row.GetObjectID();
                            var HRow = new HierarchyRow() {
                                name = $"{id}",
                                type = relate.ForeignKey
                            };
                            childHRow.children.Add(HRow);
                        }
                    }
                }
                children.Add(childHRow);
            }
            return children;
        }

        private T GetDatasetFromGeodatabase<T>(Geodatabase gdb, string dataset, string featureDataset) where T : Table {
            if (!string.IsNullOrEmpty(featureDataset)) {
                var fd = gdb.OpenDataset<FeatureDataset>(featureDataset);
                if (fd != null) {
                    return fd.OpenDataset<T>(dataset);
                }
            }
            return gdb.OpenDataset<T>(dataset);
        }


        private IEnumerable<RelationshipClassDefinition> GetRelationshipClassDefinitionsFromFeatureClass(
              Geodatabase gdb, string featureClassName) {
            return gdb.GetDefinitions<RelationshipClassDefinition>().
                    Where(defn => defn.GetOriginClass().Equals(featureClassName) || defn.GetDestinationClass().Equals(featureClassName));
        }
        
    }
}

