/*

   Copyright 2020 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;


namespace LabelLineFeatures
{
    internal class LabelFeatures : MapTool
    {
        public LabelFeatures()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Rectangle;
            SketchOutputMode = SketchOutputMode.Map;

        }

        /// <summary>
        /// Called when the sketch finishes. This is where we will create the sketch operation and then execute it.
        /// </summary>
        /// <param name="geometry">The geometry created by the sketch.</param>
        /// <returns>A Task returning a Boolean indicating if the sketch complete event was successfully handled.</returns>
        protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            QueuedTask.Run(async () =>
            {
                //select features that intersect the sketch geometry
                var selection = MapView.Active.SelectFeatures(geometry, SelectionCombinationMethod.New);
                //Get the line feature we want to label
                var featureLayer = selection.Where(f => f.Key.ShapeType == esriGeometryType.esriGeometryPolyline).FirstOrDefault().Key as FeatureLayer;
                if (featureLayer == null) return;
                //Get the list of line features that are selected
                var oidsOfSelectedFeature = selection[featureLayer];
                var oidField = await GetOIDFieldAsync(featureLayer.GetTable());                

                //Get the layer's definition
                var lyrDefn = featureLayer.GetDefinition() as CIMFeatureLayer;
                //Get the label classes - and check if the correct label class we need to label the line features with exists
                var listLabelClasses = lyrDefn.LabelClasses.ToList();
                var theLabelClass = listLabelClasses.Where(l => l.Name == "LabelSelectedFeaturesWithLength").FirstOrDefault();
                if (theLabelClass == null) //create label class and add to the collection
                {
                    theLabelClass = await CreateAndApplyLabelClassAsync(featureLayer, oidField.Name, oidsOfSelectedFeature);
                    listLabelClasses.Add(theLabelClass);
                }
                else
                    //LabelClass exists, so just need to modify the SQL query with the new OIDs selected
                    theLabelClass.WhereClause = $"{oidField.Name} IN ({String.Join(", ", oidsOfSelectedFeature.ToArray())})";
                //Turn off all the label classes except this one
                foreach (var lc in listLabelClasses)
                {
                    lc.Visibility = lc.Name == "LabelSelectedFeaturesWithLength" ?   true :  false;
                }
                //Apply the label classes back to the layer definition
                lyrDefn.LabelClasses = listLabelClasses.ToArray();
                //Set the layer definition
                featureLayer.SetDefinition(lyrDefn);
                //set the label's visiblity
                featureLayer.SetLabelVisibility(true);
            });
            return base.OnSketchCompleteAsync(geometry);
        }

        private async Task<CIMLabelClass> CreateAndApplyLabelClassAsync(FeatureLayer featureLayer, string oidField, List<long> oids)
        {

            var labelClass = await QueuedTask.Run(() =>
            {
                var labelSelectedFeaturesWithLength = new CIMLabelClass
                {
                    Name = "LabelSelectedFeaturesWithLength",
                    ExpressionEngine = LabelExpressionEngine.Arcade,
                    Expression = "$feature.MILES",
                    WhereClause = $"{oidField} IN ({String.Join(", ", oids.ToArray())})",
                    TextSymbol = SymbolFactory.Instance.ConstructTextSymbol().MakeSymbolReference(),
                    Visibility = true
                };
                var lyrDefn = featureLayer.GetDefinition() as CIMFeatureLayer;
                var listLabelClasses = lyrDefn.LabelClasses.ToList();
                listLabelClasses.Add(labelSelectedFeaturesWithLength);
                lyrDefn.LabelClasses = listLabelClasses.ToArray();
                featureLayer.SetDefinition(lyrDefn);
                return labelSelectedFeaturesWithLength;
            });
            return labelClass;
        }
        /// <summary>
        ///     Find the first field of the provided field type.
        /// </summary>
        /// <param name="table">Table or FeatureClass containing the field.</param>
        /// <param name="fieldType">
        ///     The type of field to be retrieved.
        ///     <remarks>Some types can only exist once per table.</remarks>
        /// </param>
        /// <returns>
        ///     The first occurrence of the field type is returned. If no field of the given type is found, a null reference
        ///     is returned.
        /// </returns>
        public static Task<Field> GetFieldByTypeAsync(Table table, FieldType fieldType)
        {
            return QueuedTask.Run(() =>
            {
                IReadOnlyList<Field> fields = ((TableDefinition)table.GetDefinition()).GetFields();
                return fields.FirstOrDefault(a => a.FieldType == fieldType);
            });
        }

        /// <summary>
        ///     Returns the ObjectID field from a table or feature class.
        /// </summary>
        /// <param name="table">Table or FeatureClass containing the ObjectID field.</param>
        /// <returns>The ObjectID field.</returns>
        public static Task<Field> GetOIDFieldAsync(Table table)
        {
            return GetFieldByTypeAsync(table, FieldType.OID);
        }
    }
}
