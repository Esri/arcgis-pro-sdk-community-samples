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
