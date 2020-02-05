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
