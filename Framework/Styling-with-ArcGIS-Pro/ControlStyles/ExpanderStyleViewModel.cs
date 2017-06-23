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

