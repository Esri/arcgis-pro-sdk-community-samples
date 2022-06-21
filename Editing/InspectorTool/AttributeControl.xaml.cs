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
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Mapping;
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


namespace InspectorTool
{
    /// <summary>
    /// Interaction logic for AttributeControlView.xaml
    /// </summary>
    public partial class AttributeControlView : UserControl
    {
        AttributeControlViewModel _vm = null;

        public AttributeControlView()
        {
            InitializeComponent();
            _vm = this.DataContext as AttributeControlViewModel;
        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _vm = this.DataContext as AttributeControlViewModel;

            var layerTreeView = sender as TreeView;
            
            var selectedFeature = layerTreeView.Items.CurrentItem;
            var selectionLayer = ((KeyValuePair < MapMember, List< long >> )selectedFeature).Key;

            if (layerTreeView.SelectedItem.GetType() == typeof(System.Int64))
            {
                var selectedOID = Convert.ToInt64(layerTreeView.SelectedItem);

                _vm.AttributeInspector?.LoadAsync(selectionLayer, selectedOID);
            }
            else
            {
                _vm.AttributeInspector.ClearAsync();
            }
        }
    }
}
