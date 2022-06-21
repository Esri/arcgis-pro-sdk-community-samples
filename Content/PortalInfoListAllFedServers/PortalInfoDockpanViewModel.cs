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
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalInfoListAllFedServers
{
	internal class PortalInfoDockpaneViewModel : DockPane
	{
		private const string _dockPaneID = "PortalInfo_Dockpane";
		private static ArcGISPortalManager _arcGISPortalManager = ArcGISPortalManager.Current;

		public bool envFailure { get; private set; }

		protected PortalInfoDockpaneViewModel()
		{
			_CommandDoQuery = new RelayCommand(() => CmdDoQuery());
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

		/// <summary>
		/// Returns the current active ArcGIS Online string
		/// </summary>
		private string _portalUrl;
		public string PortalUrl
		{
			get
			{
				if (string.IsNullOrEmpty(_portalUrl)) _portalUrl = ArcGISPortalManager.Current.GetActivePortal().PortalUri.ToString();
				return _portalUrl;
			}
			set
			{
				SetProperty(ref _portalUrl, value, () => _portalUrl);
			}
		}

		/// <summary>
		/// text for command query button
		/// </summary>
		private string _CommandQuery = "Run Portal Self calls";
		public string CommandQuery
		{
			get { return _CommandQuery; }
			set
			{
				SetProperty(ref _CommandQuery, value, () => CommandQuery);
			}
		}

		/// <summary>
		/// Command for executing queries.  Bind to this property in the view.
		/// </summary>
		private System.Windows.Input.ICommand _CommandDoQuery;
		public System.Windows.Input.ICommand CommandDoQuery
		{
			get { return _CommandDoQuery; }
		}

		/// <summary>
		/// Query Result display area
		/// </summary>
		private string _QueryResult;
		public string QueryResult
		{
			get
			{
				return _QueryResult;
			}
			set
			{
				SetProperty(ref _QueryResult, value, () => QueryResult);
			}
		}

		/// <summary>
		/// Execute the query command to get these info:
		/// 1. basic portal info;
		/// 2. advanced portal self calls;
		/// 3. fed server and registered ds.
		/// </summary>
		private async void CmdDoQuery()
		{
			#region setup
			StringBuilder out_str = new StringBuilder();
			ArcGISPortal arcGISportal = ArcGISPortalManager.Current.GetActivePortal();
			if (arcGISportal == null)
				return;
      await QueuedTask.Run(() =>
      {
        if (!arcGISportal.IsSignedOn())
        {
          //Calling "SignIn" will trigger the OAuth popup if your credentials are
          //not cached (eg from a previous sign in in the session)

          SignInResult result = arcGISportal.SignIn();
          if (result.success)
          {
            envFailure = false;
          }
          else
            envFailure = true;
        }//fill in username and password manually
				#endregion

				#region portal basic info
				var online = ArcGISPortalManager.Current.GetPortal(new Uri(_portalUrl));

				//print out portal info
				
				out_str.AppendLine("Portal URL: " + arcGISportal.PortalUri);
				out_str.AppendLine("User name: " + arcGISportal.GetSignOnUsername());
				#endregion
			});


			/*
			 * the following calls are only present for portals; For online, just print the info generated above.
			 */
			if (!_portalUrl.Contains("arcgis.com"))
			{
				//get web adaptor
				string wa = arcGISportal.PortalUri.Segments[1];
				_portalUrl = arcGISportal.PortalUri.ToString();

				/*
				 * Make Portal self call to understand the current role - user, publisher or admin? and other portal info
				 */
				#region portal self call
				//get portal self call's response
				UriBuilder selfURL = new UriBuilder(new Uri(_portalUrl))
				{
					Path = wa + "sharing/rest/portals/self",
					Query = "f=json"
				};
				EsriHttpResponseMessage response = new EsriHttpClient().Get(selfURL.Uri.ToString());

				dynamic portalSelf = JObject.Parse(response.Content.ReadAsStringAsync().Result);
				// if the response doesn't contain the user information then it is essentially
				// an anonymous request against the portal
				if (portalSelf.user == null)
					return;
				string userRole = portalSelf.user.role;
				string[] userPriviledges = portalSelf.user.privileges.ToObject<string[]>();

				if (portalSelf.isPortal == null)
					return;
				string IsPortal = (bool)(portalSelf.isPortal) ? "True" : "False";

				if (portalSelf.supportsSceneServices == null)
					return;
				string SupportsSceneServices = (bool)(portalSelf.supportsSceneServices) ? "True" : "False";

				if (portalSelf.helperServices == null)
					return;

				var elevationService = portalSelf.helperServices.elevation;
				var routeService = portalSelf.helperServices.route;

				out_str.AppendLine("User role: " + userRole);
				out_str.AppendLine("User Privileges:");
				userPriviledges.ToList().ForEach(i => out_str.Append("\t" + i));

				out_str.AppendLine();
				out_str.AppendLine("Supports Scene services? " + SupportsSceneServices);
				out_str.AppendLine("Is it a portal? " + IsPortal);
				out_str.AppendLine("Helper Services: (1) elevation: " + ((elevationService == null) ? "null" : elevationService.url) + "\t" +
														"(2) Route: " + ((routeService == null) ? "null" : routeService.url));
				#endregion

				/*
				 * Make rest calls to get the list of fed servers, signaling which is the hosting fed server;
				 * Also to print the registered data sources
				 */
				#region portal self server call
				selfURL = new UriBuilder(new Uri(_portalUrl))
				{
					Path = wa + "sharing/rest/portals/self/servers",
					Query = "f=json" 
				};
				response = new EsriHttpClient().Get(selfURL.Uri.ToString());

				dynamic portalSelf_servers = JObject.Parse(response.Content.ReadAsStringAsync().Result);
				// if the response doesn't contain the user information then it is essentially
				// an anonymous request against the portal
				if (portalSelf_servers == null)
					return;
				out_str.AppendLine();
				out_str.AppendLine("Fed servers:");
				int cnt = 0;
				foreach (var server in portalSelf_servers.servers)
				{
					out_str.Append("\t(" + (++cnt) + ") {" + server.url + ", " + server.adminUrl + ", " + server.serverRole + "}");

					/* 
					 * generate a new token from portal
					 * re-using old tokens would only yield empty item list for data sources
					 */
					#region generate server token
					ArcGISPortal arcGISportal_2 = ArcGISPortalManager.Current.GetActivePortal();
					string server_token = arcGISportal_2.GetToken();
					#endregion

					/*
					 * get the registered data source from each server
					 */
					#region data sources
					string wa_2 = (new Uri(server.adminUrl.Value)).Segments[1];
					//1. get registered shared folder
					selfURL = new UriBuilder(new Uri(server.adminUrl.Value))
					{
						Path = wa_2 + "/admin/data/findItems/",
						Query = "f=json&parentPath=/fileShares&types=folder&token=" + server_token
					};
					response = new EsriHttpClient().Get(selfURL.Uri.ToString());

					dynamic server_registeredFolder = JObject.Parse(response.Content.ReadAsStringAsync().Result);
					// if the response doesn't contain the user information then it is essentially
					// an anonymous request against the portal
					if (server_registeredFolder == null)
						return;
					out_str.AppendLine();
					out_str.AppendLine();
					out_str.Append("Registered Folder:\t");
					if (server_registeredFolder.items == null)
						out_str.Append("\tRegistered Folder not accessible\t");
					foreach (var item in server_registeredFolder.items)
					{
						out_str.Append("\t{" + item.type + " - " + item.info.path + "}");
					}

					//2. get registered shared egdb
					selfURL = new UriBuilder(new Uri(server.adminUrl.Value))
					{
						Path = wa_2 + "/admin/data/findItems/",
						Query = "f=json&parentPath=enterpriseDatabases&types=egdb&token=" + server_token
					};
					response = new EsriHttpClient().Get(selfURL.Uri.ToString());

					dynamic server_registeredDS = JObject.Parse(response.Content.ReadAsStringAsync().Result);
					// if the response doesn't contain the user information then it is essentially
					// an anonymous request against the portal
					if (server_registeredDS == null)
						return;
					out_str.AppendLine();
					out_str.AppendLine();
					out_str.Append("Registered egdb:\t");
					if (server_registeredDS.items == null)
						out_str.Append("\tRegistered egdb not accessible\t");
					foreach (var ds in server_registeredDS.items)
					{
						out_str.Append("\t{" + ds.type + " - " + ds.path + "}");
					}

					//3. get registered nosql
					//query="f=json&parentPath=nosqlDatabases&types=nosql"
					#endregion
				}
				#endregion
			}
			else
			{
				out_str.AppendLine();
				out_str.AppendLine($@"To see further Portal Info details you have to connect to a portal other then ArcGIS Online");
			}

			if (envFailure)
			{
				QueryResult = "Not able to sign into portal!\n\n" + out_str.ToString();
			}
			else
			{
				QueryResult = "Successfully signed into portal\n\n" + out_str.ToString();
			}

			System.Diagnostics.Debug.WriteLine(QueryResult);
		}
	}
}
