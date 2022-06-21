/*

   Copyright 2019 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       https://www.apache.org/licenses/LICENSE-2.0

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
using System.Windows.Shapes;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace CoordinateSystemAddin.UI {
    /// <summary>
    /// Interaction logic for CoordSysDialog.xaml
    /// </summary>
    public partial class CoordSysDialog : ProWindow {

        private CoordSysViewModel _vm = new CoordSysViewModel();

        /// <summary>
        /// Default constructor
        /// </summary>
        public CoordSysDialog() {
            InitializeComponent();
            this.DataContext = _vm;
            this.CoordinateSystemsControl.SelectedSpatialReferenceChanged += (s, args) => {
                _vm.SelectedSpatialReference = args.SpatialReference;
            };
        }

        /// <summary>
        /// The selected Spatial Reference based on the picker selection
        /// </summary>
        public SpatialReference SpatialReference
        {
            get
            {
                return _vm.SelectedSpatialReference;
            }
        }

        private void Close_OnClick(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
