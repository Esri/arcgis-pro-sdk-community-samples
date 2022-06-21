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
