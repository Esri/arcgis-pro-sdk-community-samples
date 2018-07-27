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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace ListGeodataContent
{
    internal class ListContentDockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "ListGeodataContent_ListContentDockpane";
        private ICommand _openGdbCmd;
        private ObservableCollection<GdbItem> _gdbDefinitions = new ObservableCollection<GdbItem>();
        private readonly object _lockGdbDefinitions = new object();

        protected ListContentDockpaneViewModel()
        {
            // By default, WPF data bound collections must be modified on the thread where the bound WPF control was created. 
            // This limitation becomes a problem when you want to fill the collection from a worker thread to produce a nice experience. 
            // For example, a search result list should be gradually filled as more matches are found, without forcing the user to wait until the 
            // whole search is complete.  

            // To get around this limitation, WPF provides a static BindingOperations class that lets you establish an 
            // association between a lock and a collection (e.g., ObservableCollection\<T>). 
            // This association allows bound collections to be updated from threads outside the main GUI thread, 
            // in a coordinated manner without generating the usual exception.  

            BindingOperations.EnableCollectionSynchronization(_gdbDefinitions, _lockGdbDefinitions);
        }

        public ICommand OpenGdbCmd
        {
            get { return _openGdbCmd ?? (_openGdbCmd = new RelayCommand(() => OpenGdbDialog(), () => true)); }
        }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            var pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            pane?.Activate();
        }

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "Show GDB Content";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }

        private string _gdbPath = string.Empty;
        public string GdbPath
        {
            get { return _gdbPath; }
            set
            {
                SetProperty(ref _gdbPath, value, () => GdbPath);
                LookupItems();
            }
        }

        public ObservableCollection<GdbItem> GdbDefinitions
        {
            get { return _gdbDefinitions; }
            set
            {
                SetProperty(ref _gdbDefinitions, value, () => GdbDefinitions);
            }
        }

        private void OpenGdbDialog()
        {
            OpenItemDialog searchGdbDialog = new OpenItemDialog
            {
                Title = "Find GDB",
                InitialLocation = @"C:\Data",
                MultiSelect = false,
                Filter = ItemFilters.geodatabases
            };

            var ok = searchGdbDialog.ShowDialog();
            if (ok != true) return;
            var selectedItems = searchGdbDialog.Items;
            foreach (var selectedItem in selectedItems)
                GdbPath = selectedItem.Path;
        }

        private void LookupItems()
        {
            QueuedTask.Run(() =>
            {
                using (Geodatabase fileGeodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(GdbPath))))
                {
                    IReadOnlyList<FeatureClassDefinition> fcdefinitions = fileGeodatabase.GetDefinitions<FeatureClassDefinition>();
                    lock (_lockGdbDefinitions)
                    {
                        _gdbDefinitions.Clear();
                        foreach (var definition in fcdefinitions)
                        {
                            _gdbDefinitions.Add(new GdbItem() {Name = definition.GetName(), Type = definition.DatasetType.ToString()});
                        }
                    }
                    IReadOnlyList<TableDefinition> tbdefinitions = fileGeodatabase.GetDefinitions<TableDefinition>();
                    lock (_lockGdbDefinitions)
                    {
                        foreach (var definition in tbdefinitions)
                        {
                            _gdbDefinitions.Add(new GdbItem() { Name = definition.GetName(), Type = definition.DatasetType.ToString() });
                        }
                    }

                }
            }).ContinueWith(t =>
            {
                if (t.Exception == null) return;
                var aggException = t.Exception.Flatten();
                foreach (var exception in aggException.InnerExceptions)
                    System.Diagnostics.Debug.WriteLine(exception.Message);
            });
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class ListContentDockpane_ShowButton : Button
    {
        protected override void OnClick()
        {
            ListContentDockpaneViewModel.Show();
        }
    }

    internal class GdbItem
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
