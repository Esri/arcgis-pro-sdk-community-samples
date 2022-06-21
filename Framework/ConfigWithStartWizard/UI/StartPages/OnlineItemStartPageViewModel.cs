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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using ArcGIS.Core.Events;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ConfigWithStartWizard.Common;
using ConfigWithStartWizard.Models;
using ArcGIS.Desktop.Core.Portal;
using System.Windows;

namespace ConfigWithStartWizard.UI.StartPages
{
  internal class OnlineItemStartPageViewModel : StartPageViewModelBase
  {

    private string _contentType = "";
    private static string _activePortalUri = "";
    private static string _token = "";
    private ObservableCollection<OnlinePortalItem> _results = new ObservableCollection<OnlinePortalItem>();
    private int _selectedItemIndex = -1;
    private static readonly object _lock = new object();
    private bool _initialized = false;
    private OnlinePortalItem _selectedResult = null;
    private SubscriptionToken _eventToken = null;
		
    public OnlineItemStartPageViewModel(string contentType)
    {
      BindingOperations.CollectionRegistering += BindingOperations_CollectionRegistering;
      _contentType = contentType;
			_visibleList = Visibility.Visible;
			_visibleText = Visibility.Collapsed;
		}

		public OnlineItemStartPageViewModel()
		{
			_visibleList = Visibility.Visible;
			_visibleText = Visibility.Collapsed;
		}

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

		void BindingOperations_CollectionRegistering(object sender, CollectionRegisteringEventArgs e)
    {
      if (!Equals(e.Collection, Results)) return;
      BindingOperations.EnableCollectionSynchronization(_results, _lock);
      _results.CollectionChanged += _results_CollectionChanged;
    }

    private void _results_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.Action == NotifyCollectionChangedAction.Remove)
        this.SelectedItemIndex = -1;
    }

    protected internal async virtual void ExecuteItemAction(string id)
    {
      _selectedResult = _results.FirstOrDefault(ri => ri.Id == id);
      if (_selectedResult == null) return;

			//Either we open a project and then create the item or
			//we download the item and then create a project from it.
			//MapPackage is a special case. We download it, create
			//a project and then add the map package to it.

			VisibleList = Visibility.Collapsed;
			VisibleText = Visibility.Visible;
			StatusTitle = $@"{_selectedResult.LinkText}: {_selectedResult.Title}";

			switch (_selectedResult.PortalItemType)
      {
        case PortalItemType.WebMap:
          {
            await Project.CreateAsync(new CreateProjectSettings()
            {
              Name = System.IO.Path.GetFileNameWithoutExtension(_selectedResult.Name)
            });
            var currentItem = ItemFactory.Instance.Create(_selectedResult.Id, ItemFactory.ItemType.PortalItem);
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
            var ps = new CreateProjectSettings()
            {
              Name = System.IO.Path.GetFileNameWithoutExtension(_selectedResult.Name),
              LocationPath = ConfigWithStartWizardModule.DefaultFolder(),
              TemplateType = TemplateType.GlobalScene
            };
            _eventToken = ArcGIS.Desktop.Mapping.Events.MapViewInitializedEvent.Subscribe((args) =>
            {
              Item currentItem = ItemFactory.Instance.Create(_selectedResult.Id, ItemFactory.ItemType.PortalItem);
              if (LayerFactory.Instance.CanCreateLayerFrom(currentItem))
              {
                var lyrParams = new LayerCreationParams(currentItem);
                QueuedTask.Run(() => LayerFactory.Instance.CreateLayer<FeatureLayer>(lyrParams, args.MapView.Map));
              }
              ArcGIS.Desktop.Mapping.Events.MapViewInitializedEvent.Unsubscribe(_eventToken);
              _eventToken = null;
            });
            try
            {
              await Project.CreateAsync(ps);
            }
            catch (Exception ex)
            {
							Status = $@"Error occurred: {ex.ToString()}";
							System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
          }
          break;
        default:
          var query = new OnlineQuery(_activePortalUri);
          query.ConfigureData("", _selectedResult.Id, _selectedResult.Name);
					Status = "Downloading";
					await new SubmitQuery().DownloadFileAsync(query);
					Status = "Opening";
					//Project package
					if (_selectedResult.PortalItemType == PortalItemType.ProjectPackage)
          {
            await Project.OpenAsync(query.DownloadFileName);
          }
          //Project Template
          else if (_selectedResult.PortalItemType == PortalItemType.MapTemplate)
          {
            var ps = new CreateProjectSettings()
            {
              Name = System.IO.Path.GetFileNameWithoutExtension(_selectedResult.Name),
              LocationPath = ConfigWithStartWizardModule.DefaultFolder(),
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
          else if (_selectedResult.PortalItemType == PortalItemType.MapPackage)
          {
            await Project.CreateAsync(new CreateProjectSettings()
            {
              Name = System.IO.Path.GetFileNameWithoutExtension(_selectedResult.Name)
            });

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

    protected internal override Task InitializeAsync()
    {
      if (_initialized)
        return Task.FromResult(0);
      return QueuedTask.Run( () =>
      {
        if (string.IsNullOrEmpty(_activePortalUri))
        {
          var portal = ArcGISPortalManager.Current.GetActivePortal();
          _activePortalUri = portal?.PortalUri.ToString();
          _token = portal?.GetToken();
        }
        if (!string.IsNullOrEmpty(_activePortalUri))
        {
          var query = new OnlineQuery(_activePortalUri);
          query.Configure(_contentType);
          //This is where you decide what the Portalitem type is.
          var submitQuery = new SubmitQuery();
          submitQuery.Exec2Async(query, _results, _activePortalUri, 100);
        }
      });
    }

    public override string Title => _contentType;

    public override string Name => _contentType;

    /// <summary>
    /// Gets the list of results from the query
    /// </summary>
    public ObservableCollection<OnlinePortalItem> Results
    {
      get
      {
        return _results;
      }
    }

    public int SelectedItemIndex
    {
      get
      {
        return _selectedItemIndex;
      }
      set
      {
        _selectedItemIndex = value;
        if (_selectedItemIndex >= 0)
        {
          var item = _results[_selectedItemIndex];
          this.ExecuteItemAction(item.Id);
        }
        NotifyPropertyChanged();
      }
    }

    public void ClearResults()
    {
      lock (_lock)
      {
        _results.Clear();
      }
    }
  }
}
