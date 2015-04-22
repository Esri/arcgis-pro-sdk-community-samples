//   Copyright 2015 Esri
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
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.Data;

namespace MappingSampleAddIns.AddLayer
{
  /// <summary>
  /// Interaction logic for AddLayerDlg.xaml
  /// </summary>
  public partial class AddLayerDlg : Window
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
    private async void btnAddLayer_Click(object sender, RoutedEventArgs e)
    {
      string url = txtUri.Text;
      this.Close();

      try
      {
        Layer lyr = await AddLayer(txtUri.Text);
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
        MessageBox.Show(this, "Failed to add the layer", "Feature Count");
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
        return Layer.Create(new Uri(uri), map);
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
  }
}