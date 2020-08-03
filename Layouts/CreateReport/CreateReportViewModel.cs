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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
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
using ArcGIS.Desktop.Reports;

namespace CreateReport
{
	internal class CreateReportViewModel : DockPane
	{
		private const string _dockPaneID = "CreateReport_CreateReport";
		private object _lock = new object();
		Map _activeMap;

		protected CreateReportViewModel()
		{
			System.Windows.Data.BindingOperations.EnableCollectionSynchronization(_layers, _lock);
		}
		/// <summary>
		/// Called when the dock pane is first initialized.
		/// </summary>
		protected override async Task InitializeAsync()
		{
			if (MapView.Active == null)
				return;

			await UpdateCollectionsAsync();
			await base.InitializeAsync();
		}


		#region Binding properties
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
		private string _heading = "Create Report";
		public string Heading
		{
			get { return _heading; }
			set
			{
				SetProperty(ref _heading, value, () => Heading);
			}
		}
		private ObservableCollection<FeatureLayer> _layers = new ObservableCollection<FeatureLayer>();
		/// <summary>
		/// Collection of layers in the active map
		/// </summary>
		public ObservableCollection<FeatureLayer> Layers
		{
			get { return _layers; }
		}
		private FeatureLayer _selectedLayer;
		/// <summary>
		/// Selected feature layer
		/// </summary>
		public FeatureLayer SelectedLayer
		{
			get { return _selectedLayer; }
			set
			{
				SetProperty(ref _selectedLayer, value, () => SelectedLayer);
				_ = GetFieldsAsync();
			}
		}
		private ObservableCollection<ReportField> _reportFields = new ObservableCollection<ReportField>();
		/// <summary>
		/// Collection of fields in the layer used to generate the report
		/// </summary>
		public ObservableCollection<ReportField> ReportFields
		{
			get { return _reportFields; }
		}


		private string _reportName;

		/// <summary>
		/// The name of the report project item created
		/// </summary>
		public string ReportName
		{
			get { return _reportName; }
			set { SetProperty(ref _reportName, value, () => ReportName); }
		}
		private ObservableCollection<string> _reportTemplates = new ObservableCollection<string>();
		/// <summary>
		/// Available report templates
		/// </summary>
		public ObservableCollection<string> ReportTemplates
		{
			get { return _reportTemplates; }
			set { SetProperty(ref _reportTemplates, value, () => ReportTemplates); }
		}
		private string _selectedReportTemplate;
		/// <summary>
		/// Selected report template
		/// </summary>
		public string SelectedReportTemplate
		{
			get { return _selectedReportTemplate; }
			set { SetProperty(ref _selectedReportTemplate, value, () => SelectedReportTemplate); }
		}
		private ObservableCollection<string> _reportStyles = new ObservableCollection<string>();
		/// <summary>
		/// Available Report styles
		/// </summary>
		public ObservableCollection<string> ReportStyles
		{
			get { return _reportStyles; }
			set { SetProperty(ref _reportStyles, value, () => ReportStyles); }
		}
		private string _selectedStyle;
		/// <summary>
		/// Selected Report Style
		/// </summary>
		public string SelectedReportStyle
		{
			get { return _selectedStyle; }
			set { SetProperty(ref _selectedStyle, value, () => SelectedReportStyle); }
		}

		private ObservableCollection<string> _statsOptions = new ObservableCollection<string>();
		/// <summary>
		/// Stats options
		/// </summary>
		public ObservableCollection<string> StatsOptions
		{
			get { return _statsOptions; }
			set { SetProperty(ref _statsOptions, value, () => StatsOptions); }
		}

