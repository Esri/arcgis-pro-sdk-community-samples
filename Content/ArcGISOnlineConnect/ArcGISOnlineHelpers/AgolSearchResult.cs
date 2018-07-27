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

using System.Web.Script.Serialization;

namespace ArcGISOnlineConnect.ArcGISOnlineHelpers
{
    /// <summary>
    /// Class used to encapsulate ArcGIS Online item search results
    /// </summary>
    public class AgolSearchResult
    {
        public string query;
        public long total;
        public long start;
        public long num;
        public long nextStart;
        public AgolItem[] results;

        /// <summary>
        /// Convert json string into ArcGIS Online search result class
        /// </summary>
        /// <param name="json">returned json string from AGOL query</param>
        /// <returns>ArcGIS online search result</returns>
        public static AgolSearchResult LoadAgolSearchResult(string json)
        {
            var serializer = new JavaScriptSerializer();
            return serializer.Deserialize<AgolSearchResult>(json);
        }
    }
}
