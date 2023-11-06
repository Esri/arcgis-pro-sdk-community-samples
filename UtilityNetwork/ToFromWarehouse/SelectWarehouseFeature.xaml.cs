using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Internal.Core.CommonControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace ToFromWarehouse
{
    //   Copyright 2019 Esri
    //   Licensed under the Apache License, Version 2.0 (the "License");
    //   you may not use this file except in compliance with the License.
    //   You may obtain a copy of the License at

    //       https://www.apache.org/licenses/LICENSE-2.0

    //   Unless required by applicable law or agreed to in writing, software
    //   distributed under the License is distributed on an "AS IS" BASIS,
    //   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    //   See the License for the specific language governing permissions and
    //   limitations under the License.

    /// <summary>
    /// Interaction logic for SelectWarehouseFeature.xaml
    /// </summary>
    public partial class SelectWarehouseFeature : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        Dictionary<Element, string> _elements = null;
        ObservableDictionary<Element, string> _selectedFeature = null;
        public SelectWarehouseFeature(Dictionary<Element, string> elements)
        {
            _selectedFeature = new ObservableDictionary<Element,string>();
            InitializeComponent();
            foreach (var element in Utilities._WarehouseNames)
            {
                cboWarehouseNames.Items.Add(element.Key);
            }
            _elements = elements;
            DataContext = this;
        }

        private RelayCommand _doneCommand = null;
        public ICommand DoneCommand => _doneCommand ?? (_doneCommand = new RelayCommand(() => Done()));

        private void Done()
        {
            ObservableDictionary<Element,string> result = new ObservableDictionary<Element,string>();
            // find the selected feature in the list to get the element
            string oid = "";
            string ag = "";
            string at = "";
            string ns = "";

            string selectedItem = cboFeatures.SelectedItem.ToString();
            ns = selectedItem.Substring(0, selectedItem.IndexOf("|")).Trim();
            selectedItem = selectedItem.Substring(selectedItem.IndexOf("|") + 1);
            oid = selectedItem.Substring(0,selectedItem.IndexOf("|")).Trim();
            selectedItem = selectedItem.Substring(selectedItem.IndexOf("|") + 1);
            ag = selectedItem.Substring(0,selectedItem.IndexOf("|")).Trim();
            selectedItem = selectedItem.Substring(selectedItem.IndexOf("|") + 1);
            at = selectedItem.ToString().Trim();

            foreach (var element in _elements)
            {
                if (element.Key.NetworkSource.Name == ns && element.Key.ObjectID.ToString() == oid && element.Key.AssetGroup.Name == ag && element.Key.AssetType.Name == at)
                {
                    result.Add(element.Key, element.Value);
                    continue;
                }
            }

            ChosenFeature = result;
            Close();
        }

        private void cboWarehouseNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cboFeatures.Items.Clear();
            foreach (var element in _elements)
            {
                if (element.Value == cboWarehouseNames.SelectedItem.ToString())
                {
                    cboFeatures.Items.Add(element.Key.NetworkSource.Name + " | " + element.Key.ObjectID + " | " + element.Key.AssetGroup.Name + " | " + element.Key.AssetType.Name);
                }
            }

            cboFeatures.IsEnabled = true;
            
        }

        private void cboFeatures_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnDone.IsEnabled = true;
        }

        public ObservableDictionary<Element, string> ChosenFeature
        {
            get => _selectedFeature;
            set => _selectedFeature = value;
        }

    }
}
