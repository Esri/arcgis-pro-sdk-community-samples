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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using ActiproSoftware.Windows.Extensions;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using Index = ArcGIS.Core.Data.Index;

namespace DatastoresDefinitionsAndDatasets
{
	internal class DockpaneViewModel : DockPane
	{
		#region Private members

		private const string _dockPaneID = "DatastoresDefinitionsAndDatasets_Dockpane";

		private Datastore _datastore;
		private readonly object _lockCollection = new object();
		private DatasetInfo _selectedDatasetInfo;
		private string _dataPath;
		private DatasetTypeCategory _datasetTypeCategory;
		private DatastoreCategory _datastoreCategory;
		private Visibility _cmdDataPathVisible;
		private string _cmdDataPathContent;
		private ICommand _cmdDataPath;
		private ICommand _cmdLoadData;

		private string _heading = "Datastore Datasets and Definitions";

		#endregion Private members

		private static bool IsOnUiThread => ArcGIS.Desktop.Framework.FrameworkApplication.TestMode || System.Windows.Application.Current.Dispatcher.CheckAccess();
		
		protected DockpaneViewModel()
		{
			Datasets = new ObservableCollection<DatasetInfo>();
			DatasetTypeCategories = new ObservableCollection<DatasetTypeCategory>();
			DefinitionDetails = new ObservableCollection<string>();
			RunOnUiThread(() =>
			{
				BindingOperations.EnableCollectionSynchronization(Datasets, _lockCollection);
				BindingOperations.EnableCollectionSynchronization(DatasetTypeCategories, _lockCollection);
				BindingOperations.EnableCollectionSynchronization(DefinitionDetails, _lockCollection);
			});
		}

		public List<DatastoreCategory> DatastoreCategories => DatastoreCategory.AllDatastoreCategories;

		public DatastoreCategory DatastoreCategory
		{
			get { return _datastoreCategory; }
			set
			{
				SetProperty(ref _datastoreCategory, value, () => DatastoreCategory);
				if (_datastoreCategory != null)
				{
					CmdDataPathVisible = string.IsNullOrEmpty(_datastoreCategory.PathCaption) ?
																Visibility.Hidden : Visibility.Visible;
					CmdDataPathContent = _datastoreCategory.PathCaption;
				}
				else CmdDataPathVisible = Visibility.Hidden;				
			}
		}

		public string DataPath
		{
			get { return _dataPath; }
			set
			{
				SetProperty(ref _dataPath, value, () => DataPath);
			}
		}
		
		public Visibility CmdDataPathVisible
		{
			get {	return _cmdDataPathVisible;	}
			set
			{
				SetProperty(ref _cmdDataPathVisible, value, () => CmdDataPathVisible);
			}
		}

		public string CmdDataPathContent
		{
			get { return _cmdDataPathContent; }
			set
			{
				SetProperty(ref _cmdDataPathContent, value, () => CmdDataPathContent);
			}
		}

		public ICommand CmdDataPath
		{
			get
			{
				return _cmdDataPath ?? (_cmdDataPath = new RelayCommand(() =>
						{
							OpenItemDialog openDialog = new OpenItemDialog()
							{
								Title = DatastoreCategory.OpenDlgTitle,
								MultiSelect = false,
								Filter = DatastoreCategory.OpenDlgFilter,
								InitialLocation = string.IsNullOrEmpty(DataPath) ? @"c:\data" : DataPath
							};
							if (openDialog.ShowDialog() == true)
							{
								foreach (Item item in openDialog.Items)
								{
									var errValidation = DatastoreCategory.ValidateDataPath(item.Path);
									if (!string.IsNullOrEmpty(errValidation)) MessageBox.Show(errValidation, "Error");
									else DataPath = item.Path;
									break;
								}
							}
						}, () => !string.IsNullOrEmpty(CmdDataPathContent)));
			}
		}