		private string _selectedStatsOption;
		/// <summary>
		/// Selected statistics
		/// </summary>
		public string SelectedStatsOption
		{
			get { return _selectedStatsOption; }
			set { SetProperty(ref _selectedStatsOption, value, () => SelectedStatsOption); }

		}
		private ObservableCollection<ReportField> _selectedFields = new ObservableCollection<ReportField>();
		/// <summary>
		/// Represents the selected fields which is a subset of the ReportFields. This is being used by the Group Fields and Stats Fields drop downs.
		/// </summary>
		public ObservableCollection<ReportField> SelectedFields
		{
			get { return _selectedFields; }
			set { SetProperty(ref _selectedFields, value, () => SelectedFields); }
		}

		private ReportField _selectedGroupField;
		/// <summary>
		/// Selected Group field
		/// </summary>
		public ReportField SelectedGroupField
		{
			get { return _selectedGroupField; }
			set
			{
				SetProperty(ref _selectedGroupField, value, () => SelectedGroupField);
				//Now make the set of templates to display only grouping types or non grouping types
				ReportTemplates.Clear();
				if (SelectedGroupField == null)
					ReportTemplates = new ObservableCollection<string>(_nonGroupingTemplates);
				else
					ReportTemplates = new ObservableCollection<string>(_groupingTemplates);

				SelectedReportTemplate = ReportTemplates[0];
			}
		}

		private ReportField _selectedStatsField;
		/// <summary>
		/// Selected Statistic field
		/// </summary>
		public ReportField SelectedStatsField
		{
			get { return _selectedStatsField; }
			set
			{
				SetProperty(ref _selectedStatsField, value, () => SelectedStatsField);
				//Gets stats collection
				//Clear it first
				StatsOptions.Clear();
				var statInfo = Enum.GetValues(typeof(FieldStatisticsFlag));
				foreach (FieldStatisticsFlag stat in statInfo)
				{
					//if (stat == FieldStatisticsFlag.NoStatistics)
					//    continue;
					StatsOptions.Add(stat.ToString());
				}
				SelectedStatsOption = StatsOptions[0];
			}
		}


		private bool _isUseSelection;
		/// <summary>
		/// Check box binding to use the selected features in map
		/// </summary>
		public bool IsUseSelection
		{
			get { return _isUseSelection; }
			set { SetProperty(ref _isUseSelection, value, () => IsUseSelection); }
		}
		private bool _isSelectAll;
		/// <summary>
		/// Check box binding to select all the fields
		/// </summary>
		public bool IsSelectAll
		{
			get { return _isSelectAll; }
			set
			{
				SetProperty(ref _isSelectAll, value, () => IsSelectAll);
				if (!IsSelectAll)
				{
					foreach (var rptFld in ReportFields)
					{
						rptFld.IsSelected = false;
					}
				}
				else
				{
					foreach (var rptFld in ReportFields)
					{
						rptFld.IsSelected = true;
					}
				}


			}
		}

		private bool _isCreateReport;

		#endregion
		#region Commands

		private ICommand _clearGroupingCommand;
		public ICommand ClearGroupingCommand
		{
			get
			{
				_clearGroupingCommand = new RelayCommand(() => ClearGrouping(), () => { return SelectedGroupField != null; });
				return _clearGroupingCommand;
			}
		}

		private void ClearGrouping()
		{
			if (SelectedGroupField != null)
				SelectedGroupField = null;
		}

		private ICommand _clearStatsFieldsCommand;
		public ICommand ClearStatsFieldsCommand
		{
			get
			{
				_clearStatsFieldsCommand = new RelayCommand(() => ClearStatsFields(), () => { return SelectedStatsField != null; });
				return _clearStatsFieldsCommand;
			}
		}

		private void ClearStatsFields()
		{
			if (SelectedStatsField != null)
				SelectedStatsField = null;
			StatsOptions.Clear();
		}

		private ICommand _createReportCmd;
		/// <summary>
		/// The button command to create the report
		/// </summary>
		public ICommand CreateReportCommand
		{
			get
			{
				_createReportCmd = new RelayCommand(() => CreateReport(), () => { return MapView.Active != null && SelectedLayer != null; });
				return _createReportCmd;
			}
		}
		private ICommand _exportReportCmd;

