//   Copyright 2019 Esri
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
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace FolderConnections
{
    /// <summary>
    /// Allows saving and loading folder connections to a Project showing how to manage folder connections in ArcGIS Pro from within an Add-in.
    /// </summary>
    /// <remarks> 
    /// 1. Open this solution in Visual Studio.
    /// 1. Click the build menu and select Build Solution.
    /// 1. Click the Start button to open ArCGIS Pro.
	/// 1. Open any project either a new or existing Project.
	/// 1. Click on the Add-in tab and see that the "Folder Connection Manager" group appears on the "add-in" tab.
    /// 1. Create a new Folder Connection in the Project window as shown below.  
	/// ![UI](Screenshots/Screen1.png)  
	/// 1. On the Add-in tab click the "Display Connections" button in the "Folder Connection Manager" Add-In group to see the list of all current folder connection path strings.
	/// ![UI](Screenshots/FolderConnect.png)  
    /// 1. On the Add-in tab click the "Save Connections" button in the "Folder Connection Manager" Add-In group.The browser pop up appears in which create a txt file on the home folder under projects to save the folderpath.
    /// ![UI](Screenshots/RemoveFolder1.png)
    /// 1. Remove the Folder Connections you just added by right-clicking on folder name in the Project window and selecting "Remove".
    /// ![UI](Screenshots/RemoveFolder.png)
    /// 1. Load your saved Folder Connection by clicking the "Load Connection" button in the "Folder Connection Manager" Add-In group. Select the text file you saved in the previous step.
    /// 1. Verify that your Folder Connections have been programmatically added back under 'Folders'.
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("FolderConnections_Module"));
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
