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

using System.Collections.Generic;

namespace LivingAtlasOfTheWorld.Models {
    /// <summary>
    /// Creates the list of online query URLS used to retrieve content
    /// </summary>
    class OnlineUriFactory {
        public static List<OnlineUri> OnlineUris = new List<OnlineUri>();

        /// <summary>
        /// Make an OnlineUri with the specified name. The order can be used for sorting.
        /// </summary>
        /// <param name="name">The name of the query</param>
        /// <param name="order">Order for sorting</param>
        /// <param name="indent">True to indent the name by 3 spaces</param>
        /// <returns>An OnlineUri</returns>
        public static OnlineUri CreateOnlineUri(string name, int order, bool indent = false) {
            return CreateOnlineUri(name, "", order);
        }
        /// <summary>
        /// Make an OnlineUri with the specified name and tag string. The order can be used for sorting.
        /// </summary>
        /// <param name="name">The name of the query</param>
        /// <param name="tags">Additional tag(s) to be added to the query string</param>
        /// <param name="order">Order for sorting</param>
        /// <param name="indent">True to indent the name by 3 spaces</param>
        /// <returns>An OnlineUri</returns>
        public static OnlineUri CreateOnlineUri(string name, string tags, int order, bool indent = false) {
            return new OnlineUri() {
                Name = indent ? "   " + name : name,
                Tags = tags,
                Order = order
            };
        }
        /// <summary>
        /// Create the OnlineUris to be used in the Browse dialog
        /// </summary>
        public static void CreateOnlineUris() {
            OnlineUris.Clear();
            OnlineUris.Add(CreateOnlineUri("All Categories", OnlineUris.Count));
            OnlineUris.Add(CreateOnlineUri("Imagery", "imagery", OnlineUris.Count));
            OnlineUris.Add(CreateOnlineUri("Basemap Imagery", "baseimagery", OnlineUris.Count, true));
            OnlineUris.Add(CreateOnlineUri("Multispectral Imagery", "multispectral", OnlineUris.Count, true));
            OnlineUris.Add(CreateOnlineUri("Temporal Imagery", "temporal", OnlineUris.Count, true));
            OnlineUris.Add(CreateOnlineUri("Event Imagery", "event_imagery", OnlineUris.Count, true));
            OnlineUris.Add(CreateOnlineUri("Basemaps", "basemaps", OnlineUris.Count));
            OnlineUris.Add(CreateOnlineUri("Esri Basemaps", "esri_basemap", OnlineUris.Count, true));
            OnlineUris.Add(CreateOnlineUri("Partner Basemaps", "partner_basemap", OnlineUris.Count, true));
            OnlineUris.Add(CreateOnlineUri("User Basemaps", "user_basemap", OnlineUris.Count, true));
            OnlineUris.Add(CreateOnlineUri("Historical Maps", "historical", OnlineUris.Count));
            OnlineUris.Add(CreateOnlineUri("Demographics & Lifestyle Basemaps", "demographics", OnlineUris.Count));
            OnlineUris.Add(CreateOnlineUri("Income & Spending", "income", OnlineUris.Count, true));
            OnlineUris.Add(CreateOnlineUri("Population & Housing", "population", OnlineUris.Count, true));
            OnlineUris.Add(CreateOnlineUri("Segmentation & Behaviors", "behaviors", OnlineUris.Count, true));
            OnlineUris.Add(CreateOnlineUri("Business & Jobs", "business", OnlineUris.Count, true));
            OnlineUris.Add(CreateOnlineUri("At Risk", "at risk", OnlineUris.Count, true));
            OnlineUris.Add(CreateOnlineUri("Landscape", "landscape", OnlineUris.Count));
            OnlineUris.Add(CreateOnlineUri("Climate & Weather", "climate", OnlineUris.Count, true));
            OnlineUris.Add(CreateOnlineUri("Ecology", "ecology", OnlineUris.Count, true));
            OnlineUris.Add(CreateOnlineUri("Species Biology", "species", OnlineUris.Count, true));
            OnlineUris.Add(CreateOnlineUri("Disturbance & Impact", "disturbance", OnlineUris.Count, true));
            OnlineUris.Add(CreateOnlineUri("Elevation", "elevation", OnlineUris.Count, true));
            OnlineUris.Add(CreateOnlineUri("Land Cover", "landcover", OnlineUris.Count, true));
            OnlineUris.Add(CreateOnlineUri("Natural Hazards", "hazards", OnlineUris.Count, true));
            OnlineUris.Add(CreateOnlineUri("Oceans", "oceans", OnlineUris.Count, true));
            OnlineUris.Add(CreateOnlineUri("Soils/Geology", "soils", OnlineUris.Count, true));
            OnlineUris.Add(CreateOnlineUri("Subsurface", "subsurface", OnlineUris.Count, true));
            OnlineUris.Add(CreateOnlineUri("Water", "water", OnlineUris.Count, true));
            OnlineUris.Add(CreateOnlineUri("Earth Observations", "earth observations", OnlineUris.Count));
            OnlineUris.Add(CreateOnlineUri("Urban Systems", "urban", OnlineUris.Count, true));
            OnlineUris.Add(CreateOnlineUri("3D Cities", "3D", OnlineUris.Count, true));
            OnlineUris.Add(CreateOnlineUri("Movement", "movement", OnlineUris.Count, true));
            OnlineUris.Add(CreateOnlineUri("Parcels", "parcels", OnlineUris.Count, true));
            OnlineUris.Add(CreateOnlineUri("People", "people", OnlineUris.Count, true));
            OnlineUris.Add(CreateOnlineUri("Planning", "planning", OnlineUris.Count, true));
            OnlineUris.Add(CreateOnlineUri("Public", "public", OnlineUris.Count, true));
            OnlineUris.Add(CreateOnlineUri("Work", "work", OnlineUris.Count, true));
            OnlineUris.Add(CreateOnlineUri("Transportation", "transportation", OnlineUris.Count));
            OnlineUris.Add(CreateOnlineUri("Boundaries & Places", "boundaries", OnlineUris.Count));
            OnlineUris.Add(CreateOnlineUri("Boundaries", "boundaries", OnlineUris.Count, true));
            OnlineUris.Add(CreateOnlineUri("Places", "places", OnlineUris.Count, true));
        }
    }
}
