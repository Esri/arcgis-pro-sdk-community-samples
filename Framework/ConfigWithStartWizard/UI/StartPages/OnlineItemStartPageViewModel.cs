/*

   Copyright 2017 Esri

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

namespace ConfigWithStartWizard.UI.StartPages
{
  internal class OnlineItemStartPageViewModel : StartPageViewModelBase
  {

    private string _contentType = "";
    private static string _activePortalUri = "";
    private static string _token = "";
    private ObservableCollection<OnlineResultItem> _results = new ObservableCollection<OnlineResultItem>();
    private int _selectedItemIndex = -1;
    private static readonly object _lock = new object();
    private bool _initialized = false;
    private OnlineResultItem _result = null;
    private SubscriptionToken _eventToken = null;


    public OnlineItemStartPageViewModel(string contentType)
    {
      BindingOperations.CollectionRegistering += BindingOperations_CollectionRegistering;
      _contentType = contentType;
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

    protected internal async virtual void ExecuteItemAction(string url)
    {
      _result = _results.FirstOrDefault(ri => ri.Id == url);
      if (_result == null) return;

      //Either we open a project and then create the item or
      //we download the item and then create a project from it.
      //MapPackage is a special case. We download it, create
      //a project and then add the map package to it.
      switch (_result.OnlineItemType)
      {
        case OnlineItemType.WebMap:
          {
            await Project.CreateAsync(new CreateProjectSettings()
            {
              Name = System.IO.Path.GetFileNameWithoutExtension(_result.Name)
            });
            var currentItem = ItemFactory.Instance.Create(_result.Id, ItemFactory.ItemType.PortalItem);
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
        case OnlineItemType.Layer:
          {
            var ps = new CreateProjectSettings()
            {
              Name = System.IO.Path.GetFileNameWithoutExtension(_result.Name),
              LocationPath = ConfigWithStartWizardModule.DefaultFolder(),
              TemplatePath = ConfigWithStartWizardModule.GetDefaultTemplates()[0] //Scene
            };
            _eventToken = ArcGIS.Desktop.Mapping.Events.MapViewInitializedEvent.Subscribe((args) =>
            {
              Item currentItem = ItemFactory.Instance.Create(_result.Id, ItemFactory.ItemType.PortalItem);
              if (LayerFactory.Instance.CanCreateLayerFrom(currentItem))
              {
                QueuedTask.Run(() => LayerFactory.Instance.CreateLayer(currentItem, args.MapView.Map));
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
              System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
          }
          break;
        default:
          var query = new OnlineQuery(_activePortalUri);
          query.ConfigureData("", _result.Id, _result.Name);

          await new SubmitQuery().DownloadFileAsync(query);

          //Project package
          if (_result.OnlineItemType == OnlineItemType.ProjectPackage)
          {
            await Project.OpenAsync(query.DownloadFileName);
          }
          //Project Template
          else if (_result.OnlineItemType == OnlineItemType.Template)
          {
            var ps = new CreateProjectSettings()
            {
              Name = System.IO.Path.GetFileNameWithoutExtension(_result.Name),
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
            }

          }
          //Map Package
          else if (_result.OnlineItemType == OnlineItemType.MapPackage)
          {
            await Project.CreateAsync(new CreateProjectSettings()
            {
              Name = System.IO.Path.GetFileNameWithoutExtension(_result.Name)
            });

            try
            {
              var mapPackage = ItemFactory.Instance.Create(query.DownloadFileName);
              MapFactory.Instance.CreateMapFromItem(mapPackage);
            }
            catch (Exception ex)
            {
              System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
          }
          break;
      }
    }

    protected internal override Task InitializeAsync()
    {
      if (_initialized)
        return Task.FromResult(0);
      return QueuedTask.Run(() =>
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
          var submitQuery = new SubmitQuery();
          submitQuery.Exec(query, _results, 100);
        }
      });
    }

    public override string Title => _contentType;

    public override string Name => _contentType;

    /// <summary>
    /// Gets the list of results from the query
    /// </summary>
    public ObservableCollection<OnlineResultItem> Results
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
