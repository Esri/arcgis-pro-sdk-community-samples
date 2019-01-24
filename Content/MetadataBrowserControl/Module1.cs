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
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;
using ArcGIS.Desktop.Core.Events;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Events;

namespace MetadataBrowserControl
{
    /// <summary>
    /// This sample demonstrates how to view the Metadata of a Project item using the Pro API in a browser control with a transform applied to it.
    /// Note: Metadata of certain items such as system styles cannot be viewed.
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution. 
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open. 
    /// 1. Open any project file that contains project items such as Maps, toolboxs, databases with metadata. 
    /// 1. In the Catalog pane, select any item that has metadata.
    /// 1. Select the Add-in tab and click the Metadata Transform viewer button.  The Metadata Viewer dockpane will be displayed.
    /// 1. The dockpane will display the metadata of the project item selected in a browser control.
    /// 1. Note that a default transform located in the ArcGIS Pro installation folder is applied to display the metadata.  `C:\Program Files\ArcGIS\Pro\Resources\Metadata\Stylesheets\XML.xsl`
    /// ![UI](Screenshots/MetadataViewer.png)
    /// </remarks>
    internal class Module1 : Module
    {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current
        {
            get
            {
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("MetadataBrowserControl_Module"));
            }
        }

        #region Overrides

        /// <summary>
        /// Initialize logic for the custom module
        /// </summary>
        /// <returns></returns>
        protected override bool Initialize()
        {
            //Subscribe to app startup event to initialize dockpane
            ApplicationStartupEvent.Subscribe(OnAppStartupReady);
            //Subscribe to the Project Item selection changed event
            ProjectWindowSelectedItemsChangedEvent.Subscribe(OnSelectedItemChanged);
            
            return base.Initialize();
        }
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }
        #endregion Overrides
        #region subscribe to events
        private void OnAppStartupReady(EventArgs obj)
        {
            //The viewmodel
            MetadataBrowserViewModel vm = FrameworkApplication.DockPaneManager.Find("MetadataBrowserControl_Dockpane1") as MetadataBrowserViewModel;
            //get the selected item
            var item = Project.Current?.SelectedItems?.FirstOrDefault();
            if (item != null && item.GetXml() != null)  //Item selected and it has metadata
                vm.DockpaneVisible = System.Windows.Visibility.Visible; //Visibility is set
            else //No item selected
                vm.DockpaneVisible = System.Windows.Visibility.Collapsed; //Visibility hidden
        }

        /// <summary>
        /// Event handler for the Project Opened event
        /// </summary>
        /// <param name="arg"></param>
        private void OnProjectOpened(ProjectEventArgs arg)
        {
            //The viewmodel
            MetadataBrowserViewModel vm = FrameworkApplication.DockPaneManager.Find("MetadataBrowserControl_Dockpane1") as MetadataBrowserViewModel;
            //get the selected item
            var item = Project.Current.SelectedItems?.FirstOrDefault();
            if (item != null && item.GetXml() != null)  //Item selected and it has metadata
                vm.DockpaneVisible = System.Windows.Visibility.Visible; //Visibility is set
            else //No item selected
                vm.DockpaneVisible = System.Windows.Visibility.Collapsed; //Visibility hidden
        }

        /// <summary>
        /// Subscribes to the Selected item changed event. ItemInfo of the selected item is instantiated.
        /// </summary>
        /// <param name="obj"></param>
        private async void OnSelectedItemChanged(ProjectWindowSelectedItemsChangedEventArgs obj)
        {
            //The viewmodel
            MetadataBrowserViewModel vm = FrameworkApplication.DockPaneManager.Find("MetadataBrowserControl_Dockpane1") as MetadataBrowserViewModel;
            if (vm != null)
            {
               // ItemMetadata info = new ItemMetadata(null, "");
                var item = Project.Current.SelectedItems?.FirstOrDefault();
                if (item != null)
                {
                    var xml = await ItemMetadata.GetXML(item);

                    if (!string.IsNullOrEmpty(xml)) //Item has xml
                    {
                        vm.ItemInformation = new ItemMetadata(item, xml);
                        vm.ApplyTransform();
                        vm.DockpaneVisible = System.Windows.Visibility.Visible;                       
                    }
                    else
                        vm.DockpaneVisible = System.Windows.Visibility.Collapsed;
                }
                else
                    vm.DockpaneVisible = System.Windows.Visibility.Collapsed;

            }
        }
#endregion

    }
}
