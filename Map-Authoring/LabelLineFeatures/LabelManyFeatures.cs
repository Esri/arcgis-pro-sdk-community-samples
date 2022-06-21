/*

   Copyright 2020 Esri

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    internal class LabelManyFeatures : MapTool
    {
        public LabelManyFeatures()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Rectangle;
            SketchOutputMode = SketchOutputMode.Map;
        }

        protected override Task OnToolActivateAsync(bool active)
        {
            return base.OnToolActivateAsync(active);
        }

        /// <summary>
        /// The features are labeled using a Field in the feature layer. The field LabelOn has values Yes or No. 
        /// If the value is "Yes", that feature will be labeled. If value is No, the feature will not be labeled.
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            QueuedTask.Run(async () =>
            {
                //select features that intersect the sketch geometry
                var selection = MapView.Active.SelectFeatures(geometry, SelectionCombinationMethod.New);
                //Get the line feature we want to label
                var featureLayer = selection.ToDictionary().Where(f => (f.Key as BasicFeatureLayer).Name == "U.S. Rivers (Generalized)").FirstOrDefault().Key as FeatureLayer;
                if (featureLayer == null) return;
                //Get the list of line features that are selected
                var oidsOfSelectedFeature = selection[featureLayer];
                if (oidsOfSelectedFeature.Count == 0) return;

                //We want to only label the selected features. So..
                //Clear the previous "Yes" values for the LabelOn field.
                //Create a QueryFilter to search for all features that have the LabelOn field value set at Yes.
                // create an instance of the inspector class
                var inspector = new ArcGIS.Desktop.Editing.Attributes.Inspector();

                var qf = new QueryFilter
                {
                    WhereClause = "LabelOn = 'Yes'"
                };
                var selectionLabelOnYes = featureLayer.Select(qf);
                var  featuresToClear = selectionLabelOnYes.GetObjectIDs()?.ToList();
                if (featuresToClear.Count > 0)
                {
                    // load the features into the inspector using a list of object IDs
                    await inspector.LoadAsync(featureLayer, featuresToClear);

                    // assign the attribute value of "No" to the field "LabelOn"
                    // if more than one features are loaded, the change applies to all features
                    inspector["LabelOn"] = "No";
                    // apply the changes as an edit operation
                    await inspector.ApplyAsync();                    
                }             

                // Now modify the features that are selected by the tool to have the value Yes
                await inspector.LoadAsync(featureLayer, oidsOfSelectedFeature);
                // assign the attribute value of "Yes" to the field "LabelOn" for the features selected
                // if more than one features are loaded, the change applies to all features
                inspector["LabelOn"] = "Yes";
                // apply the changes as an edit operation
                await inspector.ApplyAsync();
                //Get the layer's definition
                var lyrDefn = featureLayer.GetDefinition() as CIMFeatureLayer;
                //Get the label classes - and check if the correct label class we need to label the line features with exists
                var listLabelClasses = lyrDefn.LabelClasses.ToList();
                var theLabelClass = listLabelClasses.Where(l => l.Name == "LabelSelectedManyFeaturesWithLength").FirstOrDefault();
                if (theLabelClass == null) //create label class and add to the collection
                {
                    theLabelClass = await CreateAndApplyLabelClassAsync(featureLayer);
                    listLabelClasses.Add(theLabelClass);
                }
                   
                //Turn off all the label classes except this one
                foreach (var lc in listLabelClasses)
                {
                    lc.Visibility = lc.Name == "LabelSelectedManyFeaturesWithLength" ? true : false;
                }
                //Apply the label classes back to the layer definition
                lyrDefn.LabelClasses = listLabelClasses.ToArray();
                //Set the layer definition
                featureLayer.SetDefinition(lyrDefn);
                //set the label's visibility
                featureLayer.SetLabelVisibility(true);
            });
            return base.OnSketchCompleteAsync(geometry);
        }

        private async Task<CIMLabelClass> CreateAndApplyLabelClassAsync(FeatureLayer featureLayer)
        {
            var labelClass = await QueuedTask.Run(() =>
            {
                var labelSelectedFeaturesWithLength = new CIMLabelClass
                {
                    Name = "LabelSelectedManyFeaturesWithLength",
                    ExpressionEngine = LabelExpressionEngine.Arcade,
                    Expression = "$feature.MILES",
                    WhereClause = $"LabelOn = 'Yes'",
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
    }
}
