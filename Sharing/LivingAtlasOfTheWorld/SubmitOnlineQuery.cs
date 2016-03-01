//Copyright 2014 Esri

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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using LivingAtlasOfTheWorld.Common;
using LivingAtlasOfTheWorld.Models;
using ArcGIS.Desktop.Core;
using Newtonsoft.Json.Linq;

namespace LivingAtlasOfTheWorld {
    /// <summary>
    /// Submit queries to online
    /// </summary>
    class SubmitOnlineQuery {
        private StringBuilder _response;
        private string _errorResponse = "";

        /// <summary>
        /// Gets the original response from the portal
        /// </summary>
        public string Response {
            get {
                return _response.ToString();
            }
        }
        /// <summary>
        /// Gets the error response if there was an exception
        /// </summary>
        public string ErrorResponse {
            get {
                return _errorResponse;
            }
        }
        /// <summary>
        /// Gets and sets the (approx) maximum number of characters for the DEBUG response
        /// </summary>
        /// <remarks>Has no effect on the length of the actual response from online</remarks>
        public int MaxResponseLength { get; set; }

        /// <summary>
        /// Execute the given query and return the result
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<string> ExecAsync(OnlineQuery query, ObservableCollection<OnlineResultItem> results, int maxResults = 0) {

            if (maxResults == 0)
                maxResults = OnlineQuery.DefaultMaxResults;
            if (MaxResponseLength == 0)
                MaxResponseLength = OnlineQuery.DefaultMaxResponseLength;

            _response = new StringBuilder();
            _errorResponse = "";

            //slap in the initial request
            _response.AppendLine(query.FinalUrl);
            _response.AppendLine("");

            try {
                Tuple<long, long> stats = new Tuple<long, long>(-1, -1);
                do {

                    query.Start = stats.Item2;

                    Debug.WriteLine("");
                    Debug.WriteLine(query.FinalUrl);
                    Debug.WriteLine("");

                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(new Uri(query.FinalUrl));
                    req.Method = "GET";
                    req.Referer = query.Referer;
                    req.UserAgent = OnlineQuery.DefaultUserAgent;
                    //submit the query
                    var response = await req.GetResponseAsync();

                    //Headers for prosperity if we're debugging the response
                    WebHeaderCollection whc = response.Headers;
                    if (_response.Length < MaxResponseLength) {
                        for (int i = 0; i < whc.Count; i++)
                            _response.AppendLine(whc.GetKey(i) + " = " + whc.Get(i));
                        if (_response.Length > MaxResponseLength)
                            _response.AppendLine("...");
                    }
                    //read out the json
                    using (StreamReader sr = new StreamReader(response.GetResponseStream())) {
                        string raw = await sr.ReadToEndAsync();
                        //convert entity-replacement tags
                        raw = raw.Replace("&lt;", "<").Replace("&gt;", ">");

                        if (_response.Length < MaxResponseLength) {
                            _response.AppendLine("");
                            _response.AppendLine(raw);
                            if (_response.Length > MaxResponseLength)
                                _response.AppendLine("...");
                        }

                        Debug.WriteLine("");
                        Debug.WriteLine(raw);
                        Debug.WriteLine("");

                        //deserialize
                        stats = await ProcessResultsAsync(results, raw, query);
                    }
                } while (stats.Item2 < maxResults && stats.Item2 > 0);

            }
            catch (WebException we) {
                //bad request
                _response.AppendLine("");
                _response.AppendLine("WebException: " + we.Message);
                _response.AppendLine(query.FinalUrl);
                _response.AppendLine("");
                _response.AppendLine(new Uri(query.FinalUrl).Scheme.ToUpper() + " " +
                                ((int)we.Status).ToString());
                try {
                    _errorResponse = new StreamReader(we.Response.GetResponseStream()).ReadToEnd();
                    _response.AppendLine(_errorResponse);
                }
                catch {
                }
            }

            return _response.ToString();
        }

