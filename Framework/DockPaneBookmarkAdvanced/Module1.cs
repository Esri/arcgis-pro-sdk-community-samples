//Copyright 2015 Esri

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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace DockPaneBookmarkAdvanced
{
    /// <summary>
    /// This sample shows how to:  
    /// * Handle project item collection changes
    /// * Add and delete a bookmark
    /// * Leverage existing ArcGIS Pro commands
    /// * Add styling to buttons to follow ArcGIS Pro styling guidelines
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open. 
    /// 1. Open a project with a map that has bookmarks and click on the 'Add-in' tab.
    /// 1. Click the 'Show bookmarks' button to show the bookmark dockpane
    /// 1. On the 'bookmark dockpane' click the 'Get Maps' button to fill the 'Available Maps' dropdown.
    /// 1. Select a map on the 'Available Maps' dropdown.
    /// 1. Click on any of the 'Bookmark' thumbnails to zoom to a given bookmark.
    /// 1. Clock the 'New Bookmark' button.
    /// ![UI](Screenshots/Screen.png)
    /// ###UI Controls
    /// **Burger button:**
    /// 1. Click on the burger button conrol on the top right corner of the dockpane to display the menu options.
    /// 1. Select the "Outline" menu option.  The bookmarks are displayed in a list view mode.
    /// ![UI](Screenshots/burger-button.png)
    /// **Search text box:**
    /// 1. Notice the Search box control located above the list of bookmarks in the dockpane.
    /// 1. Type the name of one of your bookmarks. Click the arrow next to it and notice the bookmark gets selected in the gallery of bookmarks and the map view zooms to that bookmark.
    /// ![UI](Screenshots/search-Text.png)   
    ///  **Circular Animation:**
    ///  1. In the Project pane, right click the "Maps" folder and select "'New Map" from the context menu.
    ///  2. Notice the circular animation control dispayed on the bookmark dockpane while the new map is being opened.
    ///  ![UI](Screenshots/circular-animation.png)
    ///  ** Message label: **
    ///  1. In the drop down control that lists the colelction of maps in the project, select the new map you created in the step above.
    ///  2. Since this new map has no bookmarks associated with it yet, notice how the bookmark list view is now replaced with a Message label control.
    ///  ![UI](Screenshots/message-label.png)
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("DockPaneBookmarkAdvanced_Module"));
            }
        }
    }
}
