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

using System.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Linq;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using DatabaseConnectionProperties = ArcGIS.Core.Data.DatabaseConnectionProperties;
using Version = ArcGIS.Core.Data.Version;

namespace DeleteFeaturesBasedOnSubtypeVersioned
{
  /// <summary>
  /// Represents the ComboBox which will list the subtypes for the FeatureClass.
  /// Also encapsulates the logic for deleting the features corresponding to the selected subtype
  /// </summary>
  internal class SubtypesComboBox : ComboBox
  {
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
    protected override async void OnDropDownOpened()
    {
      await QueuedTask.Run(() =>
       {
         if (MapView.Active.GetSelectedLayers().Count == 1)
         {
           Clear();

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
         }
       });

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
      await QueuedTask.Run(async () =>
      {
        if (item == null)
          return;

        if (string.IsNullOrEmpty(item.Text))
          return;

        Layer layer = MapView.Active.GetSelectedLayers()[0];
        if (layer is FeatureLayer featureLayer)
        {

          using (Geodatabase geodatabase = featureLayer.GetTable().GetDatastore() as Geodatabase)
          using (Table table = featureLayer.GetTable())
          {
            if (geodatabase == null) return;
            EnterpriseDatabaseType enterpriseDatabaseType = ((DatabaseConnectionProperties)geodatabase.GetConnector()).DBMS;


            if (enterpriseDatabaseType != EnterpriseDatabaseType.SQLServer)
            {
              Enabled = false;
              return;
            }

            if (table.GetRegistrationType().Equals(RegistrationType.Nonversioned)) return;


            using (Version newVersion = await CreateVersionAsync(table))
            using (Geodatabase newVersionGeodatabase = newVersion.Connect())
            using (Table newVersionTable = newVersionGeodatabase.OpenDataset<Table>(table.GetName()))
            {
              string subtypeField = table.GetDefinition().GetSubtypeField();
              int code = table.GetDefinition().GetSubtypes().First(subtype => subtype.GetName().Equals(item.Text)).GetCode();
              QueryFilter queryFilter = new QueryFilter { WhereClause = string.Format("{0}={1}", subtypeField, code) };

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
                {
                  MessageBox.Show(String.Format("Could not delete features for subtype {0} : {1}", item.Text, editOperation.ErrorMessage));
                }

                await Project.Current.SaveEditsAsync();
              }
            }
          }
        }
      });

    }

    /// <summary>
    /// This method creates a new version for the workspace corresponding to the table using the version manager
    /// </summary>
    /// <param name="table">Table name</param>
    /// <returns>Newly created version <see cref="Version"/></returns>
    private async Task<Version> CreateVersionAsync(Table table)
    {
      Version version = null;
      await QueuedTask.Run(() =>
      {
        try
        {
          using (VersionManager versionManager = (table.GetDatastore() as Geodatabase).GetVersionManager())
          using (Version defaultVersion = versionManager.GetVersions().FirstOrDefault(version =>
          {
            string name = version.GetName();
            return name.ToLowerInvariant().Equals("dbo.default") || name.ToLowerInvariant().Equals("sde.default");
          }))
          {
            if (defaultVersion is null)
            {
              version = null;
            }

            string versionName = $"Version_{DateTime.UtcNow.Ticks}";
            version = versionManager.CreateVersion(new VersionDescription(versionName, $"Description of {versionName}", VersionAccessType.Public));

          }
        }
        catch (Exception e)
        {
          Console.WriteLine(e);
        }
      });
      return version;
    }
  }
}
