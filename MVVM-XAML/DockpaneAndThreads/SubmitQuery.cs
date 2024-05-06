/*

   Copyright 2024 Esri

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
using ArcGIS.Desktop.Core.Portal;
using ArcGIS.Desktop.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace DockpaneAndThreads
{

  internal class SubmitQuery
  {
    public static readonly int DefaultMaxResults = 100;
    private StringBuilder _response;
    private string _errorResponse = string.Empty;

    /// <summary>
    /// Gets the original response from the portal
    /// </summary>
    public string Response => _response != null ? _response.ToString() : string.Empty;

    /// <summary>
    /// Gets the error response if there was an exception
    /// </summary>
    public string ErrorResponse
    {
      get
      {
        return _errorResponse;
      }
    }

    /// <summary>
    /// Gets and sets the (approx) maximum number of characters for the DEBUG response
    /// </summary>
    /// <remarks>Has no effect on the length of the actual response from online</remarks>
    public int MaxResponseLength { get; set; }

    public async Task ExecDownloadAsync(OnlineQuery query, string fileName)
    {

      EsriHttpClient httpClient = new EsriHttpClient();
      var response = httpClient.Get(query.DownloadUrl);
      var stm = await response.Content.ReadAsStreamAsync();

      using (MemoryStream ms = new MemoryStream())
      {
        stm.CopyTo(ms);
        System.IO.File.WriteAllBytes(fileName, ms.ToArray());
      }
    }

		public async void QueryPortalSync(OnlineQuery query,
									 ObservableCollection<OnlinePortalItem> results,
									 string portalUrl,
									 DockpaneListBoxViewModel dockpaneListBoxViewModel,
									 int maxResults = 200)
		{
			if (maxResults == 0)
				maxResults = DefaultMaxResults;

			_response = new StringBuilder();
			_errorResponse = string.Empty;
			_response.AppendLine(query.FinalUrl);
			_response.AppendLine(string.Empty);
			try
			{
				var portal = ArcGISPortalManager.Current.GetPortal(new Uri(portalUrl));
				int startIndex = 0;
				var totalCount = 0;
				int maxCount = maxResults;
				do
				{
					query.Start = startIndex;
					PortalQueryResultSet<PortalItem> portalResults = await ArcGISPortalExtensions.SearchForContentAsync(portal, query.PortalQueryParams);
					if (portalResults.Results.Count <= 0) break;	
					foreach (var item in portalResults.Results.OfType<PortalItem>())
					{
						OnlinePortalItem ri = new()
						{
							Id = item.ID,
							Title = item.Title ?? String.Format("Item {0}", startIndex + ++totalCount),
							Name = item.Name,
							Snippet = item.Description ?? "no snippet",
							Url = item.Url ?? string.Empty,
							ThumbnailUrl = item.ThumbnailPath,
							PortalItemType = item.PortalItemType
						};
						string thumb = item.ThumbnailUri?.ToString() ?? string.Empty;
						string s = item.Description;
						string a = item.Access.ToString();
						ri.Configure(query.URL, ri.Id, thumb, s, item.PortalItemType, a);
						results.Add(ri);
						totalCount++;
						// Show the % complete
						double percent = (double)totalCount / (double)maxCount * 100.0;
						dockpaneListBoxViewModel.ActionOnGuiThread(() =>
						{
							// Update the Progress bar text and value
							dockpaneListBoxViewModel.ProgressText = $@"Added: {totalCount}";
							dockpaneListBoxViewModel.ProgressValue = percent;
						});
					}
					startIndex += portalResults.Results.Count;
				} while (totalCount < maxResults);
			}
			catch (WebException webEx)
			{
				// bad request
				_response.AppendLine(string.Empty);
				_response.AppendLine("WebException: " + webEx.Message);
				_response.AppendLine(query.FinalUrl);
				_response.AppendLine(string.Empty);
				_response.AppendLine(new Uri(query.FinalUrl).Scheme.ToUpper() + " " +
														 ((int)webEx.Status).ToString());
				try
				{
					_errorResponse = new StreamReader(webEx.Response.GetResponseStream()).ReadToEnd();
					_response.AppendLine(_errorResponse);
				}
				catch
				{
				}
				finally
				{
					_errorResponse = _response.ToString();
				}
			}
			finally
			{
			}
		}

		public async Task<(bool Failed, string Error)> QueryPortalAsync(OnlineQuery query,
                    ObservableCollection<OnlinePortalItem> results,
                    string portalUrl, DockpaneListBoxViewModel dockpaneListBoxViewModel,
										int maxResults = 200)
    {
			return await QueuedTask.Run<(bool Failed, string Error)>(async () => 
			{
				if (maxResults == 0)
        maxResults = DefaultMaxResults;

				_response = new StringBuilder();
				_errorResponse = string.Empty;

				//slap in the initial request
				_response.AppendLine(query.FinalUrl);
				_response.AppendLine(string.Empty);
				try
				{
					var portal = ArcGISPortalManager.Current.GetPortal(new Uri(portalUrl));
					int startIndex = 0;
					var totalCount = 0;
					int maxCount = maxResults; 
					do
					{
						query.Start = startIndex;
						PortalQueryResultSet<PortalItem> portalResults = await ArcGISPortalExtensions.SearchForContentAsync(portal, query.PortalQueryParams);
						if (portalResults.Results.Count <= 0) break;
						foreach (var item in portalResults.Results.OfType<PortalItem>())
						{
							OnlinePortalItem ri = new()
							{
								Id = item.ID,
								Title = item.Title ?? String.Format("Item {0}", startIndex + ++totalCount),
								Name = item.Name,
								Snippet = item.Description ?? "no snippet",
								Url = item.Url ?? string.Empty,
								ThumbnailUrl = item.ThumbnailPath,
								PortalItemType = item.PortalItemType
							};
							string thumb = item.ThumbnailUri?.ToString() ?? string.Empty;
							string s = item.Description;
							string a = item.Access.ToString();
							ri.Configure(query.URL, ri.Id, thumb, s, item.PortalItemType, a);
							results.Add(ri);
							totalCount++;
							// TODO: Progress bar: show progress usin totalCount and maxCount
							// Show the % complete
							double percent = (double)totalCount / (double)maxCount * 100.0;
							dockpaneListBoxViewModel.ActionOnGuiThread(() =>
							{
								// TOTDO: Update the Progress bar text and value on the GUI thread
								// Update the Progress bar text and value
								dockpaneListBoxViewModel.ProgressText = $@"Added: {totalCount}";
								dockpaneListBoxViewModel.ProgressValue = percent;
							});
							if (dockpaneListBoxViewModel.RefreshCancelled)
							{
								throw new Exception("User cancelled the refresh");
							}
						}
						startIndex += portalResults.Results.Count;
					} while (totalCount < maxResults);
				}
				catch (WebException webEx)
				{
					//bad request
					_response.AppendLine(string.Empty);
					_response.AppendLine("WebException: " + webEx.Message);
					_response.AppendLine(query.FinalUrl);
					_response.AppendLine(string.Empty);
					_response.AppendLine(new Uri(query.FinalUrl).Scheme.ToUpper() + " " +
															 ((int)webEx.Status).ToString());
					try
					{
						_errorResponse = new StreamReader(webEx.Response.GetResponseStream()).ReadToEnd();
						_response.AppendLine(_errorResponse);
					}
					catch
					{
					}
					finally
					{
						_errorResponse = _response.ToString();
					}
				}
				finally
				{
				}
				return (!string.IsNullOrEmpty(_errorResponse), _errorResponse);
			});
		}

  }
}
