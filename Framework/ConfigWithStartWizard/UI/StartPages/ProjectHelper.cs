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
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Dialogs;

namespace ConfigWithStartWizard.UI.StartPages
{

    #region Project helper class
    /// <summary>
    /// 
    /// </summary>
    internal class ProjectHelper
    {
        internal static async void BrowseToProject(bool bProjects = true)
        {
            try
            {
                var dlg = new OpenItemDialog();
                var myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var initFolder = Path.Combine(System.IO.Path.Combine(myDocs, "ArcGIS"), "Projects");
                if (!bProjects) initFolder = @"C:\Program Files\ArcGIS\Pro\Resources\ProjectTemplates";
                if (!Directory.Exists(initFolder))
                    initFolder = myDocs;
                dlg.Title = "Open Project";
                dlg.InitialLocation = initFolder;
                dlg.Filter = bProjects ? ItemFilters.projects : ItemFilters.project_templates;

                if (!(dlg.ShowDialog() ?? false)) return;
                var item = dlg.Items.FirstOrDefault();
                if (item == null) return;
                if (bProjects) await Project.OpenAsync(item.Path);
                else
                {
                    var ps = new CreateProjectSettings()
                    {
                        Name = System.IO.Path.GetFileNameWithoutExtension(item.Name),
                        LocationPath = ConfigWithStartWizardModule.DefaultFolder(),
                        TemplatePath = item.Path
                    };
                    await Project.CreateAsync(ps);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"Error in open project: {ex}");
            }
        }

        internal static ICommand CmdOpenProject
        {
            get
            {
                return new RelayCommand((args) => BrowseToProject(), () => true); ;
            }
        }

        internal static ICommand CmdNewProject
        {
            get
            {
                return new RelayCommand((args) => BrowseToProject(false), () => true); ;
            }
        }

    }
    #endregion
}
