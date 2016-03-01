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
using System.Net.Http;
using System.Web.Script.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Tests.APIHelpers.SharingDataContracts;

namespace CreateFeatureService
{
    /// <summary>
    /// Interaction logic for Dockpane1View.xaml
    /// </summary>
    public partial class Dockpane1View : UserControl
    {
        /// <summary>
        /// Built-in method to initialize all components within this dockpane
        /// </summary>
        public Dockpane1View()
        {
            InitializeComponent();
            serviceLinkLabel.Visibility = System.Windows.Visibility.Hidden;
            serviceLinkText.Visibility = System.Windows.Visibility.Hidden;
            copyToClipboard.Visibility = System.Windows.Visibility.Hidden;
        }

        /// <summary>
        /// Check if input fields are empty or invalid
        /// </summary>
        /// <returns></returns>
        Tuple<bool, string> checkEmptyFields()
        {
            #region
            BaseUrl.BorderBrush = Brushes.DarkBlue;
            itemId.BorderBrush = Brushes.DarkBlue;
            username.BorderBrush = Brushes.DarkBlue;
            filetype.BorderBrush = Brushes.DarkBlue;
            publishParameters.BorderBrush = Brushes.DarkBlue;
            #endregion

            string msg = "";
            if (BaseUrl.Text.Trim() == "")
            {
                BaseUrl.BorderBrush = Brushes.Red;
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
                    BaseUrl.BorderBrush = Brushes.Red;
                    msg += "\tPortal Url is invalid.\n";
                }
            }
            if (itemId.Text.Trim() == "")
            {
                itemId.BorderBrush = Brushes.Red;
                msg += "\tTtem Id cannot be empty.\n";
            }
            if (username.Text.Trim() == "")
            {
                username.BorderBrush = Brushes.Red;
                msg += "\tusername cannot be empty.\n";
            }
            else if (itemId.Text.Trim() != "")
            {
                Tuple<bool, string> res = getServiceName(BaseUrl.Text, username.Text, itemId.Text);
                if (!res.Item1)
                {
                    itemId.BorderBrush = Brushes.Red;
                    msg += "\tItem Id is not associated with existing data.\n";
                }
            }

            if (filetype.Text.Trim() == "")
            {
                filetype.BorderBrush = Brushes.Red;
                msg += "\tFile type cannot be empty.\n";
            }

            if (publishParameters.Text.Trim() == "")
            {
                publishParameters.BorderBrush = Brushes.Red;
                msg += "\tPublish Parameters cannot be empty.\n";
            }
            if (msg.Trim() == "")
                return new Tuple<bool, string>(false, msg);
            else
                return new Tuple<bool, string>(true, "Form cannot be submitted due to:\n" + msg);
        }

        /// <summary>
        /// Call back method to be called when "clearContents" button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnClickClearContents(object sender, RoutedEventArgs e)
        {
            publishSubmit.Background = Brushes.White;
            publishSubmit.IsEnabled = true;
            BaseUrl.Text = "";
            itemId.Text = "";
            username.Text = "";
            filetype.Text = "";
            publishParameters.Text = "";
            serviceLinkLabel.Visibility = System.Windows.Visibility.Hidden;
            serviceLinkText.Visibility = System.Windows.Visibility.Hidden;
            txt_serviceLink.Text = "";
            txt_serviceLink.Foreground = Brushes.White;
            txt_serviceLink.Visibility = System.Windows.Visibility.Hidden;
            copyToClipboard.Visibility = System.Windows.Visibility.Hidden;
        }

