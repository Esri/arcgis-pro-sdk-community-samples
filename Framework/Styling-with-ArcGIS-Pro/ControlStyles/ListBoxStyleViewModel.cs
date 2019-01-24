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
    public class ListBoxStyleViewModel : StyleViewModelBase
    {
        public ListBoxStyleViewModel()
        {
            _listListBoxes = null;
            LoadListBoxes();
            //SelectedExpander = _listListBoxes[0];
        }
        #region Properties

        /// <summary>
        /// Collection of Styles that will be displayed
        /// </summary>

        private ObservableCollection<string> _listListBoxes;
        public IList<string> ListOfListBoxes
        {
            get { return _listListBoxes; }
        }

        private string _selectedListBox;
        public string SelectedListBox
        {
            get { return _selectedListBox; }
            set
            {
                SetProperty(ref _selectedListBox, value, () => SelectedListBox);
                StyleXaml = string.Format("<ListBox ItemContainerStyle=\"{{DynamicResource {0}}}\">", _selectedListBox);
            }
        }

        #endregion

        public override void Initialize()
        {
            if (_listListBoxes.Count > 0)
                SelectedListBox = _listListBoxes[0];
        }

        private void LoadListBoxes()
        {
            if (_listListBoxes == null)
            {
                _listListBoxes = new ObservableCollection<string>();
                foreach (string listbox in ListBoxStyles)
                {
                    _listListBoxes.Add(listbox);
                }

                this.NotifyPropertyChanged(() => ListOfListBoxes);
            }
        }
        private static readonly string[] ListBoxStyles = {
                                             "Esri_ListBoxItemHighlightBrush"
                                              };

    }
}
