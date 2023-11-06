using ArcGIS.Desktop.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SubstitutionAddIn
{
    //   Copyright 2019 Esri
    //   Licensed under the Apache License, Version 2.0 (the "License");
    //   you may not use this file except in compliance with the License.
    //   You may obtain a copy of the License at

    //       https://www.apache.org/licenses/LICENSE-2.0

    //   Unless required by applicable law or agreed to in writing, software
    //   distributed under the License is distributed on an "AS IS" BASIS,
    //   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    //   See the License for the specific language governing permissions and
    //   limitations under the License. 

    /// <summary>
    /// Interaction logic for Substitution.xaml
    /// </summary>
    public partial class Substitution : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        private ObservableCollection<string> _chosenSubstitution = null;
        private ObservableCollection<bool> _isPermanent = null;
        
        public Substitution()
        {

            InitializeComponent();
            DataContext = this;
        }
        public Substitution(string currentValue, IList<string>availableValues)
        {
            InitializeComponent();
            
            cboSubsitutionValues.ItemsSource = availableValues;
            
            if (currentValue != null && currentValue != "")
            {
                cboSubsitutionValues.SelectedItem = currentValue;
                
                btnDone.IsEnabled = false;
            }
            
            DataContext = this;
        }

        private RelayCommand _doneCommand = null;
        public ICommand DoneCommand => _doneCommand ?? (_doneCommand = new RelayCommand(() => Done()));

        private void Done()
        {                        
            ObservableCollection<string> result = new ObservableCollection<string>();
            result.Add(cboSubsitutionValues.SelectedItem.ToString());
            ChosenSubstitution = result;

            ObservableCollection<bool> result2 = new ObservableCollection<bool>();
            result2.Add(chkPermanent.IsChecked.Value);
            IsPermanent = result2;
            
            Close();
        }

        private void cboSubstitutionValues_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnDone.IsEnabled = true;
            if (cboSubsitutionValues.SelectedItem.ToString() == "<Null>")
            {
                chkPermanent.IsChecked = false;
                chkPermanent.IsEnabled = false;
            }
            else
            {
                chkPermanent.IsEnabled = true;
            }
        }

        private void chkPermanent_Checked(object sender, RoutedEventArgs e)
        {
            if (cboSubsitutionValues.Text != "")
            {
                btnDone.IsEnabled = true;
            }
        }

        public ObservableCollection<string> ChosenSubstitution
        {
            get => _chosenSubstitution;
            set => _chosenSubstitution = value;
        }
        public ObservableCollection<bool> IsPermanent
        {
            get => _isPermanent;
            set => _isPermanent = value;
        }
    }
}
