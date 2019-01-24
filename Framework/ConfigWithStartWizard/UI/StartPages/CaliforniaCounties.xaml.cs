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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ConfigWithStartWizard.UI.StartPages
{
    /// <summary>
    /// Interaction logic for CaliforniaCounties.xaml
    /// </summary>
    public partial class CaliforniaCounties : UserControl
    {
        public CaliforniaCounties()
        {
            InitializeComponent();
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;

            var vm = DataContext as CaliforniaStartPageViewModel;
            RecentProject project = vm.CreateProject(element.Name);
            if (project != null) project.Open();
        }
    }
}
