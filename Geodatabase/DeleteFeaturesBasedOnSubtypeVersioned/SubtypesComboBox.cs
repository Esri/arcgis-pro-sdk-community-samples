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

using System.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Version = ArcGIS.Core.Data.Version;

namespace DeleteFeaturesBasedOnSubtypeVersioned
{
    /// <summary>
    /// Represents the ComboBox which will list the subtypes for the FeatureClass.
    /// Also encapsulates the logic for deleting the features corresponding to the selected subtype
    /// </summary>
    internal class SubtypesComboBox : ComboBox
    {

        private bool _isInitialized;
        private Random random;

        /// <summary>
        /// Combo Box contructor
        /// </summary>
        public SubtypesComboBox()
        {
            random = new Random();
        }

        /// <summary>
        /// On Opening the Subtype Combobox, the subtypes in the selected FeatureClass are populated in the ComboBox
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
                            var readOnlyList = table.GetDefinition().GetSubtypes();
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
        /// This method will 
        /// 1. Make sure if a Feature Layer is selected.
        /// 2. The Workspace is not null
        /// 3. Make sure that the workspace is an Enterprise SQL Server Geodatabase Workspace
        /// 
        /// and then create a new version (In a Queued Task)
        /// and Connect to the newly created version and delete all the features for the selected subtype (In a separate QueuedTask)
        /// </summary>
        /// <param name="item">The newly selected combo box item</param>
        protected override async void OnSelectionChange(ComboBoxItem item)
        {

            if (item == null)
                return;

            if (string.IsNullOrEmpty(item.Text))
                return;

            Layer layer = MapView.Active.GetSelectedLayers()[0];
                FeatureLayer featureLayer = null;
            if (layer is FeatureLayer)
            {
                featureLayer = layer as FeatureLayer;
                Geodatabase geodatabase = null;
                await QueuedTask.Run(() => geodatabase = (featureLayer.GetTable().GetDatastore() as Geodatabase));
                using (geodatabase)
                {
                    if (geodatabase == null)
                        return;   
                }
            }
            else return;

            EnterpriseDatabaseType enterpriseDatabaseType = EnterpriseDatabaseType.Unknown;
            await QueuedTask.Run(() =>
            {
                using (Table table = (MapView.Active.GetSelectedLayers()[0] as FeatureLayer).GetTable())
                {
                    try
                    {
                        var geodatabase = table.GetDatastore() as Geodatabase;
                        enterpriseDatabaseType = (geodatabase.GetConnector() as DatabaseConnectionProperties).DBMS;
                    }
                    catch (InvalidOperationException e)
                    {
                    }
                }
            });
            if (enterpriseDatabaseType != EnterpriseDatabaseType.SQLServer)
            {
                Enabled = false;
                return;
            }

            string versionName = String.Empty;
            await QueuedTask.Run(async () =>
            {
                using (Table table = featureLayer.GetTable())
                {
                    versionName = await CreateVersion(table);
                }
            });

            if (versionName == null)
                return;
            
            await QueuedTask.Run(() =>
            {
                using (Table table = featureLayer.GetTable())
                {
                    if (table.GetRegistrationType().Equals(RegistrationType.Nonversioned))
                        return;
                }
            });

			
            await QueuedTask.Run(async () =>
            {
                using (Table table = featureLayer.GetTable())
                {
                    string subtypeField = table.GetDefinition().GetSubtypeField();
                    int code = table.GetDefinition().GetSubtypes().First(subtype => subtype.GetName().Equals(item.Text)).GetCode();
                    QueryFilter queryFilter = new QueryFilter{WhereClause = string.Format("{0} = {1}", subtypeField, code)};
                    try
                    {
                        VersionManager versionManager = (table.GetDatastore() as Geodatabase).GetVersionManager();
                        Version newVersion = versionManager.GetVersions().First(version => version.GetName().Contains(versionName));
                        Geodatabase newVersionGeodatabase = newVersion.Connect();
                        using (Table newVersionTable = newVersionGeodatabase.OpenDataset<Table>(table.GetName()))
                        {
                            using (var rowCursor = newVersionTable.Search(queryFilter, false))
                            {
                               EditOperation editOperation = new EditOperation 
                                                             {
                                                               EditOperationType = EditOperationType.Long,
                                                               Name = "Delete Based On Subtype"
                                                             };

                                editOperation.Callback(context =>
                                {
                                    while (rowCursor.MoveNext())
                                    {
                                        using (Row row = rowCursor.Current)
                                        {
                                            context.Invalidate(row);
                                            row.Delete();
                                        }
                                    }
                                }, newVersionTable);
                                bool result = await editOperation.ExecuteAsync();
                                if (!result)
                                    MessageBox.Show(String.Format("Could not delete features for subtype {0} : {1}",
                                        item.Text, editOperation.ErrorMessage));
                                await Project.Current.SaveEditsAsync();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }                        
                }
            });   
        }

        /// <summary>
        /// This method calls Geoprocessing to create a new version for the Workspace corresponding to the table
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        private async Task<string> CreateVersion(Table table)
        {
            try
            {
                Version defaultVersion = (table.GetDatastore() as Geodatabase).GetVersionManager().GetVersions().FirstOrDefault(version =>
                {
                    string name = version.GetName();
                    return name.ToLowerInvariant().Equals("dbo.default") || name.ToLowerInvariant().Equals("sde.default");
                });
                if(defaultVersion == null)
                    return null;
                using (defaultVersion)
                {
                    IReadOnlyList<string> valueArray = Geoprocessing.MakeValueArray(new object[]{table, defaultVersion.GetName(), string.Format("NewVersion{0}", random.Next()), "private"});
                    List<string> values = new List<String>
                    {
                        valueArray[0].Remove(valueArray[0].LastIndexOf("\\", StringComparison.Ordinal)),
                        valueArray[1],
                        valueArray[2],
                        valueArray[3]
                    };
                    await Geoprocessing.ExecuteToolAsync("management.CreateVersion", values);
                    return valueArray[2];
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }
    }
}