		public ICommand CmdLoadData
		{
			get
			{
				return _cmdLoadData ?? (_cmdLoadData = new RelayCommand(async () =>
						{
							try
							{
								Uri path = new Uri(DataPath);
								// clear old information
								Datasets.Clear();
								DatasetTypeCategories.Clear();
								DefinitionDetails.Clear();
								if (_datastore != null)
									_datastore.Dispose();
								_datastore = await DatastoreCategory.OpenDatastore(path, DatasetTypeCategories);
							}
							catch (Exception exObj)
							{
								MessageBox.Show($@"Unable to create a Datastore for {DataPath}: {exObj.Message}", 
									"Error");
							}
						}, () => !string.IsNullOrEmpty(DataPath)));
			}
		}

		public ObservableCollection<DatasetTypeCategory> DatasetTypeCategories { get; set; }

		public DatasetTypeCategory DatasetTypeCategory
		{
			get
			{ return _datasetTypeCategory; }
			set
			{
				SetProperty(ref _datasetTypeCategory, value, () => DatasetTypeCategory);
				if (_datasetTypeCategory == null) return;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				SetupDefinitionAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			}
		}

		public ObservableCollection<DatasetInfo> Datasets { get; set; }

		public ObservableCollection<string> DefinitionDetails { get; set; }

		public DatasetInfo Dataset
		{
			get { return _selectedDatasetInfo; }
			set
			{
				SetProperty(ref _selectedDatasetInfo, value, () => Dataset);
				if (_selectedDatasetInfo == null) return;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				SetupDefinitionDetailsAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			}
		}

		private async Task SetupDefinitionAsync()
		{
			try
			{
				var lstDefinitions = await QueuedTask.Run<List<DatasetInfo>>(() =>
				{
					List<DatasetInfo> definitions = new List<DatasetInfo>();
					if (_datastore is Geodatabase)
					{
						var geodatabase = _datastore as Geodatabase;
						switch (DatasetTypeCategory.DatasetType)
						{
							case DatasetType.Table:
								definitions = geodatabase.GetDefinitions<TableDefinition>().Select(CreateDataSetInfo).ToList();
								break;
							case DatasetType.FeatureClass:
								definitions = geodatabase.GetDefinitions<FeatureClassDefinition>().Select(CreateDataSetInfo).ToList();
								break;
							case DatasetType.FeatureDataset:
								definitions = geodatabase.GetDefinitions<FeatureDatasetDefinition>().Select(CreateDataSetInfo).ToList();
								break;
							case DatasetType.RelationshipClass:
								definitions = geodatabase.GetDefinitions<RelationshipClassDefinition>().Select(CreateDataSetInfo).ToList();
								break;
							case DatasetType.AttributedRelationshipClass:
								definitions = geodatabase.GetDefinitions<AttributedRelationshipClassDefinition>().Select(CreateDataSetInfo).ToList();
								break;
						}
					}
					else if (_datastore is Database)
					{
						var database = _datastore as Database;
						IReadOnlyList<string> tableNames = database.GetTableNames();
						foreach (string tableName in tableNames)
						{
							QueryDescription queryDescription = database.GetQueryDescription(tableName);
							TableDefinition tableDefinition = database.GetDefinition(queryDescription);
							if (DatasetTypeCategory.DatasetType == DatasetType.Table || DatasetTypeCategory.DatasetType == DatasetType.FeatureClass)
							{
								definitions.Add(new DatasetInfo
								{
									Name = tableDefinition.GetName(),
									DatasetDefinition = tableDefinition
								});
							}
						}
					}
					else if (_datastore is FileSystemDatastore)
					{
						var shapefile = _datastore as FileSystemDatastore;
						FileSystemConnectionPath shapefileConnectionPath = (FileSystemConnectionPath)shapefile.GetConnector();
						DirectoryInfo directoryInfo = new DirectoryInfo(shapefileConnectionPath.Path.LocalPath);

						if (DatasetTypeCategory.DatasetType == DatasetType.FeatureClass)
						{
							FileInfo[] filesWithShpExtension = directoryInfo.GetFiles("*.shp");

							foreach (FileInfo file in filesWithShpExtension)
							{
								definitions.Add(CreateDataSetInfo(shapefile.GetDefinition<FeatureClassDefinition>(file.Name)));
							}
						}
						if (DatasetTypeCategory.DatasetType == DatasetType.Table)
						{
							FileInfo[] filesWithDbfExtension = directoryInfo.GetFiles("*.dbf");

							foreach (FileInfo file in filesWithDbfExtension)
							{
								definitions.Add(CreateDataSetInfo(shapefile.GetDefinition<TableDefinition>(file.Name)));
							}
						}
					}
					return definitions;
				});
				Datasets.Clear();
				Datasets.AddRange(lstDefinitions);
				DefinitionDetails.Clear();
			}
			catch (Exception exObj)
			{
				MessageBox.Show(exObj.Message, "Error");
			}
		}

