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
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Core;
using ConfigWithStartWizard.Models;
using Newtonsoft.Json.Linq;

namespace ConfigWithStartWizard.Common {
    internal class SubmitQuery {
        public static string DefaultUserAgent =
            "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; GTB7.1; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; InfoPath.2; .NET CLR 1.1.4322; .NET4.0C; .NET4.0E; OfficeLiveConnector.1.3; OfficeLivePatch.0.0; MS-RTC LM 8)";

        public static readonly string DefaultReferer = "Browse AGS";
        public static readonly int DefaultMaxResults = 100;
        public static readonly int DefaultMaxResponseLength = 2048;


        private StringBuilder _response;
        private string _errorResponse = "";

        /// <summary>
        /// Gets the original response from the portal
        /// </summary>
        public string Response => _response != null ? _response.ToString() : "";

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

        public Task<bool> DownloadFileAsync(OnlineQuery query) {
            return new EsriHttpClient().GetAsFileAsync(query.DownloadUrl, query.DownloadFileName);
        }

        public async Task ExecDownloadAsync(OnlineQuery query, string fileName) {

            EsriHttpClient httpClient = new EsriHttpClient();
            var response = httpClient.Get(query.DownloadUrl);
            var stm = await response.Content.ReadAsStreamAsync();

            using (MemoryStream ms = new MemoryStream()) {
                stm.CopyTo(ms);
                System.IO.File.WriteAllBytes(fileName, ms.ToArray());
            }
        }

        public async Task ExecDownloadAsync2(OnlineQuery query, string fileName) {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(query.DownloadUrl);
            req.Method = "GET";
            req.Referer = "ProjectViewer";
            req.UserAgent = SubmitQuery.DefaultUserAgent;
            //submit the query
            var response = await req.GetResponseAsync();

            using (MemoryStream ms = new MemoryStream()) {
                response.GetResponseStream().CopyTo(ms);
                System.IO.File.WriteAllBytes(fileName, ms.ToArray());
            }
        }


        /// <summary>
        /// Execute the given query and return the result
        /// </summary>
        /// <param name="query"></param>
        /// <param name="results"></param>
        /// <param name="maxResults"></param>
        /// <returns></returns>
        public string Exec(OnlineQuery query, ObservableCollection<OnlineResultItem> results,
                       int maxResults = 0) {

            if (maxResults == 0)
                maxResults = DefaultMaxResults;
            if (MaxResponseLength == 0)
                MaxResponseLength = DefaultMaxResponseLength;

            _response = new StringBuilder();
            _errorResponse = "";

            //slap in the initial request
            _response.AppendLine(query.FinalUrl);
            _response.AppendLine("");

            results.Clear();

            try {
                Tuple<long, long> stats = new Tuple<long, long>(-1, -1);
                do {

                    query.Start = stats.Item2;

                    System.Diagnostics.Debug.WriteLine("");
                    System.Diagnostics.Debug.WriteLine(query.FinalUrl);
                    System.Diagnostics.Debug.WriteLine("");

                    EsriHttpClient httpClient = new EsriHttpClient();
                    
                    var response = httpClient.Get(query.FinalUri.ToString());
                    var raw = response.Content.ReadAsStringAsync().Result;//block
                    stats = ProcessResults(results, raw, query);
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
            finally {
                //content = _response.ToString()
                //    .Replace("{", "{\r\n")
                //    .Replace("}", "\r\n}")
                //    .Replace(",\"", ",\r\n\"");
            }
            return _response.ToString();
        }

        private Tuple<long, long> ProcessResults(
            ObservableCollection<OnlineResultItem> results,
            string json,
            OnlineQuery query) {
            long num = -1;
            long next = -1;
            if (string.IsNullOrEmpty(json))
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
                    ri.Name = item.name;
                    //ri.Snippet = item.snippet ?? "no snippet";
                    ri.Url = item.url ?? "";
                    string thumb = item.thumbnail ?? "";
                    string s = item.snippet;
                    string t = item.type;
                    string a = item.access;
                    ri.Configure(query.URL, ri.Id, thumb, s, t, a);
                    results.Add(ri);
                }
                num = queryResults.num.Value;
                next = queryResults.nextStart.Value;
            }
            return new Tuple<long, long>(num, next);
        }
    }
}