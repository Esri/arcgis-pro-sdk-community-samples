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

namespace DAML
{
    /// <summary>
    /// This sample demonstrates how to modify the Pro UI using DAML
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.  
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open.
    /// 1. Open any project with at least one feature layer.
    /// 1. Notice the following customizations made to the Pro UI:
    /// * The Bookmarks button has been removed in the Navigate group on the Map tab
    /// &lt;img src="https://ArcGIS.github.io/arcgis-pro-sdk/images/ProSnippetsDAML/DeleteCoreButton.png" width="40%"&gt;
    /// * A new button has been inserted into the Navigate group on the Map Tab
    /// &lt;img src="https://ArcGIS.github.io/arcgis-pro-sdk/images/ProSnippetsDAML/NewButtonCoreGroup.png" width="40%"&gt;
    /// * With any Map view active, right click on a feature layer in the TOC. Notice the New Button context menu item added.
    /// &lt;img src="https://ArcGIS.github.io/arcgis-pro-sdk/images/ProSnippetsDAML/NewMenuItemInContextMenu.png" width="40%"&gt;
    /// * Click the Project tab to access Pro's backstage.  Notice the missing Open and New project tabs.  A new tab called "Geocode" has been inserted above the Save project button.
    /// &lt;img src="https://ArcGIS.github.io/arcgis-pro-sdk/images/ProSnippetsDAML/BackstageDeleteExistingTabsInsertNewTabs.png" width="40%"&gt;
    /// * Click the Project tab to access Pro's backstage. Click the Options tab to display the Options Property Sheet.  Notice the new "Sample Project Settings" property page inserted within the Project group.
    /// &lt;img src="https://ArcGIS.github.io/arcgis-pro-sdk/images/ProSnippetsDAML/PropertySheetOptionsDialog.png" width="40%"&gt;
    /// * With any Map active, right click on the Map in the Contents pane to access the context menu. Notice the new button inserted into the context menu.
    /// &lt;img src="https://ArcGIS.github.io/arcgis-pro-sdk/images/ProSnippetsDAML/2DMapContextMenu.png" width="40%"&gt;
    /// * With any Scene active, right click on the Scene in the Contents pane to access the context menu. Notice the new button inserted into the context menu.
    /// &lt;img src="https://ArcGIS.github.io/arcgis-pro-sdk/images/ProSnippetsDAML/SceneContextMenu.png" width="40%"&gt;
    /// * In the Catalog pane, right click on the Map Container to access its context menu. Notice a new Menu inserted into the context menu.
    /// &lt;img src="https://ArcGIS.github.io/arcgis-pro-sdk/images/ProSnippetsDAML/MapContainerContextMenu.png" width="40%"&gt;
    /// * In the Catalog pane, right click on any Map item to access its context menu. Notice a new button inserted into the context menu.
    /// &lt;img src="https://ArcGIS.github.io/arcgis-pro-sdk/images/ProSnippetsDAML/MapContentContextMenu.png" width="40%"&gt;
    /// * In the Catalog pane, right click on any Local Scene to access its context menu. Notice a new button inserted into the context menu.
    /// &lt;img src="https://ArcGIS.github.io/arcgis-pro-sdk/images/ProSnippetsDAML/SceneContentContextMenu.png" width="40%"&gt;
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("ModifyProUIWithDAML_Module"));
            }
        }

        #region Overrides
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

    }
}