        /// <summary>
        /// Callback method to be called when the click event on the "publishSubmit" Button is triggered
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnClickPublishSubmit(object sender, RoutedEventArgs e)
        {
            Tuple<bool, string> tup = checkEmptyFields();
            if (!tup.Item1)
            {
                publishSubmit.Background = Brushes.Gray;
                publishSubmit.IsEnabled = false;
                txt_serviceLink.Text = "Portal info: " + BaseUrl.Text + " || ";
                txt_serviceLink.Text += "Item info: (Item ID) " + itemId.Text + "; (user name) " + username.Text + "; (File Type) " + filetype.Text + "; (Parameters) " + publishParameters.Text + "\n";

                //Step 1: analyze the portal item; If return false, quit; Else return tuple containing publish parameters
                Tuple<bool, string> analyzeResult = analyzeService(BaseUrl.Text, itemId.Text, filetype.Text, publishParameters.Text);
                if (analyzeResult.Item1)
                {
                    //Step 2: get the item name based on its ID
                    Tuple<bool, string> itemNameRes = getServiceName(BaseUrl.Text, username.Text, itemId.Text);
                    if (itemNameRes.Item1)
                    {
                        //Step 3: check if the service name is already in use
                        Tuple<bool, string> availableRes = isServiceNameAvailable(BaseUrl.Text, itemNameRes.Item2, "Feature Service");
                        if (availableRes.Item1)
                        {
                            //Step 4: publish the service based on the portal item
                            Tuple<bool, string> publishResult = publishService(BaseUrl.Text, username.Text, itemId.Text, filetype.Text, analyzeResult.Item2);
                            if (publishResult.Item1)
                            {
                                txt_serviceLink.Text = "Service created successfully!";
                                txt_serviceLink.Foreground = Brushes.Green;
                                serviceLinkLabel.Visibility = System.Windows.Visibility.Visible;
                                serviceLinkText.Visibility = System.Windows.Visibility.Visible;
                                serviceLinkText.Text = publishResult.Item2;
                                copyToClipboard.Visibility = System.Windows.Visibility.Visible;
                            }
                            else
                            {
                                txt_serviceLink.Text = publishResult.Item2 + " for request - " + txt_serviceLink.Text;
                                txt_serviceLink.Foreground = Brushes.Red;
                            }
                        }
                        else
                        {
                            txt_serviceLink.Text = "Service Name is not available: " + availableRes.Item2 + " for request - " + txt_serviceLink.Text;
                            txt_serviceLink.Foreground = Brushes.Red;
                        }
                    }
                    else
                    {
                        txt_serviceLink.Text = "Call to get item name failed: " + itemNameRes.Item2 + " for request - " + txt_serviceLink.Text;
                        txt_serviceLink.Foreground = Brushes.Red;
                    }
                }
                else
                {
                    txt_serviceLink.Text = analyzeResult.Item2 + " for request - " + txt_serviceLink.Text;
                    txt_serviceLink.Foreground = Brushes.Red;
                }
            }
            else
                txt_serviceLink.Text = tup.Item2;
        }

        /// <summary>
        /// Gets the item title/name based on its item ID
        /// </summary>
        /// <param name="baseURI"></param>
        /// <param name="username"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        Tuple<bool, string> getServiceName(string baseURI, string username, string itemId)
        {
            EsriHttpClient myClient = new EsriHttpClient();

            #region REST call to get service name
            string requestUrl = baseURI + @"/sharing/rest/content/users/" + username + @"/items/" + itemId + "?f=json";
            var response = myClient.Get(requestUrl);
            if (response == null)
                return new Tuple<bool, String>(false, "HTTP response is null");

            string outStr = response.Content.ReadAsStringAsync().Result;

            //Deserialize the response in JSON into a usable object. 
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            OnlineItem obj = (OnlineItem)serializer.Deserialize(outStr, typeof(OnlineItem));
            if (obj == null || obj.item.title == null)
            {
                return new Tuple<bool, String>(false, "Failed item call");
            }
            return new Tuple<bool, String>(true, obj.item.title);
            #endregion
        }

        /// <summary>
        /// Check if the service name is already in use
        /// </summary>
        /// <param name="baseURI"></param>
        /// <param name="serviceName"></param>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        Tuple<bool, string> isServiceNameAvailable(string baseURI, string serviceName, string serviceType)
        {
            EsriHttpClient myClient = new EsriHttpClient();

            #region REST call to get appInfo.Item.id and user.lastLogin of the licensing portal
            string selfUri = @"/sharing/rest/portals/self?f=json";
            var selfResponse = myClient.Get(baseURI + selfUri);
            if (selfResponse == null)
                return new Tuple<bool, String>(false, "HTTP response is null");

            string outStr = selfResponse.Content.ReadAsStringAsync().Result;

            //Deserialize the response in JSON into a usable object. 
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            PortalSelf self_obj = (PortalSelf)serializer.Deserialize(outStr, typeof(PortalSelf));
            if ((self_obj == null) || (self_obj.id == null))
            {
                return new Tuple<bool, String>(false, "Failed portal self call");
            }
            #endregion

            string requestUrl = baseURI + @"/sharing/rest/portals/" + self_obj.id + @"/isServiceNameAvailable";
            requestUrl += "?f=json&type=" + serviceType + "&name=" + serviceName;

            EsriHttpResponseMessage respMsg = myClient.Get(requestUrl);
            if (respMsg == null)
                return new Tuple<bool, String>(false, "HTTP response is null");

            outStr = respMsg.Content.ReadAsStringAsync().Result;
            //Deserialize the response in JSON into a usable object. 
            AvailableResult obj = (AvailableResult)serializer.Deserialize(outStr, typeof(AvailableResult));
            if (obj == null)
                return new Tuple<bool, String>(false, "Service fails to be analyzed - " + outStr);
            return new Tuple<bool, string> (obj.available, outStr);
        }

