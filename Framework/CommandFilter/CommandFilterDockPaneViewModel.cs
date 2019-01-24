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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using CommandFilter.Models;

namespace CommandFilter {
    internal class CommandFilterDockPaneViewModel : DockPane {
        private const string _dockPaneID = "CommandFilter_CommandFilterDockPane";
        private bool _isFiltering = false;
        private CommandFilter _cmdFilter;
        private ObservableCollection<CommandFilterItem> _clickedCommands = new ObservableCollection<CommandFilterItem>();

        protected CommandFilterDockPaneViewModel() {
            _cmdFilter = new CommandFilter(_clickedCommands);
        }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show() {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            pane.Activate();
        }

        /// <summary>
        /// Gets and sets the IsFiltering status
        /// </summary>
        public bool IsFiltering {
            get { return _isFiltering; }
            set {
                SetProperty(ref _isFiltering, value, () => IsFiltering);
                NotifyPropertyChanged(new PropertyChangedEventArgs("IsFilteringText"));
                NotifyPropertyChanged(new PropertyChangedEventArgs("IsFilteringTooltip"));
                ToggleFiltering();
            }
        }
        /// <summary>
        /// Gets the Filtering text
        /// </summary>
        public string IsFilteringText {
            get {
                return _isFiltering ? "Filtering is 'ON'" : "Filtering is 'OFF'";
            }
        }

        public string IsFilteringTooltip {
            get {
                return _isFiltering ? "Uncheck to stop filtering" : "Check 'On' to start filtering";
            }
        }
        /// <summary>
        /// Gets and sets whether to show a message box when a command is filtered
        /// </summary>
        public bool ShowMessageBox {
            get {
                return _cmdFilter.PopMessageBox;
            }
            set {
                _cmdFilter.PopMessageBox = value;
                NotifyPropertyChanged(new PropertyChangedEventArgs("ShowMessageBox"));
                NotifyPropertyChanged(new PropertyChangedEventArgs("PopMessageBoxTooltip"));
            }
        }
        /// <summary>
        /// Gets a string indicating if we are popping a message box or not
        /// on the filter
        /// </summary>
        public string PopMessageBoxTooltip {
            get {
                return _cmdFilter.PopMessageBox
                    ? "Uncheck to hide Message Box on Filter"
                    : "Check 'On' to pop a Message Box on Filter";
            }
        }

        /// <summary>
        /// Gets the current command filters
        /// </summary>
        public ObservableCollection<CommandFilterItem> CommandFilters {
            get {
                return _clickedCommands;
            }
        }

        /// <summary>
        /// Toggle the filtering status
        /// </summary>
        public void ToggleFiltering() {
            if (_isFiltering) {
                //start
                _cmdFilter.StartFiltering();
            }
            else {
                //stop
                _cmdFilter.StopFiltering();
            }
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class CommandFilterDockPane_ShowButton : Button {
        protected override void OnClick() {
            CommandFilterDockPaneViewModel.Show();
        }
    }
}
