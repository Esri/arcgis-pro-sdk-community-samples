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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlStyles
{
    public class ButtonStyleViewModel : StyleViewModelBase
    {
        public ButtonStyleViewModel()
        {
            _listButtons = null;
            LoadButtons();
            //SelectedButton = _listButtons[0];
        }
        #region Properties
        
        /// <summary>
        /// Collection of Styles that will be displayed
        /// </summary>

        private ObservableCollection<string> _listButtons;
        public IList<string> ListOfButtons
        {
            get { return _listButtons; }
        }

        private string _selectedButton;
        public string SelectedButton
        {
            get { return _selectedButton; }
            set
            {                             
                SetProperty(ref _selectedButton, value, () => SelectedButton);               
                StyleXaml = string.Format("<Button Content=\"Button\" Style=\"{{DynamicResource {0}}}\" ToolTip=\"Button\">", _selectedButton);
            }
        }

        #endregion

        public override void Initialize()
        {
            if (_listButtons.Count > 0)
                SelectedButton = _listButtons[0];            
        }

        private void LoadButtons()
        {
            if (_listButtons == null)
            {
                _listButtons = new ObservableCollection<string>();
                foreach (string button in ButtonStyles)
                {
                    _listButtons.Add(button);
                }
                
                this.NotifyPropertyChanged(() => ListOfButtons);
            }
        }
        private static readonly string[] ButtonStyles = {
                                              "Esri_Button",
                                             "Esri_ButtonBack",
                                             "Esri_ButtonBackSmall",
                                             "Esri_ButtonForwardSmall",
                                             "Esri_ButtonForwardSmallBorderless",
                                             "Esri_ButtonUpSmall",
                                             "Esri_ButtonDownSmall",
                                             "Esri_ButtonUpSmallBordless",
                                             "Esri_ButtonClose",
                                             "Esri_ButtonBorderless",
                                              };
    }
}
