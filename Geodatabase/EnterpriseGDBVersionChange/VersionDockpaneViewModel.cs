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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;

namespace EnterpriseGDBVersionChange
{
	internal class VersionDockpaneViewModel : DockPane
	{
		private const string _dockPaneID = "EnterpriseGDBVersionChange_VersionDockpane";

		private FeatureLayer _FeatureLayer = null;

		protected VersionDockpaneViewModel()
		{
			// subscribe to TOC selection changed event
			ArcGIS.Desktop.Mapping.Events.TOCSelectionChangedEvent.Subscribe(async (args) =>
			{
				try
				{
					if (args == null)
						return;
					var featLayer = args.MapView?.GetSelectedLayers().OfType<FeatureLayer>().FirstOrDefault();
					if (featLayer == null)
					{
						return;
					}
					_FeatureLayer = featLayer;
					ClearProperties();
					var timer = new Stopwatch();
					timer.Start();
					CurrentLayer = await QueuedTask.Run<string>(() => _FeatureLayer.GetTable().GetName());
					GDBType = await GetGeoDatabaseType(_FeatureLayer);
					CurrentVersion = await GetCurrentVersion(_FeatureLayer);
					var versionList = await GetVersionsForFeatureLayer(_FeatureLayer);
					if (versionList.VersionType != null)
					{
						CurrentVersionType = versionList.Item1.ToString();
						ToVersions = new ObservableCollection<string>(versionList.Item2);
					}
					timer.Stop();
					var elapsedTime = timer.Elapsed;
					await RunOnUIThread(() =>
					{
						Status += $@"Selected {_FeatureLayer.Name}: {elapsedTime:m\:ss\.fff} min.{System.Environment.NewLine}";
					});
				}
				catch (Exception ex)
				{
					MessageBox.Show ($@"Exception: {ex.Message}");
				}
			});
		}

		private void ClearProperties()
		{
			CurrentLayer = string.Empty;
			GDBType = string.Empty;
			CurrentVersionType = string.Empty;
			ToVersions = null;
		}

		#region Helpers

		private static async Task<string> GetVersionNameAsync(Geodatabase geodatabase)
		{
			return await QueuedTask.Run<string>(() =>
			{
				using VersionManager versionManager = geodatabase.GetVersionManager();
				VersionBaseType versionBaseType = versionManager.GetCurrentVersionBaseType();
				if (versionBaseType == VersionBaseType.Version)
				{
					ArcGIS.Core.Data.Version version = versionManager.GetCurrentVersion();
					return version.GetName();
				}
				if (versionBaseType == VersionBaseType.HistoricalVersion)
				{
					HistoricalVersion historicalVersion = versionManager.GetCurrentHistoricalVersion();
					return historicalVersion.GetName();
				}
				return string.Empty;
			});
		}

		private static async Task<string> GetGeoDatabaseType(FeatureLayer featureLayer)
		{
			try
			{
				return await QueuedTask.Run<string>(() =>
				{
					Datastore dataStore = featureLayer.GetFeatureClass().GetDatastore();
					Geodatabase geodatabase = dataStore as Geodatabase;
					if (geodatabase == null)
						return "Not a GDB";
					return geodatabase.GetGeodatabaseType().ToString();
				});
			}
			catch (Exception ex)
			{
				return $@"Exception: {ex.Message}";
			}
		}

		private static async Task<string> GetCurrentVersion (FeatureLayer featureLayer)
		{
			return await QueuedTask.Run<string>(() =>
			{
				Datastore dataStore = featureLayer.GetFeatureClass().GetDatastore();
				Geodatabase geodatabase = dataStore as Geodatabase;
				if (geodatabase == null)
					return "Not a GDB";
				if (!geodatabase.IsVersioningSupported()) return "Versions are NOT supported";
				using VersionManager versionManager = geodatabase.GetVersionManager();
				VersionBaseType versionBaseType = versionManager.GetCurrentVersionBaseType();
				if (versionBaseType == VersionBaseType.Version)
				{
					ArcGIS.Core.Data.Version version = versionManager.GetCurrentVersion();
					return version.GetName();
				}
				if (versionBaseType == VersionBaseType.HistoricalVersion)
				{
					HistoricalVersion historicalVersion = versionManager.GetCurrentHistoricalVersion();
					return historicalVersion.GetName();
				}
				return string.Empty;
			});
		}

		private static async Task<(VersionBaseType? VersionType, List<string> VersionNames)> GetVersionsForFeatureLayer (FeatureLayer featureLayer)
		{
			return await QueuedTask.Run<(VersionBaseType? VersionType, List<string> VersionNames)>(() =>
			{
				Datastore dataStore = featureLayer.GetFeatureClass().GetDatastore();
				Geodatabase geodatabase = dataStore as Geodatabase;
				if (geodatabase == null)
					return (null, null);
				if (!geodatabase.IsVersioningSupported()) return (null, null);
				using VersionManager versionManager = geodatabase.GetVersionManager();
				VersionBaseType versionBaseType = versionManager.GetCurrentVersionBaseType();
				if (versionBaseType == VersionBaseType.Version)
				{
					List<string> versionNames = new List<string>(versionManager.GetVersionNames());
					versionNames.Remove(versionManager.GetCurrentVersion().GetName());
					return (versionBaseType, versionNames);
				}
				if (versionBaseType == VersionBaseType.HistoricalVersion)
				{
					var histVersions = versionManager.GetHistoricalVersions();
					List<string> versionNames = new List<string>(histVersions.Select((h) => h.GetName()));
					versionNames.Remove(versionManager.GetCurrentHistoricalVersion().GetName());
					return (versionBaseType, versionNames);
				}
				return (null, null);
			});
		}

