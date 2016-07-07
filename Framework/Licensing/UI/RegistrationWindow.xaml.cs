/*

   Copyright 2016 Esri

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
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ArcGIS.Desktop.Framework;

namespace Licensing.UI {
    /// <summary>
    /// Interaction logic for RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window, INotifyPropertyChanged {

        private ICommand _authorizeCommand = null;
        public RegistrationWindow() {
            InitializeComponent();
            (this.Content as FrameworkElement).DataContext = this;
        }

        /// <summary>
        /// The current Authorization ID being used to authorize the Add-in
        /// </summary>
        public string AuthorizationID {
            get{
                return Module1.AuthorizationID;
            }
            set {
                Module1.AuthorizationID = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Execute the Authorization logic
        /// </summary>
        public ICommand AuthorizeCommand {
            get {
                return _authorizeCommand ?? (_authorizeCommand = new RelayCommand(() => {
                    if (Module1.CheckLicensing(Module1.AuthorizationID)) {
                        MessageBox.Show("Add-in authorized. Thank you", "Success!",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else {
                        MessageBox.Show("Invalid Product ID", "Authorization Failed",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    this.Close();
                }));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected virtual void OnPropertyChanged([CallerMemberName] string propName = "") {
            PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
