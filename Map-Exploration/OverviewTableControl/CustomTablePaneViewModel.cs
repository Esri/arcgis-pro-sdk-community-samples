/*

   Copyright 2018 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       https://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Controls;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;

namespace TableControlSample
{
    internal class CustomTablePaneViewModel : DockPane
    {
        private const string _dockPaneID = "TableControlSample_CustomTablePane";

        private TableControlContent _tableContent = null;
        public TableControlContent TableContent
        {
            get { return _tableContent; }
            set
            {
                SetProperty(ref _tableContent, value, () => TableContent);
            }
        }
        protected CustomTablePaneViewModel()
        {
            var tocSelectionChanged = ArcGIS.Desktop.Mapping.Events.TOCSelectionChangedEvent.Subscribe(onTOCSelectionChanged(), false);
            var activeViewChanged = ArcGIS.Desktop.Mapping.Events.ActiveMapViewChangedEvent.Subscribe(onActiveMapViewChanged(), false);
            //create table content
            CreateTableControlContent();
        }

        private Action<MapViewEventArgs> onTOCSelectionChanged()
        {
            return delegate (MapViewEventArgs args)
            {
                CreateTableControlContent();
            };
        }

        private Action<ActiveMapViewChangedEventArgs> onActiveMapViewChanged()
        {
            return delegate (ActiveMapViewChangedEventArgs args)
            {
                TableContent = null;
            };
        }

        private void CreateTableControlContent()
        {
            TableControlContent tableContent = null;

            var selectedLyrs = MapView.Active.GetSelectedLayers();
            var selectedSTs = MapView.Active.GetSelectedStandaloneTables();
            MapMember item = null;

            if (selectedLyrs != null && selectedLyrs.Count == 1)
            {
                item = selectedLyrs[0];
            }
            else if (selectedSTs != null && selectedSTs.Count == 1)
            {
                item = selectedSTs[0];
            }
            if (TableControlContentFactory.IsMapMemberSupported(item))
                tableContent = TableControlContentFactory.Create(item);

            TableContent = tableContent;
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
        private string _heading = "Custom_TableView";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class CustomTablePane_ShowButton : Button
    {
        protected override void OnClick()
        {
            CustomTablePaneViewModel.Show();
        }
    }
}
