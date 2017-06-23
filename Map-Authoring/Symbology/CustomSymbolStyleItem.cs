using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace Symbology
{
    /// <summary>
    /// Represents a custom symbol style item.
    /// </summary>
    /// <remarks>Procedural symbol items do not have a preview image. So a custom png is used to display these symbol types in the dockpane</remarks>
    public class CustomSymbolStyleItem : INotifyPropertyChanged
    {
        public CustomSymbolStyleItem(SymbolStyleItem symbolStyleItem, string symbolKey)
        {
            if (symbolKey.ToLower().Contains("procedural"))
                SymbolImageSource = new BitmapImage(new Uri($@"pack://application:,,,/Symbology;component/Images/{symbolKey}.png"));

            QueuedTask.Run(() =>
                { 
                    SymbolName = symbolStyleItem.Name;
                    if (!symbolKey.ToLower().Contains("procedural"))
                        SymbolImageSource = symbolStyleItem.PreviewImage;
                    NotifyPropertyChanged("SymbolImageSource");
                });

            NotifyPropertyChanged("SymbolImageSource");

        }

        public ImageSource SymbolImageSource
        {
            get;set;           
        }

        public string SymbolName
        {
            get;set;
        }


        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void NotifyPropertyChanged([CallerMemberName] string propName = "")
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
