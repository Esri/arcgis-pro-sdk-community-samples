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
using System.Collections.Generic;
using System.IO;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Dialogs;
using ConfigWithStartWizard.Models;

namespace ConfigWithStartWizard.UI.StartPages {
    internal class CaliforniaStartPageViewModel : StartPageViewModelBase
    {
        public override string Title => "Select a county:";
        public override string Name => "California";

        private List<PathItem> _recentProjects = new List<PathItem>();

        public IReadOnlyList<PathItem> RecentProjects => _recentProjects;

        public CaliforniaStartPageViewModel () : base ()
        {

        }

        #region RecentProject Handling

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
