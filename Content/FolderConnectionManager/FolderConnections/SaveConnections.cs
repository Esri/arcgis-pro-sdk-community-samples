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

using System.Collections.Generic;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Catalog;
using System.IO;

namespace FolderConnections
{
    internal class SaveConnections : ArcGIS.Desktop.Framework.Contracts.Button
    {
        protected override void OnClick()
        {
            /// Create the list of Folder Connections
            IEnumerable<FolderConnectionProjectItem> projectFolders = Project.Current.GetItems<FolderConnectionProjectItem>();
            string folders = null;
            foreach (var item in projectFolders)
                folders += item.Path + "\n";

            /// Create the log file and write the current Folder-Connection's to it
            SaveItemDialog saveDialog = new SaveItemDialog();
            saveDialog.Title = "Save all current of Folder Connections";
            saveDialog.OverwritePrompt = true;
            saveDialog.DefaultExt = "txt";
            saveDialog.Filter = ItemFilters.textFiles;

            /// If the save dialog was not dismissed, create the file
            if (saveDialog.ShowDialog() == true)
            {
                using (StreamWriter sw = new StreamWriter(saveDialog.FilePath))
                {
                    sw.WriteLine(folders.TrimEnd(new char[] { '\r', '\n' }));
                    sw.Close();
                }
            }
        }
    }
}
