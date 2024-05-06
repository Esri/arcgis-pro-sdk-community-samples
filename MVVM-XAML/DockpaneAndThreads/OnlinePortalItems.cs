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
using System.Collections;
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
	internal class OnlinePortalItems 
	{
		private OnlineQuery _query;
		private ArcGISPortal _portal = null;
		private Queue<OnlinePortalItem> _portalItems;
		private int _maxResults;

		internal OnlinePortalItems(string _activePortalUri, string _contentType, int maxResults = 200)
		{
			_query = new OnlineQuery(_activePortalUri);
			_query.Configure(_contentType);
			_portal = null;
			// ready the queue
			_portalItems = new();
			_maxResults = maxResults;
		}

		/// <summary>
		/// Gets the next item from the query
		/// </summary>
		/// <returns>null if no more items</returns>
		internal async Task<OnlinePortalItem> GetNextAsync ()
		{
			// only at query startup
			if (_portal == null)
			{
				// delay to allow UI to update
				await QueuedTask.Run(async () => await Task.Delay(1000));
				_portal = ArcGISPortalManager.Current.GetPortal(new Uri(_query.URL));
				_query.Start = 0;

			}
			if (_portalItems.Count > 0)
				return _portalItems.Dequeue();
			// get the next batch of items
			if (_query.Start < _maxResults)
				await QueryPortalAsync();
			if (_portalItems.Count > 0)
				return _portalItems.Dequeue();
			return null;
		}

		/// <summary>
		/// Gets the number of items returned by the query
		/// </summary>
		/// <returns>items returned by the query, -1 if no query was performed</returns>
		internal int Count ()
		{
			return _maxResults;
		}

		private async Task QueryPortalAsync()
		{
			var _response = new StringBuilder();
			var _errorResponse = string.Empty;
			_response.AppendLine(_query.FinalUrl);
			_response.AppendLine(string.Empty);
			try
			{
					PortalQueryResultSet<PortalItem> portalResults = await ArcGISPortalExtensions.SearchForContentAsync(_portal, _query.PortalQueryParams);
					if (portalResults.Results.Count <= 0) return;
					var itemIndex = _query.Start;
					foreach (var item in portalResults.Results.OfType<PortalItem>())
					{
						itemIndex++;
						OnlinePortalItem ri = new()
						{
							Id = item.ID,
							Title = item.Title ?? String.Format("Item {0}", itemIndex),
							Name = item.Name,
							Snippet = item.Description ?? "no snippet",
							Url = item.Url ?? string.Empty,
							ThumbnailUrl = item.ThumbnailPath,
							PortalItemType = item.PortalItemType
						};
						string thumb = item.ThumbnailUri?.ToString() ?? string.Empty;
						string s = item.Description;
						string a = item.Access.ToString();
						ri.Configure(_query.URL, ri.Id, thumb, s, item.PortalItemType, a);
						_portalItems.Enqueue(ri);
					}
					_query.Start = itemIndex;
			}
			catch (WebException webEx)
			{
				// bad request
				_response.AppendLine(string.Empty);
				_response.AppendLine("WebException: " + webEx.Message);
				_response.AppendLine(_query.FinalUrl);
				_response.AppendLine(string.Empty);
				_response.AppendLine(new Uri(_query.FinalUrl).Scheme.ToUpper() + " " +
														 ((int)webEx.Status).ToString());
				try
				{
					_errorResponse = new StreamReader(webEx.Response.GetResponseStream()).ReadToEnd();
					_response.AppendLine(_errorResponse);
					_errorResponse = _response.ToString();
					throw new WebException(_errorResponse);
				}
				catch (Exception )
				{
					throw;
				}
			}
			finally
			{
			}
		}
	}
}
