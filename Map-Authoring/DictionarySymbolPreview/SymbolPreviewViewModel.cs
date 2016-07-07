/*

   Copyright 2016 Esri

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
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using DictionarySymbolPreview.UI;

namespace DictionarySymbolPreview {
    internal class SymbolPreviewViewModel : DockPane {
        public const string _dockPaneID = "DictionarySymbolPreview_SymbolPreview";
        private DictionarySymbolView _theSymbolView = null;

        protected SymbolPreviewViewModel() { }


        public void Initialize(DictionarySymbolView symbolView) {
            _theSymbolView = symbolView;
        }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show() {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            pane.Activate();
        }

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "My DockPane";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }
        /// <summary>
        /// Sets the layer and objectid of the selected feature
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="oid"></param>
        public void SetSelected(BasicFeatureLayer layer, long oid) {
            _theSymbolView.SelectedFeature = new Tuple<BasicFeatureLayer, long>(layer, oid);
        }

        /// <summary>
        /// Gets the layer and objectid of the selected feature
        /// </summary>
        /// <returns></returns>
        public Tuple<BasicFeatureLayer, long> GetSelected() {
            return _theSymbolView.SelectedFeature;
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class SymbolPreview_ShowButton : Button {
        protected override void OnClick() {
            SymbolPreviewViewModel.Show();
        }
    }
}
