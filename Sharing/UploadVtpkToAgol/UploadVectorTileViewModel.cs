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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Input;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Portal;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using UploadVtpkToAgol;

namespace UploadVtpkToAgol
{
    internal class UploadVectorTileViewModel : DockPane
    {
        private const string DockPaneId = "UploadVtpkToAgol_UploadVectorTile";

        protected UploadVectorTileViewModel() { }
        private RelayCommand _browseFileNameCmd;
        private RelayCommand _uploadCmd;
        private RelayCommand _queryCmd;
        private RelayCommand _addToMapCmd;

        public ICommand BrowseFileNameCmd
        {
            get { return _browseFileNameCmd ?? (_browseFileNameCmd = new RelayCommand(() => this.BrowseFileName())); }
        }

        public ICommand UploadCmd
        {
            get { return _uploadCmd ?? (_uploadCmd = new RelayCommand(async () => UploadStatus = await this.Upload())); }
        }

        public ICommand QueryCmd
        {
            get { return _queryCmd ?? (_queryCmd = new RelayCommand(async () => UploadStatus = await this.Query())); }
        }

        public ICommand AddToMapCmd
        {
            get { return _addToMapCmd ?? (_addToMapCmd = new RelayCommand(async () => await this.AddToMap())); }
        }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            var pane = FrameworkApplication.DockPaneManager.Find(DockPaneId);
            pane?.Activate();
        }

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = @"Show & Upload Vector Tile";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }

        private string _selectFileLabel = "Select a VTPK File";
        public string SelectFileLabel
        {
            get { return _selectFileLabel; }
            set
            {
                SetProperty(ref _selectFileLabel, value, () => SelectFileLabel);
            }
        }

        private string _filePath = "";
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                SetProperty(ref _filePath, value, () => FilePath);
            }
        }

        private string _uploadStatus = string.Empty;
        public string UploadStatus
        {  get { return _uploadStatus; } set { SetProperty(ref _uploadStatus, value, () => UploadStatus); } }

        private void BrowseFileName()
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog()
            {

                // Set filter for file extension and default file extension 
                DefaultExt = ".vtpk",
                Filter = "Vector Tile Package File (*.vtpk)|*.vtpk"
            };

            // Display OpenFileDialog by calling ShowDialog method 
            var result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result != true) return;
            // Open document 
            var filename = dlg.FileName;
            FilePath = filename;
        }

        private Task AddToMap()
        {
            return QueuedTask.Run(() =>
            {
                try
                {
                    // first we create an 'Item' using itemfactory
                    Item currentItem = ItemFactory.Instance.Create(FilePath);

                    // Finally add the feature service to the map
                    // if we have an item that can be turned into a layer
                    // add it to the map
                    if (LayerFactory.Instance.CanCreateLayerFrom(currentItem))
                        LayerFactory.Instance.CreateLayer(currentItem, MapView.Active.Map);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error while adding vector tile to map");
                }
            });
        }

        private Task<string> Upload()
        {
            return QueuedTask.Run(async () => await UploadImpl());
        }

        private async Task<string> UploadImpl()
        {
            // Create EsriHttpClient object
            var httpClient = new EsriHttpClient();

            // Upload vtpk file to the currently active portal
            var itemToUpload = ItemFactory.Instance.Create(FilePath);
            var tags = new string[] { "ArcGIS Pro", "SDK", "UploadVtpkToAgol Demo" };
            var portalUrl = ArcGISPortalManager.Current.GetActivePortal().PortalUri.ToString();

            var uploadDefn = new UploadDefinition(portalUrl, itemToUpload, tags);

            var result = httpClient.Upload(uploadDefn);
            if (result.Item1 == false)
                return $@"Unable to upload this item: {FilePath} to ArcGIS Online";

            // Once uploaded make another REST call to search for the uploaded data
            var portal = ArcGISPortalManager.Current.GetActivePortal();
            string userName = ArcGISPortalManager.Current.GetActivePortal().GetSignOnUsername();

            //Searching for Vector tile packages in the current user's content
            var pqp = PortalQueryParameters.CreateForItemsOfTypeWithOwner(PortalItemType.VectorTilePackage, userName, "tags: UploadVtpkToAgol Demo");

            //Execute to return a result set
            PortalQueryResultSet<PortalItem> results = await ArcGISPortalExtensions.SearchForContentAsync(portal, pqp);

            if (results.Results.Count == 0)
                return $@"Unable to find uploaded item with query: {pqp.Query}";

            // Create an item from the search results
            string itemId = results.Results[0].ID;
            var currentItem = ItemFactory.Instance.Create(itemId, ItemFactory.ItemType.PortalItem);

            // Finally add the feature service to the map
            // if we have an item that can be turned into a layer
            // add it to the map
            if (LayerFactory.Instance.CanCreateLayerFrom(currentItem))
                LayerFactory.Instance.CreateLayer(currentItem, MapView.Active.Map);
            return $@"Uploaded this item: {results.Results[0].Name} [Type: {results.Results[0].Type}] to ArcGIS Online and added the item to the Map";
         
        }


        private Task<string> Query()
        {
            return QueuedTask.Run(async () => await QueryImpl());
        }

        private async Task<string> QueryImpl()
        {

            var portal = ArcGISPortalManager.Current.GetActivePortal();

            string userName = ArcGISPortalManager.Current.GetActivePortal().GetSignOnUsername();

            //Searching for Vector tile packages in the current user's content
            var pqp = PortalQueryParameters.CreateForItemsOfTypeWithOwner(PortalItemType.VectorTilePackage, userName, "tags: UploadVtpkToAgol Demo");

            //Execute to return a result set
            PortalQueryResultSet<PortalItem> results = await ArcGISPortalExtensions.SearchForContentAsync(portal, pqp);


            if (results.Results.Count == 0)
                return $@"Unable to find uploaded item with query: {pqp.Query}";

            // Create an item from the search results
            string itemId = results.Results[0].ID;
            var currentItem = ItemFactory.Instance.Create(itemId, ItemFactory.ItemType.PortalItem);

            // Finally add the feature service to the map
            // if we have an item that can be turned into a layer
            // add it to the map
            if (LayerFactory.Instance.CanCreateLayerFrom(currentItem))
                LayerFactory.Instance.CreateLayer(currentItem, MapView.Active.Map);
            return $@"Downloaded this item: {results.Results[0].Name} [Type: {results.Results[0].Type}] to ArcGIS Online and added the item to the Map";
        }
        /// <summary>
        /// This is an alternate method of implementing a Query. 
        /// It uses the GetUserContentAsync method to return the items in the current user's content.
        /// </summary>
        /// <returns></returns>

        private async Task<string> QueryUserContentImpl()
        {

            var portal = ArcGISPortalManager.Current.GetActivePortal();

            string userName = ArcGISPortalManager.Current.GetActivePortal().GetSignOnUsername();
            //Get the content for the signed on user.  
            //Users executing this query must be signed on or an exception will be thrown.
            var results = await ArcGISPortalExtensions.GetUserContentAsync(portal, userName);
            //Check if a specific item exits
            var itemExists = results.PortalItems.Any(p => p.Tags.Contains("UploadVtpkToAgol Demo"));
            if (!itemExists)            
                return $@"Unable to find uploaded item with query: {portal.ToString()}sharing/rest/content/users/{userName}";
   
            // Create an item from the search results
            var myItem = results.PortalItems.FirstOrDefault(i => i.Tags.Contains("UploadVtpkToAgol Demo"));

            var currentItem = ItemFactory.Instance.Create(myItem.ID, ItemFactory.ItemType.PortalItem);

            // Finally add the Vextor tile package to the map
            // if we have an item that can be turned into a layer
            // add it to the map
            if (LayerFactory.Instance.CanCreateLayerFrom(currentItem))
                LayerFactory.Instance.CreateLayer(currentItem, MapView.Active.Map);
            return $@"Downloaded this item: {myItem.Name} [Type: {myItem.Type}] to ArcGIS Online and added the item to the Map";
        }
    }
   

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class UploadVectorTile_ShowButton : Button
    {
        protected override void OnClick()
        {
            UploadVectorTileViewModel.Show();
        }
    }
}
