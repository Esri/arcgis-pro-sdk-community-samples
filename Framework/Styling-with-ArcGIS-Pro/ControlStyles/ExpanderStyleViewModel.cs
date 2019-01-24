/*

   Copyright 2019 Esri

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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlStyles
{
    public class ExpanderStyleViewModel : StyleViewModelBase
    {
        public ExpanderStyleViewModel()
        {
            _listExpanders = null;
            LoadExpanders();
            //SelectedExpander = _listExpanders[0];
        }
        #region Properties

        /// <summary>
        /// Collection of Styles that will be displayed
        /// </summary>

        private ObservableCollection<string> _listExpanders;
        public IList<string> ListOfExpanders
        {
            get { return _listExpanders; }
        }

        private string _selectedExpander;
        public string SelectedExpander
        {
            get { return _selectedExpander; }
            set
            {
                SetProperty(ref _selectedExpander, value, () => SelectedExpander);
                StyleXaml = string.Format("<Expander Style=\"{{DynamicResource {0}}}\">", _selectedExpander);
            }
        }

        #endregion

        public override void Initialize()
        {
            if (_listExpanders.Count > 0)
                SelectedExpander = _listExpanders[0];
        }

        private void LoadExpanders()
        {
            if (_listExpanders == null)
            {
                _listExpanders = new ObservableCollection<string>();
                foreach (string Expander in ExpanderStyles)
                {
                    _listExpanders.Add(Expander);
                }

                this.NotifyPropertyChanged(() => ListOfExpanders);
            }
        }
        private static readonly string[] ExpanderStyles = {
                                             "Esri_ExpanderBorderless",
                                             "Esri_Expander",
                                             "Esri_ExpanderGripper",
                                             "Esri_ExpanderPlus",
                                             "Esri_ExpanderGripperPlus"
                                              };

    }
}

