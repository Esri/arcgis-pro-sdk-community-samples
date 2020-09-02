/*

   Copyright 2020 Esri

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
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ArcGIS.Core.Events;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;

namespace TableControlsDockpane
{
    public class AttributeTableViewModel : DockPane
    {
        private const string _dockPaneID = "TableControlsDockpane_AttributeTable";
        public ICommand TableCloseCommand { get; set; }

        public AttributeTableViewModel()
        {
            TableCloseCommand = new RelayCommand((param) => OnCloseTab(param), () => true);
            IsVisibleTabControl = false;
        }

        #region Public Functions

        public void RemoveAllTabs()
        {
            TabItems.Clear();
        }

        public void OpenAttributeTable()
        {
            if (MapView.Active.GetSelectedLayers().Count() > 0)
            {
                foreach (MapMember mapMember in MapView.Active.GetSelectedLayers())
                {
                    if (!TableControlContentFactory.IsMapMemberSupported(mapMember)) continue;
                    string layerName = mapMember.Name;
                    if (!TabItems.Select(n => n.TableName).Contains(layerName))
                    {
                        TabItems.Add(new TabItemViewModel(mapMember, this.Content));
                        TabControlSelectedIndex = TabItems.Count() - 1;
                    }
                    else
                    {
                        int tabIndex = 0;
                        foreach (var item in TabItems)
                        {
                            if (((TabItemViewModel)item).TableName == layerName)
                                break;
                            tabIndex++;
                        }
                        TabControlSelectedIndex = tabIndex;
                    }
                }
            }
            SetTabControlVisibility();
        }

        #endregion Public Functions

        #region Table Properties
        private ObservableCollection<TabItemViewModel> _tabItems = new ObservableCollection<TabItemViewModel>();
        public ObservableCollection<TabItemViewModel> TabItems
        {
            get { return _tabItems; }
            set
            {
                SetProperty(ref _tabItems, value, () => TabItems);
            }
        }

        private int _tabControlSelectedIndex;
        public int TabControlSelectedIndex
        {
            get { return _tabControlSelectedIndex; }
            set
            {
                SetProperty(ref _tabControlSelectedIndex, value, () => TabControlSelectedIndex);
            }
        }

        private bool _isVisibleTabControl;
        public bool IsVisibleTabControl
        {
            get { return _isVisibleTabControl; }
            set
            {
                SetProperty(ref _isVisibleTabControl, value, () => IsVisibleTabControl);
            }
        }
				#endregion

				#region Private Functions

        private void SetTabControlVisibility()
        {
            IsVisibleTabControl = TabItems.Any(); 
        }

				private void OnCloseTab(object param)
        {
            string layerName = param.ToString();
            TabItems.Remove(TabItems.First(n => n.TableName == layerName));

            TabControlSelectedIndex = 0;
            SetTabControlVisibility();
        }

        #endregion Private Functions

        public static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            pane.Activate();
        }

    }

    internal class AttributeTable_ShowButton : Button
    {
        protected override void OnClick()
        {
            AttributeTableViewModel.Show();

            string attributeTablePaneID = "TableControlsDockpane_AttributeTable";
            DockPane pane = FrameworkApplication.DockPaneManager.Find(attributeTablePaneID);
            if (pane == null || pane.DockState == DockPaneState.Hidden)
                return;

            AttributeTableViewModel attributeTableVM = (AttributeTableViewModel)pane;
            attributeTableVM.OpenAttributeTable();
        }
    }
}
