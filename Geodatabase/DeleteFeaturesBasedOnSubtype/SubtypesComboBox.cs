//   Copyright 2019 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace DeleteFeaturesBasedOnSubtype
{
    /// <summary>
    /// Represents the ComboBox which will list the subtypes for the FeatureClass.
    /// Also encapsulates the logic for deleting the features corresponding to the selected subtype
    /// </summary>
    internal class SubtypesComboBox : ComboBox
    {
        /// <summary>
        /// Combo Box contructor
        /// </summary>
        public SubtypesComboBox()
        {
        }

        /// <summary>
        /// The on comboBox selection change event. 
        /// </summary>
        protected override void OnDropDownOpened()
        {
            if (MapView.Active.GetSelectedLayers().Count == 1)
            {
                Clear();
                QueuedTask.Run(() =>
                {
                    var layer = MapView.Active.GetSelectedLayers()[0];
                    if (layer is FeatureLayer)
                    {
                        var featureLayer = layer as FeatureLayer;
                        if (featureLayer.GetTable().GetDatastore() is UnknownDatastore)
                            return;
                        using (var table = featureLayer.GetTable())
                        {
                            IReadOnlyList<Subtype> readOnlyList;
                            try
                            {
                                readOnlyList = table.GetDefinition().GetSubtypes();
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(string.Format("Unable to get subtypes - error: {0}", e));
                                return;
                            }
                            foreach (var subtype in readOnlyList)
                            {
                                Add(new ComboBoxItem(subtype.GetName()));
                            }
                        }
                    }
                });
            }
        }

        /// <summary>
        /// The on comboBox selection change event. 
        /// </summary>
        /// <param name="item">The newly selected combo box item</param>
        protected async override void OnSelectionChange(ComboBoxItem item)
        {

            if (item == null)
                return;

            if (string.IsNullOrEmpty(item.Text))
                return;
            string error = String.Empty;
            bool result = false;
            await QueuedTask.Run(async () =>
            {
                var layer = MapView.Active.GetSelectedLayers()[0];
                if (layer is FeatureLayer)
                {
                    var featureLayer = layer as FeatureLayer;
                    if (featureLayer.GetTable().GetDatastore() is UnknownDatastore)
                        return;
                    using (var table = featureLayer.GetTable())
                    {
                        var subtypeField = table.GetDefinition().GetSubtypeField();
                        var code = table.GetDefinition().GetSubtypes().First(subtype => subtype.GetName().Equals(item.Text)).GetCode();
                        var queryFilter = new QueryFilter { WhereClause = string.Format("{0} = {1}", subtypeField, code) };
                        try
                        {
                            using (var rowCursor = table.Search(queryFilter, false))
                            {
                                var editOperation = new EditOperation()
                                {
                                    Name = string.Format(@"Deleted where {0}", queryFilter.WhereClause)
                                };
                                editOperation.Callback(context =>
                                {
                                    // Note: calling the following method: "await editOperation.ExecuteAsync();"
                                    // within the context of the "using (var rowCursor ..." clause 
                                    // ensures that "rowCursor" in the following while statement is stil
                                    // defined and not disposed.  So the warning "Access to disposed closure"
                                    // doesn't apply here
                                    while (rowCursor.MoveNext())
                                    {
                                        Thread.Yield();
                                        using (var row = rowCursor.Current)
                                        {
                                            context.Invalidate(row);
                                            row.Delete();
                                        }
                                    }
                                }, table);
                                if (table.GetRegistrationType().Equals(RegistrationType.Nonversioned) &&
                                    Project.Current.HasEdits)
                                {
                                    error =
                                        "The FeatureClass is Non-Versioned and there are Unsaved Edits in the Project. Please save or discard the edits before attempting Non-Versioned Edits";
                                }
                                else
                                    result = await editOperation.ExecuteAsync();
                                if (!result)
                                    error = editOperation.ErrorMessage;
                            }
                        }
                        catch (Exception e)
                        {
                            error = e.Message;
                        }
                    }
                }
            });
            if (!result)
                MessageBox.Show(String.Format("Could not delete features for subtype {0} : {1}",
                    item.Text, error));
        }

    }
}
