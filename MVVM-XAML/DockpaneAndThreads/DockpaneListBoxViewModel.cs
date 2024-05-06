/*

   Copyright 2024 Esri

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
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Portal;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using static ArcGIS.Desktop.Internal.Core.CancellableFileChecker;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;


namespace DockpaneAndThreads
{
  internal class DockpaneListBoxViewModel : DockPane
  {
    private const string _dockPaneID = "DockpaneAndThreads_DockpaneListBox";
    private string _contentType = "Layers";
    private static string _activePortalUri = @"http://www.arcgis.com:80/";

		private static string _token = string.Empty;

		// TODO: 1) Add _lock object to support synchronization of _layerPackes across threads 
    private readonly ObservableCollection<OnlinePortalItem> _layerPackages = [];
		private readonly object _lock = new();

		private OnlinePortalItem _selectedLayerPackage;

		private double _maxProgressValue;
		private double _ProgressValue;
		private string _progressText = string.Empty;
		private Visibility _progressBarVisibility = Visibility.Collapsed;

		private Visibility _busyVisibility = Visibility.Visible;


		protected DockpaneListBoxViewModel() 
		{
			// TODO: 2) Add _lock object to support synchronization of _layerPackes across threads 
			BindingOperations.EnableCollectionSynchronization(_layerPackages, _lock);

			// indicate that we are not busy
			BusyVisibility = System.Windows.Visibility.Collapsed;
			ProgressBarVisibility = Visibility.Collapsed;

			// also disable the 'download' status display
			VisibleText = Visibility.Collapsed;
		}

		#region ListBox properties

		/// <summary>
		/// Holds the list of ArcGIS Online Layer Packages returned by our query
		/// </summary>
		public ObservableCollection<OnlinePortalItem> LayerPackages
		{
			get => _layerPackages;
		}

		public OnlinePortalItem SelectedLayerPackage {
			get => _selectedLayerPackage;
			set {
				SetProperty(ref _selectedLayerPackage, value);
				ExecuteItemAction(_selectedLayerPackage);
			} 
		}

		#endregion

		#region ProgressBar properties

		public double MaxProgressValue
		{
			get { return _maxProgressValue; }
			set
			{
				SetProperty(ref _maxProgressValue, value, () => MaxProgressValue);
			}
		}

		public double ProgressValue
		{
			get { return _ProgressValue; }
			set
			{
				SetProperty(ref _ProgressValue, value, () => ProgressValue);
			}
		}

		public string ProgressText
		{
			get { return _progressText; }
			set
			{
				SetProperty(ref _progressText, value, () => ProgressText);
			}
		}

		public Visibility ProgressBarVisibility
		{
			get { return _progressBarVisibility; }
			set
			{
				SetProperty(ref _progressBarVisibility, value, () => ProgressBarVisibility);
			}
		}

		#endregion ProgressBar properties

		#region Busy Indicator Property

    public System.Windows.Visibility BusyVisibility
    {
      get { return _busyVisibility; }
      set
      { SetProperty(ref _busyVisibility, value, () => BusyVisibility); }
    }

		#endregion

		#region Download Status Properties

		private Visibility _visibleText;
		public Visibility VisibleText
		{
			get { return _visibleText; }
			set
			{
				_visibleText = value;
				NotifyPropertyChanged("VisibleText");
			}
		}

		private Visibility _visibleList;
		public Visibility VisibleList
		{
			get { return _visibleList; }
			set
			{
				_visibleList = value;
				NotifyPropertyChanged("VisibleList");
			}
		}

		private string _statusTitle;
		public string StatusTitle
		{
			get { return _statusTitle; }
			set
			{
				_statusTitle = value;
				NotifyPropertyChanged("StatusTitle");
			}
		}

		private string _status;
		public string Status
		{
			get { return _status; }
			set
			{
				_status = value;
				NotifyPropertyChanged("Status");
			}
		}


		#endregion

		#region Other properties

		/// <summary>
		/// Text shown near the top of the DockPane.
		/// </summary>
		private string _heading = "Update a ListBox from Background Threads";
    public string Heading
    {
      get => _heading;
      set => SetProperty(ref _heading, value);
    }

		#endregion

		#region Commands

		public ICommand CmdCancelRefresh
		{
			get { return new RelayCommand(() => RefreshCancelled = true); }
		}

		public bool RefreshCancelled { get; set; }

		public ImageSource CmdCancelImg
		{
			get
			{
				var imageSource = System.Windows.Application.Current.Resources["Cancel16"] as ImageSource;
				return imageSource;
			}
		}

		//public ICommand CmdRefreshSync
  //  {
		//	get
		//	{
		//		// TODO: Refresh from UI thread the list of Layer Packages from the active portal
		//		return new RelayCommand(async () =>
		//		{
		//			try
		//			{
		//				#region Initialize the download
		//				// indicate that we are busy
		//				BusyVisibility = Visibility.Visible;
		//				// show and initialize the progress bar
		//				ProgressBarVisibility = Visibility.Visible;
		//				MaxProgressValue = 100;
		//				ProgressText = $@"Added: {0}";
		//				ProgressValue = 0;
		//				// make sure RefreshCancelled is false
		//				RefreshCancelled = false;

		//				// clear any previous results
		//				LayerPackages.Clear();
		//				#endregion Initialize the download

		//				// load portal items into our ObservableCollection
		//				var onlinePortalItems = new OnlinePortalItems(_activePortalUri, _contentType);

		//				var maxCount = onlinePortalItems.Count();
		//				var loadedCount = 0;
		//				do
		//				{
		//					// get the next item from the query
		//					var portalItem = await onlinePortalItems.GetNextAsync();
		//					if (portalItem == null)
		//						break;
		//					// add the item to the ObservableCollection
		//					LayerPackages.Add(portalItem);
		//					double percent = (double)loadedCount / (double)maxCount * 100.0;
		//					ActionOnGuiThread(() =>
		//					{
		//						ProgressText = $@"Added: {++loadedCount}";
		//						ProgressValue = percent;
		//					});
		//				} while (!RefreshCancelled);
		//			}
		//			catch (Exception ex)
		//			{
		//				// not busy anymore => hide busy indicator
		//				BusyVisibility = System.Windows.Visibility.Collapsed;
		//				ProgressBarVisibility = Visibility.Collapsed;
		//				// Show error 
		//				MessageBox.Show(ex.Message);
		//			}
		//			finally
		//			{
		//				// not busy anymore => hide busy indicator
		//				BusyVisibility = System.Windows.Visibility.Collapsed;
		//				ProgressBarVisibility = Visibility.Collapsed;
		//			}
		//		});
		//	}
		//}

		public ICommand CmdRefreshAsync
    {
      get
      {
				// TODO: Refresh from worker thread the list of Layer Packages from the active portal
				return new RelayCommand(async () =>
				{
					try
					{
						// TODO: Progress bar and busy indicator: setup
						// TODO: Progress Busy indicator make visible to show that we are busy
						BusyVisibility = System.Windows.Visibility.Visible;
						// TODO: Progress bar show and initialize the progress bar to a max value of 100 and initial status text
						ProgressBarVisibility = System.Windows.Visibility.Visible;
						MaxProgressValue = 100;
						ProgressText = $@"Added: {0}";
						ProgressValue = 0;
						// make sure RefreshCancelled is false
						RefreshCancelled = false;

						// clear any previous results
						LayerPackages.Clear();
						// load portal items into our ObservableCollection
						var onlinePortalItems = new OnlinePortalItems(_activePortalUri, _contentType);

						var maxCount = onlinePortalItems.Count();
						var loadedCount = 0;
						await QueuedTask.Run(async () =>
							{
								do
								{
									// get the next item from the query
									var portalItem = await onlinePortalItems.GetNextAsync();
									if (portalItem == null)
										break;
									// add the item to the ObservableCollection
									lock (_lock) LayerPackages.Add(portalItem);
									// TODO: Progress bar: how the % completes using loadedCount and maxCount
									double percent = (double)loadedCount / (double)maxCount * 100.0;
									ActionOnGuiThread(() =>
									{
										// TODO: Update the Progress bar text and value on the GUI thread
										// Update the Progress bar text and value
										ProgressText = $@"Added: {++loadedCount}";
										ProgressValue = percent;
									});
									if (RefreshCancelled)
									{
										throw new Exception("User canceled the refresh");
									}
								} while (!RefreshCancelled);
							});
					}
					catch (Exception ex)
					{
						// TODO: Progress bar and busy indicator: all done
						// not busy anymore => hide busy indicator
						BusyVisibility = System.Windows.Visibility.Collapsed;
						ProgressBarVisibility = Visibility.Collapsed;
						// Show error 
						MessageBox.Show(ex.Message);
					}
					finally
					{
						// TODO: Progress bar and busy indicator: all done
						// not busy anymore => hide busy indicator
						BusyVisibility = System.Windows.Visibility.Collapsed;
						ProgressBarVisibility = Visibility.Collapsed;
					}
				});
      }
    }

    public ImageSource CmdRefreshImg
    {
      get
      {
        var imageSource = System.Windows.Application.Current.Resources["GenericRefresh16"] as ImageSource;
        return imageSource;
      }
    }

		#endregion

		#region Action On Gui Thread

		internal void ActionOnGuiThread(Action theAction)
		{
			if (ProApp.Current.Dispatcher.CheckAccess())
			{ // We are on the GUI thread => run theAction directly without switching thread
				theAction();
			}
			else
			{ // Using Dispatcher to perform this action on the GUI thread.
				ProApp.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, theAction);
			}
		}

		#endregion

		#region Helpers

		protected internal async void ExecuteItemAction(OnlinePortalItem portalItem)
		{
			if (portalItem == null) return;

			//Either we open a project and then create the item or
			//we download the item and then create a project from it.
			//MapPackage is a special case. We download it, create
			//a project and then add the map package to it.

			VisibleList = Visibility.Collapsed;
			VisibleText = Visibility.Visible;
			StatusTitle = $@"{portalItem.LinkText}: {portalItem.Title}";

			var error = string.Empty;
			try
			{
				switch (portalItem.PortalItemType)
				{
					case PortalItemType.WebMap:
						{
							await Project.CreateAsync(new CreateProjectSettings()
							{
								Name = System.IO.Path.GetFileNameWithoutExtension((string)portalItem.Name)
							});
							var currentItem = ItemFactory.Instance.Create((string)portalItem.Id, ItemFactory.ItemType.PortalItem);
							if (MapFactory.Instance.CanCreateMapFrom(currentItem))
							{
								await QueuedTask.Run(() =>
								{
									var newMap = MapFactory.Instance.CreateMapFromItem(currentItem);
									FrameworkApplication.Panes.CreateMapPaneAsync(newMap);
								});
							}
						}
						break;
					case PortalItemType.Layer:
						{
							try
							{
								Item currentItem = ItemFactory.Instance.Create((string)portalItem.Id, ItemFactory.ItemType.PortalItem);
								if (LayerFactory.Instance.CanCreateLayerFrom(currentItem))
								{
									var lyrParams = new LayerCreationParams(currentItem);
									_ = QueuedTask.Run(() => LayerFactory.Instance.CreateLayer<FeatureLayer>(lyrParams, MapView.Active.Map));
								}
							}
							catch (Exception ex)
							{
								error = $@"Error occurred: {ex.ToString()}";
								System.Diagnostics.Debug.WriteLine(ex.ToString());
							}
						}
						break;
					default:
						var defaultPath = CoreModule.CurrentProject.DefaultGeodatabasePath;
						var query = new OnlineQuery(_activePortalUri);
						query.ConfigureData("", portalItem.Id, portalItem.Name);
						Status = "Downloading";
						await DownloadFileAsync(query);
						Status = "Opening";
						if (portalItem.PortalItemType == PortalItemType.LayerPackage)
						{
							// Layer Package
							Uri myUri = new Uri(query.DownloadFileName);
							var layerParams = new LayerCreationParams(myUri) { MapMemberPosition = MapMemberPosition.AddToTop };
							var featLayer = await QueuedTask.Run(() => LayerFactory.Instance.CreateLayer<FeatureLayer>(layerParams, MapView.Active.Map));
						}
						//Project package
						else if (portalItem.PortalItemType == PortalItemType.ProjectPackage)
						{
							await Project.OpenAsync(query.DownloadFileName);
						}
						//Project Template
						else if (portalItem.PortalItemType == PortalItemType.MapTemplate)
						{
							var ps = new CreateProjectSettings()
							{
								Name = System.IO.Path.GetFileNameWithoutExtension(portalItem.Name),
								LocationPath = defaultPath,
								TemplatePath = query.DownloadFileName
							};
							try
							{
								await Project.CreateAsync(ps);
							}
							catch (Exception ex)
							{
								System.Diagnostics.Debug.WriteLine(ex.ToString());
								Status = $@"Error occurred: {ex.ToString()}";
							}
						}
						//Map Package
						else if (portalItem.PortalItemType == PortalItemType.MapPackage)
						{
							try
							{
								var mapPackage = ItemFactory.Instance.Create(query.DownloadFileName);
								MapFactory.Instance.CreateMapFromItem(mapPackage);
							}
							catch (Exception ex)
							{
								System.Diagnostics.Debug.WriteLine(ex.ToString());
								Status = $@"Error occurred: {ex.ToString()}";
							}
						}
						break;
				}
			}
			catch { }
			finally {
				VisibleList = Visibility.Visible;
				VisibleText = Visibility.Collapsed;
			}
			if (!string.IsNullOrEmpty(error))
			{
				ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(error);
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

		private Task<bool> DownloadFileAsync(OnlineQuery query)
		{
			return new EsriHttpClient().GetAsFileAsync(query.DownloadUrl, query.DownloadFileName);
		}

		#endregion
	}

	/// <summary>
	/// Button implementation to show the DockPane.
	/// </summary>
	internal class DockpaneListBox_ShowButton : Button
  {
    protected override void OnClick()
    {
      DockpaneListBoxViewModel.Show();
    }
  }
}
