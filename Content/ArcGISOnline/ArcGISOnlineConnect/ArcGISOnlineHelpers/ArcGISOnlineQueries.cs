//   Copyright 2015 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 


using System.Collections.Generic;

namespace ArcGISOnlineConnect.ArcGISOnlineHelpers
{
    /// <summary>
    /// List of ArcGIS online queries with input parameters to test
    /// </summary>
    public class ArcGISOnlineQueries
    {

    /// <summary>
    /// The available canned queries thus far
    /// </summary>
    public enum AGSQueryType {
        None = 0,
        GetRest,
        GetSelf,
        GetSearch,
        GetUserContent,
        GetUserContentForFolder,
        GetGroupMetadata,
        GetGroupContent,
        GetGroupUsers,
        GetUserTags,
        GetAdditemStatus,
        GetItem,
        GetItemInfo,
        GetItemPkInfo,
        GetItemData,
        GetItemComments,
        GetEsriBasemapGroupMetadata,
        GetEsriBasemapGroupContent,
        GetEsriFeaturedContent,
        GetBasemapGroupContent
    }

        /// <summary>
        /// Preformatted AGS queries
        /// </summary>
        public static class AGSQueries
        {
            /// <summary>
            /// The qt uris, key'd by AGS qt type
            /// </summary>
            public static IDictionary<AGSQueryType, string> ArcGisOnlineQueryTypesDictionary = new Dictionary
                <AGSQueryType, string>()
            {
                {AGSQueryType.None, ""},
                {AGSQueryType.GetRest, @"/sharing/rest?f=pjson"},
                {AGSQueryType.GetSelf, @"/sharing/rest/community/self?f=pjson"},
                {AGSQueryType.GetSearch, @"/sharing/rest/search?q={0}&f=pjson;Search Query;Redlands"},
                {AGSQueryType.GetUserContent, "/sharing/content/users/{0}/?f=pjson;User Name;<Enter your ArcGIS Online Username>"},
                {AGSQueryType.GetUserContentForFolder, "/sharing/content/users/{0}/{1}?f=pjson;User Name,Folder;<Enter your ArcGIS Online Username>,<Enter a content folder id>"},
                {AGSQueryType.GetGroupMetadata, "/sharing/community/groups/{0}?f=pjson;Group;<Enter one of you ArcGIS Group Ids>"},
                {
                    AGSQueryType.GetGroupContent,
                    "/sharing/search?q=(group:{0}) -type:\"Code Attachment\"-type:\"Service Definition\"-type:\"Featured Items\" -type:\"Symbol Set\" -type:\"Color Set\" -type:\"Feature Collection\" &num=10&sortField=title&sortOrder=asc&f=pjson;Group;<Enter one of you ArcGIS Group Ids>"
                },
                {AGSQueryType.GetAdditemStatus, "/sharing/content/users/{0}/items/{1}/status?f=pjson;User Name,Items;<Enter your ArcGIS Online Username>,<Enter an ArcGIS Online Item Id>"},
                {AGSQueryType.GetItem, "/sharing/content/items/{0}?f=pjson;Items;<Enter an ArcGIS Online Item Id>"},
                {AGSQueryType.GetItemInfo, "/sharing/content/items/{0}/info/{1};Items;<Enter an ArcGIS Online Item Id>"},
                {AGSQueryType.GetItemPkInfo, "/sharing/content/items/{0}/item.pkinfo;Items;<Enter an ArcGIS Online Item Id>"},
                {AGSQueryType.GetItemData, "/sharing/content/items/{0}/data;Items;<Enter an ArcGIS Online Item Id>"},
                {AGSQueryType.GetItemComments, "/sharing/content/items/{0}/comments?f=pjson;Items;<Enter an ArcGIS Online Item Id>"},
                {
                    AGSQueryType.GetEsriBasemapGroupMetadata,
                    "/sharing/community/groups?q=owner:esri title:\"ArcMap basemaps\"&f=pjson"
                },
                {
                    AGSQueryType.GetEsriBasemapGroupContent,
                    "/sharing/rest/search?q=(group:{0} AND (typekeywords:\"image service\" OR typekeywords:\"layer package\" OR typekeywords:\"map service\" OR typekeywords:\"Web Map\"))&num=100&sortField=name&sortOrder=asc;Group;<Enter one of you ArcGIS Group Ids>"
                },
                {
                    AGSQueryType.GetEsriFeaturedContent,
                    "/sharing/content/items/3893564201854711a3e9608f14149015/relatedItems?relationshipType=FeaturedItems2Item&direction=forward&f=pjson"
                },
                {
                    AGSQueryType.GetBasemapGroupContent,
                    "/sharing/search?q=(group:{0} AND (typekeywords:\"image service\" OR typekeywords:\"layer package\" OR typekeywords:\"map service\" OR typekeywords:\"Web Map\"))&num=100&sortField=name&sortOrder=asc&f=pjson;Group;<Enter one of you ArcGIS Group Ids>"
                },
                {AGSQueryType.GetGroupUsers, "/sharing/rest/community/groups/{0}/users;Group;<Enter one of you ArcGIS Group Ids>"},
                {AGSQueryType.GetUserTags, "/sharing/rest/community/users/{0}/tags;User Name;<Enter your ArcGIS Online Username>"}
            };

            /// <summary>
            /// The initial template or format string for the specified
            /// type...
            /// </summary>
            /// <param name="qt"></param>
            /// <returns></returns>
            public static string ArcGisOnlineQuery(AGSQueryType qt)
            {
                return ArcGisOnlineQueryTypesDictionary[qt];
            }

        }
    }
}

