/*

   Copyright 2018 Esri

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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ArcGIS.Desktop.Framework.Controls;

namespace LivingAtlasOfTheWorld.UI {
    /// <summary>
    /// Interaction logic for BrowseLayersDialog.xaml
    /// </summary>
    public partial class BrowseLayersDialog : ProWindow {

        private BrowseLayersViewModel _vm = null;

        /// <summary>
        /// Default constructor
        /// </summary>
        public BrowseLayersDialog() {
            _vm = new BrowseLayersViewModel();
            this.DataContext = _vm;
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
            _vm.AddLayerToMap(e.Uri.ToString());
            e.Handled = true;
        }

        private void Close_OnClick(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
