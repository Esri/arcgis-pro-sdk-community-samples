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
