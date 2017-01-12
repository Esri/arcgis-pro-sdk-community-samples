//   Copyright 2017 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System;
using System.Collections.Generic;
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
using System.Xml.Linq;
namespace FontStyles
{
    /// <summary>
    /// Interaction logic for Dockpane1View.xaml
    /// </summary>
    public partial class Dockpane1View : UserControl
    {
        public Dockpane1View()
        {
            InitializeComponent();
            this.Loaded += Dockpane1View_Loaded;
        }

        private void Dockpane1View_Loaded(object sender, RoutedEventArgs e)
        {
            var xaml = Properties.Resources.DockpaneXaml;
            var startText = xaml.IndexOf(@"<!--Start-->");
            if (startText >= 0) xaml = xaml.Substring(startText + @"<!--Start-->".Length);
            var stopText = xaml.IndexOf(@"<!--Stop-->");
            if (stopText >= 0) xaml = xaml.Substring(0, stopText);
            XmlTextEditor.Text = xaml;



            //Alternative Method for displaying Xaml is by using the XamlWriter Class.
            //This commented code below shows this method.
            //string xaml = System.Windows.Markup.XamlWriter.Save(this.XamlGrid);
            //string indentedXaml = XElement.Parse(xaml).ToString();
            // XmlTextEditor.Text = indentedXaml;
        }
    }
}
