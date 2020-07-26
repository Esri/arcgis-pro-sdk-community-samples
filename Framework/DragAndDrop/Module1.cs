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
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace DragAndDrop
{
  /// <summary>
  /// This sample shows how to implement drag &amp; drop using a custom dockpane tree control.
  /// </summary>
  /// <remarks> 
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
  /// 1. Make sure that the Sample data is unzipped in c:\data 
  /// 1. The project used for this sample is -C:\Data\SDK\SDK.gdb-. 
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. In ArcGIS Pro create a new project using the Map template.
  /// 1. ArcGIS Pro displays a map view.  
  /// ### Drag and drog from Catalog or the file system
  /// 1. From the Add-in tab click the Drag and Drop button
  /// ![UI](Screenshots/Screen1.png)
  /// 1. Use the Windows file explorer to navigate to a Geodatabase folder, for example -C:\Data\SDK\SDK.gdb-, then drag &amp; drop the folder into the dropzone in the Drag &amp; Drop dockpane.
  /// ![UI](Screenshots/Screen2.png)
  /// 1. Notice that after the drop the Tree Control is being populated with the feature classes contained in the dropped Geodatabase.
  /// 1. Drag one of the feature classes listed in the Tree Control onto the Map pane. 
  /// ![UI](Screenshots/Screen3.png)
  /// 1. The feature class is added to the map.  
  /// ![UI](Screenshots/Screen4.png)
  /// ### Drag and drop from the Map's TOC
  /// 1. From the Add-In tab, click the Drag and Drop TOC Items button. The "Drag and drop TOC items" dockpane will open.
  /// 1. Drag any layer(s) from the active map's TOC to the text box in the dockpane.
  /// 1. The text box in the dockpane will list information about the map members being dragged and dropped.
  /// ![UI](Screenshots/Screen5.png)
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("DragAndDrop_Module"));
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
