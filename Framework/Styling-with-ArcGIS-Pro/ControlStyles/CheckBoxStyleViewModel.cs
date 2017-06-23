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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlStyles
{
    class CheckBoxStyleViewModel : StyleViewModelBase
    {
        public CheckBoxStyleViewModel()
        {
            LoadCheckBoxes();
        }
        public override void Initialize()
        {
            if (_listChBx.Count > 0)
                SelectedCheckBox = _listChBx[0];    
        }
        private void LoadCheckBoxes()
        {
            if (_listChBx == null)
            {
                _listChBx = new ObservableCollection<string>();
                foreach (string chkBx in CheckBoxStyles)
                {
                    _listChBx.Add(chkBx);
                }

                this.NotifyPropertyChanged(() => ListOfCheckBoxes);
            }
        }
        private static readonly string[] CheckBoxStyles = { "CheckBox"};

        #region properties
        /// <summary>
        /// Collection of Styles that will be displayed
        /// </summary>

        private ObservableCollection<string> _listChBx;
        public IList<string> ListOfCheckBoxes
        {
            get { return _listChBx; }
        }

        private string _selectedCheckBox;
        public string SelectedCheckBox
        {
            get { return _selectedCheckBox; }
            set
            {
                SetProperty(ref _selectedCheckBox, value, () => SelectedCheckBox);
                StyleXaml = string.Format("<CheckBox Content=\"CheckBox\"  IsChecked=\"True\" IsEnabled=\"True\"></CheckBox>");
            }
        }
        #endregion

    }
}