		/// <summary>
		/// The button command to export the report
		/// </summary>
		public ICommand ExportReportCommand
		{
			get
			{
				_exportReportCmd = new RelayCommand(() => ExportReport(), () => { return _isCreateReport; });
				return _exportReportCmd;
			}
		}
		#endregion


		/// <summary>
		/// Creates the report
		/// </summary>
		/// <returns></returns>
		private async Task CreateReport()
		{
			ReportDataSource reportDataSource = null;
			Report report = null;
			_isCreateReport = false;
			var reportTemplate = await GetReportTemplate(SelectedReportTemplate);
			await QueuedTask.Run(() =>
			{
							//Adding all fields to the report
							List<CIMReportField> reportFields = new List<CIMReportField>();
				var selectedReportFields = ReportFields.Where((fld) => fld.IsSelected);
				foreach (var rfld in selectedReportFields)
				{
					var cimReportField = new CIMReportField() { Name = rfld.Name };
					reportFields.Add(cimReportField);
								//Defining grouping field
								if (rfld.Name == SelectedGroupField?.Name)
						cimReportField.Group = true;
								//To DO: Do sort info here.
							}

							//Report field statistics
							List<ReportFieldStatistic> reportFieldStats = new List<ReportFieldStatistic>();

				if (SelectedStatsField != null)
				{
					ReportFieldStatistic reportStat = new ReportFieldStatistic();
					reportStat.Field = SelectedStatsField.Name;
					reportStat.Statistic = (FieldStatisticsFlag)Enum.Parse(typeof(FieldStatisticsFlag), SelectedStatsOption);
					reportFieldStats.Add(reportStat);
				}

							//Set Datasource
							reportDataSource = new ReportDataSource(SelectedLayer, "", IsUseSelection, reportFields);

				try
				{
					report = ReportFactory.Instance.CreateReport(ReportName, reportDataSource, null, reportFieldStats, reportTemplate, SelectedReportStyle);
					_isCreateReport = true;
				}
				catch (System.InvalidOperationException e)
				{
					if (e.Message.Contains("Group field defined for a non-grouping template"))
					{
						MessageBox.Show("A group field cannot be defined for a non-grouping template.");
					}
					else if (e.Message.Contains("Grouping template specified but no group field defined"))
					{
						MessageBox.Show("A group field should be defined for a grouping template.");
					}
				}
			});
		}
		/// <summary>
		/// Exports report
		/// </summary>
		/// <returns></returns>
		private async Task ExportReport()
		{
			ReportProjectItem reportProjItem = Project.Current.GetItems<ReportProjectItem>().FirstOrDefault(item => item.Name.Equals(ReportName));
			Report report = null;
			await QueuedTask.Run(() => report = reportProjItem?.GetReport());
			if (report == null)
			{
				MessageBox.Show($"{ReportName} report not found.");
				return;
			}

			//Define Export Options
			var exportOptions = new ReportExportOptions
			{
				ExportPageOption = ExportPageOptions.ExportAllPages,
				TotalPageNumberOverride = 12,
				StartingPageNumberLabelOffset = 0

			};
			//Create PDF format with appropriate settings
			PDFFormat pdfFormat = new PDFFormat();
			pdfFormat.Resolution = 300;
			pdfFormat.OutputFileName = Path.Combine(Project.Current.HomeFolderPath, report.Name);
			await ExportAReportToPdf(report, pdfFormat, exportOptions, IsUseSelection);
			MessageBox.Show($"{ReportName} report exported to {pdfFormat.OutputFileName}");


		}

		#region Private methods and props

		private List<string> _groupingTemplates = new List<string>();
		private List<string> _nonGroupingTemplates = new List<string>();

		private async Task ExportAReportToPdf(Report report, PDFFormat pdfFormat, ReportExportOptions reportExportOptions, bool isUseSelectionSet = true)
		{
			await QueuedTask.Run(() =>
			{

				report?.ExportToPDF(ReportName, pdfFormat, reportExportOptions, isUseSelectionSet);
			});
		}
		private async Task<ReportTemplate> GetReportTemplate(string reportName)
		{
			var reportTemplates = await ReportTemplateManager.GetTemplatesAsync();
			return reportTemplates.Where(r => r.Name == reportName).First();
		}

