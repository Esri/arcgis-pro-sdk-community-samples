/*

   Copyright 2018 Esri

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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;

namespace ConfigWithStartWizard.UI.StartPages {
    /// <summary>
    /// Business logic for the StartPage
    /// </summary>
    internal class OutofBoxStartPageViewModel : StartPageViewModelBase {
        #region Private Properties

        private FileInfo _selectedProjectFile;
        
        #endregion

        #region Properties

        public override string Title => "Select a project:";
        /// <summary>
        /// Collection of all ArcGIS Pro project files.
        /// </summary>
        public ICollection<FileInfo> ProProjects
        {
            get
            {
                var fileInfos = Project.GetRecentProjects().Select(f => new FileInfo(f));
                return new Collection<FileInfo>(fileInfos.ToList());
            }
        }

        /// <summary>
        /// ArcGIS Pro project file is selected and will be opened.
        /// </summary>
        public FileInfo SelectedProjectFile
        {
            get { return _selectedProjectFile; }
            set
            {
                _selectedProjectFile = value;
                Project.OpenAsync(SelectedProjectFile.FullName);
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command opens the ArcGIS Pro OpenItemDialog API method to browse to a specific project file.
        /// </summary>
        public ICommand OpenProjectCommand => ProjectHelper.CmdOpenProject;

        #endregion
    }

}
