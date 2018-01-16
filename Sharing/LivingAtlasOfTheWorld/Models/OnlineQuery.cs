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
using System.Text;
using LivingAtlasOfTheWorld.Common;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Portal;

namespace LivingAtlasOfTheWorld.Models {
    /// <summary>
    /// Represents the key attributes required to structure online search queries
    /// </summary>
    class OnlineQuery {
        public static readonly string DefaultReferer = "Browse AGS";
        public static readonly int DefaultMaxResults = 100;
        public static readonly int DefaultMaxResponseLength = 2048;
        public static readonly int DefaultNumResultsPerQuery = 25;//AGS default is 10
        public static readonly string DefaultEsriLayerContentTypes = "(type:\"Map Service\" OR type:\"Image Service\" OR type:\"Feature Service\" OR type:\"WMS\" OR type:\"KML\")";
        public static readonly string DefaultEsriWebMapContentTypes = "(type:\"Web Map\" OR type:\"Explorer Map\" OR type:\"Web Mapping Application\" OR type:\"Online Map\")";

        public static readonly string RestAPIReference = "http://resources.arcgis.com/en/help/arcgis-rest-api/";
        public static string EsriGroupId = "b36bd80e51f54ad698f9ae5f292d9ab1";
        public static readonly string QueryBase = "/sharing/rest/search?q=";
        private string _response = "";

        /// <summary>
        /// Gets and sets the Portal URL.
        /// </summary>
        public string Portal
        {
            get
            {
                return ArcGISPortalManager.Current.GetActivePortal().PortalUri.ToString();
            }
        }

        /// <summary>
        /// Gets and sets an associated group id
        /// </summary>
        public string GroupID { get; set; }

        /// <summary>
        /// Gets and sets keywords associated with the query (can be comma delimited)
        /// </summary>
        /// <remarks>These are key~words~, not key~phrases~</remarks>
        public string Keywords { get; set; }

        /// <summary>
        /// Gets and sets the OnlineUri associated with the query - a preconfigured content filter that has
        /// a descriptive name.
        /// </summary>
        public OnlineUri OnlineUri { get; set; }

        /// <summary>
        /// Gets and sets the referer (for the HTTP Header)
        /// </summary>
        public string Referer { get; set; }
        /// <summary>
        /// Gets and Sets the starting number for the query
        /// </summary>
        public int Start { get; set; }
       
        /// <summary>
        /// Gets and Sets the max number of results per query
        /// </summary>
        public int Num { get; set; }
        /// <summary>
        /// Gets and sets the content type for the query
        /// </summary>
        //public string Content { get; set; }
        public PortalItemType Content
        {
            get;  set;            
        }

        /// <summary>
        /// Gets and sets the most recent response from the Portal
        /// </summary>
        public string Response {
            get {
                return _response;
            }
            set {
                _response = value;
            }
        }

        public PortalQueryParameters PortalQuery
        {
            get
            {
                return this.MakePortalQuery();
            }
        }

       
        private PortalQueryParameters MakePortalQuery ()
        {            
            StringBuilder query = new StringBuilder();
            if (!this.GroupID.IsEmpty())
            {
                query.Append(String.Format("group:{0} ", this.GroupID));
            }
            query.Append((string)(this.Content == PortalItemType.Layer ? DefaultEsriLayerContentTypes : DefaultEsriWebMapContentTypes));
            if (!this.Keywords.IsEmpty())
            {
                //tokenize
                string[] _keys = this.Keywords.Split(new char[] { ' ', ',', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);
                query.Append(" AND (");
                string sep = "";
                foreach (string key in _keys)
                {
                    query.Append(String.Format("{0}\"{1}\"", sep, key));
                    sep = " OR ";
                }
                query.Append(")");
            }
            if (!this.OnlineUri.Tags.IsEmpty())
            {
                query.Append(String.Format(" AND (tags:{0})", this.OnlineUri.Tags));
            }
            PortalQueryParameters pq = new PortalQueryParameters(query.ToString());
            pq.StartIndex = Start;
           

            if (Start > 0)
                query.Append(String.Format("&start={0}", this.Start)); 

            pq.Limit = DefaultNumResultsPerQuery;
            query.Append(String.Format("&num={0}", pq.Limit));

            return pq;
            

        }

    }
}
