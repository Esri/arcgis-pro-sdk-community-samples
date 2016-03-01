//   Copyright 2016 Esri
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
using System.Threading;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;


namespace FeatureDynamicMenu
{
    internal class DynamicFeatureSelectionMenu : DynamicMenu
    {
        public delegate void FeatureSelectedDelegate(BasicFeatureLayer layer, long oid);

        private readonly FeatureSelectedDelegate _delegate = null;
        private readonly List<Tuple<BasicFeatureLayer, long>> _selectedFeatures = new List<Tuple<BasicFeatureLayer, long>>();

       public DynamicFeatureSelectionMenu()
        {
            _delegate = OnFeatureSelected;
        }
        protected override void OnPopup()
        {
            _selectedFeatures.Clear();
            Add("Select item to flash the feature:", "", false, true, true);
            if (FeatureSelectionDynamic.FeatureSelection.Count == 0)
            {
                this.Add("Nothing is selected");
            }
            else
            {
                foreach (var kvp in FeatureSelectionDynamic.FeatureSelection)
                {
                    string layer = kvp.Key.Name;
                    var oids = kvp.Value;
                    foreach (var oid in oids)
                    {
                        Add(string.Format("{0}: oid {1}", layer, oid),
                            "", false, true, false, _delegate, kvp.Key, oid);
                        //This is a hack here
                        _selectedFeatures.Add(new Tuple<BasicFeatureLayer, long>(kvp.Key, oid));
                    }
                    this.AddSeparator();
                }
            }

        }

        //protected override void OnClick(int index)
        //{
        //    BasicFeatureLayer bfl = _selectedFeatures[index].Item1;
        //    long oid = _selectedFeatures[index].Item2;
        //    System.Windows.MessageBox.Show(
        //        string.Format("You clicked on {0}: {1}", bfl.Name, oid));
        //    base.OnClick(index);
        //}

        void OnFeatureSelected(BasicFeatureLayer layer, long oid)
        {
            var mapView = MapView.Active;
            mapView?.FlashFeature(layer, oid);
            Thread.Sleep(1000);
            mapView?.FlashFeature(layer, oid);
        }
    }

   

}
