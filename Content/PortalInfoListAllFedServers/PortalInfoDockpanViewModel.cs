/*

   Copyright 2026 Esri

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
			_CommandDoQuery = new RelayCommand(async () => await CmdDoQueryAsync());
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
		private async Task CmdDoQueryAsync()
		{
			StringBuilder out_str = new StringBuilder();
			ArcGISPortal arcGISportal = ArcGISPortalManager.Current.GetActivePortal();
			if (arcGISportal == null)
				return;

			try
			{
				envFailure = false;
				await EnsureSignedInAsync(arcGISportal);
				AppendBasicPortalInfo(out_str, arcGISportal);

				if (!arcGISportal.PortalUri.ToString().Contains("arcgis.com", StringComparison.OrdinalIgnoreCase))
				{
					string webAdapter = arcGISportal.PortalUri.Segments[1];
					_portalUrl = arcGISportal.PortalUri.ToString();
					await AppendPortalSelfInfoAsync(out_str, _portalUrl, webAdapter);
					await AppendPortalServerInfoAsync(out_str, _portalUrl);
				}
				else
				{
					out_str.AppendLine();
					out_str.AppendLine(@"To see further Portal Info details you have to connect to a portal other then ArcGIS Online");
				}
			}
			catch (Exception ex)
			{
				envFailure = true;
				out_str.AppendLine();
				out_str.AppendLine("Error while retrieving portal information.");
				out_str.AppendLine(ex.Message);
				System.Diagnostics.Debug.WriteLine(ex);
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

		private async Task EnsureSignedInAsync(ArcGISPortal arcGISportal)
		{
			await QueuedTask.Run(() =>
			{
				if (!arcGISportal.IsSignedOn())
				{
					SignInResult result = arcGISportal.SignIn();
					envFailure = !result.success;
				}
			});
		}

		private static void AppendBasicPortalInfo(StringBuilder out_str, ArcGISPortal arcGISportal)
		{
			out_str.AppendLine("Portal URL: " + arcGISportal.PortalUri);
			out_str.AppendLine("User name: " + arcGISportal.GetSignOnUsername());
		}

		private async Task AppendPortalSelfInfoAsync(StringBuilder out_str, string portalUrl, string webAdapter)
		{
			UriBuilder selfURL = new UriBuilder(new Uri(portalUrl))
			{
				Path = webAdapter + "sharing/rest/portals/self",
				Query = "f=json"
			};

			JObject portalSelf = await GetJsonResponseAsync(selfURL.Uri.ToString());
			if (portalSelf["user"] == null)
				return;

			string userRole = portalSelf["user"]?["role"]?.ToString();
			string[] userPriviledges = portalSelf["user"]?["privileges"]?.ToObject<string[]>() ?? Array.Empty<string>();
			string isPortal = (bool?)portalSelf["isPortal"] == true ? "True" : "False";
			string supportsSceneServices = (bool?)portalSelf["supportsSceneServices"] == true ? "True" : "False";
			string elevationServiceUrl = portalSelf["helperServices"]?["elevation"]?["url"]?.ToString() ?? "null";
			string routeServiceUrl = portalSelf["helperServices"]?["route"]?["url"]?.ToString() ?? "null";

			out_str.AppendLine("User role: " + userRole);
			out_str.AppendLine("User Privileges:");
			userPriviledges.ToList().ForEach(i => out_str.Append("\t" + i));
			out_str.AppendLine();
			out_str.AppendLine("Supports Scene services? " + supportsSceneServices);
			out_str.AppendLine("Is it a portal? " + isPortal);
			out_str.AppendLine("Helper Services: (1) elevation: " + elevationServiceUrl + "\t" +
										"(2) Route: " + routeServiceUrl);
		}

		private async Task AppendPortalServerInfoAsync(StringBuilder out_str, string portalUrl)
		{
			UriBuilder selfURL = new UriBuilder(new Uri(portalUrl))
			{
				Path = ArcGISPortalManager.Current.GetActivePortal().PortalUri.Segments[1] + "sharing/rest/portals/self/servers",
				Query = "f=json"
			};

			JObject portalSelfServers = await GetJsonResponseAsync(selfURL.Uri.ToString());
			JArray servers = portalSelfServers["servers"] as JArray;
			if (servers == null)
				return;

			out_str.AppendLine();
			out_str.AppendLine("Fed servers:");

			int cnt = 0;
			foreach (var server in servers)
			{
				string serverUrl = server["url"]?.ToString() ?? string.Empty;
				string serverAdminUrl = server["adminUrl"]?.ToString() ?? string.Empty;
				string serverRole = server["serverRole"]?.ToString() ?? string.Empty;
				out_str.Append("\t(" + (++cnt) + ") {" + serverUrl + ", " + serverAdminUrl + ", " + serverRole + "}");

				ArcGISPortal arcGISportal = ArcGISPortalManager.Current.GetActivePortal();
				string serverToken = arcGISportal.GetToken();
				await AppendRegisteredDataSourcesAsync(out_str, serverAdminUrl, serverToken);
			}
		}

		private async Task AppendRegisteredDataSourcesAsync(StringBuilder out_str, string serverAdminUrl, string serverToken)
		{
			if (string.IsNullOrEmpty(serverAdminUrl))
				return;

			string webAdapter = (new Uri(serverAdminUrl)).Segments[1];
			JObject serverRegisteredFolder = await GetJsonResponseAsync(BuildDataFindItemsUrl(serverAdminUrl, webAdapter, "/fileShares", "folder", serverToken));
			out_str.AppendLine();
			out_str.AppendLine();
			out_str.Append("Registered Folder:\t");
			AppendItems(out_str, serverRegisteredFolder["items"] as JArray, "\tRegistered Folder not accessible\t", "info", "path");

			JObject serverRegisteredDS = await GetJsonResponseAsync(BuildDataFindItemsUrl(serverAdminUrl, webAdapter, "enterpriseDatabases", "egdb", serverToken));
			out_str.AppendLine();
			out_str.AppendLine();
			out_str.Append("Registered egdb:\t");
			AppendItems(out_str, serverRegisteredDS["items"] as JArray, "\tRegistered egdb not accessible\t", null, "path");
		}

		private static string BuildDataFindItemsUrl(string serverAdminUrl, string webAdapter, string parentPath, string type, string serverToken)
		{
			UriBuilder selfURL = new UriBuilder(new Uri(serverAdminUrl))
			{
				Path = webAdapter + "/admin/data/findItems/",
				Query = "f=json&parentPath=" + parentPath + "&types=" + type + "&token=" + serverToken
			};

			return selfURL.Uri.ToString();
		}

		private static void AppendItems(StringBuilder out_str, JArray items, string emptyMessage, string nestedProperty, string pathProperty)
		{
			if (items == null)
			{
				out_str.Append(emptyMessage);
				return;
			}

			foreach (var item in items)
			{
				string type = item["type"]?.ToString() ?? string.Empty;
				string path = nestedProperty == null
					? item[pathProperty]?.ToString() ?? string.Empty
					: item[nestedProperty]?[pathProperty]?.ToString() ?? string.Empty;
				out_str.Append("\t{" + type + " - " + path + "}");
			}
		}

		private static async Task<JObject> GetJsonResponseAsync(string url)
		{
			EsriHttpResponseMessage response = new EsriHttpClient().Get(url);
			string responseText = await response.Content.ReadAsStringAsync();
			return JObject.Parse(responseText);
		}
	}
}
