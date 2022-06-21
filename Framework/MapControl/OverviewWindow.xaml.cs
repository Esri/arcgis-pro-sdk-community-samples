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
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Mapping.Controls;
using ArcGIS.Desktop.Framework.Controls;

namespace MapControl {
    /// <summary>
    /// Interaction logic for OverviewWindow.xaml
    /// </summary>
    public partial class OverviewWindow : ProWindow, INotifyPropertyChanged {

        private MapControlContent _viewContent = null;
        public OverviewWindow() {
            InitializeComponent();
            (this.Content as FrameworkElement).DataContext = this;

            var props = TypeDescriptor.GetProperties(typeof (ArcGIS.Desktop.Mapping.Controls.MapControl))["ViewContent"];
            props.AddValueChanged(this, new EventHandler(ViewContent_Changed));
        }

        private void ViewContent_Changed(object sender, EventArgs e) {
            System.Diagnostics.Debug.WriteLine("ViewContent_Changed");
            
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { }; 

        public MapControlContent ViewContent {
            get {
                return _viewContent;
            }
            set {
                _viewContent = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propName = "") {
            PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

    }
}
