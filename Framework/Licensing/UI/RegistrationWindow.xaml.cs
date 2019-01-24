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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Controls;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;

namespace Licensing.UI
{
    /// <summary>
    /// Interaction logic for RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : ProWindow, INotifyPropertyChanged {

        private ICommand _authorizeCommand = null;
        public RegistrationWindow() {
            InitializeComponent();
            (this.Content as FrameworkElement).DataContext = this;
        }

        /// <summary>
        /// The current Authorization Id being used to authorize the Add-in
        /// </summary>
        public string AuthorizationId {
            get{
                return Module1.AuthorizationId;
            }
            set {
                Module1.AuthorizationId = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Execute the Authorization logic
        /// </summary>
        public ICommand AuthorizeCommand {
            get {
                return _authorizeCommand ?? (_authorizeCommand = new RelayCommand(() => {
                    if (Module1.CheckLicensing(Module1.AuthorizationId)) {
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
