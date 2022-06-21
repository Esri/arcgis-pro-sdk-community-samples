//   Copyright 2019 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 



using Newtonsoft.Json;

namespace ArcGISOnlineConnect.ArcGISOnlineHelpers
{
    /// <summary>
    /// Class used to encapsulate ArcGIS Online Content (from a content query result)
    /// </summary>
    public class AgolUserContent
    {
        public string username;
        public long total;
        public long start;
        public long num;
        public long nextStart;
        public string currentFolder;
        public AgolItem[] items;
        public AgolFolder[] folders;

        /// <summary>
        /// Convert json string into ArcGIS Online user content class
        /// </summary>
        /// <param name="json">returned json string from AGOL query</param>
        /// <returns>ArcGIS online user content</returns>
        public static AgolUserContent LoadAgolUserContent(string json)
        {
            return JsonConvert.DeserializeObject<AgolUserContent>(json);
        }
    }
}
