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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Portal;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;


namespace GetAddins.Backstage
{
    internal class GetAddinsBackstageViewModel : BackstageTab
    {
        private static readonly object _lock = new object();
        public GetAddinsBackstageViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(_addInsCollection, _lock);
            _visibleList = Visibility.Visible;
        }

        /// <summary>
        /// Called when the backstage tab is selected.
        /// </summary>
        protected async override Task InitializeAsync()
        {

            _addInsCollection = await GetAddinsAsync();

        }

        /// <summary>
        /// Called when the backstage tab is unselected.
        /// </summary>
        protected override Task UninitializeAsync()
        {
            return base.UninitializeAsync();
        }

        private string _tabHeading = "Add-ins";
        public string TabHeading
        {
            get
            {
                return _tabHeading;
            }
            set
            {
                SetProperty(ref _tabHeading, value, () => TabHeading);
            }
        }

        private ObservableCollection<AddInItem> _addInsCollection = new ObservableCollection<AddInItem>();
        /// <summary>
        /// Gets the list of results from the query
        /// </summary>
        public ObservableCollection<AddInItem> AddInsCollection
        {
            get
            {
                return _addInsCollection;
            }          
        }

        private string _arcgisOnline = @"http://www.arcgis.com:80/";
        public string PortalUrl
        {
            get { return _arcgisOnline; }
            set
            {
                SetProperty(ref _arcgisOnline, value, () => PortalUrl);
            }
        }

        

        private AddInItem _selectedItem;
        public AddInItem SelectedItem
        {
            get
            { return _selectedItem; }
            set
            {
                SetProperty(ref _selectedItem, value, () => SelectedItem);

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
        private PortalQueryParameters query;

        /// <summary>
        /// Gets a collection of web map items from ArcGIS Online
        /// </summary>
        /// <returns></returns>
        private async Task<ObservableCollection<AddInItem>> GetAddinsAsync()
        {         
            try
            {
                await QueuedTask.Run(async () =>
                {
                    ArcGISPortal portal = ArcGISPortalManager.Current.GetPortal(new Uri(_arcgisOnline));
                    query = PortalQueryParameters.CreateForItemsOfType(PortalItemType.ArcGISProAddIn);

                    PortalQueryResultSet<PortalItem> results = await ArcGIS.Desktop.Core.ArcGISPortalExtensions.SearchForContentAsync(portal, query);

                    if (results == null)
                        return;

                    foreach (var item in results.Results.OfType<PortalItem>())
                    {
                        lock(_lock)
                            AddInsCollection.Add(new AddInItem(item));                        
                        
                    }
                });

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return _addInsCollection;
        }

        protected internal async virtual void ExecuteItemAction(string id)
        {
            _selectedItem = _addInsCollection.FirstOrDefault(ri => ri.ID == id);
            if (_selectedItem == null) return;

            try
            {
                var downloadFileName =  System.IO.Path.Combine(
                                                 System.IO.Path.Combine(
                                                     Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"ArcGIS\AddIns\ArcGISPro"), _selectedItem.Name);
                await _selectedItem.AddInPortalItem.GetItemDataAsync(downloadFileName);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

            }
                    
        }
    }
}