		private async Task GetFieldsAsync()
		{
			ReportFields.Clear();
			SelectedFields.Clear();
			if (SelectedLayer == null)
				return;

			await QueuedTask.Run((Action)(() =>
			{
				foreach (FieldDescription fd in SelectedLayer?.GetFieldDescriptions())
				{
					string shapeField = SelectedLayer.GetFeatureClass().GetDefinition().GetShapeField();
					if (fd.Name == shapeField) continue; //filter out the shape field.
								var defFieldAction = (Action)(() =>
								{
								var field = new ReportField { IsSelected = false, DisplayName = fd.Alias, Name = fd.Name };
								ReportFields.Add(field);
								field.FieldSelectionChanged += this.Field_FieldSelectionChanged;
											//field.IsSelected = true;
										});
					ActionOnGuiThread(defFieldAction);
				}
			}));
		}

		private void Field_FieldSelectionChanged(object sender, FieldSelectionChangedEventArgs e)
		{
			var reportField = e.ChangedReportField;
			if (reportField.IsSelected)
				SelectedFields.Add(new ReportField { DisplayName = reportField.DisplayName, Name = reportField.Name });
			else
				SelectedFields.Remove(SelectedFields.Where((fld) => fld.Name == reportField.Name).FirstOrDefault());
		}

		public async Task UpdateCollectionsAsync()
		{
			_activeMap = MapView.Active.Map;
			//Get the layers in active map
			GetLayers();

			//Get the report Templates
			await QueuedTask.Run(() =>
			{
							//Creating a collection of grouping and non grouping report templates
							foreach (ReportTemplate reportTemplate in ReportTemplateManager.GetTemplates())
				{
					if (reportTemplate.Name.Contains("Grouping"))
						_groupingTemplates.Add(reportTemplate.Name);
					else
						_nonGroupingTemplates.Add(reportTemplate.Name);
				}
			});
			//Initialize the report template collection with non grouping styles - since non grouping field is selected.
			ReportTemplates = new ObservableCollection<string>(_nonGroupingTemplates);
			SelectedReportTemplate = ReportTemplates[0];
			//Get the report Styles
			await QueuedTask.Run(() =>
			{
				foreach (string reportStyle in ReportStylingManager.GetStylings())
				{
					var defReportStyleAction = (Action)(() =>
								{
								ReportStyles.Add(reportStyle);
							});
					ActionOnGuiThread(defReportStyleAction);
				}
			});
			SelectedReportStyle = ReportStyles[0];

		}
		private void GetLayers()
		{
			Layers.Clear();
			if (_activeMap == null)
				return;
			ReportName = $"Report_{_activeMap.Name}";
			var layers = _activeMap.GetLayersAsFlattenedList().OfType<FeatureLayer>();
			//if (layers.Count == 0)
			//    return;
			lock (_lock)
			{
				foreach (var layer in layers)
				{
					lock (_lock)
						Layers.Add(layer);
				}
			}
			SelectedLayer = Layers.Count > 0 ? Layers[0] : null;
		}

		/// <summary>
		/// We have to ensure that GUI updates are only done from the GUI thread.
		/// </summary>
		private void ActionOnGuiThread(Action theAction)
		{
			if (System.Windows.Application.Current.Dispatcher.CheckAccess())
			{
				//We are on the GUI thread
				theAction();
			}
			else
			{
				//Using the dispatcher to perform this action on the GUI thread.
				ProApp.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, theAction);
			}
		}
		#endregion
	}

	/// <summary>
	/// Button implementation to show the DockPane.
	/// </summary>
	internal class CreateReport_ShowButton : Button
	{
		protected override void OnClick()
		{
			CreateReportViewModel.Show();
		}
	}
}
