//   Copyright 2014 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Web.Script.Serialization;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Tests.APIHelpers.SharingDataContracts;

namespace UploadItem
{
    /// <summary>
    /// Interaction logic for UploadDockPaneView.xaml
    /// </summary>
    public partial class Dockpane1View : UserControl
    {
        /// <summary>
        /// Built-in method to initialize all components within this dockpane
        /// </summary>
        public Dockpane1View()
        {
            InitializeComponent();
            itemIdLabel.Visibility = System.Windows.Visibility.Hidden;
            itemIdText.Visibility = System.Windows.Visibility.Hidden;
            copyToClipboard.Visibility = System.Windows.Visibility.Hidden;
        }

        private Brush _errorBorderBrush
        {
            get
            {
                var colorString = FrameworkApplication.ApplicationTheme == ApplicationTheme.Default
                    ? "#C6542D"
                    : "#C75028";
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorString));
            }
        }

        private Brush _greenBorderBrush
        {
            get
            {
                var colorString = FrameworkApplication.ApplicationTheme == ApplicationTheme.Default
                    ? "#5A9359"
                    : "#58AD57";
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorString));
            }
        }

        /// <summary>
        /// Check if input fields are empty or invalid
        /// </summary>
        /// <returns></returns>
        Tuple<bool, string> checkEmptyFields()
        {
            #region
            var brush = FrameworkApplication.ApplicationTheme == ApplicationTheme.Default
                ? Brushes.DarkBlue
                : Brushes.AliceBlue;
            BaseUrl.BorderBrush = brush;
            itemPath.BorderBrush = brush;
            thumbnailPath.BorderBrush = brush;
            tags.BorderBrush = brush;
            #endregion

            string msg = "";
            if (BaseUrl.Text.Trim() == "")
            {
                BaseUrl.BorderBrush =_errorBorderBrush;
                msg += "\tPortal Url cannot be empty.\n";
            }
            else
            {
                Uri uriResult;
                bool result = Uri.TryCreate(BaseUrl.Text.Trim(), UriKind.Absolute, out uriResult)
                              && (uriResult.Scheme == Uri.UriSchemeHttp
                                  || uriResult.Scheme == Uri.UriSchemeHttps);
                if (!result)
                {
                    BaseUrl.BorderBrush =_errorBorderBrush;
                    msg += "\tPortal Url is invalid.\n";
                }
            }
            if (itemPath.Text.Trim() == "" || !File.Exists(itemPath.Text.Trim()))
            {
                itemPath.BorderBrush =_errorBorderBrush;
                msg += "\tItem Path cannot be empty or non-existent.\n";
            }

            if (thumbnailPath.Text.Trim() == "" || !File.Exists(thumbnailPath.Text.Trim()))
            {
                thumbnailPath.BorderBrush =_errorBorderBrush;
                msg += "\tThumbnail Path cannot be empty or non-existent.\n";
            }

            if (tags.Text.Trim() == "")
            {
                tags.BorderBrush =_errorBorderBrush;
                msg += "\tTags cannot be empty.\n";
            }
            else
            {
                String[] tags_arr = tags.Text.Split(new Char[] { ',', ':', ';', '\t' });
                /*
                int emptyCellCount = 0;
                for (int i = 0; i < tags_arr.Length; i++)
                {
                    if (tags_arr[i] == "")
                        emptyCellCount++;
                }
                 * */
                tags_arr = tags_arr.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                if (tags_arr.Length == 0)
                {
                    tags.BorderBrush =_errorBorderBrush;
                    msg += "\tTag array cannot be empty.\n";
                }
            }
            if (msg.Trim() == "")
                return new Tuple<bool,string>(false, msg);
            else
                return new Tuple<bool, string>(true, "Form cannot be submitted due to:\n" + msg);
        }

        /// <summary>
        /// Call back method to be called when "uploadsubmit" button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnClickUploadSubmit(object sender, RoutedEventArgs e)
        {
            Tuple<bool, string> tup = checkEmptyFields();
            if (!tup.Item1)
            {

                uploadSubmit.IsEnabled = false;
                txt_itemLink.Text = "Portal info: " + BaseUrl.Text + " || ";
                txt_itemLink.Text += "Item info: (Item Path) " + itemPath.Text + "; (Thumbnail Path) " + thumbnailPath.Text + "; (Tags) " + tags.Text + "\n";
                Tuple<bool, string> result = uploadItem(BaseUrl.Text, itemPath.Text, thumbnailPath.Text, tags.Text);
                if (result.Item1)
                {
                    txt_itemLink.Text = "Item uploaded successfully!";
                    txt_itemLink.Foreground = _greenBorderBrush;
                    itemIdLabel.Visibility = System.Windows.Visibility.Visible;
                    itemIdText.Visibility = System.Windows.Visibility.Visible;
                    itemIdText.Text = result.Item2;
                    copyToClipboard.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    txt_itemLink.Text = result.Item2 + " for request - " + txt_itemLink.Text;
                    txt_itemLink.Foreground =_errorBorderBrush;
                }

                uploadSubmit.IsEnabled = true;
            }
            else
                txt_itemLink.Text = tup.Item2;
        }

        /// <summary>
        /// Call back method to be called when "clearContents" button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnClickClearContents(object sender, RoutedEventArgs e)
        {

            uploadSubmit.IsEnabled = true;
            BaseUrl.Text = "";
            itemPath.Text = "";
            thumbnailPath.Text = "";
            tags.Text = "";
            txt_itemLink.Text = "";
            itemIdLabel.Visibility = System.Windows.Visibility.Hidden;
            itemIdText.Visibility = System.Windows.Visibility.Hidden;
            copyToClipboard.Visibility = System.Windows.Visibility.Hidden;
        }

        /// <summary>
        /// Uploads a local item to online with its parameters
        /// </summary>
        /// <param name="baseURI"></param>
        /// <param name="itemPathStr"></param>
        /// <param name="thumbnail"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        Tuple<bool, string> uploadItem(string baseURI, string itemPathStr, string thumbnail, String tags)
        {
            String[] tags_arr = tags.Split(new Char[] { ',', ':', '\t' });
      
            EsriHttpClient myClient = new EsriHttpClient();
            Tuple<bool, string> response = null;
            Item itemToUpload = ItemFactory.Instance.Create(itemPathStr);
            if (itemToUpload != null)
            {
                response = myClient.Upload(baseURI, itemToUpload, thumbnail, tags_arr);
                if (response.Item1)
                {
                    Tuple<bool, SDCItem> result = SearchPortalItemREST(itemToUpload.Name, itemToUpload.Type, baseURI);
                    if(result.Item1)
                        return new Tuple<bool, string>(true, result.Item2.id);
                    else
                        return new Tuple<bool, string>(true,"Cannot find item online");
                }
                else
                    return new Tuple<bool, String>(false, "Upload fails - " + response.Item2);
            }else
                return new Tuple<bool, String>(false, "Null item cannot be uploaded - " + response.Item2);
        }

        /// <summary>
        /// Search for item in the active portal or the portal URL specified.
        /// Returns itemdID or null if item not found.
        /// </summary>
        /// <param name="itemName">Item Name as a string</param>
        /// <param name="itemType">Item TYpe as a string. Use types used by AGO/portal</param>
        /// <param name="portalURL">PortalURL as a string. Optional. When specified, searchs in that URL, else under active portal</param>
        /// <returns>PortalItem. Null if item was not found</returns>
        public static Tuple<bool, SDCItem> SearchPortalItemREST(string @itemName, string itemType, string portalURL)
        {
            //string extension = System.IO.Path.GetExtension(@itemName);
            //@itemName = @itemName.Substring(0, @itemName.Length - extension.Length);

            try
            {
                #region Construct and make REST query
                if (!portalURL.EndsWith("/"))
                    portalURL += "/";

                string queryURL = portalURL + @"sharing/rest/search?q=" + @itemName + " AND type:" + itemType + "&f=json";
                EsriHttpClient myClient = new EsriHttpClient();
                var response = myClient.Get(queryURL);
                if (response == null)
                    return new Tuple<bool, SDCItem>(false, null);

                string outStr = response.Content.ReadAsStringAsync().Result;
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                SearchResult sr = (SearchResult)serializer.Deserialize(outStr, typeof(SearchResult));
                #endregion

                #region Get ItemID from search result, rerun the search for the item and return the full item
                if (sr != null)
                {
                    while (sr.nextStart <= sr.total)
                    {
                        for (int i = 0; i < sr.results.Count; i++)
                        {
                            //string srID = sr.results[i].id; //Technically, only one item with a particular name and type can exist
                            //string itemQueryURL = portalURL + @"sharing/rest/content/items/" + srID + "?f=json&token=" + pToken;
                            //SDCItem resultItem = (SDCItem)RESTcaller(itemQueryURL, typeof(SDCItem));
                            //if ((resultItem.title == itemName) && (resultItem.type == itemType))
                            //    return resultItem;
                            if ((sr.results[i].name == itemName) && (sr.results[i].type.Contains(itemType)))
                                return new Tuple<bool, SDCItem>(true, sr.results[i]);
                        }
                        if (sr.nextStart <= 0)
                            return new Tuple<bool, SDCItem>(false, null);

                        #region prepare for the next batch of search results
                        queryURL = portalURL + @"sharing/rest/search?q=" + itemName + " AND type:" + itemType + "&f=json&start=" + sr.nextStart;
                        response = myClient.Get(queryURL);
                        if (response == null)
                            return new Tuple<bool, SDCItem>(false, null);
                        outStr = response.Content.ReadAsStringAsync().Result;
                        sr = (SearchResult)serializer.Deserialize(outStr, typeof(SearchResult));
                        #endregion
                    }
                    return new Tuple<bool, SDCItem>(false, null);
                }
                else
                    return new Tuple<bool, SDCItem>(false, null);
                #endregion
            }
            catch (Exception exRESTsearch)
            {
                Console.WriteLine("Exception occurred when search for item through REST call. Exception: " + exRESTsearch.ToString());
                return null;
            }
        }


        /// <summary>
        /// Opens up a file browse dialogue for user to choose item from local
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void itemPathBrowse_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();          
 
            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".xlsx";
            dlg.Filter = "Excel (.xlsx)|*.xlsx|KML|*.kml|Layer|*.lyr|Layer Package|*.lpk|Map Document|*.mxd|Map Package|*.mpk|Tile Package|*.tpk|Geoprocessing Package|*.gpk|Address Locator|*.gcpk|Basemap Package|*.bpk|Mobile Package|*.mmpk|Vector Tile Package|*.vtpk";
 
            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();
 
            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                itemPath.Text = filename;
             }
        }

        /// <summary>
        /// Opens up a file browse dialogue for user to choose thumbnail from local
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void thumbnailPathBrowse_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".jpg";
            dlg.Filter = "BMP|*.bmp|GIF|*.gif|JPG (*.jpg,*.jpeg)|*.jpg;*.jpeg|PNG|*.png|TIFF (*.tif,*.tiff)|*.tif;*.tiff";

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                thumbnailPath.Text = filename;
            }
        }

        /// <summary>
        /// call back method to be triggered when "Get active portal" button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GetActivePortal_Click(object sender, RoutedEventArgs e) {
            BaseUrl.Text = ArcGISPortalManager.Current.GetActivePortal().PortalUri.ToString();
        }

        /// <summary>
        /// call back method to be triggered when "Copy to clipboard" button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void copyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(itemIdText.Text);
        }
    }
}

