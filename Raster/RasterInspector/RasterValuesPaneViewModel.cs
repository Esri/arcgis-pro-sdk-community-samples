// Copyright 2017 Esri

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
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace RasterInspector
{
    /// <summary>
    /// View model for the raster values pane.
    /// </summary>
    internal class RasterValuesPaneViewModel : DockPane
    {
        private const string _dockPaneID = "RasterInspector_RasterValuesPane";
        private object[,] _rasterValues = new object[3,3];
        private static RasterValuesPaneViewModel _instance = null;

        protected RasterValuesPaneViewModel() {
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

        public static RasterValuesPaneViewModel Current
        {
            get
            {
                return _instance ?? (_instance = (RasterValuesPaneViewModel)FrameworkApplication.DockPaneManager.Find(_dockPaneID));
            }
        }

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "Raster Values";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }

        /// <summary>
        /// Raster values read from a 3x3 window at the cursor location.
        /// </summary>
        public object[,] RasterValues
        {
            get { return _rasterValues; }
            set
            {
                // raster values read on the MCT but needs to update the UI
                RasterModule.RunOnUIThread(() =>
                {
                    _rasterValues = value;
                });

                SetProperty(ref _rasterValues, value, () => RasterValues);
            }
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class RasterValuesPane_ShowButton : Button
    {
        protected override void OnClick()
        {
            RasterValuesPaneViewModel.Show();
        }
    }
}
