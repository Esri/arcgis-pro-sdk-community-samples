/*

   Copyright 2023 Esri

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
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.DDL;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TableFeatureClassOperations
{
  /// <summary>
  /// This code sample demonstrates how to perform some a sequence of edit operations.   It will compute the maximum value of a specific field, duplicate records, select newly created records, update old records, and create a log of the changes performed on a table.
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data). The sample data contains an ArcGIS Pro project and data to be used for this sample. Make sure that the Sample data is unzipped in c:\data and C:\Data\Admin is available.
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. You can run the add-in using the debugger, but to see its full functionality you should run the add-in wihtout the debugger first since some of the functionality like Progress Dialogs are not supported when running ArcGIS Pro from the debugger.
  /// 1. Open the project 'C:\Data\Admin\AdminSample.aprx'.  
  /// 1. Add 'PopulationByCounty' as a standalone table to the map.
  /// 1. Add a new field to the 'U.S. Counties (Generalized)' feature class called 'Changed' of type 'Date'.
  /// 1. Select the 'Table/FeatureClass' tab, and then click on 'Common Table Operations' to bring up the 'Common Table Operations' dockpane.
  /// 1. From the 'Standalone Table' dropdown select 'PopulationByCounty':
  /// ![Common Table Operations](Screenshots/Screenshot1.png)  
  /// 1. Select one or more records in the 'PopulationByCounty' table on the 'Common Table Operations' dockpane.  
  /// ![Common Table Operations selection](Screenshots/Screenshot2.png)  
  /// 1. Click the 'Duplicate' button to duplicate the selected records.
  /// ![Duplicate](Screenshots/Screenshot3.png)  
  /// 1. Click the 'Show Selected Records' on the bottom of the 'PopulationByCounty' Table.
  /// ![Selected Records](Screenshots/Screenshot4.png)  
  /// 1. Click the 'Change Log' button in order to show the 'Change Log' dialog.   
  /// ![Change Log](Screenshots/Screenshot5.png)  
  /// ![Change Log Dialog](Screenshots/Screenshot6.png)  
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("TableFeatureClassOperations_Module");

    public static MemoryConnectionProperties MemoryConnectionProperties => new MemoryConnectionProperties("memory");

    public static string LogTableName => "LogTable";

    public static string GetLogTableName()
    {
      return $@"memory/{LogTableName}";
    }

    /// <summary>
    /// Call from MCT
    /// </summary>
    /// <returns></returns>
    public static void CreateLogTable()
    {
      CreateLogTable(MemoryConnectionProperties, LogTableName);
    }

    /// <summary>
    /// Call from MCT
    /// </summary>
    /// <returns></returns>
    public static Table GetLogTable()
    {
      using var geoDb = new Geodatabase(MemoryConnectionProperties);
      return geoDb.OpenDataset<Table>(LogTableName);
    }

    #region Create Log table

    /// <summary>
    /// Call from MCT
    /// </summary>
    /// <param name="memoryCPs"></param>
    /// <param name="tableName"></param>
    /// <returns></returns>
    private static TableToken CreateLogTable(MemoryConnectionProperties memoryCPs,
      string tableName)
    {
      using var geoDb = new Geodatabase(memoryCPs);
      return CreateLogTable(geoDb, tableName);
    }

    /// <summary>
    /// Call from MCT
    /// </summary>
    /// <param name="geoDb"></param>
    /// <param name="fcName"></param>
    /// <returns></returns>
    private static TableToken CreateLogTable(Geodatabase geoDb, string fcName)
    {
      var objectIDFieldDescription = new ArcGIS.Core.Data.DDL.FieldDescription("ObjectID", FieldType.OID);
      var stringFieldDescription = new ArcGIS.Core.Data.DDL.FieldDescription("TableName", FieldType.String)
      {
        AliasName = "Table Name"
      };
      var intFieldDescription = new ArcGIS.Core.Data.DDL.FieldDescription("Operation", FieldType.String);
      var foiFieldDescription = new ArcGIS.Core.Data.DDL.FieldDescription("ChangedOid", FieldType.BigInteger)
      {
        AliasName = "Changed Object ID"
      };
      var dateFieldDescription = new ArcGIS.Core.Data.DDL.FieldDescription("Changed", FieldType.Date);

      // Assemble a list of all of our field descriptions
      var fieldDescriptions = new List<ArcGIS.Core.Data.DDL.FieldDescription>() {
                    objectIDFieldDescription,
                    stringFieldDescription,
                    intFieldDescription,
                    foiFieldDescription,
                    dateFieldDescription
                };
      // Create a FeatureClassDescription object to describe the feature class
      // that we want to create
      var fcDescription = new TableDescription(fcName, fieldDescriptions);

      // Create a SchemaBuilder object
      SchemaBuilder schemaBuilder = new(geoDb);

      // Add the creation of the new feature class to our list of DDL tasks
      var tableToken = schemaBuilder.Create(fcDescription);

      // Execute the DDL
      bool success = schemaBuilder.Build();
      if (success) return tableToken;
      else return null;
    }

    #endregion Create Log table

    #region Overrides

    /// <summary>
    /// Called by Framework when ArcGIS Pro is closing
    /// </summary>
    /// <returns>False to prevent Pro from closing, otherwise True</returns>
    protected override bool CanUnload()
    {
      //TODO - add your business logic
      //return false to ~cancel~ Application close
      return true;
    }

    #endregion Overrides

  }
}
