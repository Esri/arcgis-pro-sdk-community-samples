/*

   Copyright 2018 Esri

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
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ConfigWithStartWizard.UI {
    /// <summary>
    /// Interaction logic for SignOnStatus.xaml
    /// </summary>
    public partial class SignOnStatus : UserControl {
        public SignOnStatus() {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
            try {
                if (this.ActivePortalURLText.Text != "No portal set") {
                    Process.Start(new ProcessStartInfo(this.ActivePortalURLText.Text));
                }
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine("Failed to process a link with error {0}", ex.Message);
            }
            e.Handled = true;
        }

        private void Hyperlink_RequestNavigate2(object sender, RequestNavigateEventArgs e) {
            try {
                if (this.SignInSignOutText.Text == "Sign in") {
                    ((SignOnStatusViewModel)this.DataContext).Signin();
                }
                else {
                    ((SignOnStatusViewModel)this.DataContext).Signout();
                }
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine("Failed to process a link with error {0}", ex.Message);
            }
            e.Handled = true;
        }
    }
}
