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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Core.Portal;
using ArcGIS.Desktop.Core;
using System.Windows.Input;

namespace UploadItem
{
    internal class UploadItemViewModel : DockPane
    {
        private const string _dockPaneID = "UploadItem_UploadItem";

        protected UploadItemViewModel()
        {
            _submitItemCommand = new RelayCommand(() => UploadToAGOLAsync(), () => CanExecute());
        }

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
        #region Properies for binding
        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "Upload AGOL item";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }

        /// <summary>
        /// Url of the ArcGIS Online or active portal
        /// </summary>
        private string _baseUrl;
        public string BaseUrl
        {
            get { return _baseUrl; }
            set { SetProperty(ref _baseUrl, value, () => BaseUrl); }
        }

        /// <summary>
        /// Full path to the item on disk that is to be uploaded.
        /// </summary>
        private string _itemPath;
        public string ItemPath
        {
            get { return _itemPath; }
            set { SetProperty(ref _itemPath, value, () => ItemPath); }
        }
        /// <summary>
        /// Full path to the thumbnail image of the item
        /// </summary>
        private string _thumbnailPath;
        public string ItemThumbnailPath
        {
            get { return _thumbnailPath; }
            set { SetProperty(ref _thumbnailPath, value, () => ItemThumbnailPath); }
        }

        /// <summary>
        /// Tags for the item to upload
        /// </summary>
        private string _itemTags;
        public string ItemTags
        {
            get { return _itemTags; }
            set { SetProperty(ref _itemTags, value, () => ItemTags); }
        }

        /// <summary>
        /// ID of the uploaded item
        /// </summary>
        private string _itemID;
        public string ItemID
        {
            get { return _itemID; }
            set { SetProperty(ref _itemID, value, () => ItemID); }
        }
        #endregion
        #region Commands
        /// <summary>
        /// Command to get the active portal
        /// </summary>
        private RelayCommand _getActivePortalCommand;        
        public ICommand GetActivePortalCommand
        {
            get {
                if (_getActivePortalCommand == null)
                    _getActivePortalCommand = new RelayCommand(() => GetActivePortal(), () => true);
                return _getActivePortalCommand;
            }
        }
        /// <summary>
        /// Command to browse for item and thumbnail
        /// </summary>
        private RelayCommand _browseCommand;
        public ICommand BrowseCommand
        {
            get
            {
                if (_browseCommand == null)
                    _browseCommand = new RelayCommand(BrowseImpl, () => true);
                return _browseCommand;
            }
        }

        /// <summary>
        /// Submit command - Uploads to active portal
        /// </summary>
        private RelayCommand _submitItemCommand;
        public RelayCommand SubmitItemCommand
        {
            get
            {

                return _submitItemCommand;
            }
        }

        
        #endregion
        #region private methods

        /// <summary>
        /// Uploads the items to Active portal
        /// </summary>
        /// <returns></returns>
        private async Task UploadToAGOLAsync()
        {
            var uploadResult = await UploadItemAsync(BaseUrl, ItemPath, ItemThumbnailPath, ItemTags);
            ItemID = uploadResult.Item2;
        }
        /// <summary>
        /// Uploads a local item to online with its parameters
        /// </summary>
        /// <param name="baseURI"></param>
        /// <param name="itemPathStr"></param>
        /// <param name="thumbnail"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        async Task<Tuple<bool, string>> UploadItemAsync(string baseURI, string itemPathStr, string thumbnail, string tags)
        {
            String[] tags_arr = tags.Split(new Char[] { ',', ':', '\t' });

            EsriHttpClient myClient = new EsriHttpClient();
            Tuple<bool, string> response = null;

            var itemToUpload = ItemFactory.Instance.Create(itemPathStr);
            if (itemToUpload != null)
            {
                //Create the upload defintion
                var uploadDfn = new UploadDefinition(baseURI, itemToUpload, tags_arr);
                uploadDfn.Thumbnail = thumbnail;
                //upload item
                response = myClient.Upload(uploadDfn);
                //response = myClient.Upload(baseURI, itemToUpload, thumbnail, tags_arr);  //obsolete
                if (response.Item1) //Upload was successfull 
                {
                    //Search for the uploaded item to get its ID.
                    Tuple<bool, PortalItem> result = await SearchPortalForItemsAsync(baseURI, itemToUpload);
                    if (result.Item1) //search successful  
                    {
                        ItemID = result.Item2.ToString();
                        return new Tuple<bool, string>(true, result.Item2.ID);                        
                    }
                        
                    else //item not found   
                    {
                        ItemID = "Cannot find item online";
                        return new Tuple<bool, string>(true, "Cannot find item online");
                    }                        
                }
                else //Upload failed
                {
                    ItemID = "Upload failed";
                    return new Tuple<bool, String>(false, "Upload failed");
                }                    
            }
            else   //Item was not created
            {
                ItemID = "Item cannot be created";
                return new Tuple<bool, String>(false, "Item cannot be created");
            }                
        }

