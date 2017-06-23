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
    public class RadioButtonStyleViewModel : StyleViewModelBase
    {
        public RadioButtonStyleViewModel()
        {
            LoadRadioBtns();
        }
        public override void Initialize()
        {
            if (_listRadioBtns.Count > 0)
                SelectedRadioBtn = _listRadioBtns[0];
        }
        private void LoadRadioBtns()
        {
            if (_listRadioBtns == null)
            {
                _listRadioBtns = new ObservableCollection<string>();
                foreach (string chkBx in CheckBoxStyles)
                {
                    _listRadioBtns.Add(chkBx);
                }

                this.NotifyPropertyChanged(() => ListOfRadioBtns);
            }
        }
        private static readonly string[] CheckBoxStyles = { "RadioButton" };

        #region properties
        /// <summary>
        /// Collection of Styles that will be displayed
        /// </summary>

        private ObservableCollection<string> _listRadioBtns;
        public IList<string> ListOfRadioBtns
        {
            get { return _listRadioBtns; }
        }

        private string _selectedRadioBtn;
        public string SelectedRadioBtn
        {
            get { return _selectedRadioBtn; }
            set
            {
                SetProperty(ref _selectedRadioBtn, value, () => SelectedRadioBtn);
                StyleXaml = string.Format("<RadioButton Content=\"RadioButton\" IsChecked=\"True\" IsEnabled=\"True\"></RadioButton>");
            }
        }
        #endregion
    }
}
