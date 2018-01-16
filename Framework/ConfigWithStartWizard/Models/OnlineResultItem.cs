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
namespace ConfigWithStartWizard.Models {
    public enum OnlineItemType {
        ProjectPackage = 0,
        Template,
        WebMap,
        MapPackage,
        Layer,
        RulePackage
    };
    /// <summary>
    /// An individual result from an online query
    /// </summary>
    class OnlineResultItem {
        //private BitmapImage _thumbnail = null;
        private string _thumbnailUrl = "";
        private string _snippet = "";
        private string _linkText = "";
        /// <summary>
        /// Gets and sets the item id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Gets and sets the item title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets and sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets and sets the item snippet
        /// </summary>
        public string Snippet
        {
            get
            {
                return _snippet;
            }
            set
            {
                _snippet = value;
            }
        }
        /// <summary>
        /// Gets and sets the item url
        /// </summary>
        public string Url { get; set; }

        public string LinkText => _linkText;
        /// <summary>
        /// Gets and sets the online item type
        /// </summary>
        public OnlineItemType OnlineItemType { get; set; }
        // /// <summary>
        // /// Gets and sets the underlying item for this result
        // /// </summary>
        // public ArcGIS.Desktop.Core.Item Item { get; set; }
        /// <summary>
        /// Gets the Thumbnail url of the ResultItem
        /// </summary>
        public string ThumbnailUrl
        {
            get
            {
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

            _thumbnailUrl = !string.IsNullOrEmpty(thumbnail)
                ? string.Format("{0}/sharing/content/items/{1}/info/{2}", portal, id, thumbnail)
                : @"http://static.arcgis.com/images/desktopapp.png";
            return _thumbnailUrl;
        }

        /// <summary>
        /// Configure the online item result
        /// </summary>
        /// <param name="portal"></param>
        /// <param name="id"></param>
        /// <param name="thumbnail"></param>
        /// <param name="snippet"></param>
        /// <param name="itemType"></param>
        /// <param name="access"></param>
        public void Configure(string portal, string id, string thumbnail,
                              string snippet, string itemType, string access) {

            //Note: private item thumbnails cannot be retrieved via URL reference
            _thumbnailUrl = !string.IsNullOrEmpty(thumbnail) && access == "public"
               ? string.Format("{0}/sharing/content/items/{1}/info/{2}", portal, id, thumbnail)
               : @"http://static.arcgis.com/images/desktopapp.png";

            if (itemType == "Project Package") {
                this.OnlineItemType = OnlineItemType.ProjectPackage;
                _snippet = snippet + "\r\nArcGIS Pro Project Package";
                _linkText = $"Open {itemType}";
            }
            else if (itemType == "Project Template") {
                this.OnlineItemType = OnlineItemType.Template;
                _snippet = snippet ?? "Project Template";
                _linkText = $"Create Project";
            }
            else if (itemType == "Web Map" || itemType == "Web Scene") {
                this.OnlineItemType = OnlineItemType.WebMap;
                _snippet = snippet ?? this.Name;
                _linkText = $"Open {itemType}";
            }
            else if (itemType == "Pro Map") {
                this.OnlineItemType = OnlineItemType.MapPackage;
                _snippet = snippet ?? this.Name;
                _linkText = $"Open Map Package";
            }
            else if (itemType == "Feature Service" || itemType == "Map Service" || itemType == "Layer Package") {
                this.OnlineItemType = OnlineItemType.Layer;
                _snippet = snippet ?? this.Name;
                _linkText = $"Open {itemType}";
            }
            else if (itemType == "Rule Package") {
                this.OnlineItemType = OnlineItemType.RulePackage;
                _snippet = snippet ?? this.Name;
                _linkText = $"Open {itemType}";
            }
        }
    }
}

