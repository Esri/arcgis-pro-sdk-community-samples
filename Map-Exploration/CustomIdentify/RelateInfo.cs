//   Copyright 2016 Esri
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
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace CustomIdentify
{
    public class RelateInfo
    {
        /// <summary>
        /// Represents the Relationship Class information
        /// </summary>
        /// <param name="featureLayer"></param>
        /// <param name="selection"></param>
        public RelateInfo(FeatureLayer featureLayer, Selection selection)
        {
            _featureLayer = featureLayer;
            _selection = selection;            
            _featureClassName = _featureLayer.GetFeatureClass().GetName();
            
        }

        private FeatureLayer _featureLayer;

        private string _featureClassName;

        private Selection _selection;

        private Geodatabase _geodatabase;

        private Dictionary<string, FeatureClass> _layersInMapFeatureClassMap = new Dictionary<string, FeatureClass>(); 

        /// <summary>
        /// Gets the hierarcchical row that describes the selected feature
        /// </summary>
        /// <param name="selectedFeatures"></param>
        /// <returns></returns>
        public async Task<List<PopupContent>> GetPopupContent(Dictionary<BasicFeatureLayer, List<long>> selectedFeatures)
        {
            //_layersInMapFeatureClassMap = GetMapLayersFeatureClassMap(); //gets all the feature layers from the map and add it to the dictionary with the Feature class as its key         
            var popupContents = new List<PopupContent>();
            try
            {
                var objectIDS = _selection.GetObjectIDs();

                if (_geodatabase == null)
                    _geodatabase = await GetGDBFromLyrAsync(_featureLayer);

                var kvpMapMember = selectedFeatures.FirstOrDefault(s => s.Key.GetTable().GetName().Equals(_featureClassName));
                
                foreach (var objectID in objectIDS)
                {
                    //List<HierarchyRow> hrows = new List<HierarchyRow>(); //this makes the custom popup show only one record.
                    //var newRow = await GetRelationshipChildren(_featureClassName, objectID);
                    //hrows.Add(newRow);
                    popupContents.Add(new DynamicPopupContent(kvpMapMember.Key, objectID, _featureClassName, this));
                }
            }
            catch (Exception)
            {

            }
            return popupContents;
        }

        public async Task<HierarchyRow> GetRelationshipChildren(string featureClassName, long objectID, string rcException = "")
        {
            
            var value = await GetSelectedItemDisplayValue(featureClassName, objectID);

            var newHRow = new HierarchyRow()
            {
                name = value,
                type = featureClassName
            };

            var relationshipClassDefinitions = GetRelationsshipClassDefinitionsFromFeatureClass(featureClassName);
           



            foreach (var relationshipClassDefinition in relationshipClassDefinitions)
            {
                var rcName = relationshipClassDefinition.GetName(); //get the name
                if (rcException == rcName) //exception so we don't go in circles
                    continue;
                var relationshipClass = _geodatabase.OpenDataset<RelationshipClass>(rcName); //open the relationship class

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

                            childHRow.children.Add(await GetRelationshipChildren(destination, row.GetObjectID(), rcName)); //recursive: to get the attributes of the related feature
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

                        childHRow.children.Add(await GetRelationshipChildren(origin, row.GetObjectID(), rcName));
                    }
                    newHRow.children.Add(childHRow);
                }

            }
            return newHRow;
        }

        private async Task<Geodatabase> GetGDBFromLyrAsync(BasicFeatureLayer lyr)
        {
            Geodatabase geodatabase = null;
            await QueuedTask.Run(() => geodatabase = (lyr.GetTable().GetDatastore() as Geodatabase));
            return geodatabase;
        }
        
        private IEnumerable<RelationshipClassDefinition> GetRelationsshipClassDefinitionsFromFeatureClass(string featureClassName)
        {
            
            return _geodatabase.GetDefinitions<RelationshipClassDefinition>().
                    Where(defn => defn.GetOriginClass().Equals(featureClassName) || defn.GetDestinationClass().Equals(featureClassName));
        }

        private async Task<string> GetSelectedItemDisplayValue(string featureClassName, long objectID)
        {
            string value = "";
            FeatureClass featureClass = null;
            foreach (var kvp in CustomIdentify.LayersInMapFeatureClassMap)
            {
                if (kvp.Value.GetName() == featureClassName)
                    featureClass = kvp.Value;

            }
            //FeatureClass featureClass = _geodatabase.OpenDataset<FeatureClass>(featureClassName);

            if (featureClass == null)
                return value;
            
            string displayField = GetDisplayField(featureClass); //could be null if the feature class is not a layer in the map.
            if (!string.IsNullOrEmpty(displayField))
                value = await GetAttributeValue(featureClass, displayField, objectID);
            else
                value = objectID.ToString();
            return value;
        }
        private string GetDisplayField(FeatureClass featureClass)
        {
            string displayField = "";
            
            Map map = MapView.Active.Map;
            if (map == null)
                return displayField;

            //Get flattened layers from Map. If the feature class exists in the map, get that feature Layer and its display field. If not, return null.
            var layer = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(lyr => lyr.GetFeatureClass().GetName() == featureClass.GetName());

            if (layer == null)
                return "";
            CIMFeatureLayer currentCIMFeatureLayer = layer.GetDefinition() as CIMFeatureLayer;
            CIMFeatureTable cimFeatureTable = currentCIMFeatureLayer.FeatureTable;
            
            displayField = cimFeatureTable.DisplayField;               
            
            return displayField;

        }

        //private Dictionary<string, FeatureClass> GetMapLayersFeatureClassMap()
        //{
        //    Dictionary<string, FeatureClass> lyrFeatureClassMap = new Dictionary<string, FeatureClass>();

        //    Map map = MapView.Active.Map;
        //    if (map == null)
        //        return null;
        //    var layers = map.GetLayersAsFlattenedList().OfType<FeatureLayer>();

        //    foreach (var lyr in layers)
        //    {
        //        string fc = lyr.GetFeatureClass().GetName();
        //        FeatureClass featureClass = _geodatabase.OpenDataset<FeatureClass>(fc);

        //        if (featureClass != null)
        //            lyrFeatureClassMap.Add(lyr.Name, featureClass);

        //    }


        //    return lyrFeatureClassMap;
        //}
        private Task<string> GetAttributeValue(FeatureClass featureClass, string fieldName, long objectId)
        {

            return QueuedTask.Run(() =>
            {
                string value = "";

                    try
                    {
                        var oidField = featureClass.GetDefinition().GetObjectIDField();
                        QueryFilter queryFilter = new QueryFilter
                        {
                            WhereClause = string.Format("({0} in ({1}))", oidField, objectId)
                        };
                        using (RowCursor rowCursor = featureClass.Search(queryFilter, false))
                        {
                            while (rowCursor.MoveNext())
                            {
                                using (Row row = rowCursor.Current)
                                {
                                    value = Convert.ToString(row[fieldName]);
                                    
                                }
                            }
                        }
                    }
                    catch (GeodatabaseFieldException fieldException)
                    {
                        // One of the fields in the where clause might not exist. There are multiple ways this can be handled:
                        // Handle error appropriately
                    }
                    catch (Exception exception)
                    {
                        // logger.Error(exception.Message);
                    }               
                return value;
            });            
        }



    }
}

