/*

   Copyright 2019 Esri

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
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatastoresDefinitionsAndDatasets
{
	/// <summary>
	/// Encapsulates Datastore Types supported by this sample
	/// </summary>
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
			string openDlgFilter, string openDlgTitle)
		{
			Name = name;
			PathCaption = pathCaption;
			Type = type;
			OpenDlgFilter = openDlgFilter;
			OpenDlgTitle = openDlgTitle;
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
		/// <param name="path"></param>
		/// <param name="dataTypes"></param>
		/// <returns></returns>
		public Task<Datastore> OpenDatastore (Uri path, ObservableCollection<DatasetTypeCategory> dataTypes)
		{
			Task<Datastore> datastore = null;
			switch (Type)
			{
				case EnumDatastoreType.FileGDB:
					PopulateFileEnterpriseDatasetTypes(dataTypes);
					datastore = QueuedTask.Run<Datastore>(() =>
					{
						return new Geodatabase(new FileGeodatabaseConnectionPath(path));
					});
					break;
				case EnumDatastoreType.EnterpriseGDB:
					PopulateFileEnterpriseDatasetTypes(dataTypes);
					datastore = QueuedTask.Run<Datastore>(() =>
					{
						return new Geodatabase(new DatabaseConnectionFile(path));
					});
					break;
				case EnumDatastoreType.WebGDB:
					PopulateServiceGeodatabaseDatabaseShapeFileDataTypes(dataTypes);
					datastore = QueuedTask.Run<Datastore>(() =>
					{
						return new Geodatabase(new ServiceConnectionProperties(path));
					});
					break;
				//case EnumDatastoreType.EnterpriseDB:
				//	PopulateServiceGeodatabaseDatabaseShapeFileDataTypes(dataTypes);
				//	datastore = QueuedTask.Run<Datastore>(() =>
				//	{
				//		return new Database(new DatabaseConnectionFile(path));
				//	});
				//	break;
				case EnumDatastoreType.SqliteDB:
					PopulateServiceGeodatabaseDatabaseShapeFileDataTypes(dataTypes);
					datastore = QueuedTask.Run<Datastore>(() =>
					{
						return new Database(new SQLiteConnectionPath(path));
					});
					break;
				case EnumDatastoreType.ShapeFile:
					PopulateServiceGeodatabaseDatabaseShapeFileDataTypes(dataTypes);
					datastore = QueuedTask.Run<Datastore>(() =>
					{
						return new FileSystemDatastore(new FileSystemConnectionPath(path, FileSystemDatastoreType.Shapefile));
					});
					break;
			}
			return datastore;
		}

		/// <summary>
		/// called to validate if the DataPath works for the given type of datastore
		/// </summary>
		/// <param name="DataPath"></param>
		/// <returns>Validation Error message or null if ok</returns>
		public string ValidateDataPath (string DataPath)
		{
			var ext = System.IO.Path.GetExtension(DataPath).ToLower();
			switch (Type)
			{
				case EnumDatastoreType.FileGDB:
					if (ext != ".gdb"
						  || System.IO.Directory.Exists (DataPath) == false)
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
				case EnumDatastoreType.WebGDB:
					if (!DataPath.ToLower().StartsWith ("http"))
					{
						return $@"The URL: {DataPath} is not a valid.  Please specify the ArcGIS Server URL for your feature service.";
					}
					break;
				//case EnumDatastoreType.EnterpriseDB:
				//	break;
				case EnumDatastoreType.SqliteDB:
					if (!ext.Contains(".sqlite")
							|| System.IO.Directory.Exists(DataPath) == false)
					{
						return $@"The path: {DataPath} is not a valid sqlite database path.  "".Sqlite"" in the folder path name is expected.";
					}
					break;
				case EnumDatastoreType.ShapeFile:
					if (System.IO.Directory.Exists(DataPath) == false)
					{
						return $@"The path: {DataPath} is not a Folder that contains shape files.";
					}
					break;
			}
			return null;
		}
		#region Static Members

		private static List<DatastoreCategory> LstDatastoreTypes = new List<DatastoreCategory>();

		/// <summary>
		/// List of all Datastore Types supported 
		/// </summary>
		public static List<DatastoreCategory> AllDatastoreCategories
		{
			get
			{
				if (LstDatastoreTypes.Count == 0)
				{
					LstDatastoreTypes.Add(new DatastoreCategory(EnumDatastoreType.FileGDB,  "File Geodatabase", "GDB Path", ItemFilters.Geodatabases, "Select a File GeoDatabase"));
					LstDatastoreTypes.Add(new DatastoreCategory(EnumDatastoreType.EnterpriseGDB, "Enterprise Geodatabase", "Enterprise GDB", ItemFilters.Databases, "Select a database connection file"));
					LstDatastoreTypes.Add(new DatastoreCategory(EnumDatastoreType.WebGDB, "Feature Service", "", ItemFilters.Folders, "Select a folder"));
					//LstDatastoreTypes.Add(new DatastoreCategory(EnumDatastoreType.EnterpriseDB, "Enterprise Database", "Enterprise DB", ItemFilters.Folders, "Select a folder"));
					LstDatastoreTypes.Add(new DatastoreCategory(EnumDatastoreType.SqliteDB, "Sqlite Database", "SQL Lite DB", ItemFilters.Folders, "Select a folder"));
					LstDatastoreTypes.Add(new DatastoreCategory(EnumDatastoreType.ShapeFile, "Shape File", "Shape Folder", ItemFilters.Folders, "Select a folder"));
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
