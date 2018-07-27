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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace CustomCatalog.ViewModel
{
    class PaneHeader1ViewModel : PanelViewModelBase
    {
        private SubPanel1ViewModel _subPanel1ViewModel;
        private SubPanel2ViewModel _subPanel2ViewModel;
        private SubPanel3ViewModel _subPanel3ViewModel;
        public PaneHeader1ViewModel()
        {
            _subPanel1ViewModel = new SubPanel1ViewModel();
            _subPanel2ViewModel = new SubPanel2ViewModel();
            _subPanel3ViewModel = new SubPanel3ViewModel();

            OptionsMenu = new ObservableCollection<OptionsMenuItem>
            {
                new OptionsMenuItem(new BitmapImage(new Uri("pack://application:,,,/CustomCatalog;component/Resources/colorwheel-32.png")), "Option 1", _subPanel1ViewModel),
                new OptionsMenuItem(new BitmapImage(new Uri("pack://application:,,,/CustomCatalog;component/Resources/EvilGenius32.png")), "Option 2", _subPanel2ViewModel),
                new OptionsMenuItem(new BitmapImage(new Uri("pack://application:,,,/CustomCatalog;component/Resources/panda-32.png")), "Option 3", _subPanel3ViewModel)
            };
            SelectedOption = OptionsMenu[0];
        }
        public override string DisplayName
        {
            get { return "Project View"; }
        }
        private ObservableCollection<OptionsMenuItem> _optionsMenu = new ObservableCollection<OptionsMenuItem>();
        public ObservableCollection<OptionsMenuItem> OptionsMenu
        {
            get { return _optionsMenu; }
            set { SetProperty(ref _optionsMenu, value, () => OptionsMenu); }
        }

        private SubPanelViewModelBase _currentSubPanelPage;
        public SubPanelViewModelBase CurrentSubPanelPage
        {
            get { return _currentSubPanelPage; }
            set { SetProperty(ref _currentSubPanelPage, value, () => CurrentSubPanelPage); }
        }

        private OptionsMenuItem _selectionOption;
        public OptionsMenuItem SelectedOption
        {
            get { return _selectionOption; }
            set {
                SetProperty(ref _selectionOption, value, () => SelectedOption);
                CurrentSubPanelPage = value.SubPanelViewModel;              
            }
        }
    }
}
