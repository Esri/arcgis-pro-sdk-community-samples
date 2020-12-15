using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Controls;

namespace SymbolSearcherControl
{
	internal class SymbolPickerViewModel : DockPane
	{
		private const string _dockPaneID = "SymbolSearcherControl_SymbolPicker";

		protected SymbolPickerViewModel() {


		}

		protected override Task InitializeAsync()
		{
			return QueuedTask.Run(() =>
			{
				//Search for symbols in the selected style
				StyleProjectItem si = Project.Current.GetItems<StyleProjectItem>().FirstOrDefault();
				if (si != null)
				{
					var lstStyleItems = si.SearchSymbols(StyleItemType.PointSymbol, string.Empty).Select((s) => s as StyleItem);
					RunOnUIThread(() =>
					{
						_pickerStyleItems = new ObservableCollection<StyleItem>();
						_pickerStyleItems.AddRange(lstStyleItems);
						NotifyPropertyChanged(() => PickerStyleItems);
					});
				}
			});
		}

		private ObservableCollection<StyleItem> _pickerStyleItems;

		public ObservableCollection<StyleItem> PickerStyleItems
		{
			get { return _pickerStyleItems; }
			set
			{
				SetProperty(ref _pickerStyleItems, value, () => PickerStyleItems);
			}
		}

		private StyleItem _selectedPickerStyleItem;

		public StyleItem SelectedPickerStyleItem
		{
			get { return _selectedPickerStyleItem; }
			set
			{
				SetProperty(ref _selectedPickerStyleItem, value, () => SelectedPickerStyleItem);
				MessageBox.Show($@"SelectedPickerStyleItem: {_selectedPickerStyleItem?.Name}");
			}
		}

		private PickerViewOption _viewingOption;

		public PickerViewOption ViewingOption
		{
			get { return _viewingOption; }
			set
			{
				SetProperty(ref _viewingOption, value, () => ViewingOption);
			}
		}

		private PickerGroupOption _groupingOption;

		public PickerGroupOption GroupingOption
		{
			get { return _groupingOption; }
			set
			{
				SetProperty(ref _groupingOption, value, () => GroupingOption);
			}
		}

		private bool _showOptionsDropDown;

		public bool ShowOptionsDropDown
		{
			get { return _showOptionsDropDown; }
			set
			{
				SetProperty(ref _showOptionsDropDown, value, () => ShowOptionsDropDown);
			}
		}

		#region Helpers

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

		#endregion Helpers

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
		private string _heading = "Standalone SymbolPicker control";
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
	internal class SymbolPicker_ShowButton : Button
	{
		protected override void OnClick()
		{
			SymbolPickerViewModel.Show();
		}
	}
}
