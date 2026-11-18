/*

   Copyright 2026 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       https://www.apache.org/licenses/LICENSE-2.0

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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;

namespace SolarSuitability.UI
{
    /// <summary>
    /// Business logic for the StartPage
    /// </summary>
    internal class StartPageViewModel : PropertyChangedBase
    {


        #region Properties
        /// <summary>
        /// Collection of all ArcGIS Pro project files.
        /// </summary>
        public ICollection<FileInfo> ProProjects
        {
            get
            {
                var fileInfos = Project.GetRecentProjects().Select(f => new FileInfo(f));
                var projectCollection = new Collection<FileInfo>(fileInfos.ToList());
                if (projectCollection.Count == 1) _selectedProjectFile = projectCollection[0];
                return projectCollection;
            }
        }
        private ObservableCollection<string> _serviceAreas = new ObservableCollection<string>()
        {
            "Redlands Packing House District",
            "Mountain Grove",
            "Citrus Plaza",
            "Redlands Historic Downtown",
            "University Grove"
        };
        public ObservableCollection<string> ServiceAreas
        {
            get
            {
                return _serviceAreas;
            }
            set => SetProperty(ref _serviceAreas, value);
        }

        private string _selectedServiceArea = "Redlands Packing House District";
        public string SelectedServiceArea
        {
            get => _selectedServiceArea;
            set
            {
                SetProperty(ref _selectedServiceArea, value);
            }
        }
        private FileInfo _selectedProjectFile;
        /// <summary>
        /// ArcGIS Pro project file is selected and will be opened.
        /// </summary>
        public FileInfo SelectedProjectFile
        {
            get => _selectedProjectFile;
            set => SetProperty(ref _selectedProjectFile, value);
        }
        #endregion

        #region Commands
        private ICommand _about;
        /// <summary>
        /// Command opens the ArcGIS Pro OpenItemDialog API method to browse to a specific project file.
        /// </summary>
        public ICommand OpenProjectCommand
        {
            get
            {
                return StartPageViewModelHelper.OpenProjectCommand;
            }
        }
        /// <summary>
        /// Command opens the selected ArcGIS Pro project.
        /// </summary>
        public ICommand OpenSelectedProjectCommand
        {
            get
            {
                return new RelayCommand((args) => OpenSelectedProject(), () => SelectedProjectFile != null);
            }
        }
        /// <summary>
        /// Open selected project.
        /// </summary>
        /// <remarks>
        /// If project is not on disk, you will be prompted to remove the project from the list of recent projects.
        /// </remarks>
        private void OpenSelectedProject()
        {
            if (!File.Exists(SelectedProjectFile.FullName))
            {
                var remove = MessageBox.Show($"Cannot open {SelectedProjectFile.Name}. Project does not exist. Do you want to remove the project shortcut from your list of recent projects?", "Project not found", System.Windows.MessageBoxButton.YesNo);
                if (remove == System.Windows.MessageBoxResult.Yes)
                {
                    Project.RemoveRecentProject(SelectedProjectFile.FullName);
                    NotifyPropertyChanged(nameof(ProProjects));
                }
                return;
            }
            Project.OpenAsync(SelectedProjectFile.FullName);
        }
        /// <summary>
        /// Command opens the ArcGIS Pro backstage
        /// </summary>
        public ICommand AboutArcGISProCommand
        {
            get
            {
                if (_about == null)
                    _about = new RelayCommand(() => FrameworkApplication.OpenBackstage("esri_core_aboutTab"));
                return _about;
            }
        }
        #endregion
    }
    #region StartPageViewModel helper class
    /// <summary>
    /// 
    /// </summary>
    internal class StartPageViewModelHelper
    {
        internal static void BrowseToProject()
        {

      //Project.OpenAsync($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\ArcGIS\\Projects\\SolarEstimation\\SolarEstimation.aprx");
      Project.OpenAsync($"C:\\Data\\SolarEstimation\\SolarEstimation.aprx");
    }
        internal static ICommand OpenProjectCommand
        {
            get
            {

                return new RelayCommand((args) => BrowseToProject(), () => true); ;
            }
        }
        #endregion
    }
}
