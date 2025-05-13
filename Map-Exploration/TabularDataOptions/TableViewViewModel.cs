/*

   Copyright 2025 Esri

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
using ActiproSoftware.Windows.Extensions;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace TabularDataOptions
{
	internal class TableViewViewModel : DockPane
	{
		private const string _dockPaneID = "TabularDataOptions_TableView";

		private MapMember _selectedMapMember;

		/// <summary>
		/// used to lock collections for use by multiple threads
		/// </summary>
		private readonly object _lockCollections = new();

		/// <summary>
		/// UI lists, read-only collections, and theDictionary
		/// </summary>
		private readonly ObservableCollection<MapMember> _mapMembers = [];

		protected TableViewViewModel()
		{
			BindingOperations.EnableCollectionSynchronization(_mapMembers, _lockCollections);
			// subscribe to the map view changed event... that's when we update the list of feature layers
			ActiveMapViewChangedEvent.Subscribe((args) =>
			{
				if (args.IncomingView == null) return;
				GetMapMembers();
			});
			// in case we have a map already open
			GetMapMembers(true);
		}


		#region Properties

		/// <summary>
		/// List of the current active map's mapmembers
		/// </summary>
		public ObservableCollection<MapMember> MapMembers
		{
			get { return _mapMembers; }
		}

		/// <summary>
		/// The selected map member
		/// </summary>
		public MapMember SelectedMapMember
		{
			get { return _selectedMapMember; }
			set
			{
				SetProperty(ref _selectedMapMember, value);
				// make enable the TableView buttons
				_TablePaneEx = null;
				_TablePane = null;
				_isTableViewOpen = false;
				NotifyPropertyChanged(() => TableViewOpenCaption);
				NotifyPropertyChanged(() => IsTableViewOpen);
			}
		}

		#endregion Properties

		#region TableView Command Properties

		private ITablePaneEx _TablePaneEx;
		private Pane _TablePane;

		public string TableViewOpenCaption
		{
			get
			{
				return _isTableViewOpen ? "Close TableView" : "Open TableView";
			}
		}

		private bool _isTableViewOpen = false;

		public ICommand CmdTableViewOpen
		{
			get
			{
				return new RelayCommand(() =>
				{
					try
					{
						if (_isTableViewOpen)
						{
							// close the table view
							// check the open panes to see if it's open but just needs activating
							IEnumerable<ITablePane> tablePanes = FrameworkApplication.Panes.OfType<ITablePane>();
							foreach (var tablePane in tablePanes)
							{
								if (SelectedMapMember != null
										&& tablePane.MapMember != SelectedMapMember) continue;
								_TablePane = tablePane as Pane;
								_TablePane?.Close();
								_TablePaneEx = null;
								_TablePane = null;
							}
						}
						else
						{
							// open the table view
							// check the open panes to see if it's open but just needs activating
							// TableView: Open the TableView for the selected MapMember
							IEnumerable<ITablePane> tablePanes
								= FrameworkApplication.Panes.OfType<ITablePane>();
							bool isOpen = false;
							foreach (var tablePane in tablePanes)
							{
								if (tablePane.MapMember != SelectedMapMember) continue;
								_TablePane = tablePane as Pane;
								_TablePane?.Activate();
								_TablePaneEx = _TablePane as ITablePaneEx;
								isOpen = true;
							}
							if (!isOpen)
							{
								var iTablePane = FrameworkApplication.Panes.OpenTablePane(SelectedMapMember);
								_TablePane = iTablePane as Pane;
								_TablePaneEx = iTablePane as ITablePaneEx;
							}
						}
						_isTableViewOpen = !_isTableViewOpen;
						NotifyPropertyChanged(() => TableViewOpenCaption);
						NotifyPropertyChanged(() => IsTableViewOpen);
					}
					catch (Exception ex)
					{
						MessageBox.Show($@"Error in TableViewOpen: {ex.Message}");
					}
				}, () => SelectedMapMember != null);
			}
		}

		public bool IsTableViewOpen => _isTableViewOpen;

		public string TableViewHideFieldsCaption
		{
			get
			{
				return _isTableViewHideFields ? "Show all fields" : "Hide some fields";
			}
		}

		private bool _isTableViewHideFields = false;

		public ICommand CmdTableViewHideFields
		{
			get
			{
				return new RelayCommand(async () =>
				{
					try
					{
						if (_TablePaneEx != null)
						{
							_TablePane?.Activate();
							//_TablePaneEx.TableView.Refresh();
							if (_isTableViewHideFields)
							{
								// show all fields
								_TablePaneEx.TableView?.ShowAllFields();
							}
							else
							{
								// hide the objectid field
								var oidName = await QueuedTask.Run<string>(() =>
								{
									if (_selectedMapMember is BasicFeatureLayer featureLayer)
									{
										return featureLayer.GetTable().GetDefinition().GetObjectIDField();
									}
									if (_selectedMapMember is StandaloneTable standaloneTable)
									{
										return standaloneTable.GetTable().GetDefinition().GetObjectIDField();
									}
									return string.Empty;
								});
								if (!string.IsNullOrEmpty(oidName))
								{
									// TableView: hide the object id field
									_TablePaneEx.TableView?.SetHiddenFields([oidName]);
								}
							}
						}
						_isTableViewHideFields = !_isTableViewHideFields;
						NotifyPropertyChanged(() => TableViewHideFieldsCaption);
					}
					catch (Exception ex)
					{
						MessageBox.Show($@"Error in TableViewHideFields: {ex.Message}");
					}
				}, () => SelectedMapMember != null);
			}
		}

		public ICommand CmdToggleValueIDs
		{
			get
			{
				return new RelayCommand(() =>
				{
					try
					{
						if (_TablePaneEx != null)
						{
							_TablePane?.Activate();
							// toggle Fields
							// TableView: Toggle the field names between the field alias
							// and the database field name
							_TablePaneEx.TableView?.ToggleFieldAlias();
						}
					}
					catch (Exception ex)
					{
						MessageBox.Show($@"Error in TableViewHideFields: {ex.Message}");
					}
				}, () => SelectedMapMember != null);
			}
		}

		public ICommand CmdSwitchSelected
		{
			get
			{
				return new RelayCommand(() =>
				{
					try
					{
						if (_TablePaneEx != null)
						{
							_TablePane?.Activate();
							// toggle ViewMode              
							_TablePaneEx.TableView?.SetViewMode(_TablePaneEx.TableView?.ViewMode == TableViewMode.eSelectedRecords
								? TableViewMode.eAllRecords : TableViewMode.eSelectedRecords);
						}
					}
					catch (Exception ex)
					{
						MessageBox.Show($@"Error in TableViewHideFields: {ex.Message}");
					}
				}, () => SelectedMapMember != null);
			}
		}

		public ICommand CmdFind
		{
			get
			{
				return new RelayCommand(() =>
				{
					try
					{
						if (_TablePaneEx != null)
						{
							_TablePane?.Activate();
							// toggle ViewMode
							// TableView: Initiate the "Find"
							_TablePaneEx.TableView?.Find();
						}
					}
					catch (Exception ex)
					{
						MessageBox.Show($@"Error in TableViewHideFields: {ex.Message}");
					}
				}, () => SelectedMapMember != null);
			}
		}

		public ICommand CmdFindReplace
		{
			get
			{
				return new RelayCommand(() =>
				{
					try
					{
						if (_TablePaneEx != null)
						{
							_TablePane?.Activate();
							// toggle ViewMode
							// TableView: Initiate the "Find and Replace" input
							_TablePaneEx.TableView?.FindAndReplace();
						}
					}
					catch (Exception ex)
					{
						MessageBox.Show($@"Error in TableViewHideFields: {ex.Message}");
					}
				}, () => SelectedMapMember != null);
			}
		}

		#endregion Command Properties

		#region Image Properties

		public ImageSource ImgTableViewOpen
		{
			get
			{
				var imageSource = System.Windows.Application.Current.Resources["InteractiveTable32"] as ImageSource;
				return imageSource;
			}
		}

		public ImageSource ImgTableViewHideFields
		{
			get
			{
				var imageSource = System.Windows.Application.Current.Resources["TableHideField32"] as ImageSource;
				return imageSource;
			}
		}

		public ImageSource ImgToggleValueIDs
		{
			get
			{
				var imageSource = System.Windows.Application.Current.Resources["ToggleValueIDs32"] as ImageSource;
				return imageSource;
			}
		}

		public ImageSource ImgSwitchSelected
		{
			get
			{
				var imageSource = System.Windows.Application.Current.Resources["TableSwitchHighlight32"] as ImageSource;
				return imageSource;
			}
		}

		public ImageSource ImgFind
		{
			get
			{
				var imageSource = System.Windows.Application.Current.Resources["Search32"] as ImageSource;
				return imageSource;
			}
		}

		public ImageSource ImgFindReplace
		{
			get
			{
				var imageSource = System.Windows.Application.Current.Resources["FindReplaceMenu32"] as ImageSource;
				return imageSource;
			}
		}

		#endregion Image Properties

		#region Helper Methods

		/// <summary>
		/// This method is called to use the current active mapview and retrieve all 
		/// MapMembers that are part of the map in the current map view.
		/// </summary>
		private void GetMapMembers(bool startUp = false)
		{
			QueuedTask.Run(() =>
			{
				var map = MapView.Active?.Map;
				if (map == null)
				{
					// no active map ... use the first visible map instead
					var firstMapPane = ProApp.Panes.OfType<IMapPane>().FirstOrDefault();
					map = firstMapPane?.MapView?.Map;
				}
				if (map == null)
				{
					if (!startUp) MessageBox.Show("Can't find a MapView");
					return;
				}
				MapMembers.Clear();
				MapMembers.AddRange(map.GetLayersAsFlattenedList().OfType<BasicFeatureLayer>());
				MapMembers.AddRange(map.GetStandaloneTablesAsFlattenedList().OfType<MapMember>());
			});
		}

		/// <summary>
		/// utility function to enable an action to run on the UI thread (if not already)
		/// </summary>
		/// <param name="action">the action to execute</param>
		/// <returns></returns>
		internal static void RunOnUiThread(Action action)
		{
			try
			{
				if (IsOnUiThread)
					action();
				else
					System.Windows.Application.Current.Dispatcher.BeginInvoke(action);
			}
			catch (Exception ex)
			{
				ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($@"Error in RunOnUiThread: {ex.Message}");
			}
		}

		/// <summary>
		/// Determines whether the calling thread is the thread associated with this 
		/// System.Windows.Threading.Dispatcher, the UI thread.
		/// 
		/// If called from a View model test it always returns true.
		/// </summary>
		public static bool IsOnUiThread => ArcGIS.Desktop.Framework.FrameworkApplication.TestMode || System.Windows.Application.Current.Dispatcher.CheckAccess();

		#endregion Helper Methods

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
		private string _heading = "Controlling Pro TableViews";
		public string Heading
		{
			get => _heading;
			set => SetProperty(ref _heading, value);
		}
	}

	/// <summary>
	/// Button implementation to show the DockPane.
	/// </summary>
	internal class TableView_ShowButton : Button
	{
		protected override void OnClick()
		{
			TableViewViewModel.Show();
		}
	}
}
