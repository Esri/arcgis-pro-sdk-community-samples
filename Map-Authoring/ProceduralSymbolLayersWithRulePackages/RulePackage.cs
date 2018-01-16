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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Portal;

namespace ProceduralSymbolLayersWithRulePackages
{
    /// <summary>
    /// Represents the Rule package used to render the building footprint layer.
    /// </summary>
    internal class RulePackage
    {
        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="portalItem"></param>       
        public RulePackage(PortalItem portalItem)
        {
            _portalItem = portalItem;
            _id = portalItem.ID;
            _title = portalItem.Title;
            _name = portalItem.Name;
            _thumbnail = portalItem.ThumbnailPath;
            _snippet = string.IsNullOrEmpty(portalItem.Summary) ? _title : portalItem.Summary;
            //_portal = portal;
        }

        private PortalItem _portalItem;
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

        //private string _thumbnailUrl;

        public string ThumbnailUrl => _thumbnail;

        public PortalItem Portalitem => _portalItem;

        /// <summary>
        /// Dictionary that maps the rule package's attributes to the layer's attributes.
        /// </summary>
        public readonly IDictionary<string, IDictionary<string, string>> RpkAttributeExpressionMapping = new Dictionary<string, IDictionary<string, string>>
        {
               { "Paris Rule package 2014", new Dictionary<string, string> { { "Level_of_Detail", "[Level_of_Detail]"}, { "Num_Floors" , "[Floors]" }, { "Year", "[Year]" } } },
               { "Venice Rule package 2014", new Dictionary<string, string> { { "Nbr_of_Floors", "[Floors]" }, { "Roof_Type", "[roof]" }} },
               { "Extrude/Color/Rooftype Rule package 2014", new Dictionary<string, string> { { "Height", "[Floors]*3"}, { "roofForm", "[roof]" }} }
        };
    }
}
