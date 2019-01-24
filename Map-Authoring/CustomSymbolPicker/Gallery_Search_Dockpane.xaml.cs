//   Copyright 2019 Esri
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
using ArcGIS.Core;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading;
using ArcGIS.Desktop.Framework.Threading.Tasks;


namespace CustomSymbolPicker
{
    /// <summary>
    /// Interaction logic for Gallery_Search_DockpaneView.xaml
    /// </summary>
    public partial class Gallery_Search_DockpaneView : UserControl
    {
       
        /// <summary>
        /// This is a custom symbol gallery
        /// </summary>
        public Gallery_Search_DockpaneView()
        {
            InitializeComponent();
        }

       
        #region Event handlers

        //Rebuild the list of referenced styles - this is to make sure we always have the complete list of styles in current project
        private void referencedStyles_DropDownOpened(object sender, EventArgs e)
        {
            ((Gallery_Search_DockpaneViewModel)this.DataContext).FillReferencedStyleList();
            referencedStyles.Items.Refresh();
        }

        #endregion
       
    }
    
}