		private DatasetInfo CreateDataSetInfo(Definition definition)
		{
			return new DatasetInfo
			{
				Name = definition.GetName(),
				DatasetDefinition = definition
			};
		}

		private async Task SetupDefinitionDetailsAsync()
		{
			DefinitionDetails.Clear();
			try
			{
				var lstDefs = await QueuedTask.Run<List<string>>(() =>
				{
					Definition datasetDefinition = Dataset.DatasetDefinition;
					List<string> lstDefDetails = new List<string>();
					if (datasetDefinition is TableDefinition)
					{
						TableDefinition tableDefinition = datasetDefinition as TableDefinition;
						lstDefDetails.Add($"Object ID Field: {tableDefinition.GetObjectIDField()}");
						StringBuilder stringBuilder = new StringBuilder();

						if (!(_datastore is FileSystemDatastore))
						{
							lstDefDetails.Add($"Alias Name: {tableDefinition.GetAliasName()}");
							lstDefDetails.Add($"CreatedAt Field: {tableDefinition.GetCreatedAtField()}");
							lstDefDetails.Add($"Creator Field: {tableDefinition.GetCreatorField()}");
							lstDefDetails.Add($"Subtype Field: {tableDefinition.GetSubtypeField()}");
							lstDefDetails.Add($"Default Subtype Code: {tableDefinition.GetDefaultSubtypeCode()}");
							lstDefDetails.Add($"EditedAt Field: {tableDefinition.GetEditedAtField()}");
							lstDefDetails.Add($"Editor Field: {tableDefinition.GetEditorField()}");
							lstDefDetails.Add($"Global ID Field: {tableDefinition.GetGlobalIDField()}");
							lstDefDetails.Add($"Model Name: {tableDefinition.GetModelName()}");
							foreach (var subtype in tableDefinition.GetSubtypes())
							{
								stringBuilder.Append(subtype.GetCode()).Append(": ").Append(subtype.GetName()).Append(Environment.NewLine);
							}
							lstDefDetails.Add($"Subtypes: {stringBuilder}");
						}
						stringBuilder = new StringBuilder();
						foreach (Index index in tableDefinition.GetIndexes())
						{
							stringBuilder.Append(index.GetName()).Append(",");
							string order = index.IsAscending() ? "Ascending" : "Descending";
							stringBuilder.Append(order).Append(", ");
							string unique = index.IsUnique() ? "Unique" : "Not Unique";
							stringBuilder.Append(unique);
						}
						lstDefDetails.Add($"Indexes: {stringBuilder}");
					}

					if (datasetDefinition is FeatureClassDefinition)
					{
						FeatureClassDefinition featureClassDefinition = datasetDefinition as FeatureClassDefinition;
						if (!(_datastore is FileSystemDatastore))
						{
							lstDefDetails.Add($"Area Field: {featureClassDefinition.GetAreaField()}");
							lstDefDetails.Add($"Length Field: {featureClassDefinition.GetLengthField()}");
						}
						lstDefDetails.Add($"Shape Field: {featureClassDefinition.GetShapeField()}");
						lstDefDetails.Add($"Shape Type: {featureClassDefinition.GetShapeType()}");
						lstDefDetails.Add($"Spatial Reference Name: {featureClassDefinition.GetSpatialReference().Name}");
						Envelope extent = featureClassDefinition.GetExtent();
						lstDefDetails.Add($"Extent Details: XMin-{extent.XMin} XMax-{extent.XMax} YMin-{extent.YMin} YMax-{extent.YMax}");
					}

					if (datasetDefinition is FeatureDatasetDefinition)
					{
						FeatureDatasetDefinition featureDatasetDefinition = datasetDefinition as FeatureDatasetDefinition;
						lstDefDetails.Add($"Spatial Reference Name: {featureDatasetDefinition.GetSpatialReference().Name}");
						try
						{
							Envelope extent = featureDatasetDefinition.GetExtent();
							lstDefDetails.Add($"Extent Details: XMin-{extent.XMin} XMax-{extent.XMax} YMin-{extent.YMin} YMax-{extent.YMax}");
						}
						catch (Exception)
						{
							lstDefDetails.Add("Could not get extent");
						}
					}

					if (datasetDefinition is RelationshipClassDefinition)
					{
						RelationshipClassDefinition relationshipClassDefinition = datasetDefinition as RelationshipClassDefinition;
						lstDefDetails.Add($"Alias Name: {relationshipClassDefinition.GetAliasName()}");
						lstDefDetails.Add($"Cardinality: {relationshipClassDefinition.GetCardinality()}");
						lstDefDetails.Add($"Origin Class: {relationshipClassDefinition.GetOriginClass()}");
						lstDefDetails.Add($"Destination Class: {relationshipClassDefinition.GetDestinationClass()}");
						lstDefDetails.Add($"Origin Primary Key: {relationshipClassDefinition.GetOriginKeyField()}");
						lstDefDetails.Add($"Origin Foreign Key: {relationshipClassDefinition.GetOriginForeignKeyField()}");
						lstDefDetails.Add($"Is Attachement?: {relationshipClassDefinition.IsAttachmentRelationship()}");
						lstDefDetails.Add($"Is Composite Relationship?: {relationshipClassDefinition.IsComposite()}");
					}

					if (datasetDefinition is AttributedRelationshipClassDefinition)
					{
						AttributedRelationshipClassDefinition relationshipClassDefinition = datasetDefinition as AttributedRelationshipClassDefinition;
						lstDefDetails.Add($"Destination Key: {relationshipClassDefinition.GetDestinationKeyField()}");
						lstDefDetails.Add($"Destination Foreign Key: {relationshipClassDefinition.GetDestinationForeignKeyField()}");
						lstDefDetails.Add($"Object ID Field: {relationshipClassDefinition.GetObjectIDField()}");
					}
					return lstDefDetails;
				});
				DefinitionDetails.AddRange(lstDefs);
			}
			catch (Exception exObj)
			{
				MessageBox.Show(exObj.Message, "Error");
			}
		}

		/// <summary>
		/// Show the DockPane.
		/// </summary>
		internal static void Show()
		{
			DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
			if (pane == null)
				return;

			pane.Activate();
		}

		/// <summary>
		/// Text shown near the top of the DockPane.
		/// </summary>
		public string Heading
		{
			get { return _heading; }
			set
			{
				SetProperty(ref _heading, value, () => Heading);
			}
		}

		internal static void RunOnUiThread(Action action)
		{
			try
			{
				if (IsOnUiThread)
					action();
				else
					Application.Current.Dispatcher.Invoke(action);
			}
			catch (Exception ex)
			{
				MessageBox.Show($@"Error in OpenAndActivateMap: {ex.Message}");
			}
		}
	}

	/// <summary>
	/// Button implementation to show the DockPane.
	/// </summary>
	internal class Dockpane_ShowButton : Button
	{
		protected override void OnClick()
		{
			DockpaneViewModel.Show();
		}
	}

}
