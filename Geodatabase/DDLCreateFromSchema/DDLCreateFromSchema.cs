//   Copyright 2026 Esri
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
using ArcGIS.Core.Data.DDL;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Hosting;
using ArcGIS.Core.Internal.CIM;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DDLCreateFromSchema
{
  /// <summary>
  /// This sample creates a command line utility that takes 2 parameters:
  /// - a schema XML file (created using the Pro Export XML Workspace Document Geoprocessing tool with Export Options "Schema only")
  /// - a .SDE connection file that points to the target Geodatabase (with a user that has rights to create feature classes)
  /// 
  /// The utility uses the CoreHost library and .NET to parse the XML file into an object structure that is then used 
  /// to create feature classes in the target geodatabase
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 2. Set the Debug / Debug launch profile to have 2 parameters: 
  /// - a path to an XML file that has the schema
  /// - a path to an SDE file that has the connection information for the target Geodatabase
  /// 3. Click the Start button to start the utility with the provided parameters
  /// 4. A command window will appear and its content will be descriptive of what the utility is doing
  /// 5. Once successfully done the target geodatabase will contain the schema described in the XML file
  /// </remarks>
  internal class Program
  {
    //[STAThread] must be present on the Application entry point
    [STAThread]
    static int Main(string[] args)
    {
      //Call Host.Initialize before constructing any objects from ArcGIS.Core
      Host.Initialize();

      if (args.Length < 2)
      {
        Console.WriteLine("Usage: DDLCreateFromSchema <SchemaXmlPath> <SdeConnectionFilePath>");
        return 1;
      }

      string xmlPath = args[0];
      Uri sdeConnectionFile = new Uri(args[1]);

      var parser = new ArcGisSchemaParser();
      var schema = parser.Parse(xmlPath);
      bool success = false;

      // schema.FeatureClasses now contains the parsed feature classes and their fields

      // Connect to the (geo)database
      try
      {
        using (var sdeConnection = new Geodatabase(new DatabaseConnectionFile(sdeConnectionFile)))
        {
          // Apply the schema to the database
          foreach (var featureClass in schema.FeatureClasses)
          {
            Console.WriteLine($"Processing featureclass {featureClass.Name}");
            // Check if the feature class already exists
            try
            {
              sdeConnection.OpenDataset<FeatureClass>(featureClass.Name);
              Console.WriteLine($"Feature class {featureClass.Name} already exists in datastore '{sdeConnectionFile}'. Skipping creation.");
              continue; // Skip if the feature class already exists
            }
            catch (Exception ex)
            {
              Console.WriteLine($"Error opening feature class {featureClass.Name}: {ex.Message}");
            }

            // Assemble a list of all of our field descriptions
            ShapeDescription shapeDescription = null;
            List<ArcGIS.Core.Data.DDL.FieldDescription> fieldDescriptions = [];
            foreach (FieldSchema fieldSchema in featureClass.Fields)
            {
              string fieldName = fieldSchema.Name;

              // Skip on ST_Length() and other "functions"
              if (fieldSchema.Name.Contains("()"))
                continue;

              // "Sanitize" the field name 
              if (fieldSchema.Name.Contains('.'))
                fieldName = fieldSchema.Name.Substring(fieldSchema.Name.LastIndexOf('.') + 1);

              Console.WriteLine($"Processing field schema {fieldName}");

              if (fieldSchema.Type == "esriFieldTypeGeometry")
              {
                Console.WriteLine($"Field schema {fieldName} is a geometry");

                GeometryDefSchema geomDef = fieldSchema.GeometryDef ?? throw new InvalidOperationException("Geometry field must have a GeometryDef schema.");
                ArcGIS.Core.Geometry.SpatialReference spatialReference = SpatialReferences.WGS84;
                if (geomDef.SpatialReferenceWkt == null)
                  Console.WriteLine("Geometry field must have a valid Spatial Reference WKT.");
                //throw new InvalidOperationException("Geometry field must have a valid Spatial Reference WKT.");
                else
                {
                  SpatialReferenceBuilder spatialReferenceBuilder = new SpatialReferenceBuilder(geomDef.SpatialReferenceWkt);
                  spatialReference = spatialReferenceBuilder.ToSpatialReference();
                }
                GeometryType geometryType = geomDef.GeometryType switch
                {
                  "esriGeometryPoint" => GeometryType.Point,
                  "esriGeometryMultipoint" => GeometryType.Multipoint,
                  "esriGeometryPolyline" => GeometryType.Polyline,
                  "esriGeometryPolygon" => GeometryType.Polygon,
                  "esriGeometryEnvelope" => GeometryType.Envelope,
                  "esriGeometryMultiPatch" => GeometryType.Multipatch,
                  _ => throw new InvalidOperationException($"Unsupported geometry type: {geomDef.GeometryType}")
                };
                shapeDescription = new ShapeDescription(geometryType, spatialReference)
                {
                  HasM = geomDef.HasM,
                  HasZ = geomDef.HasZ
                };
                /*
                if (geomDef.GeometryType.Equals("esriGeometryMultiPatch"))
                  shapeDescription = new ShapeDescription(GeometryType.Multipatch, spatialReference)
                  {
                    HasM = geomDef.HasM,
                    HasZ = geomDef.HasZ
                  };
                else
                  shapeDescription = new ShapeDescription(Enum.Parse<GeometryType>(geomDef.GeometryType switch
                  {
                    "esriGeometryPoint" => "Point",
                    "esriGeometryPolyline" => "Polyline",
                    "esriGeometryPolygon" => "Polygon",
                    "esriGeometryEnvelope" => "Envelope",
                    "esriGeometryMultipoint" => "Multipoint",
                    "esriGeometryMultiPatch" => "MultiPatch",
                    "esriGeometryUnknown" => "Unknown",
                    "esriGeometryLine" => "Polyline", // Alias for compatibility
                    "esriGeometryRing" => "Polygon", // Alias for compatibility
                    "esriGeometryCircularArc" => "Polyline", // Alias for compatibility
                    "esriGeometryBezierCurve" => "Polyline", // Alias for compatibility
                    "esriGeometryEllipticArc" => "Polyline", // Alias for compatibility
                    "esriGeometryCircularString" => "Polyline", // Alias for compatibility
                    _ => throw new InvalidOperationException($"Unsupported geometry type: {geomDef.GeometryType}")
                  }), spatialReference)
                  {
                    HasM = geomDef.HasM,
                    HasZ = geomDef.HasZ
                  };
                */
              }
              else
              {
                Console.WriteLine($"Field schema {fieldName} is {fieldSchema.Type}");

                ArcGIS.Core.Data.DDL.FieldDescription ddlFieldDescription = new ArcGIS.Core.Data.DDL.FieldDescription(fieldName, fieldSchema.Type switch
                {
                  "String" => ArcGIS.Core.Data.FieldType.String,
                  "Integer" => ArcGIS.Core.Data.FieldType.Integer,
                  "Double" => ArcGIS.Core.Data.FieldType.Double,
                  "Date" => ArcGIS.Core.Data.FieldType.Date,
                  "OID" => ArcGIS.Core.Data.FieldType.OID,
                  "esriFieldTypeOID" => ArcGIS.Core.Data.FieldType.OID,
                  "esriFieldTypeGlobalID" => ArcGIS.Core.Data.FieldType.GlobalID,
                  "esriFieldTypeString" => ArcGIS.Core.Data.FieldType.String,
                  "esriFieldTypeDouble" => ArcGIS.Core.Data.FieldType.Double,
                  "esriFieldTypeInteger" => ArcGIS.Core.Data.FieldType.Integer,
                  "esriFieldTypeDate" => ArcGIS.Core.Data.FieldType.Date,
                  "esriFieldTypeSmallInteger" => ArcGIS.Core.Data.FieldType.SmallInteger,
                  "esriFieldTypeSingle" => ArcGIS.Core.Data.FieldType.Single,
                  "esriFieldTypeGUID" => ArcGIS.Core.Data.FieldType.GUID,
                  _ => throw new InvalidOperationException($"Unsupported field type: {fieldSchema.Type}")
                })
                {
                  AliasName = fieldSchema.AliasName ?? fieldSchema.Name, // Use field name if alias is not provided
                  IsNullable = fieldSchema.IsNullable,
                  Length = fieldSchema.Length > 0 ? fieldSchema.Length : (fieldSchema.Type == "String" ? 255 : 0), // Default length for String if not specified
                  Precision = fieldSchema.Type == "Double" ? 15 : 0, // Default precision for Double 
                  Scale = fieldSchema.Type == "Double" ? 5 : 0 // Default scale for Double
                };

                fieldDescriptions.Add(ddlFieldDescription);
              }
            }
            string featureClassName = featureClass.Name;
            // "Sanitize" the feature class name 
            if (featureClass.Name.Contains('.'))
              featureClassName = featureClass.Name.Substring(featureClass.Name.LastIndexOf('.') + 1);

            var fcDescription = new FeatureClassDescription(featureClassName, fieldDescriptions, shapeDescription);

            // Create a SchemaBuilder object
            SchemaBuilder schemaBuilder = new SchemaBuilder(sdeConnection);

            // Add the creation of the new feature class to our list of DDL tasks
            schemaBuilder.Create(fcDescription);

            // Execute the DDL
            try
            {
              success = schemaBuilder.Build();
            }
            catch (Exception ex)
            {
              Console.WriteLine($"Error applying schema changes: {ex.Message}");
              //throw;
            }

            if (success)
              Console.WriteLine($@"Feature Class '{featureClass.Name}' created successfully in datastore '{sdeConnectionFile}'.");
            else
              Console.WriteLine($@"Feature Class '{featureClass.Name}' creation failed in datastore '{sdeConnectionFile}'. Check the log for details.");
          }
        }
      }
      //catch (ArcGIS.Core.Data.Exceptions.GeodatabaseEditingException eex)
      //{
      //  Console.WriteLine($"An edit session is in progress. Please stop the edit session and try again.\n {eex}");
      //}
      catch (Exception ex)
      {
        Console.WriteLine($"Error connecting to database: {ex.Message}");
        return 1;
      }

      return success ? 0 : 1; // Return 0 for success, 1 for failure
    }
  }
}
