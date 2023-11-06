/*

   Copyright 2023 Esri

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
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ProStartPageConfig.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProStartPageConfig.Helpers
{

  internal class ProStartPageHelper
  {

    private static string _defaultLocation = "";
    internal static string InitialLocation
    {
      get
      {
        if (string.IsNullOrEmpty(_defaultLocation))
        {
          if (!string.IsNullOrEmpty(ApplicationOptions.GeneralOptions.CustomHomeFolder)
            && ApplicationOptions.GeneralOptions.HomeFolderOption == OptionSetting.UseCustom)
          {
            _defaultLocation = ApplicationOptions.GeneralOptions.CustomHomeFolder;
          }
          else
          {
            var myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _defaultLocation = Path.Combine(System.IO.Path.Combine(myDocs, "ArcGIS"), "Projects");
            if (!Directory.Exists(_defaultLocation))
              _defaultLocation = myDocs;
          }
        }
        return _defaultLocation;
      }
    }

    internal static string NextProjectName => GetNextProjectName();

    internal static string GetNextProjectName(string seedName = "", string initialLocation = "")
    {
      //defaults
      if (string.IsNullOrEmpty(initialLocation))
        initialLocation = InitialLocation;
      if (string.IsNullOrEmpty(seedName))
        seedName = "MyProject";

      var i = 1;
      var finalName = $"{seedName}{i}";
      var project_path = System.IO.Path.Combine(initialLocation, finalName);
      while (Directory.Exists(project_path))
      {
        finalName = $"{seedName}{++i}";
        project_path = System.IO.Path.Combine(initialLocation, finalName);
      }
      return finalName;
    }

    internal static string UntitledPath
    {
      get
      {
        string tempFolder = System.IO.Path.GetTempPath();
        return System.IO.Path.Combine(tempFolder, Guid.NewGuid().ToString());
      }
    }

    internal static void BrowseToProject()
    {
      var dlg = new OpenItemDialog();


      dlg.Title = "Open Project";
      dlg.InitialLocation = InitialLocation;
      dlg.Filter = ArcGIS.Desktop.Catalog.ItemFilters.Projects;

      if (dlg.ShowDialog() ?? false)
      {
        var item = dlg.Items.FirstOrDefault();
        if (item != null)
        {
          Project.OpenAsync(item.Path);
        }
      }
    }

    internal static void BrowseToTemplate()
    {
      var dlg = new OpenItemDialog();

      var myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
      var initFolder = Path.Combine(System.IO.Path.Combine(myDocs, "ArcGIS"), "ProjectTemplates");
      if (!Directory.Exists(initFolder))
        initFolder = myDocs;
      dlg.Title = "Create New Project From Template";
      dlg.InitialLocation = initFolder;
      dlg.Filter = ArcGIS.Desktop.Catalog.ItemFilters.Project_Templates;

      if (dlg.ShowDialog() ?? false)
      {
        var item = dlg.Items.FirstOrDefault();
        if (item != null)
        {
          //template type is irrelevant
          CreateWithTemplate(item.Path);
        }
      }
    }

    internal static void CreateWithTemplateType(
      TemplateType templateType = TemplateType.Catalog, string templatePath = "")
    {
      CreateProjectSettings create_proj_params = null;
      var createPrjDlg = new CreateANewProjectWindow();
      createPrjDlg.Owner = FrameworkApplication.Current.MainWindow;

      if (templateType == TemplateType.Untitled)
      {
        //just launch straight into it
        create_proj_params = new CreateProjectSettings();
        create_proj_params.Name = "Untitled";
        create_proj_params.LocationPath = ProStartPageHelper.UntitledPath;
        create_proj_params.TemplateType = TemplateType.Untitled;
        create_proj_params.CreateNewProjectFolder = true;
      }
      else if (createPrjDlg.ShowDialog() ?? false)
      {
        create_proj_params = new CreateProjectSettings();
        create_proj_params.Name = createPrjDlg.ProjectName;
        create_proj_params.LocationPath = createPrjDlg.ProjectLocation;
        if (!string.IsNullOrEmpty(templatePath))
          create_proj_params.TemplatePath = templatePath;
        else
          create_proj_params.TemplateType = templateType;
        create_proj_params.CreateNewProjectFolder =
                          createPrjDlg.CreateFolderForProject;
      }
      //create the new project
      if (create_proj_params != null)
        Project.CreateAsync(create_proj_params);
    }

    internal static void CreateWithTemplate(string templatePath)
    {
      if (string.IsNullOrEmpty(templatePath))
        return;
        
      CreateProjectSettings create_proj_params = null;
      var createPrjDlg = new CreateANewProjectWindow();
      createPrjDlg.Owner = FrameworkApplication.Current.MainWindow;

      if (createPrjDlg.ShowDialog() ?? false)
      {
        create_proj_params = new CreateProjectSettings();
        create_proj_params.Name = createPrjDlg.ProjectName;
        create_proj_params.LocationPath = createPrjDlg.ProjectLocation;

        create_proj_params.TemplatePath = templatePath;
        create_proj_params.CreateNewProjectFolder =
                          createPrjDlg.CreateFolderForProject;
      }
      //create the new project
      if (create_proj_params != null)
        Project.CreateAsync(create_proj_params);
    }

    internal static ICommand OpenProjectCommand
    {
      get
      {

        return new RelayCommand((args) => BrowseToProject(), () => true); ;
      }
    }

    internal static ICommand OpenOtherTemplateCommand
    {
      get
      {

        return new RelayCommand((args) => BrowseToTemplate(), () => true); ;
      }
    }
  }
}
