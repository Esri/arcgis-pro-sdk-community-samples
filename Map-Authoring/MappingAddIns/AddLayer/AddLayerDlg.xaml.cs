//   Copyright 2017 Esri
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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Framework.Controls;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;

namespace MappingSampleAddIns.AddLayer
{
    /// <summary>
    /// Interaction logic for AddLayerDlg.xaml
    /// </summary>
    public partial class AddLayerDlg : ProWindow
    {
        public AddLayerDlg()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Button click event that gets fired when it is clicked on
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnAddLayer_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string url = TxtUri.Text;

            try
            {
                Layer lyr = await AddLayer(TxtUri.Text);
                FeatureLayer flyr = await GetFeatureLayer(lyr);
                if (flyr != null)
                {
                    MessageBox.Show(
                      String.Format("{0}: {1}",
                        "Total number of features in the layer",
                        flyr.GetCount()));
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to add the layer", "Feature Count");
                return;
            }

        }

        /// <summary>
        /// Adds a layer to the current map using the given path or url
        /// </summary>
        /// <remarks>Gets run in the worker thread</remarks>
        /// <param name="uri"></param>
        /// <returns></returns>
        public Task<Layer> AddLayer(string uri)
        {
            return QueuedTask.Run(() =>
            {
                Map map = MapView.Active.Map;
                return LayerFactory.Instance.CreateLayer(new Uri(uri), map);
            });
        }

        /// <summary>
        /// QI to a FeatureLayer, in case of Feature-Service-Layer, it takes the first layer from the group layer
        /// </summary>
        /// <param name="lyr"></param>
        /// <returns></returns>
        public Task<FeatureLayer> GetFeatureLayer(Layer lyr)
        {
            return QueuedTask.Run(() =>
            {
                if (lyr is ServiceLayer)
                    return null;

                if (lyr is ILayerContainer)
                    return ((ILayerContainer)lyr).GetLayersAsFlattenedList()[0] as FeatureLayer;
                else
                    return lyr as FeatureLayer;
            });
        }

        private void TxtUri_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            BtnAddLayer.IsEnabled = !string.IsNullOrEmpty ( TxtUri.Text);
        }

        public void btnGetLayer_Click (object sender, RoutedEventArgs e)
        {
            OpenItemDialog pathDialog = new OpenItemDialog()
            {
              Title = "Select Layer to Add",
              InitialLocation = @"C:\Data\",
              MultiSelect = false,
              Filter = ItemFilters.composite_addToMap
            };
            bool? ok = pathDialog.ShowDialog();

            if (ok == true)
            {
                IEnumerable<Item> selectedItems = pathDialog.Items;
                foreach (Item selectedItem in selectedItems)
                    TxtUri.Text = selectedItem.Path;
            }
            this.Topmost = true;
        }
    }

}