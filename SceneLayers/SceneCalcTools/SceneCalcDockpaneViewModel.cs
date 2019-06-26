/*

   Copyright 2019 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;


namespace SceneCalcTools
{

	#region DockPane Activate
	internal class SceneCalcDockpaneViewModel : DockPane
	{
		private const string _dockPaneID = "SceneCalcTools_SceneCalcDockpane";

		protected SceneCalcDockpaneViewModel()
		{

			_sketchPolygonCmd = new RelayCommand(() => Module1.Current.RunPolygonTool(), () => true);
			_elevationToolCmd = new RelayCommand(() => Module1.Current.RunElevationTool(), () => true);
			_calculateVolumeCmd = new RelayCommand(() => Module1.Current.CalculateVolume(), () => true);

			// Subscribe to the following events:
			MapSelectionChangedEvent.Subscribe(OnMapSelectionChanged);
			ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);

		}

		#endregion

		#region Commands

		private RelayCommand _sketchPolygonCmd;
		public ICommand SketchPolygonCmd
		{
			get
			{
				return _sketchPolygonCmd;
			}
		}

		private RelayCommand _elevationToolCmd;
		public ICommand ElevationToolCmd
		{
			get
			{
				return _elevationToolCmd;
			}
		}


		private RelayCommand _calculateVolumeCmd;
		public ICommand CalculateVolumeCmd
		{
			get
			{
				return _calculateVolumeCmd;
			}
		}


		#endregion

		#region Properties

		private System.Windows.Media.SolidColorBrush _sketchToolBackground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
		public System.Windows.Media.SolidColorBrush SketchToolBackground
		{
			get
			{
				return _sketchToolBackground;
			}
			set
			{
				SetProperty(ref _sketchToolBackground, value, () => SketchToolBackground);
			}
		}

		private System.Windows.Media.SolidColorBrush _elevationToolBackground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
		public System.Windows.Media.SolidColorBrush ElevationToolBackground
		{
			get
			{
				return _elevationToolBackground;
			}
			set
			{
				SetProperty(ref _elevationToolBackground, value, () => ElevationToolBackground);
			}
		}

		private double _totalVolume = 0;
		public double TotalVolume
		{
			get
			{
				return _totalVolume;
			}
			set
			{
				_totalVolume = value;
				NotifyPropertyChanged();
			}
		}

		private double _totalSurfaceArea = 0;
		public double TotalSurfaceArea
		{
			get
			{
				return _totalSurfaceArea;
			}
			set
			{
				_totalSurfaceArea = value;
				NotifyPropertyChanged();
			}
		}

		private double _totalWeightInTons = 0;
		public double TotalWeightInTons
		{
			get
			{
				return _totalWeightInTons;
			}
			set
			{
				_totalWeightInTons = value;
				NotifyPropertyChanged();
			}
		}

		private double _referencePlaneArea = 0;
		public double ReferencePlaneArea
		{
			get
			{
				return _referencePlaneArea;
			}
			set
			{
				_referencePlaneArea = value;
				NotifyPropertyChanged();
			}
		}

		private double _referencePlaneElevation = 0;
		public double ReferencePlaneElevation
		{
			get
			{
				return _referencePlaneElevation;
			}
			set
			{
				_referencePlaneElevation = value;
				NotifyPropertyChanged();
			}
		}

		private string _referencePlaneDirection = string.Empty;
		public string ReferencePlaneDirection
		{
			get
			{
				return _referencePlaneDirection;
			}
			set
			{
				_referencePlaneDirection = value;
				NotifyPropertyChanged();
			}
		}

		private int _selectedPolygonsCount = 0;
		public int SelectedPolygonsCount
		{
			get
			{
				return _selectedPolygonsCount;
			}
			set
			{
				_selectedPolygonsCount = value;
				NotifyPropertyChanged();
			}
		}

		private double _diffInVolume = 0;
		public double DiffInVolume
		{
			get
			{
				return _diffInVolume;
			}
			set
			{
				_diffInVolume = value;
				NotifyPropertyChanged();
			}
		}

		private double _diffInSurfaceArea = 0;
		public double DiffInSurfaceArea
		{
			get
			{
				return _diffInSurfaceArea;
			}
			set
			{
				_diffInSurfaceArea = value;
				NotifyPropertyChanged();
			}
		}

		private double _diffInWeight = 0;
		public double DiffInWeight
		{
			get
			{
				return _diffInWeight;
			}
			set
			{
				_diffInWeight = value;
				NotifyPropertyChanged();
			}
		}

		private string _pointCloudFile = string.Empty;
		public string PointCloudFile
		{
			get
			{
				return _pointCloudFile;
			}
			set
			{
				_pointCloudFile = value;
				NotifyPropertyChanged();
			}
		}

		private string _meshFile = string.Empty;
		public string MeshFile
		{
			get
			{
				return _meshFile;
			}
			set
			{
				_meshFile = value;
				NotifyPropertyChanged();
			}
		}

		private Visibility _dockpaneVisible = Visibility.Visible;
		/// <summary>
		/// Controls the visible state of the controls on the dockpane
		/// </summary>
		public Visibility DockpaneVisible
		{
			get
			{
				return _dockpaneVisible;
			}
			set { SetProperty(ref _dockpaneVisible, value, () => DockpaneVisible); }
		}


		#endregion

		#region Event Handlers

		private async void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs activeMapViewChangedEventArgs)
		{

			await QueuedTask.Run(() =>
			{
				// If no active mapview, then return
				if (MapView.Active == null) return;
				// Check for needed feature class
				var featLayer = MapView.Active.Map.FindLayers("Clip_Polygon_Asphalt").FirstOrDefault() as FeatureLayer;
				// If this key layer is not present, deactivate the dockpane
				if (featLayer == null)
				{
					// Deactivate
					FrameworkApplication.State.Deactivate("ShowPerMapPane");
				}
				else
				{
					FrameworkApplication.State.Activate("ShowPerMapPane");
				}
			});
		}

		// Called after the feature selection changed
		private void OnMapSelectionChanged(MapSelectionChangedEventArgs args)
		{

			Module1.Current.FeatureSelectionChanged();

		}

		#endregion Event Handlers


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

	}

	/// <summary>
	/// Button implementation to show the DockPane.
	/// </summary>
	internal class SceneCalcDockpane_ShowButton : Button
	{
		protected override void OnClick()
		{
			SceneCalcDockpaneViewModel.Show();
		}
	}

}
