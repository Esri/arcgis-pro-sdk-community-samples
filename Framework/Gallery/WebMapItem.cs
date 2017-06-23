//   Copyright 2017 Esri
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework.Contracts;

namespace GalleryDemo
{
    /// <summary>
    /// Represents a web map item
    /// </summary>
    public class WebMapItem
    {

        public WebMapItem(string portal, string id = "", string title = "", string name = "", string thumbnail = "",
            string snippet = "", string owner="Unknown")
        {
            _id = id;
            _title = title;
            _name = name;
            _thumbnail = thumbnail;
            _snippet = string.IsNullOrEmpty(snippet) ? title : snippet;
            _portal = portal;
            _group = owner;
        }

        private string _id;
        public string ID => _id;

        private string _title;
        public string Title => _title;

        private string _name;
        public string Name => _name;

        private string _thumbnail;
        public string Thumbnail => _thumbnail;

        private string _snippet;
        public string Snippet => _snippet;

        private string _thumbnailUrl;
        public string Text => _name;       

        public string Icon => ThumbnailUrl;
        public string ThumbnailUrl
        {
            get { return SetThumbnailURL(); }
        }

        private string _portal;

        private string _group;
        public string Group
        {
            get { return _group; }
        }
        /// <summary>
        /// Builds the thumbnail URL of a webmap item.
        /// </summary>
        /// <returns></returns>
        private string SetThumbnailURL()
        {

            _thumbnailUrl = !string.IsNullOrEmpty(Thumbnail)
                ? string.Format("{0}/sharing/content/items/{1}/info/{2}", _portal, ID, Thumbnail)
                : @"http://static.arcgis.com/images/desktopapp.png";
            return _thumbnailUrl;
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
