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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DDLCreateFromSchema
{
/// <summary>
/// Represents the schema of a workspace, including its feature classes.
/// </summary>
/// <remarks>
/// A workspace schema defines the structure and organization of feature classes within a workspace. This
/// class provides access to the collection of feature class schemas.
/// </remarks>
  public class WorkspaceSchema
  {
    public List<FeatureClassSchema> FeatureClasses { get; set; } = new();
  }

  /// <summary>
  /// Represents the schema definition for a feature class, including its name, dataset type, and associated fields.
  /// </summary>
  /// <remarks>
  /// A feature class schema defines the structure of a feature class, which typically includes
  /// metadata such as the name, the type of dataset it belongs to, and a collection of fields describing the attributes
  /// of the feature class.
  /// </remarks>
  public class FeatureClassSchema
  {
    public string Name { get; set; }
    public string DatasetType { get; set; }
    public List<FieldSchema> Fields { get; set; } = new();
  }

  /// <summary>
  /// Represents the schema definition for a field in a data structure, including its name, type, and other attributes.
  /// </summary>
  /// <remarks>
  /// This class provides metadata about a field, such as its data type, whether it allows null values,
  /// its length, and optional alias name. It can also include geometry-specific information if applicable.
  /// </remarks>
  public class FieldSchema
  {
    public string Name { get; set; }
    public string Type { get; set; }
    public bool IsNullable { get; set; }
    public int Length { get; set; }
    public string AliasName { get; set; }
    public GeometryDefSchema GeometryDef { get; set; }
  }

  /// <summary>
  /// Represents the schema definition for a geometry, including its type, dimensional properties, and spatial
  /// reference.
  /// </summary>
  /// <remarks>
  /// This class provides information about the geometry's type, whether it includes M and Z values, 
  /// and its spatial reference system in Well-Known Text (WKT) format.
  /// </remarks>
  public class GeometryDefSchema
  {
    public string GeometryType { get; set; }
    public bool HasM { get; set; }
    public bool HasZ { get; set; }
    public string SpatialReferenceWkt { get; set; }
  }

  /// <summary>
  /// Parses an ArcGIS XML schema file and extracts workspace schema information.
  /// </summary>
  /// <remarks>
  /// This method reads an XML file containing ArcGIS schema definitions and constructs a  <see
  /// cref="WorkspaceSchema"/> object representing the workspace, including feature classes,  fields, and geometry
  /// definitions. The XML file must conform to the ArcGIS schema format as created by the Pro Export XML Workspace Document Geoprocessing tool 
  /// with Export Options "Schema only"
  /// </remarks>
  public class ArcGisSchemaParser
  {

    /// <summary>
    /// This method reads an XML file containing ArcGIS schema definitions and constructs a  <see
    /// cref="WorkspaceSchema"/> object representing the workspace, including feature classes,  
    /// fields, and geometry definitions. 
    /// </summary>
    /// <param name="xmlPath"></param>
    /// <returns></returns>
    public WorkspaceSchema Parse(string xmlPath)
    {
      XNamespace esri = ""; // "http://www.esri.com/schemas/ArcGIS/10.8";
      var doc = XDocument.Load(xmlPath);

      var workspace = new WorkspaceSchema();

      var datasetDefs = doc.Descendants(esri + "DatasetDefinitions")
          .Elements(esri + "DataElement")
          .Where(e => (string)e.Attribute(XName.Get("type", "http://www.w3.org/2001/XMLSchema-instance")) == "esri:DEFeatureClass");

      foreach (var dataElement in datasetDefs)
      {
        var fc = new FeatureClassSchema
        {
          Name = (string)dataElement.Element(esri + "Name"),
          DatasetType = (string)dataElement.Element(esri + "DatasetType")
        };

        var fields = dataElement
            .Element(esri + "Fields")?
            .Element(esri + "FieldArray")?
            .Elements(esri + "Field");

        if (fields != null)
        {
          foreach (var field in fields)
          {
            var fieldSchema = new FieldSchema
            {
              Name = (string)field.Element(esri + "Name"),
              Type = (string)field.Element(esri + "Type"),
              IsNullable = (string)field.Element(esri + "IsNullable") == "true",
              Length = (int?)field.Element(esri + "Length") ?? 0,
              AliasName = (string)field.Element(esri + "AliasName")
            };

            var geomDef = field.Element(esri + "GeometryDef");
            if (geomDef != null)
            {
              var spatialRef = geomDef.Element(esri + "SpatialReference");
              fieldSchema.GeometryDef = new GeometryDefSchema
              {
                GeometryType = (string)geomDef.Element(esri + "GeometryType"),
                HasM = (string)geomDef.Element(esri + "HasM") == "true",
                HasZ = (string)geomDef.Element(esri + "HasZ") == "true",
                SpatialReferenceWkt = (string)spatialRef?.Element(esri + "WKT")
              };
            }

            fc.Fields.Add(fieldSchema);
          }
        }

        workspace.FeatureClasses.Add(fc);
      }

      return workspace;
    }
  }
}
