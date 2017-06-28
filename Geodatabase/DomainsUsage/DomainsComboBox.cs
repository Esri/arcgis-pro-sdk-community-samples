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

using ArcGIS.Core.Data;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using Version = ArcGIS.Core.Data.Version;

namespace DomainsUsage
{
    /// <summary>
    /// Represents the ComboBox
    /// </summary>
    internal class DomainsComboBox : ComboBox
    {
        private string gdbItemsOwner;

        /// <summary>
        /// Combo Box contructor
        /// </summary>
        public DomainsComboBox()
        {
            Enabled = false;
            TOCSelectionChangedEvent.Subscribe(UpdateDomainList);
        }

        /// <summary>
        /// This method makes sure
        /// 1. The Mapview is Active
        /// 2. There is at least one Layer selected
        /// 3. The selected Layer is a FeatureLayer
        /// 4. The selected Layer is backed by an Enterprise Sql Server Geodatabase FeatureClass
        /// 
        /// If all of these hold good, the DatabaseClient is used to execute a query which creates
        /// a Database Table containing the gdb_items records corresponding to all domains. The Table is
        /// then opened using the API and the domains combobox populated. Finally, the Table is deleted
        /// </summary>
        /// <param name="mapViewEventArgs"></param>
        private async void UpdateDomainList(MapViewEventArgs mapViewEventArgs)
        {
            if (MapView.Active == null ||
                mapViewEventArgs.MapView.GetSelectedLayers().Count < 1 ||
                !(mapViewEventArgs.MapView.GetSelectedLayers()[0] is FeatureLayer))
            {
                Enabled = false;
                return;
            }
            EnterpriseDatabaseType enterpriseDatabaseType = EnterpriseDatabaseType.Unknown;
            await QueuedTask.Run(() =>
            {
                using (Table table = (mapViewEventArgs.MapView.GetSelectedLayers()[0] as FeatureLayer).GetTable())
                {
                    if(!(table.GetDatastore() is Geodatabase))
                        return;
                    try
                    {
                        var gdb = table.GetDatastore() as Geodatabase;
                        var gdbConnector = gdb.GetConnector() as DatabaseConnectionProperties;
                        enterpriseDatabaseType = gdbConnector != null ? gdbConnector.DBMS : EnterpriseDatabaseType.Unknown;
                    }
                    catch 
                    {
                        System.Diagnostics.Debug.WriteLine("Exception was thrown!");
                    }
                }
            });
            if (enterpriseDatabaseType != EnterpriseDatabaseType.SQLServer)
            {
                Enabled = false;
                return;
            }

            Enabled = true;
            Clear();
            await QueuedTask.Run(() =>
             {
                 using (Table table = (mapViewEventArgs.MapView.GetSelectedLayers()[0] as FeatureLayer).GetTable())
                 {
                     var geodatabase = table.GetDatastore() as Geodatabase;
                     Version defaultVersion = geodatabase.GetVersionManager().GetVersions().FirstOrDefault(version =>
                     {
                         string name = version.GetName();
                         return name.ToLowerInvariant().Equals("dbo.default") || name.ToLowerInvariant().Equals("sde.default");
                     });
                     if (defaultVersion == null)
                         return;


                     string tableName = String.Format("NewTable{0}{1}{2}{3}", DateTime.Now.Hour, DateTime.Now.Minute,
                         DateTime.Now.Second, DateTime.Now.Millisecond);
                     gdbItemsOwner = defaultVersion.GetName().Split('.')[0];
                     string statement =
                         String.Format(
                             @"select {1}.GDB_ITEMTYPES.Name as Type, {1}.GDB_ITEMS.Name into {0} from {1}.GDB_ITEMS JOIN {1}.GDB_ITEMTYPES ON {1}.GDB_ITEMS.Type = {1}.GDB_ITEMTYPES.UUID where {1}.GDB_ITEMTYPES.Name = 'Domain' OR {1}.GDB_ITEMTYPES.Name = 'Coded Value Domain' OR {1}.GDB_ITEMTYPES.Name = 'Range Domain'",
                             tableName, gdbItemsOwner);
                     try
                     {
                         DatabaseClient.ExecuteStatement(geodatabase, statement);
                     }
                     catch (GeodatabaseTableException exception)
                     {
                         MessageBox.Show(exception.Message);
                         return;
                     }

                     var newTable = geodatabase.OpenDataset<Table>(tableName);

                     using (RowCursor rowCursor = newTable.Search(null, false))
                     {
                         while (rowCursor.MoveNext())
                         {
                             using (Row row = rowCursor.Current)
                             {
                                 Add(new ComboBoxItem(row["Name"].ToString()));
                             }
                         }
                     }
                     statement = String.Format(@"DROP TABLE {0}", tableName);
                     DatabaseClient.ExecuteStatement(geodatabase, statement);
                 }
             });
        }

        /// <summary>
        /// The MakeQueryLayer Geoprocessing tool is used to create a Query Layer consisting of
        /// all the datasets which reference the selected domain 
        /// </summary>
        /// <param name="item">The newly selected combo box item</param>
        protected override void OnSelectionChange(ComboBoxItem item)
        {

            if (item == null)
                return;

            if (string.IsNullOrEmpty(item.Text))
                return;

            if(String.IsNullOrEmpty(gdbItemsOwner))
                return;
            // HACK: Currently the API does not have any Getters for obtaining the Connection Properties or any other info. This will be fixed soon. 
            // Until then we are getting the first enterprise connection to make the GP call.
            string physicalPath = Project.Current.GetItems<GDBProjectItem>().First(projectItem => projectItem.Path != null && projectItem.Path.Contains(".sde")).Path;

            string QueryLayerName = String.Format("RelatedDomains{0}", item.Text);
            string Query = String.Format("SELECT ClassItems.Name, ClassItems.Path FROM (SELECT Relationships.OriginID AS ClassID, Relationships.DestID AS DomainID FROM {1}.GDB_ITEMRELATIONSHIPS AS Relationships INNER JOIN {1}.GDB_ITEMRELATIONSHIPTYPES AS RelationshipTypes ON Relationships.Type = RelationshipTypes.UUID WHERE RelationshipTypes.Name = 'DomainInDataset') AS DomainRelationships INNER JOIN {1}.GDB_ITEMS AS DomainItems ON DomainRelationships.DomainID = DomainItems.UUID INNER JOIN {1}.GDB_ITEMS AS ClassItems ON DomainRelationships.ClassID = ClassItems.UUID WHERE DomainItems.Name = '{0}'", item.Text, gdbItemsOwner);
            IReadOnlyList<string> valueArray = Geoprocessing.MakeValueArray(new object[] { physicalPath, QueryLayerName, Query, "Name" });
            Geoprocessing.ExecuteToolAsync("management.MakeQueryLayer", valueArray);
        }

    }
}
