//Copyright 2019 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using LivingAtlasOfTheWorld.UI;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace LivingAtlasOfTheWorld {
    internal class AddLayerButton_button1 : Button {
        private static bool _isOpen = false;
        private BrowseLayersDialog _dlg = null;

        protected override void OnClick() {
            if (_isOpen)
                return;
            _isOpen = true;
            _dlg = new BrowseLayersDialog();
            _dlg.Closing += bld_Closing;
            _dlg.Owner = FrameworkApplication.Current.MainWindow;
            _dlg.Show();
        }

        void bld_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            _dlg = null;
            _isOpen = false;
        }
    }
}
