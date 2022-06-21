/*
   Copyright 2019 Esri
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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Xml;
using System.Windows;

namespace MetadataViewer
{
    /// <summary>
    /// Metadata viewer properties implementation
    /// </summary>
    internal class MetadataViewerViewModel : DockPane
    {
        private const string _dockPaneID = "MetadataViewer_MetadataViewer";

        protected MetadataViewerViewModel() { }

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
        #region public properties


        private ItemInfo _itemInfo;
        /// <summary>
        /// Selected item in the catalog
        /// </summary>
        public ItemInfo ItemInformation
        {
            get { return _itemInfo; }
            set
            {
                SetProperty(ref _itemInfo, value, () => ItemInformation);
            }
        }

        /// <summary>
        /// Validation text for the metadata
        /// </summary>
        private string _validationText;
        public string ValidationText
        {
            get { return _validationText; }
            set
            {
                SetProperty(ref _validationText, value, () => ValidationText);
            }
        }

        /// <summary>
        /// Visibility of dockpane
        /// </summary>
        private Visibility _dockpaneVisible;
        public Visibility DockpaneVisible
        {
            get { return _dockpaneVisible; }
            set
            {
                SetProperty(ref _dockpaneVisible, value, () => DockpaneVisible);
            }
        }
#endregion
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class MetadataViewer_ShowButton : Button
    {
        protected override void OnClick()
        {
            MetadataViewerViewModel.Show();
        }
    }
}
