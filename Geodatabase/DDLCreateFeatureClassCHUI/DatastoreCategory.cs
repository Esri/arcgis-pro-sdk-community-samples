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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Encapsulates Datastore Types supported by this sample
/// </summary>
namespace DDLCreateFeatureClassCHUI
{
  
  /// <summary>
  /// Represents a category of datastore, including its type, name, and associated metadata.
  /// </summary>
  /// <remarks>
  /// A datastore category defines the characteristics and behavior of a specific type of datastore, 
  /// such as its name, type, and configuration details for interacting with it. This class is used  to encapsulate
  /// information about supported datastore types and provides methods for validating  paths and opening
  /// datastores.</remarks>
  public class DatastoreCategory
  {
    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <param name="pathCaption"></param>
    /// <param name="openDlgFilter"></param>
    /// <param name="openDlgTitle"></param>
    public DatastoreCategory(EnumDatastoreType type,
      string name, string pathCaption,
      string openDlgTitle)
    {
      Name = name;
      PathCaption = pathCaption;
      Type = type;
      OpenDlgTitle = openDlgTitle;
      OpenDlgFilter = type switch
      {
        EnumDatastoreType.FileGDB => "File Geodatabase (*.gdb)|*.gdb",
        EnumDatastoreType.EnterpriseGDB => "SDE Connection File (*.sde)|*.sde",
        _ => throw new NotSupportedException($"Unsupported datastore type: {type}")
      };
    }

    /// <summary>
    /// datastore category name
    /// </summary>
    public string Name { get; internal set; }
    /// <summary>
    /// Caption to be shown on button to query for data path entry
    /// </summary>
    public string PathCaption { get; internal set; }
    /// <summary>
    /// Type of datastore
    /// </summary>
    public EnumDatastoreType Type { get; internal set; }
    /// <summary>
    /// Filter used for openItem dialog
    /// </summary>
    public string OpenDlgFilter { get; internal set; }
    /// <summary>
    /// Title used for openItem dialog
    /// </summary>
    public string OpenDlgTitle { get; internal set; }

    /// <summary>
    /// Method opens a 'datastore' using the given path, then Datatypes is populated with options for the given store
    /// </summary>
    /// <remarks>
    /// Focused on FileGDB and Enterprise GDB for now, but could be extended to support other datastore types.
    /// </remarks>
    /// <param name="path"></param>
    /// <returns></returns>
    public Geodatabase OpenDatastore(Uri path) //, ObservableCollection<DatasetTypeCategory> dataTypes)
    {
      switch (Type)
      {
        case EnumDatastoreType.FileGDB:
          //PopulateFileEnterpriseDatasetTypes(dataTypes);
          return new Geodatabase(new FileGeodatabaseConnectionPath(path));
        case EnumDatastoreType.EnterpriseGDB:
          //PopulateFileEnterpriseDatasetTypes(dataTypes);
          return new Geodatabase(new DatabaseConnectionFile(path));
      }
      return null; // Ensuring there is a default return
    }

    /// <summary>
    /// called to validate if the DataPath works for the given type of datastore
    /// </summary>
    /// <param name="DataPath"></param>
    /// <returns>Validation Error message or null if ok</returns>
    public string ValidateDataPath(string DataPath)
    {
      var ext = System.IO.Path.GetExtension(DataPath).ToLower();
      switch (Type)
      {
        case EnumDatastoreType.FileGDB:
          if (ext != ".gdb"
              || System.IO.Directory.Exists(DataPath) == false)
          {
            return $@"The path: {DataPath} is not a valid File Geodatabase path";
          }
          break;
        case EnumDatastoreType.EnterpriseGDB:
          if (ext != ".sde")
          {
            return $@"The selection: {DataPath} is not a valid .SDE connection file";
          }
          break;
      }
      return null;
    }
    #region Static Members

    private static readonly List<DatastoreCategory> LstDatastoreTypes = [];

    /// <summary>
    /// List of all Datastore Types supported 
    /// </summary>
    public static List<DatastoreCategory> AllDatastoreCategories
    {
      get
      {
        if (LstDatastoreTypes.Count == 0)
        {
          LstDatastoreTypes.Add(new DatastoreCategory(EnumDatastoreType.FileGDB, "File Geodatabase", "File GDB Path", "Select a File GeoDatabase"));
          LstDatastoreTypes.Add(new DatastoreCategory(EnumDatastoreType.EnterpriseGDB, "Enterprise Geodatabase", "Enterprise GDB", "Select a database connection file"));
        }
        return LstDatastoreTypes;
      }
    }

    /// <summary>
    /// Called to update Observable collection of DataType Categories
    /// </summary>
    /// <param name="dataTypeCategories"></param>
    public static void PopulateServiceGeodatabaseDatabaseShapeFileDataTypes(ObservableCollection<DatasetTypeCategory> dataTypeCategories)
    {
      dataTypeCategories.Clear();
      dataTypeCategories.Add(new DatasetTypeCategory("Table", DatasetType.Table));
      dataTypeCategories.Add(new DatasetTypeCategory("Feature Class", DatasetType.FeatureClass));
    }

    /// <summary>
    /// Called to update Observable collection of DataType Categories
    /// </summary>
    /// <param name="dataTypeCategories"></param>
    public static void PopulateFileEnterpriseDatasetTypes(ObservableCollection<DatasetTypeCategory> dataTypeCategories)
    {
      dataTypeCategories.Clear();
      dataTypeCategories.Add(new DatasetTypeCategory("Table", DatasetType.Table));
      dataTypeCategories.Add(new DatasetTypeCategory("Feature Class", DatasetType.FeatureClass));
      dataTypeCategories.Add(new DatasetTypeCategory("Feature Dataset", DatasetType.FeatureDataset));
      dataTypeCategories.Add(new DatasetTypeCategory("Relationship Class", DatasetType.RelationshipClass));
      dataTypeCategories.Add(new DatasetTypeCategory("Attributed Relationship Class", DatasetType.AttributedRelationshipClass));
    }

    #endregion Static Members

  }
}