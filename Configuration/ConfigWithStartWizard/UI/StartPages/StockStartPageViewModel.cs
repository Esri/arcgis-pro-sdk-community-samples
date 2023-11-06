/*

   Copyright 2019 Esri

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
using System.Collections.Generic;
using System.Windows.Input;
using ArcGIS.Desktop.Core;
using ConfigWithStartWizard.Models;

namespace ConfigWithStartWizard.UI.StartPages {
    internal class StockStartPageViewModel : StartPageViewModelBase {

        private List<PathItem> _templates = null;
        private List<PathItem> _recentProjects = new List<PathItem>();

        //private SignOnStatusViewModel _ssvm = null;
        private int _selectedProject = -1;
        private int _selectedTemplate = -1;

        public StockStartPageViewModel()
        {
            foreach (var path in Project.GetRecentProjects())
            {
                _recentProjects.Add(new PathItem(path));
            }
            Initialize();
        }

        private void Initialize() {
            _templates = TemplateInfo.GetDefaultTemplates();
        }

        public override string Title => "Select a project:";

        public IReadOnlyList<PathItem> RecentProjects => _recentProjects;

        public IReadOnlyList<PathItem> DefaultTemplates => _templates;

        public int SelectedProjectIndex
        {
            get
            {
                return _selectedProject;
            }
            set
            {
                _selectedProject = value;
                NotifyPropertyChanged();
                if (_selectedProject < 0) return;
                var item = RecentProjects[_selectedProject];
                Project.OpenAsync(item.Path);
            }
        }

        public int SelectedTemplateIndex
        {
            get
            {
                return _selectedTemplate;
            }
            set
            {
                _selectedTemplate = value;
                NotifyPropertyChanged();
                if (_selectedTemplate < 0) return;
                var item = DefaultTemplates[_selectedTemplate];

                var ps = new CreateProjectSettings()
                {
                    Name = System.IO.Path.GetFileNameWithoutExtension(item.Name),
                    LocationPath = ConfigWithStartWizardModule.DefaultFolder(),
                    TemplatePath = item.Path
                };
                Project.CreateAsync(ps);
            }
        }


        #region Commands

        /// <summary>
        /// Command opens the ArcGIS Pro OpenItemDialog API method to browse to a specific project file.
        /// </summary>
        public ICommand CmdOpenProject => ProjectHelper.CmdOpenProject;


        /// <summary>
        /// Command opens the ArcGIS Pro OpenItemDialog API method to browse to a specific project template file.
        /// </summary>
        public ICommand CmdNewProject => ProjectHelper.CmdNewProject;

        #endregion
    }
}
