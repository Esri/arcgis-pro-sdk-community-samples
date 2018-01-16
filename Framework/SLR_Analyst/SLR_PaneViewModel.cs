/*

   Copyright 2018 Esri

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
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Core;
using System.Collections.ObjectModel;
using System.Windows;

namespace SLR_Analyst
{
	internal class SLR_PaneViewModel : ViewStatePane
	{
		private const string _viewPaneID = "SLR_Analyst_SLR_Pane";

		private ObservableCollection<KeyValueWithTooltip> _landusechartResult;
		private ObservableCollection<KeyValueWithTooltip> _parcelchartResult;
		private ObservableCollection<KeyValueWithTooltip> _streetchartResult;
		
		public bool CkbParcelChecked;
		public bool CkbLandUseChecked;
		public bool CkbStreetChecked;

		/// <summary>
		/// Consume the passed in CIMView. Call the base constructor to wire up the CIMView.
		/// </summary>
		public SLR_PaneViewModel(CIMView view)
			: base(view) {
			_landusechartResult = new ObservableCollection<KeyValueWithTooltip>();
			_parcelchartResult = new ObservableCollection<KeyValueWithTooltip>();
			_streetchartResult = new ObservableCollection<KeyValueWithTooltip>();
		}

		/// <summary>
		/// Create a new instance of the pane.
		/// </summary>
		internal static SLR_PaneViewModel Create()
		{
			var view = new CIMGenericView
			{
				ViewType = _viewPaneID
			};
			return FrameworkApplication.Panes.Create(_viewPaneID, new object[] { view }) as SLR_PaneViewModel;
		}

		#region Public Methods to update UI Content

		/// <summary>
		/// Updates the LandUse observable collection on the UI thread.  This is because of a bug in the
		/// WPFToolKit DataVisualization library which doesn't allow updating on a brackground thread
		/// </summary>
		/// <param name="keyValueWithTooltips"></param>
		public void UpdateLandUse(IList<KeyValueWithTooltip> keyValueWithTooltips)
		{
			LandUseChartResult.Clear();
			foreach (var kv in keyValueWithTooltips)
			{
				LandUseChartResult.Add(kv.Clone());
			}
		}

		/// <summary>
		/// Updates the Street observable collection on the UI thread.  This is because of a bug in the
		/// WPFToolKit DataVisualization library which doesn't allow updating on a brackground thread
		/// </summary>
		/// <param name="keyValueWithTooltips"></param>
		public void UpdateStreet(IList<KeyValueWithTooltip> keyValueWithTooltips)
		{
			StreetChartResult.Clear();
			foreach (var kv in keyValueWithTooltips)
			{
				StreetChartResult.Add(kv.Clone());
			}
		}

		/// <summary>
		/// Updates the Parcel observable collection on the UI thread.  This is because of a bug in the
		/// WPFToolKit DataVisualization library which doesn't allow updating on a brackground thread
		/// </summary>
		/// <param name="keyValueWithTooltips"></param>
		public void UpdateParcel(IList<KeyValueWithTooltip> keyValueWithTooltips)
		{
			ParcelChartResult.Clear();
			foreach (var kv in keyValueWithTooltips)
			{
				ParcelChartResult.Add(kv.Clone());
			}
		}

		#endregion Public Methods to update UI Content

		#region Public UI Properties

		/// <summary>
		/// Land Use Chart
		/// </summary>
		public ObservableCollection<KeyValueWithTooltip> LandUseChartResult
		{
			get
			{
				return _landusechartResult;
			}
			set
			{
				SetProperty(ref _landusechartResult, value, () => LandUseChartResult);
			}
		}

		/// <summary>
		/// Parcel Chart
		/// </summary>
		public ObservableCollection<KeyValueWithTooltip> ParcelChartResult
		{
			get
			{
				return _parcelchartResult;
			}
			set
			{
				SetProperty(ref _parcelchartResult, value, () => ParcelChartResult);
			}
		}

		/// <summary>
		/// Street Chart
		/// </summary>
		public ObservableCollection<KeyValueWithTooltip> StreetChartResult
		{
			get
			{
				return _streetchartResult;
			}
			set
			{
				SetProperty(ref _streetchartResult, value, () => StreetChartResult);
			}
		}

		public Visibility IsVisibleStreet => CkbStreetChecked ? Visibility.Visible : Visibility.Hidden;
		public Visibility IsVisibleParcel => CkbParcelChecked ? Visibility.Visible : Visibility.Hidden;
		public Visibility IsVisibleLandUse => CkbLandUseChecked ? Visibility.Visible : Visibility.Hidden;

		public void UpdateVisibility ()
		{
			NotifyPropertyChanged("IsVisibleStreet");
			NotifyPropertyChanged("IsVisibleParcel");
			NotifyPropertyChanged("IsVisibleLandUse");
		}

		private string _paneTitle;
		public string PaneTitle
		{
			get
			{
				return _paneTitle;
			}
			set
			{
				SetProperty(ref _paneTitle, value, () => PaneTitle);
			}
		}

		public string _paneConfiguration;
		public string PaneConfiguration
		{
			get
			{
				return _paneConfiguration;
			}
			set
			{
				SetProperty(ref _paneConfiguration, value, () => PaneConfiguration);
			}
		}

		/// <summary>
		/// Report Text Box Updates
		/// </summary>
		private string _reportText;
		public string ReportText
		{
			get { return _reportText; }
			set
			{
				SetProperty(ref _reportText, value, () => ReportText);
			}
		}

		#endregion Public UI Properties

		#region Pane Overrides

		/// <summary>
		/// Must be overridden in child classes used to persist the state of the view to the CIM.
		/// </summary>
		public override CIMView ViewState
		{
			get
			{
				_cimView.InstanceID = (int)InstanceID;
				return _cimView;
			}
		}

		/// <summary>
		/// Called when the pane is initialized.
		/// </summary>
		protected async override Task InitializeAsync()
		{
			await base.InitializeAsync();
		}

		/// <summary>
		/// Called when the pane is uninitialized.
		/// </summary>
		protected async override Task UninitializeAsync()
		{
			await base.UninitializeAsync();
		}

		#endregion Pane Overrides
	}

	/// <summary>
	/// Button implementation to create a new instance of the pane and activate it.
	/// </summary>
	internal class SLR_Pane_OpenButton : Button
	{
		protected override void OnClick()
		{
			SLR_PaneViewModel.Create();
		}
	}
}
