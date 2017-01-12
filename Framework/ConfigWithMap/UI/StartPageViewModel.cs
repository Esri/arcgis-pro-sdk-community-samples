/*

   Copyright 2017 Esri

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
using System.Collections.ObjectModel;
using System.IO;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;

namespace ConfigWithMap.UI
{
    /// <summary>
    /// Business logic for the StartPage
    /// </summary>
    class StartPageViewModel : PropertyChangedBase
    {
        #region Private 

        private readonly ObservableCollection<RecentProject> _recentProjects = new ObservableCollection<RecentProject>();

        private void LoadRecentProjects()
        {
            // Here we can present any recent projects we want
            // Maybe there are some canned ones in the Solution folder
            // Or there are some in a solution folder that is per user where they have read/write access

            //Assembly assembly = Assembly.GetExecutingAssembly();
            //string location = assembly.Location;
            //DirectoryInfo dirInfo = Directory.GetParent(location);
            //string path = Path.Combine(dirInfo.FullName, "Projects");
            const string path = @"C:\Configurations\Projects";
            if (!Directory.Exists(path)) return;
            var projects = Directory.GetFiles(path, "*.aprx");
            foreach (var project in projects)
            {
                var name = Path.GetFileNameWithoutExtension(project);
                var rp = new RecentProject {Name = name, Path = project};
                _recentProjects.Add(rp);
            }
        }

        #endregion

        #region CTor

        public StartPageViewModel()
        {
            LoadRecentProjects();
        }

        #endregion

        #region Properties

        public string UserName
        {
            get { return ConfigWithMapModule.UserName; }
        }

        public ReadOnlyObservableCollection<RecentProject> RecentProjects
        {
            get { return new ReadOnlyObservableCollection<RecentProject>(_recentProjects); }
        }

        public RecentProject SelectedProject
        {
            get
            { return null; }
            set
            {
                RecentProject project = value;
                project.Open();
            }
        }

        internal RecentProject CreateProject(string name)
        {
            name = name.Trim();
            name = name.TrimStart('_');
            name = name.Replace("_", " ");
            //Assembly assembly = Assembly.GetExecutingAssembly();
            //string location = assembly.Location;
            //DirectoryInfo dirInfo = Directory.GetParent(location);
            string path = @"c:\data\configurations\projects";

            string subPath = Path.Combine(path, name);
            subPath = Path.ChangeExtension(subPath, ".aprx");

            if (!File.Exists(subPath))
            {
                var hasDefault = File.Exists(Path.Combine(path, "San Diego.aprx"));
                var msg = hasDefault
                    ? "Using the default project 'San Diego.aprx' instead."
                    : "The default project 'San Diego.aprx' doesn't exist either. Please add this project before running this demo.";
                MessageBox.Show($@"This project for {name} @ {subPath} cannot be found. {msg}");
                if (!hasDefault) return null;
            }

            if (!File.Exists(subPath))
                subPath = Path.Combine(path, "San Diego.aprx");
            RecentProject rp = new RecentProject { Name = name, Path = subPath };
            return rp;
        }
        #endregion
        
    }


    #region Recent Projects

    internal class RecentProject
    {
        public string Name
        {
            get;
            internal set;
        }

        public string FullName => System.IO.Path.Combine(Name, ".aprx");

        public string Path
        {
            get;
            internal set;
        }

        public void Open()
        {
            try
            {
                Project.OpenAsync(Path);
            }
            catch
            {
                MessageBox.Show($@"Unable to load project {Path}");
            }
        }
    }

    #endregion Recent Projects
}
