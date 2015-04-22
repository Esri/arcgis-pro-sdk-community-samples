//   Copyright 2014 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System.ComponentModel;
using System.Windows.Data;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace Localization
{
    internal class DockpaneLocalizedViewModel : DockPane
    {

        private const string DockPaneId = "Localization_DockpaneLocalized";
        private ICollectionView _sampleRecords;
        
        /// <summary>
        /// Dockpane constructor is used to initialize some sample data
        /// </summary>
        internal DockpaneLocalizedViewModel()
        {
            SampleRecords = CollectionViewSource.GetDefaultView(SampleRecord.SampleRecords);
        }

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

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            var pane = FrameworkApplication.DockPaneManager.Find(DockPaneId);
            if (pane == null)
                return;

            pane.Activate();
        }

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        public string Heading
        {
            get { return Properties.Resources.DockPaneTitle; }
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class DockpaneLocalized_ShowButton : Button
    {
        protected override void OnClick()
        {
            DockpaneLocalizedViewModel.Show();
        }
    }
}
