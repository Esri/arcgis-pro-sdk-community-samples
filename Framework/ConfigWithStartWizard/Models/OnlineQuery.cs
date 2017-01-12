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

namespace ConfigWithStartWizard.Models {
    /// <summary>
    /// FYI - item download: http://prosdk.maps.arcgis.com/sharing/rest/content/items/ba363ce08a0541ac98ce31cbffa6bed0/data?token=
    /// Response headers:
    /// Content-Disposition = attachment; filename=\"Esri.ppkx\"\r\nAccept-Ranges = bytes
    /// Content-Length = 178654
    /// Content-Type = application/octet-stream
    /// Date = Wed, 31 Aug 2016 23:41:46 GMT
    /// ETag = \"1cc185860d286627659253b73c4ce5f1-1\"
    /// Last-Modified = Thu, 25 Aug 2016 20:36:11 GMT
    /// Server = AmazonS3
    /// </summary>
    internal class OnlineQuery {

        private static Dictionary<string, string> _onlineQueries = new Dictionary<string, string>();
        private string _portalURL = "";
        private string _contentType = "";
        private string _path = "";
        private string _query = "";
        private string _token = "";
        private string _format = "";
        private string _name = "";
        private string _downloadFileName = "";
        private long _start = 0;

        static OnlineQuery() {
            _onlineQueries.Add("Rule packages", "/sharing/rest/search?q=type:\"Rule Package\"");
            _onlineQueries.Add("Projects", "/sharing/rest/search?q=owner:\"cmac860\" AND type:Project Package");
            _onlineQueries.Add("Templates", "/sharing/rest/search?q=owner:\"esri_solutions\" AND type:\"Project Template\"");
            _onlineQueries.Add("Map Packages", "/sharing/rest/search?q=type:\"Pro Map\"");
            _onlineQueries.Add("Web Maps", "/sharing/rest/search?q=(type:\"Web Scene\" OR type:\"Web Map\")");
            _onlineQueries.Add("Layers", "/sharing/rest/search?q=(type:\"Layer Package\" OR type:\"Feature Service\" OR type:\"Map Service\")");
            _onlineQueries.Add("Item Data", "/sharing/rest/content/items/{0}/data");
        }

        public static IReadOnlyList<string> Content => _onlineQueries.Keys.ToList();

        public OnlineQuery(string portalURL) {
            _portalURL = portalURL;
        }

        public void Configure(string contentType) {
            Configure(contentType, "", 0);
        }

        public void Configure(string contentType, string token) {
            Configure(contentType, token, 0);
        }

        public void Configure(string contentType, string token, int start) {
            if (!_onlineQueries.ContainsKey(contentType))
                throw new ArgumentException($"Invalid content type: {contentType}");
            _contentType = contentType;
            _token = token;
            _start = start;
            var query = _onlineQueries[_contentType].Split(new char[] { '?' });
            _path = query[0];
            _format = "json";

            _query = query[1] ?? "";
        }

        public void ConfigureData(string token, string id, string name) {
            _contentType = "Item Data";
            _token = token;
            _name = name;
            _start = 0;
            _format = "";
            _query = string.Format(_onlineQueries["Item Data"], id);
            if (!string.IsNullOrEmpty(_token))
                _query += $"?token={_token}";
        }

        public string URL => _portalURL;
        public string Path => _path;

        public long Start
        {
            get
            {
                return _start;
            }
            set
            {
                _start = value;
            }
        }

        public string Query
        {
            get
            {
                var query = _query;
                if (!string.IsNullOrEmpty(_token))
                    query += $"&token={_token}";
                if (_start > 0)
                    query += $"&start={_start}";
                query += $"&f={_format}";
                return query;
            }
        }

        public string Name => _name ;
        public string FinalUrl => URL + Path + "?" + Query;

        public string DownloadUrl => _portalURL + _query;

        public string DownloadFileName
        {
            get
            {
                if (string.IsNullOrEmpty(_downloadFileName))
                    _downloadFileName = ConfigWithStartWizardModule.UniqueFileName(
                                             System.IO.Path.Combine(
                                                  System.IO.Path.Combine(
                                                      Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"), Name));
                return _downloadFileName;
            }
        }
            


        public Uri FinalUri => new Uri(this.FinalUrl, UriKind.Absolute);
    }
}