		private static void ChangeVersions(FeatureLayer featureLayer, string toVersionName)
		{
			Datastore dataStore = featureLayer.GetFeatureClass().GetDatastore();
			Geodatabase geodatabase = dataStore as Geodatabase;
			using VersionManager versionManager = geodatabase.GetVersionManager();
			VersionBaseType versionBaseType = versionManager.GetCurrentVersionBaseType();
			if (versionBaseType == VersionBaseType.Version)
			{
				ArcGIS.Core.Data.Version fromVersion = versionManager.GetCurrentVersion();
				ArcGIS.Core.Data.Version toVersion = versionManager.GetVersion(toVersionName);

				// Switch between versions
				MapView.Active.Map.ChangeVersion(fromVersion, toVersion);
			}
			if (versionBaseType == VersionBaseType.HistoricalVersion)
			{
				HistoricalVersion fromHistoricalVersion = versionManager.GetCurrentHistoricalVersion();
				HistoricalVersion toHistoricalVersion = versionManager.GetHistoricalVersion(toVersionName);

				// Switch between historical versions
				MapView.Active.Map.ChangeVersion(fromHistoricalVersion, toHistoricalVersion);
			}
		}

		#endregion Helpers

		#region Properties

		/// <summary>
		/// CurrentLayer name
		/// </summary>
		private string _CurrentLayer;
		public string CurrentLayer
		{
			get => _CurrentLayer;
			set => SetProperty(ref _CurrentLayer, value);
		}

		private string _GDBType;
		public string GDBType
		{
			get => _GDBType;
			set => SetProperty(ref _GDBType, value);
		}

		private string _CurrentVersionType;
		public string CurrentVersionType
		{
			get => _CurrentVersionType;
			set => SetProperty(ref _CurrentVersionType, value);
		}

		private string _CurrentVersion;
		public string CurrentVersion
		{
			get => _CurrentVersion;
			set => SetProperty(ref _CurrentVersion, value);
		}

		private ObservableCollection<string> _ToVersions;
		public ObservableCollection<string> ToVersions
		{
			get => _ToVersions;
			set => SetProperty(ref _ToVersions, value);
		}

		private string _ToVersion;
		public string ToVersion
		{
			get => _ToVersion;
			set => SetProperty(ref _ToVersion, value);
		}

		private string _Status;
		public string Status
		{
			get => _Status;
			set => SetProperty(ref _Status, value);
		}

		public ICommand CmdChangeVersion
		{
			get => new RelayCommand((args) => 
			{
				// change version 
				try
				{
					QueuedTask.Run(async () =>
					{
						var timer = new Stopwatch();
						timer.Start();
						ChangeVersions(_FeatureLayer, ToVersion);
						CurrentVersion = await GetCurrentVersion(_FeatureLayer);
						var versionList = await GetVersionsForFeatureLayer(_FeatureLayer);
						if (versionList.VersionType != null)
						{
							CurrentVersionType = versionList.VersionType.ToString();
							ToVersions = new ObservableCollection<string>(versionList.VersionNames);
						}
						timer.Stop();
						var elapsedTime = timer.Elapsed;
						await RunOnUIThread(() =>
						{
							Status += $@"Change Version {_FeatureLayer.Name} {elapsedTime:m\:ss\.fff} min.{System.Environment.NewLine}";
						});

					});
				}
				catch (Exception ex)
				{
					MessageBox.Show($@"Can't change version: {ex.Message}");
				}
			}, () => (ToVersion != null && _FeatureLayer != null));
		}

		public System.Windows.Media.ImageSource ImgChangeVersion
		{
			get { return System.Windows.Application.Current.Resources["VersionChanges16"] as System.Windows.Media.ImageSource; }
		}

		#endregion Properties


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
		private string _heading = "Switch GDB Version";
		public string Heading
		{
			get => _heading;
			set => SetProperty(ref _heading, value);
		}

		#region Helper functions

		/// <summary>
		/// Utility function to enable an action to run on the UI thread (if not already)
		/// </summary>
		/// <param name="action">the action to execute</param>
		/// <returns></returns>
		internal static Task RunOnUIThread(Action action)
		{
			if (OnUIThread)
			{
				action();
				return Task.FromResult(0);
			}
			else
				return Task.Factory.StartNew(action, System.Threading.CancellationToken.None, TaskCreationOptions.None, QueuedTask.UIScheduler);
		}

		/// <summary>
		/// Determines if the application is currently on the UI thread
		/// </summary>
		private static bool OnUIThread
		{
			get
			{
				if (FrameworkApplication.TestMode)
					return QueuedTask.OnWorker;
				else
					return System.Windows.Application.Current.Dispatcher.CheckAccess();
			}
		}

		#endregion Helper functions
	}

	/// <summary>
	/// Button implementation to show the DockPane.
	/// </summary>
	internal class VersionDockpane_ShowButton : Button
	{
		protected override void OnClick()
		{
			VersionDockpaneViewModel.Show();
		}
	}
}
