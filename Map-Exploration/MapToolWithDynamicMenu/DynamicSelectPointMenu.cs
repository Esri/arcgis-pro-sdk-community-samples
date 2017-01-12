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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace MapToolWithDynamicMenu
{
    internal class DynamicSelectPointMenu : DynamicMenu
    {
        internal delegate void ClickAction(Tuple<string, string, long> selectedTrippTuple);
        private const string ImagePath =
            @"pack://application:,,,/MapToolWithDynamicMenu;Component/Images/esri_PntFeature.png";
        private static IList<Tuple<string, string, long>> _menuItems = new List<Tuple<string, string, long>>();

        public static void SetMenuPoints(IList<Tuple<string, string, long>> trippleTuples)
        {
            _menuItems = new List<Tuple<string, string, long>>(trippleTuples);
        }
         
        protected override void OnPopup()
        {
            if (_menuItems == null || _menuItems.Count == 0)
            {
                this.Add("No features found", "", false, true, true);
            }
            else
            {
                ClickAction theAction = OnMenuItemClicked;
                Add("Select feature:", "", false, true, true);
                foreach (var tuple in _menuItems)
                {
                    var layer = tuple.Item1;
                    var oid = tuple.Item3;
                    Add($"{layer}: Id {oid}",
                            ImagePath, 
                            false, true, false, theAction, tuple);
                    }
                }
            }

        private static void OnMenuItemClicked(Tuple<string, string, long> selectedTrippTuple)
        {
            QueuedTask.Run(() =>
            {
                var mapView = MapView.Active;
                var layers =
                    mapView.Map.GetLayersAsFlattenedList()
                        .Where(l => l.Name.Equals(selectedTrippTuple.Item1, StringComparison.CurrentCultureIgnoreCase));
                foreach (var featureLayer in layers.OfType<FeatureLayer>())
                {
                    // select the features with a given OID
                    featureLayer.Select(new QueryFilter()
                    {
                        WhereClause = $@"{selectedTrippTuple.Item2} = {selectedTrippTuple.Item3}"
                    });
                    break;
                }
            });
        }
    }
}
