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
    /// Class used to encapsulate ArcGIS Online Folder content query result
    /// </summary>
    public class AgolFolderContent
    {
        /// <summary>
        /// User name
        /// </summary>
        public string username;
        /// <summary>
        /// Total
        /// </summary>
        public long total;
        /// <summary>
        /// start (for multi page)
        /// </summary>
        public long start;
        /// <summary>
        /// number of total records
        /// </summary>
        public long num;
        /// <summary>
        /// nextstart (for multi page)
        /// </summary>
        public long nextStart;
        /// <summary>
        /// Current folder
        /// </summary>
        public AgolFolder currentFolder;
        /// <summary>
        /// Items
        /// </summary>
        public AgolItem[] items;

        /// <summary>
        /// Convert json string into ArcGIS Online Folder content query result
        /// </summary>
        /// <param name="json">returned json string from AGOL query</param>
        /// <returns>ArcGIS online Folder content result</returns>
        public static AgolFolderContent LoadAgolFolderContent(string json)
        {
            var serializer = new JavaScriptSerializer();
            return serializer.Deserialize<AgolFolderContent>(json);
        }
    }
}
