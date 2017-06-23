//Copyright 2017 Esri

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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using System.IO;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace ConstructionTool
{
    /// <summary>
    /// A button implementation showing how to modify attributes of a selected feature.
    /// </summary>
    internal class AttributeButton : Button
    {
        protected override async void OnClick()
        {
            await PerformAttributeChange();
        }

        /// <summary>
        /// This function takes the selected features in the map view, finds the first field of type string in the each feature class
        /// and modifies the attribute value to a random string.
        /// </summary>
        /// <returns>Indicator if the edit operation was successful.</returns>
        private async Task<bool> PerformAttributeChange()
        {
            try
            {
                // retrieve the currently selected features in the map view            
                var currentSelectedFeatures = await QueuedTask.Run(() =>
                {
                    return MapView.Active.Map.GetSelection();
                });

                // for each of the map members in the selected layers
                foreach (var mapMember in currentSelectedFeatures)
                {
                    var featureLayer = mapMember.Key as BasicFeatureLayer;
                    // .. get the underlying table
                    var table = await QueuedTask.Run(() =>
                    {
                        return featureLayer.GetTable();
                    });

                    // retrieve the first field of type string
                    var stringField = table.GetFieldByTypeAsync(FieldType.String).Result;
                    var stringFieldName = stringField != null ? stringField.Name : String.Empty;

                    // check if the returned string of the field name actually contains something
                    // meaning if the current MapMember actually contains a field of type string
                    if (String.IsNullOrEmpty(stringFieldName))
                        continue;
#if notthis
                    #region Use edit operations for attribute changes
                    // create a new edit operation to encapsulate the string field modifications
                    var modifyStringsOperation = new EditOperation
                    {
                        Name = String.Format("Modify string field '{0}' in layer {1}.", stringFieldName, mapMember.Key.Name)
                    };
                    ICollection<long> oidSet = new List<long>();
                    var iCnt = 0;
                    // with each ObjectID of the selected feature
                    foreach (var oid in currentSelectedFeatures[mapMember.Key])
                    {
                        // set up a new dictionary with fields to modify
                        var modifiedAttributes = new Dictionary<string, object>
                        {
                            // add the name of the string field and the new attribute value to the dictionary
                            // in this example a random string is used
                            {stringFieldName, string.Format("Update {0} on: {1:s}", ++iCnt, DateTime.Now)}
                        };

                        // put the modify operation on the editor stack
                        modifyStringsOperation.Modify(mapMember.Key, oid, modifiedAttributes);
                        oidSet.Add(oid);
                    }

                    // execute the modify operation to apply the changes
                    await modifyStringsOperation.ExecuteAsync();
                    #endregion
#endif
                    #region Use the feature inspector for attribute changes
//#if OrUseThis
                    // as an alternative approach
                    // use the feature inspector class
                    var featureInspector = new Inspector(true);

                    // fill the feature inspector with the oids from the feature layer               
                    await featureInspector.LoadAsync(mapMember.Key, currentSelectedFeatures[mapMember.Key]);

                    // change the attribute value for the string field
                    featureInspector[stringFieldName] = Path.GetRandomFileName().Replace(".", "");

                    // app. the new values
                    await featureInspector.ApplyAsync();
//#endif
                    #endregion
                }
            }
            catch (Exception ex)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("An error occurred while updating attribute column data {0}", ex.ToString());
            }
            return true;
        }
    }
}
