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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace FontStyles
{
    internal class Dockpane1ViewModel : DockPane
    {
        private const string _dockPaneID = "FontStyles_Dockpane1";
        private const string _menuID = "FontStyles_Dockpane1_Menu";


        protected Dockpane1ViewModel()
        {
            ContentHeader = "Content Header";
            SubContentHeader = "Sub Content Header";
            TextBox1 = "6/23/2013";
            TextBox2 = "Liquibus voluptatur ab ium, quianimillam lam volorem di beaquos ut ulparum laborerum dolorerspel ius disque pel isque volori dus del id etomnihil ignisqu idiorectur aciam ipsus soluptae";
            Label1 = "Label disque pel isque volori";
            Label2 = "Label sfasdfErecti untia cusandipsa dolor";
            Label3 = "Label size";
            DefaultFont = "Default text. SfasdfErecti untia cusandipsa dolor santotatio es ium latentur acillac erionet fugit autdolupic imagnihit autatur sinctatur?";
            DefaultFont1 = "Default text. Quiatiam quidem. Neque quiatia";
            DefaultFont2 = "Label description etc";
            _selectedPanelIndex = 0;
            IsControlPanelActive = true;
            SampleRecords = CollectionViewSource.GetDefaultView(SampleRecord.SampleRecords);
        }
        
        public string DefaultFont { get; set; }

        public string DefaultFont1 { get; set; }

        public string DefaultFont2 { get; set; }

        public string Label1 { get; set; }

        public string Label2 { get; set; }

        public string Label3 { get; set; }

        public string TextBox1 { get; set; }

        public string TextBox2 { get; set; }

        private ICollectionView _sampleRecords;

        /// <summary>
        /// Sample Records list of sample record objects
        /// </summary>
        public ICollectionView SampleRecords
        {
            get { return _sampleRecords; }
            set
            {
                SetProperty(ref _sampleRecords, value, () => SampleRecords);
            }
        }

        public string ContentHeader { get; set; }
        
        public string SubContentHeader { get; set; }

        public bool IsControlPanelActive { get; set; }

        private int _selectedPanelIndex;

        public int SelectedPanelIndex
        {
            get
            {
                return _selectedPanelIndex;
            }
            set 
            {
                SetProperty(ref _selectedPanelIndex, value, () => SelectedPanelIndex);

                IsControlPanelActive = _selectedPanelIndex == 0;
                NotifyPropertyChanged(() => IsControlPanelActive);
            }
        }
        
        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            var pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            pane.Activate();
        }

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "ArcGIS Pro Font Styles";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }

        #region Burger Button

        /// <summary>
        /// Tooltip shown when hovering over the burger button.
        /// </summary>
        public string BurgerButtonTooltip
        {
            get { return "Options"; }
        }

        /// <summary>
        /// Menu shown when burger button is clicked.
        /// </summary>
        public System.Windows.Controls.ContextMenu BurgerButtonMenu
        {
            get { return FrameworkApplication.CreateContextMenu(_menuID); }
        }
        #endregion
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class Dockpane1_ShowButton : Button
    {
        protected override void OnClick()
        {
            Dockpane1ViewModel.Show();
        }
    }

    /// <summary>
    /// Button implementation for the button on the menu of the burger button.
    /// </summary>
    internal class Dockpane1_MenuButton : Button
    {
        protected override void OnClick()
        {
        }
    }
}
