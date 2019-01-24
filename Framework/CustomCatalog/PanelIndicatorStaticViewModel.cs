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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Mapping;
using CustomCatalog.ViewModel;

namespace CustomCatalog
{
    internal class PanelIndicatorStaticViewModel : DockPane
    {
        private const string _dockPaneID = "CustomCatalog_PanelIndicatorStatic";
        private const string _menuID = "CustomCatalog_PanelIndicatorStatic_Menu";
        private PaneHeader1ViewModel _paneH1VM;
        private PaneHeader2ViewModel _paneH2VM;
        protected PanelIndicatorStaticViewModel()
        {
            PrimaryMenuList.Add(new TabControl() { Text = "Project", Tooltip = "PaneHeader1 Tooltip" });
            PrimaryMenuList.Add(new TabControl() { Text = "Portal", Tooltip = "PaneHeader2 Tooltip" });
            _paneH1VM = new PaneHeader1ViewModel();
            _paneH2VM = new PaneHeader2ViewModel();
            _selectedPanelHeaderIndex = 0;
            CurrentPage = _paneH1VM;
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
        #region properties
        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "My DockPane";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }
        private List<TabControl> _primaryMenuList = new List<TabControl>();
        public List<TabControl> PrimaryMenuList
        {
            get { return _primaryMenuList; }
        }
        private int _selectedPanelHeaderIndex = 0;
        public int SelectedPanelHeaderIndex
        {
            get { return _selectedPanelHeaderIndex; }
            set
            {
                SetProperty(ref _selectedPanelHeaderIndex, value, () => SelectedPanelHeaderIndex);
                if (_selectedPanelHeaderIndex == 0)
                    CurrentPage = _paneH1VM;
                if (_selectedPanelHeaderIndex == 1)
                    CurrentPage = _paneH2VM;
            }
        }
        private PanelViewModelBase _currentPage;
        public PanelViewModelBase CurrentPage
        {
            get { return _currentPage; }
            set
            {
                SetProperty(ref _currentPage, value, () => CurrentPage);
            }
        }
        
        #endregion
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
    internal class PanelIndicatorStatic_ShowButton : Button
    {
        protected override void OnClick()
        {
            PanelIndicatorStaticViewModel.Show();
        }
    }
    /// <summary>
    /// Button implementation for the button on the menu of the burger button.
    /// </summary>
    internal class PanelIndicatorStatic_MenuButton : Button
    {
        protected override void OnClick()
        {
        }
    }
}
