//Copyright 2018 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Internal.Mapping;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geocode
{
    /// <summary>
    /// Construct a URI for the ArcGIS Online geocode service
    /// </summary>
    public class GeocodeURI
    {
        //http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer/find?text=380+New+York+Street%2C+Redlands%2C+CA+92373&outFields=match_addr,addr_type,region,postal,country&outSR=102100&maxLocations=5&f=pjson

        private static string _baseURL = "http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer/";
        private static string _verb = "find";
        private static string _format = "pjson";//change to "pjson" to "pretty-print"
        private StringBuilder _url = new StringBuilder();

        /// <summary>
        /// Default constructor. Provide the search text for geocoding and a flag indicating whether or not
        /// to escape the search text. The default is True (i.e. escape the text)
        /// </summary>
        /// <param name="text">The text to search for</param>
        /// <param name="maxResults">The number of results to search for</param>
        /// <param name="escape">True to escape the text, False to include it un-escaped</param>
        public GeocodeURI(string text, int maxResults, bool escape = true)
        {
            //build the URL
            _url.Append(_baseURL);
            _url.Append(_verb);
            string queryStringFormat = "text={0}&outFields={1}&outSR={2}&maxLocations={3}&f={4}";

            //MapView activeView = ProSDKSampleModule.ActiveMapView;
            MapView activeView = MapView.Active;

            int wkid = activeView.Map.SpatialReference.Wkid;

            wkid = 4326;
            //fill in the parameters
            string query = string.Format(queryStringFormat,
                                           escape ? System.Web.HttpUtility.UrlPathEncode(text) : text,
                                           CandidateAttributes.outFields,
                                           wkid,
                                           maxResults,
                                           _format);
            _url.Append("?");
            _url.Append(query);
        }
        /// <summary>
        /// The Uri containing all the relevant parameters and search text
        /// </summary>
        public Uri Uri
        {
            get
            {
                return new Uri(_url.ToString(), UriKind.Absolute);
            }
        }
    }
}
