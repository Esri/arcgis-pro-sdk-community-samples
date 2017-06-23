using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ControlStyles
{
    public class DataGridStyleViewModel : StyleViewModelBase
    {
        public DataGridStyleViewModel()
        {
            _listDataGrids = null;
            LoadDataGrids();
            var cust = new Customer();
            foreach (var item in cust.Customers)
                _customers.Add(item);
            //SelectedExpander = _listDataGrids[0];
        }
        #region Properties

        /// <summary>
        /// Collection of Styles that will be displayed
        /// </summary>

        private ObservableCollection<string> _listDataGrids;
        public IList<string> ListOfDataGrids
        {
            get { return _listDataGrids; }
        }

        private string _selectedDataGrid;
        public string SelectedDataGrid
        {
            get { return _selectedDataGrid; }
            set
            {
                SetProperty(ref _selectedDataGrid, value, () => SelectedDataGrid);
                StyleXaml = string.Format("<DataGrid Style=\"{{DynamicResource {0}}}\">", _selectedDataGrid);
            }
        }
        private ObservableCollection<Customer> _customers = new ObservableCollection<Customer>();
        /// <summary>
        /// Sample Records list of customer record objects
        /// </summary>
        public ObservableCollection<Customer> Customers
        {
            get { return _customers; }
            set
            {
                SetProperty(ref _customers, value, () => Customers);
            }
        }


        #endregion

        public override void Initialize()
        {
           
            if (_listDataGrids.Count > 0)
                SelectedDataGrid = _listDataGrids[0];
        }

        private void LoadDataGrids()
        {
            if (_listDataGrids == null)
            {
                _listDataGrids = new ObservableCollection<string>();
                foreach (string dg in DataGridStyles)
                {
                    _listDataGrids.Add(dg);
                }

                this.NotifyPropertyChanged(() => ListOfDataGrids);
            }
        }
        private static readonly string[] DataGridStyles = {
                                             "Esri_DataGrid",
                                             "Esri_DataGridHeaderless",
                                             "Esri_DataGridRowHeaderless"
                                              };
        
    }
}
