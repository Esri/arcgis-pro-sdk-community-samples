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

using System.Windows;
using System.Collections.Generic;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Catalog;

namespace FolderConnections
{ 
    internal class DisplayConnections : Button
    {
        protected override void OnClick()
        {
            /// Get an iterable object containing all the current folder connections
            IEnumerable<FolderConnectionProjectItem> projectFolders = Project.Current.GetItems<FolderConnectionProjectItem>();

            /// Iterate over the list of connections and append each to a string
            string folders = null;
            foreach (var item in projectFolders)
                folders += item.Path + "\n";

            /// Show a message box listing each of the folder connections
            MessageBox.Show(folders, "Folder Connections");
        }
    }
}
