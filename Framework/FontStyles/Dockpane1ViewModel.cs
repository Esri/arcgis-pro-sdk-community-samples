//   Copyright 2015 Esri
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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace FontStyles
{
    internal class Dockpane1ViewModel : DockPane
    {
        private const string _dockPaneID = "FontStyles_Dockpane1";
        private const string _menuID = "FontStyles_Dockpane1_Menu";
        private string _contentHeader;
        private string _textBox1;
        private string _label1;
        private string _label2;
        private string _defaultFont;
        private string _subContentHeader;
        private string _defaultFont1;
        private string _defaultfont2;
        private string _textBox2;
        private string _label3;


        protected Dockpane1ViewModel() {

            _contentHeader = "Content Header";
            _textBox1 = "6/23/2013";
            _label1 = "Label disque pel isque volori";
            _label2 = "Label sfasdfErecti untia cusandipsa dolor";
            _defaultFont = "Default text. SfasdfErecti untia cusandipsa dolor santotatio es ium latentur acillac erionet fugit autdolupic imagnihit autatur sinctatur?";
            _subContentHeader = "Sub Content Header";
            _defaultFont1 = "Default text. Quiatiam quidem. Neque quiatia";
            _defaultfont2 = "Label description etc";
            _textBox2 = "Liquibus voluptatur ab ium, quianimillam lam volorem di beaquos ut ulparum laborerum dolorerspel ius disque pel isque volori dus del id etomnihil ignisqu idiorectur aciam ipsus soluptae";
            _label3 = "Label size";
            _selectedPanelIndex = 0;
            IsControlPanelActive = true;
        
        
        }
        public string Label3
        {
            get { return _label3; }
            set { _label3 = value; }
        }


        public string TextBox2
        {
            get { return _textBox2; }
            set { _textBox2 = value; }
        }

        public string Defaultfont2
        {
            get { return _defaultfont2; }
            set { _defaultfont2 = value; }
        }

        public string DefaultFont1
        {
            get { return _defaultFont1; }
            set { _defaultFont1 = value; }
        }

        public string SubContentHeader
        {
            get { return _subContentHeader; }
            set { _subContentHeader = value; }
        }

        public string DefaultFont
        {
            get { return _defaultFont; }
            set { _defaultFont = value; }
        }


        public string Label1
        {
            get { return _label1; }
            set { _label1 = value; }
        }

        public string Label2
        {
            get { return _label2; }
            set { _label2 = value; }
        }

        public string TextBox1
        {
            get { return _textBox1; }
            set { _textBox1 = value; }
        }

        public string ContentHeader
        {
            get { return _contentHeader; }
            set { _contentHeader = value; }
        }

        public bool IsControlPanelActive
        {
            get;
            set;
        }

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
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
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
