//   Copyright 2018 Esri
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
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Portal;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using SharingContracts = ArcGIS.Desktop.Tests.APIHelpers.SharingDataContracts;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace CreateFeatureService
{
	/// <summary>
	/// This sample provides a new tab and controls that allow you to create
	///  feature service from the csv file that has been uploaded to AGOL or
	///  portal.
	/// </summary>
	internal class Dockpane1ViewModel : DockPane
	{
		private const string DockPaneID = "CreateFeatureService_Dockpane1";
		private const string RestAPIFileTypes = "serviceDefinition|shapefile|csv|excel|tilePackage|featureService|featureCollection|fileGeodatabase|geojson|scenepackage|vectortilepackage|imageCollection|mapService|sqliteGeodatabase";
		private ObservableCollection<CsvPortalItem> _csvPortalItems = new ObservableCollection<CsvPortalItem>();
		
		protected Dockpane1ViewModel()
		{
			var brush = FrameworkApplication.ApplicationTheme == ApplicationTheme.Default
			? Brushes.DarkBlue
			: Brushes.AliceBlue;
			BaseUrlBorderBrush = brush;
			PortalItemBorderBrush = brush;
			UsernameBorderBrush = brush;
			FileTypeBorderBrush = brush;
			PublishParametersBorderBrush = brush;
			AnalyzeParametersBorderBrush = brush;
			AnalyzeParameters = "{}";
			BindingOperations.EnableCollectionSynchronization(_csvPortalItems, new object());
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
		Tuple<bool, string> CheckEmptyFields(bool bAnalyze = false)
		{
			#region
			var brush = FrameworkApplication.ApplicationTheme == ApplicationTheme.Default
					? Brushes.DarkBlue
					: Brushes.AliceBlue;
			BaseUrlBorderBrush = brush;
			PortalItemBorderBrush = brush;
			UsernameBorderBrush = brush;
			FileTypeBorderBrush = brush;
			PublishParametersBorderBrush = brush;
			AnalyzeParametersBorderBrush = brush;
			#endregion

			string msg = string.Empty;
			if (string.IsNullOrEmpty(BaseUrl))
			{
				BaseUrlBorderBrush = _errorBorderBrush;
				msg += "\tPortal Url cannot be empty.\n";
			}
			else
			{
				Uri uriResult;
				bool result = Uri.TryCreate(BaseUrl, UriKind.Absolute, out uriResult)
											&& (uriResult.Scheme == Uri.UriSchemeHttp
													|| uriResult.Scheme == Uri.UriSchemeHttps);
				if (!result)
				{
					BaseUrlBorderBrush = _errorBorderBrush;
					msg += "\tPortal Url is invalid.\n";
				}
			}
			if (string.IsNullOrEmpty(CsvPortalItem.Id))
			{
				PortalItemBorderBrush = _errorBorderBrush;
				msg += "\titem Id cannot be empty.\n";
			}
			if (string.IsNullOrEmpty(Username))
			{
				UsernameBorderBrush = _errorBorderBrush;
				msg += "\tusername cannot be empty.\n";
			}
			else if (string.IsNullOrEmpty(CsvPortalItem.Id))
			{
				Tuple<bool, string> res = GetServiceName(BaseUrl, Username, CsvPortalItem.Id);
				if (!res.Item1)
				{
					PortalItemBorderBrush = _errorBorderBrush;
					msg += "\tItem Id is not associated with existing data.\n";
				}
			}
			if (string.IsNullOrEmpty(FileType))
			{
				FileTypeBorderBrush = _errorBorderBrush;
				msg += "\tFile type cannot be empty.\n";
			}
			if (bAnalyze)
			{
				if (string.IsNullOrEmpty(AnalyzeParameters))
				{
					AnalyzeParametersBorderBrush = _errorBorderBrush;
					msg += "\tAnalyze Parameters cannot be empty.\n";
				}
			}
			else
			{
				if (string.IsNullOrEmpty(PublishParameters))
				{
					PublishParametersBorderBrush = _errorBorderBrush;
					msg += "\tPublish Parameters cannot be empty.\n";
				}
			}
			if (string.IsNullOrEmpty(msg))
				return new Tuple<bool, string>(false, msg);
			else
				return new Tuple<bool, string>(true, "Form cannot be submitted due to:\n" + msg);
		}

		/// <summary>
		/// Command called when "clear Contents" button is clicked
		/// </summary>
		public ICommand CmdClearContent
		{
			get
			{
				return new RelayCommand(() =>
				{
					BaseUrl = "";
					CsvPortalItem = null;
					Username = "";
					FileType = "";
					PublishParameters = "";
				}, () => true);
			}
		}

		/// <summary>
		/// Command called when "Fix It" button is clicked
		/// </summary>
		public ICommand CmdFixIt
		{
			get
			{
				return new RelayCommand(() =>
				{
					PublishParameters = Properties.Resources.PointOfInterest_json;
				}, () => !string.IsNullOrEmpty(PublishParameters));
			}
		}

		/// <summary>
		/// Command called when the "publishAnalyze" Button is clicked
		/// </summary>
		public ICommand CmdAnalyzeSubmit
		{
			get
			{
				return new RelayCommand(() =>
				{
					Tuple<bool, string> tup = CheckEmptyFields(true);
					if (!tup.Item1)
					{
						//publishAnalyze.IsEnabled = false;
						ServiceInfo = $@"Portal info: {BaseUrl}{Environment.NewLine}";
						ServiceInfo += "Item info: (Item ID) " + CsvPortalItem.Id + "; (user name) " + Username + "; (File Type) " + FileType + "; (Parameters) " + AnalyzeParameters + "\n";

						//Step 1: analyze the portal item; If return false, quit; Else return tuple containing publish parameters
						Tuple<bool, string> analyzeResult = AnalyzeService(BaseUrl, CsvPortalItem.Id, FileType, AnalyzeParameters);
						if (analyzeResult.Item1)
						{
							PublishParameters = analyzeResult.Item2;
						}
						else
						{
							ServiceInfo = analyzeResult.Item2 + " for request - " + ServiceInfo;
							ServiceInfoForeground = _errorBorderBrush;
						}
					}
					else
						ServiceInfo = tup.Item2;
				}, () => CsvPortalItem != null);
			}
		}

		/// <summary>
		/// Command called when the "publishSubmit" Button is clicked
		/// </summary>
		public ICommand CmdPublishSubmit
		{
			get
			{
				return new RelayCommand(() =>
				{
					Tuple<bool, string> tup = CheckEmptyFields();
					if (!tup.Item1)
					{
						//publishSubmit.IsEnabled = false;
						ServiceInfo = $@"Portal info: {BaseUrl}{Environment.NewLine}";
						ServiceInfo += "Item info: (Item ID) " + CsvPortalItem.Id + "; (user name) " + Username + "; (File Type) " + FileType + "; (Parameters) " + PublishParameters + "\n";

						//Step 1: get the item name based on its ID
						Tuple<bool, string> itemNameRes = GetServiceName(BaseUrl, Username, CsvPortalItem.Id);
						if (itemNameRes.Item1)
						{
							//Step 2: check if the service name is already in use
							Tuple<bool, string> availableRes = IsServiceNameAvailable(BaseUrl, itemNameRes.Item2, "Feature Service");
							if (availableRes.Item1)
							{
								//Step 3: publish the service based on the portal item
								Tuple<bool, string> publishResult = PublishService(BaseUrl, Username, CsvPortalItem.Id, FileType, PublishParameters);
								if (publishResult.Item1)
								{
									ServiceInfo = "Service created successfully!";
									ServiceInfoForeground = _greenBorderBrush;
									ServiceLinkText = publishResult.Item2;
									//serviceLinkLabel.Visibility = System.Windows.Visibility.Visible;
									//serviceLinkText.Visibility = System.Windows.Visibility.Visible;
								}
								else
								{
									ServiceInfo = publishResult.Item2 + " for request - " + ServiceInfo;
									ServiceInfoForeground = _errorBorderBrush;
								}
							}
							else
							{
								ServiceInfo = "Service Name is not available: " + availableRes.Item2 + " for request - " + ServiceInfo;
								ServiceInfoForeground = _errorBorderBrush;
							}
						}
						else
						{
							ServiceInfo = "Call to get item name failed: " + itemNameRes.Item2 + " for request - " + ServiceInfo;
							ServiceInfoForeground = _errorBorderBrush;
						}
					}
					else
						ServiceInfo = tup.Item2;

				}, () => CsvPortalItem != null);
			}
		}

		public ICommand CmdRefreshPortalContent
		{
			get
			{
				return new RelayCommand(async () =>
				{
					_csvPortalItems.Clear();

					// Use the active portal connection
					var portal = ArcGISPortalManager.Current.GetActivePortal();
					string userName = ArcGISPortalManager.Current.GetActivePortal().GetSignOnUsername();

					//Searching for csv in the current user's content
					var pqp = PortalQueryParameters.CreateForItemsOfTypeWithOwner(PortalItemType.CSV, userName);
					//pqp.Query += $@"owner:\""{userName}\""";

					//Execute to return a result set
					PortalQueryResultSet<PortalItem> results = await ArcGISPortalExtensions.SearchForContentAsync(portal, pqp);
					if (results.Results.Count == 0)
					{
						MessageBox.Show("Please refer to the sample instructions and upload the sample csv file to your Portal content");
					}
					else
					{
						foreach (var pItem in results.Results)
						{
							var theFileType = string.Empty;
							switch (pItem.PortalItemType)
							{
								case PortalItemType.CSV:
									theFileType = "csv";
									break;
								case PortalItemType.MicrosoftExcel:
									theFileType = "excel";
									break;
							}
							_csvPortalItems.Add(new CsvPortalItem() { Name = pItem.Name, Id = pItem.ID, FileType = theFileType });
						}
					}
				}, () => true);
			}
		}

		/// <summary>
		/// Gets the item title/name based on its item ID
		/// </summary>
		/// <param name="baseURI"></param>
		/// <param name="username"></param>
		/// <param name="itemId"></param>
		/// <returns></returns>
		Tuple<bool, string> GetServiceName(string baseURI, string username, string itemId)
		{
			EsriHttpClient myClient = new EsriHttpClient();

			#region REST call to get service name
			string requestUrl = baseURI + @"/sharing/rest/content/users/" + username + @"/items/" + itemId + "?f=json";
			var response = myClient.Get(requestUrl);
			if (response == null)
				return new Tuple<bool, String>(false, "HTTP response is null");
			string outStr = response.Content.ReadAsStringAsync().Result;

			//De-serialize the response in JSON into a usable object. 
			JavaScriptSerializer serializer = new JavaScriptSerializer();
			SharingContracts.OnlineItem obj =
				(SharingContracts.OnlineItem)serializer.Deserialize(outStr, typeof(SharingContracts.OnlineItem));
			if (obj?.item?.title == null)
			{
				SharingContracts.Error err = (SharingContracts.Error)serializer.Deserialize(outStr, typeof(SharingContracts.Error));
				var msg = string.IsNullOrEmpty(err.message) ? $@"error code: {err.code}" : err.message;			
				return new Tuple<bool, String>(false, $@"Failed item call: {msg}");
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
		Tuple<bool, string> IsServiceNameAvailable(string baseURI, string serviceName, string serviceType)
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
			SharingContracts.PortalSelf self_obj = (SharingContracts.PortalSelf)serializer.Deserialize(outStr, typeof(SharingContracts.PortalSelf));
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
			//De-serialize the response in JSON into a usable object. 
			SharingContracts.AvailableResult obj = (SharingContracts.AvailableResult)serializer.Deserialize(outStr, typeof(SharingContracts.AvailableResult));
			if (obj == null)
				return new Tuple<bool, String>(false, "Service fails to be analyzed - " + outStr);
			return new Tuple<bool, string>(obj.available, outStr);
		}

		/// <summary>
		/// Post "analyze" request on the portal item
		/// </summary>
		/// <param name="baseURI"></param>
		/// <param name="itemId"></param>
		/// <param name="fileType"></param>
		/// <param name="analyzeParameters"></param>
		/// <returns></returns>
		Tuple<bool, string> AnalyzeService(string baseURI, string itemId, string fileType, string analyzeParameters)
		{
			EsriHttpClient myClient = new EsriHttpClient();
			string requestUrl = baseURI + @"/sharing/rest/content/features/analyze";

			var postData = new List<KeyValuePair<string, string>>();
			postData.Add(new KeyValuePair<string, string>("f", "json"));
			postData.Add(new KeyValuePair<string, string>("itemId", itemId));
			postData.Add(new KeyValuePair<string, string>("fileType", fileType));
			postData.Add(new KeyValuePair<string, string>("analyzeParameters", analyzeParameters));
			HttpContent content = new FormUrlEncodedContent(postData);

			EsriHttpResponseMessage respMsg = myClient.Post(requestUrl, content);
			if (respMsg == null)
				return new Tuple<bool, String>(false, "HTTP response is null");

			string outStr = respMsg.Content.ReadAsStringAsync().Result;
			//De-serialize the response in JSON into a usable object. 
			JavaScriptSerializer serializer = new JavaScriptSerializer();
			//De-serialize the response in JSON into a usable object. 
			SharingContracts.AnalyzedService obj = (SharingContracts.AnalyzedService)serializer.Deserialize(outStr, typeof(SharingContracts.AnalyzedService));
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
		/// <param name="fileType"></param>
		/// <param name="publishParameters"></param>
		/// <returns>tuple: bool ok/failed string: result msg/error msg</returns>
		Tuple<bool, string> PublishService(string baseURI, string username, string itemId,
				string fileType, string publishParameters)
		{
			EsriHttpClient myClient = new EsriHttpClient();
			string requestUrl = $@"{baseURI}/sharing/rest/content/users/{username}/publish";

			var postData = new List<KeyValuePair<string, string>>();
			postData.Add(new KeyValuePair<string, string>("f", "json"));
			postData.Add(new KeyValuePair<string, string>("itemId", itemId));
			postData.Add(new KeyValuePair<string, string>("fileType", fileType));
			postData.Add(new KeyValuePair<string, string>("publishParameters", publishParameters));
			HttpContent content = new FormUrlEncodedContent(postData);

			EsriHttpResponseMessage respMsg = myClient.Post(requestUrl, content);
			if (respMsg == null)
				return new Tuple<bool, String>(false, "HTTP response is null");

			string outStr = respMsg.Content.ReadAsStringAsync().Result;
			//De-serialize the response in JSON into a usable object. 
			JavaScriptSerializer serializer = new JavaScriptSerializer();
			//De-serialize the response in JSON into a usable object. 
			SharingContracts.PublishedServices obj = (SharingContracts.PublishedServices)serializer.Deserialize(outStr, typeof(SharingContracts.PublishedServices));
			if (obj?.services == null)
			{
				return new Tuple<bool, String>(false, "Service creation fails - " + outStr);
			}

			string respReturn = "";
			foreach (SharingContracts.PublishedService ps in obj.services)
				respReturn += ps.serviceurl;
			if (respReturn == "")
				return new Tuple<bool, String>(false, "Service creation fails - " + outStr);
			else
				return new Tuple<bool, String>(true, respReturn);
		}

		public ICommand CmdGetActivePortal
		{
			get
			{
				return new RelayCommand(() =>
									 {
										 BaseUrl = ArcGISPortalManager.Current.GetActivePortal().PortalUri.ToString();
										 Username = ArcGISPortalManager.Current.GetActivePortal().GetSignOnUsername();
									 }, () => true);
			}
		}

		private Brush _serviceInfoForeground;
		public Brush ServiceInfoForeground
		{
			get { return _serviceInfoForeground; }
			set
			{
				SetProperty(ref _serviceInfoForeground, value, () => ServiceInfoForeground);
			}
		}

		private Brush _baseUrlBorderBrush;
		public Brush BaseUrlBorderBrush
		{
			get { return _baseUrlBorderBrush; }
			set
			{
				SetProperty(ref _baseUrlBorderBrush, value, () => BaseUrlBorderBrush);
			}
		}

		private Brush _portalItemBorderBrush;
		public Brush PortalItemBorderBrush
		{
			get { return _portalItemBorderBrush; }
			set
			{
				SetProperty(ref _portalItemBorderBrush, value, () => PortalItemBorderBrush);
			}
		}

		private Brush _usernameBorderBrush;
		public Brush UsernameBorderBrush
		{
			get { return _usernameBorderBrush; }
			set
			{
				SetProperty(ref _usernameBorderBrush, value, () => UsernameBorderBrush);
			}
		}

		private Brush _fileTypeBorderBrush;
		public Brush FileTypeBorderBrush
		{
			get { return _fileTypeBorderBrush; }
			set
			{
				SetProperty(ref _fileTypeBorderBrush, value, () => FileTypeBorderBrush);
			}
		}

		private Brush _analyzeParametersBorderBrush;
		public Brush AnalyzeParametersBorderBrush
		{
			get { return _analyzeParametersBorderBrush; }
			set
			{
				SetProperty(ref _analyzeParametersBorderBrush, value, () => AnalyzeParametersBorderBrush);
			}
		}

		private Brush _publishParametersBorderBrush;
		public Brush PublishParametersBorderBrush
		{
			get { return _publishParametersBorderBrush; }
			set
			{
				SetProperty(ref _publishParametersBorderBrush, value, () => PublishParametersBorderBrush);
			}
		}

		private string _serviceLinkText;
		public string ServiceLinkText
		{
			get { return string.IsNullOrEmpty(_serviceLinkText) ? string.Empty : _serviceLinkText.Trim(); }
			set
			{
				SetProperty(ref _serviceLinkText, value, () => ServiceLinkText);
			}
		}

		private string _baseUrl;
		public string BaseUrl
		{
			get { return string.IsNullOrEmpty(_baseUrl) ? string.Empty : _baseUrl.Trim(); }
			set
			{
				SetProperty(ref _baseUrl, value, () => BaseUrl);
			}
		}

		private string _username;
		public string Username
		{
			get { return string.IsNullOrEmpty(_username) ? string.Empty : _username.Trim(); }
			set
			{
				SetProperty(ref _username, value, () => Username);
			}
		}

		private string _fileType;
		public string FileType
		{
			get { return _fileType; }
			set
			{
				SetProperty(ref _fileType, value, () => FileType);
			}
		}

		public IList<string> FileTypes
		{
			get
			{
				IList<string> fileTypes = RestAPIFileTypes.Split("|".ToCharArray()).ToList<string>();
				return fileTypes;
			}
		}

		private string _analyzeParameters;
		public string AnalyzeParameters
		{
			get { return _analyzeParameters; }
			set
			{
				SetProperty(ref _analyzeParameters, value, () => AnalyzeParameters);
			}
		}

		private string _publishParameters;
		public string PublishParameters
		{
			get { return _publishParameters; }
			set
			{
				SetProperty(ref _publishParameters, value, () => PublishParameters);
			}
		}

		private string _serviceInfo;
		public string ServiceInfo
		{
			get { return _serviceInfo; }
			set
			{
				SetProperty(ref _serviceInfo, value, () => ServiceInfo);
			}
		}
		
		public ObservableCollection<CsvPortalItem> CsvPortalItems 
		{
			get
			{
				return _csvPortalItems;
			}
			set
			{
				SetProperty(ref _csvPortalItems, value, () => CsvPortalItems);
			}
		}
		
		private CsvPortalItem _csvPortalItem;
		public CsvPortalItem CsvPortalItem
		{
			get
			{
				return _csvPortalItem;
			}
			set
			{
				SetProperty(ref _csvPortalItem, value, () => CsvPortalItem);
				if (_csvPortalItem != null && !string.IsNullOrEmpty(_csvPortalItem.FileType))
				{
					FileType = _csvPortalItem.FileType;
				}
			}
		}

		/// <summary>
		/// Print the de-serialized object into JSON formatted string
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

		/// <summary>
		/// Show the DockPane.
		/// </summary>
		internal static void Show()
		{
			DockPane pane = FrameworkApplication.DockPaneManager.Find(DockPaneID);
			if (pane == null) return;
			pane.Activate();
		}

		/// <summary>
		/// Text shown near the top of the DockPane.
		/// </summary>
		private string _heading = "My DockPane";
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
	internal class Dockpane1_ShowButton : Button
	{
		protected override void OnClick()
		{
			Dockpane1ViewModel.Show();
		}
	}
}