        /// <summary>
        /// Searches the active portal for the item that has been uploaded.
        /// </summary>
        /// <param name="portalUrl"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private async Task<Tuple<bool, PortalItem>> SearchPortalForItemsAsync(string portalUrl, Item item)
        {
            var portal = ArcGISPortalManager.Current.GetPortal(new Uri(portalUrl));
            //Get the PortalItemType of the item
            var portalItemType = GetProtalItemTypeFromItem(item);
            //Get the item name without the extension
            var portalItemName = System.IO.Path.GetFileNameWithoutExtension(item.Name);
            //Create the Query and the params
            var pqp = PortalQueryParameters.CreateForItemsOfType(portalItemType, $@"""{portalItemName}"" tags:""{ItemTags}""");
            //Search active portal
            PortalQueryResultSet<PortalItem> results = await ArcGISPortalExtensions.SearchForContentAsync(portal, pqp);
            //Iterate through the returned items for THE item.
            var myPortalItem = results.Results?.OfType<PortalItem>().FirstOrDefault();
            if (myPortalItem == null)
                return null;
            return new Tuple<bool, PortalItem>(true, myPortalItem);
        }

        /// <summary>
        /// Gets the active portal
        /// </summary>
        private void GetActivePortal()
        {
            BaseUrl = ArcGISPortalManager.Current.GetActivePortal().PortalUri.ToString();
        }

        /// <summary>
        /// Displayes the Browse dialog. 
        /// </summary>
        /// <remarks>The parameter is passed in to see if the dialog should browse for an item or the thumbnail</remarks>
        /// <param name="param"></param>
        private void BrowseImpl(object param)
        {
            if (param == null)
                return;
            string title = "Browse";
            string filter = "";
            string defaultExt = "";

            switch (param.ToString()) //Item or thumbnail browse?
            {
                case "ItemPath":
                    defaultExt = ".lyr";
                    filter = "Layer|*.lyr|Excel (.xlsx)|*.xlsx|KML|*.kml|Layer Package|*.lpk|Map Document|*.mxd|Map Package|*.mpk|Tile Package|*.tpk|Geoprocessing Package|*.gpk|Address Locator|*.gcpk|Basemap Package|*.bpk|Mobile Package|*.mmpk|Vector Tile Package|*.vtpk|All files (*.*)|*.*";
                    title = "Browse to item";
                    break;
                case "ItemThumbnailPath":
                    defaultExt = ".bmp";
                    filter = "BMP|*.bmp|GIF|*.gif|JPG(*.jpg,*.jpeg)|*.jpg;*.jpeg|PNG|*.png |TIFF(*.tif,*.tiff)|*.tif; .tiff";
                    title = "Browse to image";
                    break;
            }
            var filename = OpenBrowseDialog(title, filter, defaultExt);
            switch (param.ToString())
            {
                case "ItemPath":
                    ItemPath = filename;
                    break;
                case "ItemThumbnailPath":
                    ItemThumbnailPath = filename;
                    break;
            }
        }
        /// <summary>
        /// Display the browse dialog with the filters
        /// </summary>
        /// <param name="title"></param>
        /// <param name="filter"></param>
        /// <param name="defaultExt"></param>
        /// <returns></returns>
        private string OpenBrowseDialog(string title, string filter, string defaultExt)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            //Dialog title
            dlg.Title = title;

            // Set filter for file extension and default file extension
            dlg.DefaultExt = defaultExt;
            dlg.Filter = filter;

            // Display OpenFileDialog by calling ShowDialog method
            var result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox           
            if (result == true)
            {
                // Open document
                return dlg.FileName;
            }
            else
                return null;
        }
       
        /// <summary>
        /// Gets the PortalItemType of an Item object that has been uploaded
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private PortalItemType GetProtalItemTypeFromItem(Item item)
        {
            PortalItemType portalItemType = PortalItemType.WebMap; //default
            var itemExtension = System.IO.Path.GetExtension(item.Name);
            //Get the PortalItemType based on the file extension
            switch (itemExtension)
            {
                case ".lyr":
                    portalItemType = PortalItemType.Layer;
                    break;
                case ".xlsx":
                    portalItemType = PortalItemType.MicrosoftExcel;
                    break;
                case ".kml":
                    portalItemType = PortalItemType.KML;
                    break;
                case ".lpk":
                    portalItemType = PortalItemType.LayerPackage;
                    break;
                case ".mxd":
                    portalItemType = PortalItemType.MapDocument;
                    break;
                case ".mpk":
                    portalItemType = PortalItemType.MapPackage;
                    break;
                case ".tpk":
                    portalItemType = PortalItemType.TilePackage;
                    break;
                case ".gpk":
                    portalItemType = PortalItemType.GeoprocessingPackage;
                    break;
                case ".gcpk":
                    portalItemType = PortalItemType.LocatorPackage;
                    break;
                case ".bpk":
                    portalItemType = PortalItemType.BasemapPackage;
                    break;
                case ".mmpk":
                    portalItemType = PortalItemType.MobileMapPackage;
                    break;
                case ".vtpk":
                    portalItemType = PortalItemType.VectorTilePackage;
                    break;
                case ".rpk":
                    portalItemType = PortalItemType.RulePackage;
                    break;
            }
            return portalItemType;
        }

        private bool CanExecute()
        {

            if (!string.IsNullOrEmpty(BaseUrl) && !string.IsNullOrEmpty(ItemPath) && !string.IsNullOrEmpty(ItemThumbnailPath) && !string.IsNullOrEmpty(ItemTags))
                return true;
            else return false;
        }
        #endregion
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class UploadItem_ShowButton : Button
    {
        protected override void OnClick()
        {
            UploadItemViewModel.Show();
        }
    }
}
