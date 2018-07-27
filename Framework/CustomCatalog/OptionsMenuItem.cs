/*

   Copyright 2018 Esri

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
using CustomCatalog.ViewModelsSharkFin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CustomCatalog
{
    public class OptionsMenuItem : INotifyPropertyChanged
    {
        public OptionsMenuItem(BitmapImage imageUri, string optionString, SubPanelViewModelBase subPanelViewModel)
        {
            _imageSource = imageUri;
            _optionString = optionString;
            _subPanelViewModelBase = subPanelViewModel;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnNotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        private SubPanelViewModelBase _subPanelViewModelBase;

        public SubPanelViewModelBase SubPanelViewModel
        {
            get { return _subPanelViewModelBase; }
            set {
                _subPanelViewModelBase = value;
                OnNotifyPropertyChanged("SubPanelViewModel");
            }
        }
        private ImageSource _imageSource;
        public ImageSource ImageSource
        {
            get { return _imageSource; }
            set { _imageSource = value; }
        }

        private string _optionString;
        public string OptionString
        {
            get { return _optionString; }
            set { _optionString = value; }
        }
    }
}
