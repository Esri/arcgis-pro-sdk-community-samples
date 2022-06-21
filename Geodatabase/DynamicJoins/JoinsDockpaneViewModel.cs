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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using ActiproSoftware.Windows.Extensions;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Events;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Core;
using ArcGIS.Desktop.Mapping;
using Newtonsoft.Json.Linq;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;

namespace DynamicJoins
{
  internal class JoinsDockpaneViewModel : DockPane
  {
		#region Private members

		private const string _dockPaneID = "DynamicJoins_JoinsDockpane";
		private string _heading = "Joins DockPane";

		private ICommand _cmdGenerateJoin;
		private ICommand _cmdClearJoin;

		private Field _leftField;
		private Field _rightField;

		private GDBProjectItem _selectedLeftGDBProjectItem;
		private GDBProjectItem _selectedRightGDBProjectItem;

		private TableInfo _selectedLeftTable;
		private TableInfo _selectedRightTable;

		private Visibility _visibleRelationship = Visibility.Hidden;

		private readonly object _lockCollection = new object();

    private readonly ObservableCollection<GDBProjectItem> _leftGdbProjectItems = new ObservableCollection<GDBProjectItem>();
    private readonly ObservableCollection<TableInfo> _leftTables = new ObservableCollection<TableInfo>();
    private readonly ObservableCollection<FieldListItem> _leftFields = new ObservableCollection<FieldListItem>();
    private readonly ObservableCollection<GDBProjectItem> _rightGdbProjectItems = new ObservableCollection<GDBProjectItem>();
    private readonly ObservableCollection<TableInfo> _rightTables = new ObservableCollection<TableInfo>();
    private readonly ObservableCollection<FieldListItem> _rightFields = new ObservableCollection<FieldListItem>();

		#endregion Private members

		private static bool IsOnUiThread => ArcGIS.Desktop.Framework.FrameworkApplication.TestMode || System.Windows.Application.Current.Dispatcher.CheckAccess();

		protected JoinsDockpaneViewModel()
		{
			BindingOperations.EnableCollectionSynchronization(_leftGdbProjectItems, _lockCollection);
			BindingOperations.EnableCollectionSynchronization(_leftTables, _lockCollection);
			BindingOperations.EnableCollectionSynchronization(_leftFields, _lockCollection);
			BindingOperations.EnableCollectionSynchronization(_rightGdbProjectItems, _lockCollection);
			BindingOperations.EnableCollectionSynchronization(_rightTables, _lockCollection);
			BindingOperations.EnableCollectionSynchronization(_rightFields, _lockCollection);
			ProjectItemsChangedEvent.Subscribe(OnProjectCollectionChanged, false);
			RefreshDatabaseItems();
		}

		public ObservableCollection<GDBProjectItem> LeftDataItems => _leftGdbProjectItems;
		public ObservableCollection<GDBProjectItem> RightDataItems => _rightGdbProjectItems;

		public GDBProjectItem SelectedLeftGDBProjectItem
		{
			get {	return _selectedLeftGDBProjectItem;	}
			set
			{
				SetProperty(ref _selectedLeftGDBProjectItem, value, () => SelectedLeftGDBProjectItem);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				UpdateTables(SelectedLeftGDBProjectItem, _leftTables);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			}
		}

		public GDBProjectItem SelectedRightGDBProjectItem
		{
			get { return _selectedRightGDBProjectItem; }
			set
			{
				SetProperty(ref _selectedRightGDBProjectItem, value, () => SelectedRightGDBProjectItem);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				UpdateTables(SelectedRightGDBProjectItem, _rightTables);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			}
		}

		public ObservableCollection<TableInfo> LeftTables => _leftTables;
		public ObservableCollection<TableInfo> RightTables => _rightTables;

		public TableInfo SelectedLeftTable
		{
			get { return _selectedLeftTable; }
			set
			{
				SetProperty(ref _selectedLeftTable, value, () => SelectedLeftTable);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				UpdateFields(SelectedLeftTable, _leftFields);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			}
		}

    public TableInfo SelectedRightTable
		{
			get { return _selectedRightTable; }
			set
			{
				SetProperty(ref _selectedRightTable, value, () => SelectedRightTable);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				UpdateFields(SelectedRightTable, _rightFields);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			}
		}

		public ObservableCollection<FieldListItem> LeftFields => _leftFields;
		public ObservableCollection<FieldListItem> RightFields => _rightFields;

		public FieldListItem SelectedLeftField
		{
			set
			{
				LeftField = value?.Field?.Name;
			}
		}

		public FieldListItem SelectedRightField
		{
			set
			{
				RightField = value?.Field?.Name;
			}
		}

