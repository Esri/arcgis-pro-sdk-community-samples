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

using ArcGIS.Desktop.Core.Portal;
using LivingAtlasOfTheWorld.Common;

namespace LivingAtlasOfTheWorld.Models {
    /// <summary>
    /// An individual result from an online query
    /// </summary>
    class OnlineResultItem {
        public OnlineResultItem(PortalItem portalItem)
        {
            PortalResultItem = portalItem;
            _thumbnailUrl = portalItem.ThumbnailPath;
            Id = portalItem.ID;
            Title = portalItem.Title;
            Snippet = portalItem.Summary;
        }
        public PortalItem PortalResultItem { get; set; }
        //private BitmapImage _thumbnail = null;
        private string _thumbnailUrl = "";
        /// <summary>
        /// Gets and sets the item id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Gets and sets the item title
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Gets and sets the item snippet
        /// </summary>
        public string Snippet { get; set; }
        /// <summary>
        /// Gets and sets tne item url
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// Gets and sets the underlying item for this result
        /// </summary>
        public ArcGIS.Desktop.Core.Item Item { get; set; }
        /// <summary>
        /// Gets the Thumbnail url of the ResultItem
        /// </summary>
        public string ThumbnailUrl {
            get {
                return _thumbnailUrl;
            }
        }

        /// <summary>
        /// Provide the item thumbnail URL for the given item id and thumbnail url
        /// </summary>
        /// <param name="portal">The portal web url</param>
        /// <param name="thumbnail">The thumbnail url from the online item</param>
        /// <param name="id">the online item id</param>
        /// <returns></returns>
        public string SetThumbnailURL(string portal, string id, string thumbnail) {

            _thumbnailUrl = !thumbnail.IsEmpty()
                ? string.Format("{0}/sharing/content/items/{1}/info/{2}", portal, id, thumbnail)
                : "";
            return _thumbnailUrl;
        }
    }
}
