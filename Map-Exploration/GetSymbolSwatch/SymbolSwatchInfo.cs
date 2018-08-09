using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace GetSymbolSwatch
{
  public class SymbolSwatchInfo
  {
    public SymbolSwatchInfo(string featureclassName, string rendererType, ImageSource previewImage)
    {
      this.FeatureClassName = featureclassName;
      this.RendererType = rendererType;
      this.SymbolImage = previewImage;
    }

    public string FeatureClassName { get; set; }
    public string Note { get; set; }
    public string RendererType { get; set; }
    public ImageSource SymbolImage { get; set; }
  }
}
