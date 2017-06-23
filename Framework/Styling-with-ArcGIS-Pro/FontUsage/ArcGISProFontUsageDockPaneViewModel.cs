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

using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Xml.Linq;

namespace ControlStyles.FontUsage
{
    internal class ArcGISProFontUsageDockPaneViewModel : DockPane
    {
        private const string _dockPaneID = "ControlStyles_FontUsage_ArcGISProFontUsageDockPane";

        protected ArcGISProFontUsageDockPaneViewModel() 
        {
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
        private string _heading = "ArcGIS Pro TextBlock Styles";
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
    internal class ArcGISProFontUsageDockPane_ShowButton : Button
    {
        protected override void OnClick()
        {
            ArcGISProFontUsageDockPaneViewModel.Show();
        }
    }
}