		public Visibility VisibleRelationship
		{
			get { return _visibleRelationship; }
			set
			{
				SetProperty(ref _visibleRelationship, value, () => VisibleRelationship);
			}
		}

		public ICommand CmdClearJoin
		{
			get
			{
				return _cmdClearJoin ?? (_cmdClearJoin = new RelayCommand(() =>
								{
									LeftField = null;
									RightField = null;
								}, () => !string.IsNullOrEmpty(LeftField)
											|| !string.IsNullOrEmpty(RightField)));
			}
		}

		public String LayerName { get; set; }

		public bool IsLeftOuterJoin { get; set; }

    public string LeftField
    {
      get { return _leftField?.Name; }
      set
      {
				if (string.IsNullOrEmpty(value))
				{
					_leftField = null;
				}
				else
				{
					var field = LeftFields.FirstOrDefault((f) => f.Name == value);
					_leftField = field.Field;
				}
        NotifyPropertyChanged(() => LeftField);
				VisibleRelationship = string.IsNullOrEmpty(LeftField) && string.IsNullOrEmpty(RightField)
					? Visibility.Hidden : Visibility.Visible;
			}
    }

    public string RightField
    {
      get { return _rightField?.Name; }
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					_rightField = null;
				}
				else
				{
					var field = RightFields.FirstOrDefault((f) => f.Name == value);
					_rightField = field.Field;
				}
				NotifyPropertyChanged(() => RightField);
				VisibleRelationship = string.IsNullOrEmpty(LeftField) && string.IsNullOrEmpty(RightField)
					? Visibility.Hidden : Visibility.Visible;
			}
		}

    public List<CardinalityInfo> Cardinalities => new List<CardinalityInfo> { new CardinalityInfo {Name = "OneToOne", RelationshipCardinality = RelationshipCardinality.OneToOne},
                                                                              new CardinalityInfo {Name = "OneToMany", RelationshipCardinality = RelationshipCardinality.OneToMany},
                                                                              new CardinalityInfo {Name = "ManyToMany", RelationshipCardinality = RelationshipCardinality.ManyToMany}
                                                                            };
    public CardinalityInfo SelectedCardinality { get; set; }

    public ICommand CmdGenerateJoin
    {
      get
      {
        return _cmdGenerateJoin ?? (_cmdGenerateJoin = new RelayCommand(() =>
        {
          QueuedTask.Run(() =>
          {
            Table leftTable = null;
            Table rightTable = null;
            RelationshipClass relationshipClass = null;
            try
            {
              leftTable = GetTable(SelectedLeftGDBProjectItem, SelectedLeftTable);
              rightTable = GetTable(SelectedRightGDBProjectItem, SelectedRightTable);

              if (leftTable.GetDatastore().GetConnectionString().Equals(rightTable.GetDatastore().GetConnectionString()))
              {
								string commaSeparatedFields = null;
								if (LeftFields.Count > 1 || RightFields.Count > 1)
								{
									var fields = new List<string>();
									fields.AddRange(LeftFields.Select(item => $"{leftTable.GetName()}.{item.Field.Name}"));
									fields.AddRange(RightFields.Select(item => $"{rightTable.GetName()}.{item.Field.Name}"));
									var stringBuilder = new StringBuilder();
									foreach (var field in fields)
										stringBuilder.Append(field).Append(",");
									stringBuilder.Remove(stringBuilder.Length - 1, 1);
									commaSeparatedFields = stringBuilder.ToString();
								}
								if (leftTable.GetDatastore() is Geodatabase && rightTable.GetDatastore() is Geodatabase)
                {
                  MakeQueryTableLayer(leftTable, rightTable, commaSeparatedFields);
                  return;
                }
                if (leftTable.GetDatastore() is Database && rightTable.GetDatastore() is Database)
                {
                  MakeQueryLayer(commaSeparatedFields, leftTable, rightTable);
                  return;
                }
              }
              relationshipClass = MakeJoin(relationshipClass, leftTable, rightTable);
            }
            catch (Exception ex)
			  {
				  System.Diagnostics.Debug.WriteLine($@"Exception in CmdGenerateJoin: {ex.ToString()}");
			  }
            finally
            {
              leftTable?.Dispose();
              rightTable?.Dispose();
              relationshipClass?.Dispose();
            }
            
          });
        }, () => !string.IsNullOrEmpty(LeftField)
							&& !string.IsNullOrEmpty(RightField)
							&& !string.IsNullOrEmpty(LayerName)));
      }
    }

    private void MakeQueryLayer(string commaSeparatedFields, Table leftTable, Table rightTable)
    {
      string fields = string.IsNullOrEmpty(commaSeparatedFields) ? "*" : commaSeparatedFields;
      var database = leftTable.GetDatastore() as Database;
      var joinClause = IsLeftOuterJoin
        ? $"{leftTable.GetName()} LEFT OUTER JOIN {rightTable.GetName()} ON {leftTable.GetName()}.{_leftField.Name} = {rightTable.GetName()}.{_rightField.Name}"
        : $"{leftTable.GetName()} INNER JOIN {rightTable.GetName()} ON {leftTable.GetName()}.{_leftField.Name} = {rightTable.GetName()}.{_rightField.Name}";
      var queryDescription = database.GetQueryDescription($"SELECT {fields} FROM {joinClause}", LayerName);
      var table = database.OpenTable(queryDescription);
      LayerFactory.Instance.CreateLayer<FeatureLayer>(new FeatureLayerCreationParams(table as FeatureClass) { Name = LayerName }, MapView.Active.Map);
    }

    private RelationshipClass MakeJoin(RelationshipClass relationshipClass, Table leftTable, Table rightTable)
    {
      var virtualRelationshipClassDescription = new VirtualRelationshipClassDescription(_leftField, _rightField,
        SelectedCardinality.RelationshipCardinality);
      relationshipClass = leftTable.RelateTo(rightTable, virtualRelationshipClassDescription);
      var joinDescription = new JoinDescription(relationshipClass)
      {
        JoinDirection = JoinDirection.Forward
      };
      joinDescription.JoinType = IsLeftOuterJoin ? JoinType.LeftOuterJoin : JoinType.InnerJoin;
      if (LeftFields.Count > 1)
        joinDescription.TargetFields = LeftFields.Select(item => item.Field).ToList();
      var join = new Join(joinDescription);
      var joinedTable = @join.GetJoinedTable();
      if (joinedTable is FeatureClass)
        LayerFactory.Instance.CreateLayer<FeatureLayer>(new FeatureLayerCreationParams(joinedTable as FeatureClass) { Name = LayerName }, MapView.Active.Map);
      return relationshipClass;
    }

		private void RefreshDatabaseItems()
		{
			_leftGdbProjectItems.Clear();
			_rightGdbProjectItems.Clear();
			QueuedTask.Run(() =>
			{
				var gdbProjectItems = Project.Current.GetItems<GDBProjectItem>();
				foreach (var candidate in gdbProjectItems)
				{
					if (!_leftGdbProjectItems.Any(
								gdbProjectItem => gdbProjectItem.GetDatastore().GetConnectionString().Equals(candidate.GetDatastore().GetConnectionString())))
						_leftGdbProjectItems.Add(candidate);
					if (!_rightGdbProjectItems.Any(
						 gdbProjectItem => gdbProjectItem.GetDatastore().GetConnectionString().Equals(candidate.GetDatastore().GetConnectionString())))
						_rightGdbProjectItems.Add(candidate);
				}
			});
		}

		private void OnProjectCollectionChanged(ProjectItemsChangedEventArgs args)
		{
			var dataItem = args?.ProjectItem as GDBProjectItem;
			if (dataItem == null)
				return;

			QueuedTask.Run(() =>
			{
				switch (args.Action)
				{
					case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
						{
							if (!_leftGdbProjectItems.Any(
									gdbProjectItem => gdbProjectItem.GetDatastore().GetConnectionString().Equals(dataItem.GetDatastore().GetConnectionString())))
								_leftGdbProjectItems.Add(dataItem);
							if (!_rightGdbProjectItems.Any(
								 gdbProjectItem => gdbProjectItem.GetDatastore().GetConnectionString().Equals(dataItem.GetDatastore().GetConnectionString())))
								_rightGdbProjectItems.Add(dataItem);
						}
						break;
					case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
						{
							var matchingItem =
								_leftGdbProjectItems.FirstOrDefault(
									item => item.GetDatastore().GetConnectionString().Equals(dataItem.GetDatastore().GetConnectionString()));
							if (matchingItem != null)
								_leftGdbProjectItems.Remove(matchingItem);
							matchingItem =
								_rightGdbProjectItems.FirstOrDefault(
									item => item.GetDatastore().GetConnectionString().Equals(dataItem.GetDatastore().GetConnectionString()));
							if (matchingItem != null)
								_rightGdbProjectItems.Remove(matchingItem);
						}
						break;
				}
			});
		}

		private async Task UpdateTables(GDBProjectItem selectedLeftGDBProjectItem, ObservableCollection<TableInfo> observedTables)
		{
			var tables = new List<TableInfo>();
			await QueuedTask.Run(() =>
			{
				var datastore = selectedLeftGDBProjectItem.GetDatastore();
				if (datastore is Geodatabase)
				{
					var geodatabase = datastore as Geodatabase;
					var featureClassDefinitions = geodatabase.GetDefinitions<FeatureClassDefinition>();
					var tableDefinitions = geodatabase.GetDefinitions<TableDefinition>();
					tables.AddRange(
						featureClassDefinitions.Select(
							definition => new TableInfo { Definition = definition, Name = definition.GetName() }));
					tables.AddRange(
						tableDefinitions.Select(definition => new TableInfo { Definition = definition, Name = definition.GetName() }));
				}
				if (datastore is Database)
				{
					var database = datastore as Database;
					var tableNames = database.GetTableNames();
					foreach (var tableName in tableNames)
					{
						var tableDefinition = database.GetDefinition(database.GetQueryDescription(tableName));
						tables.Add(new TableInfo { Definition = tableDefinition, Name = tableDefinition.GetName() });
					}
				}
			});
			observedTables.Clear();
			observedTables.AddRange(tables);
		}

		private async Task UpdateFields(TableInfo selectedTable, ObservableCollection<FieldListItem> observableFields)
		{
			if (selectedTable == null)
				return;
			IReadOnlyList<Field> fields = null;
			await QueuedTask.Run(() => { fields = selectedTable.Definition.GetFields(); });
			observableFields.Clear();
			observableFields.AddRange(fields.Select(field => new FieldListItem { Field = field, Name = field.Name }));
		}

		private void MakeQueryTableLayer(Table leftTable, Table rightTable, string commaSeparatedFields)
    {
      var tables = IsLeftOuterJoin
        ? $"{leftTable.GetName()} LEFT OUTER JOIN {rightTable.GetName()} On {leftTable.GetName()}.{_leftField.Name} = {rightTable.GetName()}.{_rightField.Name}"
        : $"{leftTable.GetName()} INNER JOIN {rightTable.GetName()} On {leftTable.GetName()}.{_leftField.Name} = {rightTable.GetName()}.{_rightField.Name}";
      var queryDef = new QueryDef
      {
        Tables = tables
      };
      if (!string.IsNullOrEmpty(commaSeparatedFields))
        queryDef.SubFields = commaSeparatedFields;
      var queryTableDescription = new QueryTableDescription(queryDef);
      var geodatabase = leftTable.GetDatastore() as Geodatabase;
      var queryTable = geodatabase.OpenQueryTable(queryTableDescription);
      LayerFactory.Instance.CreateLayer<FeatureLayer>(new FeatureLayerCreationParams(queryTable as FeatureClass) { Name = LayerName }, MapView.Active.Map);
    }

    private Table GetTable(GDBProjectItem selectedProjectItem, TableInfo selectedTable)
    {
      Table table = null;
      var datastore = selectedProjectItem.GetDatastore();
      if (datastore is Geodatabase)
      {
        table = (datastore as Geodatabase).OpenDataset<Table>(selectedTable.Name);
      }
      if (datastore is Database)
      {
        var database = (datastore as Database);
        var queryDescription = database.GetQueryDescription(selectedTable.Name);
        table = database.OpenTable(queryDescription);
      }
      return table;
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

    public string Heading
    {
      get { return _heading; }
      set
      {
        SetProperty(ref _heading, value, () => Heading);
      }
    }
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class JoinsDockpane_ShowButton : Button
  {
    protected override void OnClick()
    {
      JoinsDockpaneViewModel.Show();
    }
  }

	/// <summary>
	/// Encapsulates Table name and defintions
	/// </summary>
  internal class TableInfo
  {
		/// <summary>
		/// Name
		/// </summary>
    public string Name { get; set; }
		/// <summary>
		/// table definition
		/// </summary>
    public TableDefinition Definition { get; set; }
  }

	/// <summary>
	/// Encapsulates Cadinality settins
	/// </summary>
  internal class CardinalityInfo
  {
		/// <summary>
		/// Description
		/// </summary>
    public string Name { get; set; }
		/// <summary>
		/// Relationship cardinality
		/// </summary>
    public RelationshipCardinality RelationshipCardinality { get; set; }
  }

	/// <summary>
	/// 
	/// </summary>
  internal class FieldListItem
  {
		/// <summary>
		/// Field info
		/// </summary>
    public Field Field { get; set; }
		/// <summary>
		/// Name
		/// </summary>
    public string Name { get; set; }
		/// <summary>
		/// Flag selection
		/// </summary>
    public bool IsSelected { get; set; }
  }
}
