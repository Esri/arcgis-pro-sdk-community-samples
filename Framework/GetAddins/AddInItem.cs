/*

   Copyright 2019 Esri

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
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Core.Portal;

namespace GetAddins
{
    internal class AddInItem
    {
        public AddInItem(PortalItem portalItem)
        {
            _id = portalItem.ID;
            _title = portalItem.Title;
            _name = portalItem.Name;
            _thumbnailUrl = portalItem.ThumbnailPath;
            _snippet = string.IsNullOrEmpty(portalItem.Summary) ? _title : portalItem.Summary;
            _group = portalItem.Owner;
            _linkText = "Install Add-in";
            AddInPortalItem = portalItem;

        }

        public PortalItem  AddInPortalItem;

        private string _id;
        public string ID => _id;

        private string _title;
        public string Title => _title;

        private string _name;
        public string Name => _name;

        private string _thumbnailUrl;
        public string ThumbnailUrl => _thumbnailUrl;


        private string _snippet;
        public string Snippet => _snippet;
        public string Text => _name;
        private string _linkText = "";
        public string LinkText => _linkText;

        private string _group;
        public string Group
        {
            get { return _group; }
        }

        public override string ToString()
        {
            return Name;
        }
       
    }
}