        /// <summary>
        /// Execute the given query and return the result
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<string> ExecWithEsriClientAsync(OnlineQuery query, ObservableCollection<OnlineResultItem> results, int maxResults = 0) {

            if (maxResults == 0)
                maxResults = OnlineQuery.DefaultMaxResults;
            if (MaxResponseLength == 0)
                MaxResponseLength = OnlineQuery.DefaultMaxResponseLength;

            _response = new StringBuilder();
            _errorResponse = "";

            //slap in the initial request
            _response.AppendLine(query.FinalUrl);
            _response.AppendLine("");

            try {
                Tuple<long, long> stats = new Tuple<long, long>(-1, -1);
                do {

                    query.Start = stats.Item2;

                    Debug.WriteLine("");
                    Debug.WriteLine(query.FinalUrl);
                    Debug.WriteLine("");

                    EsriHttpClient httpClient = new EsriHttpClient();
                    //submit the query
                    EsriHttpResponseMessage response = await httpClient.GetAsync(new Uri(query.FinalUrl).ToString());
                    HttpResponseHeaders headers = response.Headers;

                    //read out the json
                    string raw = await response.Content.ReadAsStringAsync();
                    //convert entity-replacement tags
                    raw = raw.Replace("&lt;", "<").Replace("&gt;", ">");
                    if (_response.Length < MaxResponseLength) {
                        _response.AppendLine("");
                        _response.AppendLine(raw);
                        if (_response.Length > MaxResponseLength)
                            _response.AppendLine("...");
                    }

                    Debug.WriteLine("");
                    Debug.WriteLine(raw);
                    Debug.WriteLine("");

                    //deserialize
                    stats = await ProcessResultsAsync(results, raw, query);

                } while (stats.Item2 < maxResults && stats.Item2 > 0);

            }
            catch (WebException we) {
                //bad request
                _response.AppendLine("");
                _response.AppendLine("WebException: " + we.Message);
                _response.AppendLine(query.FinalUrl);
                _response.AppendLine("");
                _response.AppendLine(new Uri(query.FinalUrl).Scheme.ToUpper() + " " +
                                ((int)we.Status).ToString());
                try {
                    _errorResponse = new StreamReader(we.Response.GetResponseStream()).ReadToEnd();
                    _response.AppendLine(_errorResponse);
                }
                catch {
                }
            }

            return _response.ToString();
        }

        private Task<Tuple<long, long>> ProcessResultsAsync(ObservableCollection<OnlineResultItem> results, string json, OnlineQuery query) {
            //do this in the background
            return Task.Run(() => {
                long num = -1;
                long next = -1;
                if (json.IsEmpty())
                    return new Tuple<long, long>(num, next);

                //process the query results
                dynamic queryResults = JObject.Parse(json);
                if (queryResults.error != null)
                    //there was an error in the query
                    return new Tuple<long, long>(num, next);

                long numberOfTotalItems = queryResults.total.Value;
                int count = 0;

                if (numberOfTotalItems > 0) {
                    //these are the results
                    List<dynamic> userItemResults = new List<dynamic>();
                    // store the results in the list
                    userItemResults.AddRange(queryResults.results);
                    foreach (dynamic item in userItemResults) {
                        count++;
                        OnlineResultItem ri = new OnlineResultItem();
                        ri.Id = item.id;
                        ri.Title = item.title ?? String.Format("Item {0}", count);
                        ri.Snippet = item.snippet ?? "no snippet";
                        ri.Url = item.url ?? "";
                        string thumb = item.thumbnail ?? "";
                        ri.SetThumbnailURL(query.Portal, ri.Id, thumb);
                        results.Add(ri);
                    }
                    num = queryResults.num.Value;
                    next = queryResults.nextStart.Value;
                }
                return new Tuple<long, long>(num, next);
            });
        }
    }
}
