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

namespace GeoProcesssingWithMemoryGDB
{
  internal class SetupMemoryGDB : Button
  {
    protected override async void OnClick()
    {
      Module1.OpenStatsDockpane();
      var memoryCPs = new MemoryConnectionProperties("memory");
      var state = "Create File GDB";
      try
      {
        await QueuedTask.Run(() =>
        {
          DeleteFeatureClass(memoryCPs, Module1.TestFcName);
          CreateFeatureClass(memoryCPs, Module1.TestFcName);
        });
        Module1.MemoryGDBStatsViewModel.MemoryStatus = "Created";
      }
      catch (Exception ex)
      {
        MessageBox.Show($@"Exception in [{state}]: {ex}");
      }
    }

    #region Create FC

    private static void CreateFeatureClass(MemoryConnectionProperties memoryCPs,
      string fcName)
    {
      using var geoDb = new Geodatabase(memoryCPs);
      CreateFeatureClass(geoDb, fcName);
    }

    private static void CreateFeatureClass(FileGeodatabaseConnectionPath connectionPath,
      string fcName)
    {
      using var geoDb = new Geodatabase(connectionPath);
      CreateFeatureClass(geoDb, fcName);
    }

    private static void CreateFeatureClass(Geodatabase geoDb, string fcName)
    {
      var hasZ = false;
      var hasM = false;
      // Create a ShapeDescription object
      var shapeDescription = new ShapeDescription(GeometryType.Polygon,
                                                  SpatialReferences.WebMercator)
      {
        HasM = hasM,
        HasZ = hasZ
      };
      var objectIDFieldDescription = new ArcGIS.Core.Data.DDL.FieldDescription("ObjectID", FieldType.OID);
      var stringFieldDescription = new ArcGIS.Core.Data.DDL.FieldDescription("TheString", FieldType.String);
      var intFieldDescription = new ArcGIS.Core.Data.DDL.FieldDescription("TheInteger", FieldType.Integer);
      var dblFieldDescription = new ArcGIS.Core.Data.DDL.FieldDescription("TheDouble", FieldType.Double);
      var dateFieldDescription = new ArcGIS.Core.Data.DDL.FieldDescription("TheDate", FieldType.Date);


      // Assemble a list of all of our field descriptions
      var fieldDescriptions = new List<ArcGIS.Core.Data.DDL.FieldDescription>() {
                    objectIDFieldDescription,
                    stringFieldDescription,
                    intFieldDescription,
                    dblFieldDescription,
                    dateFieldDescription
                };
      // Create a FeatureClassDescription object to describe the feature class
      // that we want to create
      var fcDescription =
        new FeatureClassDescription(fcName, fieldDescriptions, shapeDescription);

      // Create a SchemaBuilder object
      SchemaBuilder schemaBuilder = new(geoDb);

      // Add the creation of the new feature class to our list of DDL tasks
      schemaBuilder.Create(fcDescription);

      // Execute the DDL
      bool success = schemaBuilder.Build();
    }

    #endregion Create FC

    #region Delete FC

    private static void DeleteFeatureClass(MemoryConnectionProperties memoryCPs,
          string fcName)
    {
      using var gdb = new Geodatabase(memoryCPs);
      try
      {
        var fc = gdb.OpenDataset<FeatureClass>(fcName);
        DeleteFeatureClass(gdb, fc);
      }
      catch { }
    }

    private static void DeleteFeatureClass(FileGeodatabaseConnectionPath connectionPath,
          string fcName)
    {
      using var gdb = new Geodatabase(connectionPath);
      try
      {
        var fc = gdb.OpenDataset<FeatureClass>(fcName);
        DeleteFeatureClass(gdb, fc);
      }
      catch { }
    }

    private static void DeleteFeatureClass(Geodatabase geodatabase, FeatureClass featureClass)
    {
      // Create a FeatureClassDescription object
      FeatureClassDescription featureClassDescription = new FeatureClassDescription(featureClass.GetDefinition());

      // Create a SchemaBuilder object
      SchemaBuilder schemaBuilder = new(geodatabase);

      // Add the deletion fo the feature class to our list of DDL tasks
      schemaBuilder.Delete(featureClassDescription);

      // Execute the DDL
      bool success = schemaBuilder.Build();
    }

    #endregion Delete FC
  }
}