        /// <summary>
        /// Post "analyze" request on the portal item
        /// </summary>
        /// <param name="baseURI"></param>
        /// <param name="itemId"></param>
        /// <param name="filetype"></param>
        /// <param name="analyzeParameters"></param>
        /// <returns></returns>
        Tuple<bool, string> analyzeService(string baseURI, string itemId, string filetype, string analyzeParameters)
        {
            EsriHttpClient myClient = new EsriHttpClient();
            string requestUrl = baseURI + @"/sharing/rest/content/features/analyze";

            var postData = new List<KeyValuePair<string, string>>();
            postData.Add(new KeyValuePair<string, string>("f", "json"));
            postData.Add(new KeyValuePair<string, string>("itemId", itemId));
            postData.Add(new KeyValuePair<string, string>("filetype", filetype));
            postData.Add(new KeyValuePair<string, string>("analyzeParameters", analyzeParameters));
            HttpContent content = new FormUrlEncodedContent(postData);

            EsriHttpResponseMessage respMsg = myClient.Post(requestUrl, content);
            if (respMsg == null)
                return new Tuple<bool, String>(false, "HTTP response is null");

            string outStr = respMsg.Content.ReadAsStringAsync().Result;
            //Deserialize the response in JSON into a usable object. 
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            //Deserialize the response in JSON into a usable object. 
            AnalyzedService obj = (AnalyzedService)serializer.Deserialize(outStr, typeof(AnalyzedService));
            if (obj == null)
                return new Tuple<bool, String>(false, "Service fails to be analyzed - " + outStr);
            if (obj.publishParameters != null)
            {
                string respReturn = SerializeToString(obj.publishParameters);
                if (respReturn == "")
                    return new Tuple<bool, String>(false, "Service fails to be analyzed - " + outStr);
                else
                    return new Tuple<bool, String>(true, respReturn);
            }
            return new Tuple<bool, String>(false, "Service fails to be analyzed - " + outStr);
        }

        /// <summary>
        /// Post "publish" request on the portal item
        /// </summary>
        /// <param name="baseURI"></param>
        /// <param name="username"></param>
        /// <param name="itemId"></param>
        /// <param name="filetype"></param>
        /// <param name="publishParameters"></param>
        /// <returns></returns>
        Tuple<bool, string> publishService(string baseURI, string username, string itemId, string filetype, string publishParameters)
        {
            EsriHttpClient myClient = new EsriHttpClient();
            string requestUrl = baseURI + @"/sharing/rest/content/users/" + username + @"/publish";

            var postData = new List<KeyValuePair<string, string>>();
            postData.Add(new KeyValuePair<string, string>("f", "json"));
            postData.Add(new KeyValuePair<string, string>("itemId", itemId));
            postData.Add(new KeyValuePair<string, string>("filetype", filetype));
            postData.Add(new KeyValuePair<string, string>("publishParameters", publishParameters));
            HttpContent content = new FormUrlEncodedContent(postData);

            EsriHttpResponseMessage respMsg = myClient.Post(requestUrl, content);
            if(respMsg==null)
                return new Tuple<bool, String>(false, "HTTP response is null");

            string outStr = respMsg.Content.ReadAsStringAsync().Result;
            //Deserialize the response in JSON into a usable object. 
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            //Deserialize the response in JSON into a usable object. 
            PublishedServices obj = (PublishedServices)serializer.Deserialize(outStr, typeof(PublishedServices));
            if (obj == null)
                return new Tuple<bool, String>(false, "Service creation fails - " + outStr);

            string respReturn = "";
            foreach (PublishedService ps in obj.services)
                respReturn += ps.serviceurl;
            if (respReturn == "")
                return new Tuple<bool, String>(false, "Service creation fails - " + outStr);
            else
                return new Tuple<bool, String>(true, respReturn);

        }

        /// <summary>
        /// Call back method to be called when "Get active portal" button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GetActivePortal_Click(object sender, RoutedEventArgs e)
        {
            BaseUrl.Text = PortalManager.GetActivePortal().ToString();
        }

        /// <summary>
        /// Call back method to be called when "paste to clip board" button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void pasteFromClipboard_Click(object sender, RoutedEventArgs e)
        {
            itemId.Text = Clipboard.GetText();
        }

        /// <summary>
        /// Call back method to be called when "copy to clip board" button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void copyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(serviceLinkText.Text);
        }

        /// <summary>
        /// Print the deserialized object into JSON formatted string
        /// </summary>
        /// <param name="objectToSerialize"></param>
        /// <returns></returns>
        string SerializeToString(object objectToSerialize)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var ser = new DataContractJsonSerializer(objectToSerialize.GetType());
                ser.WriteObject(ms, objectToSerialize);
                return Encoding.Default.GetString(ms.ToArray());
            }
        }
    }
}
