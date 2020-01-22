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
using System.Windows.Input;
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
    internal class ClearLabels : Button
    {
        protected override void OnClick()
        {
            ResetLabels();
        }

        private static void ResetLabels()
        {
            QueuedTask.Run(async () =>
            {

                //Get the line feature we want to clear labels for
                var featureLayer = MapView.Active.Map.GetLayersAsFlattenedList().Where(f => f.Name == "U.S. Rivers (Generalized)").FirstOrDefault() as FeatureLayer;
                if (featureLayer == null) return;

                // create an instance of the inspector class
                var inspector = new ArcGIS.Desktop.Editing.Attributes.Inspector();

                //Clear the previous "Yes" values for the LabelOn field.
                //Create a QueryFilter to search for all features that have the LabelOn field value set at Yes.
                var qf = new QueryFilter
                {
                    WhereClause = "LabelOn = 'Yes'"
                };
                var selectionLabelOnYes = featureLayer.Select(qf);
                var featuresToClear = selectionLabelOnYes.GetObjectIDs()?.ToList();
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
            });
        }
    }
}
