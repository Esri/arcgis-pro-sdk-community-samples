/*

   Copyright 2022 Esri

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
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WebViewBrowser
{
	internal class WebViewBrowserViewModel : ViewStatePane
	{
		private const string _viewPaneID = "WebViewBrowser_WebViewBrowser";
		private const string StartUri = "https://www.esri.com";
		private ObservableCollection<string> _NavList = new ObservableCollection<string>();
		private int _NavPosition = -1;

		public event EventHandler WebViewClicked;

		protected void OnWebViewClicked(EventArgs e)
		{
      WebViewClicked?.Invoke(this, e);
    }

    /// <summary>
    /// Consume the passed in CIMView. Call the base constructor to wire up the CIMView.
    /// </summary>
    public WebViewBrowserViewModel(CIMView view)
		  : base(view) 
		  {
			_NavList.CollectionChanged += _NavList_CollectionChanged;
			_NavList.Add(StartUri);
		  }

		private void _NavList_CollectionChanged(object sender,
					System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			var collection = sender as ObservableCollection<string>;
			switch (e.Action)
			{
				case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
					_NavPosition++;
					NotifyPropertyChanged(() => CanNavigateForward);
					NotifyPropertyChanged(() => CanNavigateBack);
					break;
				case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
					_NavPosition = collection.Count-1;
					NotifyPropertyChanged(() => CanNavigateForward);
					NotifyPropertyChanged(() => CanNavigateBack);
					break;
			}
		}

		private Uri _sourceUri = new Uri(StartUri);
		/// <summary>
		/// SourceUri is used to interface with the WebViewBrowser control
		/// </summary>
		public Uri SourceUri
		{
			get { return _sourceUri; }
			set
			{
				SetProperty(ref _sourceUri, value, () => SourceUri);
				if (_sourceUri.AbsoluteUri != _navInput)
				{
					_navInput = _sourceUri.AbsoluteUri;
					NotifyPropertyChanged(() => NavInput);
				}
			}
		}

		private string _navInput = "https://www.esri.com";
		/// <summary>
		/// NavInput is used to provide a text input field for navigation in the UI
		/// </summary>
		public string NavInput
		{
			get { return _navInput; }
			set
			{
				SetProperty(ref _navInput, value, () => NavInput);
			}
		}

		public ICommand SearchEnterHit
		{
			get
			{
				return new RelayCommand(() =>
				{
					if (!string.IsNullOrEmpty(NavInput))
					{
						try
						{
							SourceUri = new Uri(NavInput);
							// remove navigation track tail
							for (int idx = _NavPosition+1; idx > 0 && idx < _NavList.Count; idx++)
							{
								_NavList.RemoveAt(idx);
							}
							_NavList.Add(_navInput);
						}
						catch (Exception ex)
						{
							System.Diagnostics.Debug.WriteLine(ex.Message);
						}
					}
				});
			}
		}

		public ICommand CmdNavigateBack
		{
			get
			{
				return new RelayCommand(() =>
				{
					if (_NavPosition > 0)
					{
						SourceUri = new Uri(_NavList[--_NavPosition]);
						NotifyPropertyChanged(() => CanNavigateForward);
						NotifyPropertyChanged(() => CanNavigateBack);
					}
				});
			}
		}

		public bool CanNavigateBack
		{
			get
			{
				return _NavPosition > 0;
			}
		}


		public ICommand CmdNavigateForward
		{
			get
			{
				return new RelayCommand(() =>
				{
					if (_NavPosition < _NavList.Count - 1)
					{ 
						SourceUri = new Uri(_NavList[++_NavPosition]);
						NotifyPropertyChanged(() => CanNavigateForward);
						NotifyPropertyChanged(() => CanNavigateBack);
					}
				});
			}
		}

		public bool CanNavigateForward
		{
			get
			{
				return _NavPosition < _NavList.Count - 1;
			}
		}

		/// <summary>
		/// Create a new instance of the pane.
		/// </summary>
		internal static WebViewBrowserViewModel Create()
		{
			var view = new CIMGenericView
			{
				ViewType = _viewPaneID
			};
			return FrameworkApplication.Panes.Create(_viewPaneID, new object[] { view }) as WebViewBrowserViewModel;
		}

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
	internal class WebViewBrowser_OpenButton : Button
	{
		protected override void OnClick()
		{
			WebViewBrowserViewModel.Create();
		}
	}
}
