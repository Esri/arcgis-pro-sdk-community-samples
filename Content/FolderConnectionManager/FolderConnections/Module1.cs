//   Copyright 2015 Esri
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
    /// Allows saving and loading folder connections to a Project.
    /// </summary>
    /// <remarks> 
    /// 1. Open ArcGIS Pro and either a new or existing Project.
    /// 2. Create a new Folder Connection in the Project window.
    /// ![UI](Screenshots/FolderConnect.png)  
    /// 3. Click the "Save Connections" button in the "Folder Connection Manager" Add-In pane. The current Folder /// Connection's will be saved as a list in the text file.
    /// 4. Remove the Folder Connection you just created by right-clicking on it in the Project window and selecting "Remove".
    /// ![UI](Screenshots/RemoveFolder.png)
    /// 5. Load your saved Folder Connection by clicking the "Load Connection" button in the "Folder Connection Manager" /// Add-In pane. Select the text file you saved in step three.
    /// 6. Verify that your Folder Connection has returned to the Project.
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
