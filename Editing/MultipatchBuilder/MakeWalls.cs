/*

   Copyright 2020 Esri

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
using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MultipatchBuilder
{
  internal class MakeWalls : ComboBox
  {
    /// <summary>
    /// Combo Box constructor. Make sure the combox box is enabled if raster layer is selected
    /// and subscribe to the layer selection changed event.
    /// </summary>
    public MakeWalls()
    {
      var textures = new string [] { "Glass", "Red-solid", "Gray-solid" };      
      foreach (var texture in textures)
      {
        Add(MakeComboboxItem (texture));
      }
      SelectedIndex = 0;
    }

    private ComboBoxItem MakeComboboxItem (string sTexture)
    {
      var bitmapPath = $@"pack://application:,,,/MultipatchBuilder;component/Textures/{sTexture.Replace("-","")}Icon.bmp";

      Uri uri = new Uri(bitmapPath, UriKind.Absolute);
      var icon = ArcGIS.Desktop.Internal.Framework.Utilities.BitmapUtil.GetBitmapImage(uri);
      System.Diagnostics.Debug.WriteLine(icon.GetType());

      ComboBoxItem cbi = new ComboBoxItem(sTexture, bitmapPath);
      return cbi;
    }

    /// <summary>
    /// Destructor. Unsubscribe from the layer selection changed event.
    /// </summary>
    ~MakeWalls()
    {
      Clear();
    }

    /// <summary>
    /// The on comboBox selection change event. 
    /// </summary>
    /// <param name="item">The newly selected combo box item</param>
    protected override void OnSelectionChange(ComboBoxItem item)
    {
      if (item == null) return;
      Module1.SelectedTexture = item.Text;
    }
  }
}
