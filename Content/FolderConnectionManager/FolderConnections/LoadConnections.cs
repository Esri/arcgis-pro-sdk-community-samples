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
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Catalog;

namespace FolderConnections
{
    internal class LoadConnections : ArcGIS.Desktop.Framework.Contracts.Button
    {
        protected async override void OnClick()
        {
            try
            {
                /// Read a file for line-delinated directory paths, and attempt to add them to the Project's Folder Connections
                OpenItemDialog openDialog = new OpenItemDialog();
                openDialog.Title = "Select a Folder Connections file";
                openDialog.MultiSelect = false;
                openDialog.Filter = ItemFilters.textFiles;

                /// If the user clicks OK in the dialog, load the listed directories from the file to the Project's Folder Connections
                if (openDialog.ShowDialog() == true)
                {
                    IEnumerable<Item> selectedItem = openDialog.Items;
                    foreach (Item i in selectedItem)
                    {
                        System.IO.StreamReader file = new System.IO.StreamReader(i.Path);
                        string line;
                        while ((line = file.ReadLine()) != null)
                        {
                            /// Add all folder connections to the current Project's folder connections
                            string notFound = "";
                            if (Directory.Exists(line))
                            {
                                var folderToAdd = ItemFactory.Create(line);
                                await Project.Current.AddAsync(folderToAdd);
                            }
                            else
                            {
                                notFound += "\r\n" + line;
                            }

                            /// Report any folder connections that could not be found
                            if (notFound != "")
                            {
                                MessageBox.Show("The following directories were not found and could not be added to the Folder Connections: " + notFound);
                            }

                        }
                    }
                }
            }

            catch (Exception ex)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Error adding a folder to the project: " + ex.ToString());
            }
        }
    }
}
