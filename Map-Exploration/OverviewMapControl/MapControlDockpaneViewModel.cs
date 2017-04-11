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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Controls;

namespace OverviewMapControl
{
    /// <summary>
    /// Viewmodel for the dockpane that has the map control
    /// </summary>
    internal class MapControlDockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "OverviewMapControl_MapControlDockpane";
        
        protected MapControlDockpaneViewModel()
        {
            ActiveMap = MapView.Active?.Map.Name;
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

        private string _activeMap;
        /// <summary>
        /// The active map
        /// </summary>
        public string ActiveMap
        {
            get { return _activeMap;}
            set { SetProperty(ref _activeMap, value, () => ActiveMap); }
        }

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "My MapControl DockPane";
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
    internal class MapControlDockpane_ShowButton : Button
    {
        protected override void OnClick()
        {
            MapControlDockpaneViewModel.Show();
        }
    }
}
