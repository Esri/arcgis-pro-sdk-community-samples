//   Copyright 2019 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using Microsoft.Win32;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace WorkwithProjects
{
  internal class WorkWithProjectsViewModel : DockPane
  {
    private const string _dockPaneID = "WorkwithProjects_WorkWithProjects";

    /// <summary>
    /// used to lock collections for use by multiple threads
    /// </summary>
    private readonly object _lockMxdItemCollection = new object();
    private readonly object _lockTemplateFilesCollection = new object();
    private ObservableCollection<Item> _mxdItems = new ObservableCollection<Item>();

    protected WorkWithProjectsViewModel()
    {

      BindingOperations.EnableCollectionSynchronization(MxdItems, _lockMxdItemCollection);
    }

    #region Properties

    private string _Filter = @".mxd";
    /// <summary>
    /// Folder Filter property
    /// </summary>
    public string Filter
    {
      get { return _Filter; }
      set
      {
        SetProperty(ref _Filter, value, () => Filter);
      }
    }

    private string _openProjectPath = @"";
    /// <summary>
    /// Open Project Path property
    /// </summary>
    public string OpenProjectPath
    {
      get { return _openProjectPath; }
      set
      {
        SetProperty(ref _openProjectPath, value, () => OpenProjectPath);
      }
    }

    private string _folderPath = @"";
    /// <summary>
    /// Folder Path property
    /// </summary>
    public string FolderPath
    {
      get { return _folderPath; }
      set
      {
        SetProperty(ref _folderPath, value, () => FolderPath);
      }
    }

    private string _projectPath = @"";
    /// <summary>
    /// Project Path property
    /// </summary>
    public string ProjectPath
    {
      get { return _projectPath; }
      set
      {
        SetProperty(ref _projectPath, value, () => ProjectPath);
      }
    }

    private bool _isCatalog;
    private bool _isMap;
    private bool _isLocalScene;
    private bool _isGlobalScene;
    private bool _isFile;

    public bool IsCatalog
    {
      get { return _isCatalog; }
      set
      {
        SetProperty(ref _isCatalog, value, () => IsCatalog);
        SelectTemplateType("Catalog");
      }
    }
     public bool IsMap
    {
      get { return _isMap; }
      set
      {
        SetProperty(ref _isMap, value, () => IsMap);
        SelectTemplateType("Map");
      }
    }
    public bool IsLocalScene
    {
      get { return _isLocalScene; }
      set
      {
        SetProperty(ref _isLocalScene, value, () => IsLocalScene);
        SelectTemplateType("LocalScene");
      }
    }
    public bool IsGlobalScene
    {
      get { return _isGlobalScene; }
      set
      {
        SetProperty(ref _isGlobalScene, value, () => IsGlobalScene);
        SelectTemplateType("GlobalScene");
      }
    }
    public bool IsFile
    {
      get { return _isFile; }
      set
      {
        SetProperty(ref _isFile, value, () => IsFile);
        SelectTemplateType("File");
      }
    }
    private TemplateType _SelectedTemplateType = TemplateType.Untitled;
    private bool _reentry = false;
    private void SelectTemplateType (string templateType)
    {
      if (_reentry) return;
      _reentry = true;
      FileTemplateVisibility = System.Windows.Visibility.Collapsed;
      switch (templateType)
      {
        case "Catalog":
          IsMap = false; IsLocalScene = false; IsGlobalScene = false; IsFile = false;
          _SelectedTemplateType = TemplateType.Catalog;
          break;
        case "Map":
          IsCatalog = false; IsLocalScene = false; IsGlobalScene = false; IsFile = false;
          _SelectedTemplateType = TemplateType.Map;
          break;
        case "LocalScene":
          IsCatalog = false; IsMap = false; IsGlobalScene = false; IsFile = false;
          _SelectedTemplateType = TemplateType.LocalScene;
          break;
        case "GlobalScene":
          IsCatalog = false; IsMap = false; IsLocalScene = false; IsFile = false;
          _SelectedTemplateType = TemplateType.GlobalScene;
          break;
        case "File":
          IsCatalog = false; IsMap = false; IsLocalScene = false; IsGlobalScene = false;
          _SelectedTemplateType = TemplateType.Untitled;
          RefreshTemplates();
          FileTemplateVisibility = System.Windows.Visibility.Visible;
          break;
      }
      _reentry = false;
    }

    public ICommand CmdCreateProject
    {
      get
      {
        return new RelayCommand(async () => {
          // Create from template
          await CreateProjectFromTemplate(ProjectName, ProjectPath, _SelectedTemplateType, _templateFile);
        });
      }
    }

    private string _projectName = @"";
    /// <summary>
    /// Project Name property
    /// </summary>
    public string ProjectName
    {
      get { return _projectName; }
      set
      {
        SetProperty(ref _projectName, value, () => ProjectName);
      }
    }

    /// <summary>
    /// collection of mxd items.  Bind to this property in the view.
    /// </summary>
    public ObservableCollection<Item> MxdItems
    {
      get { return _mxdItems; }
    }

    /// <summary>
    /// Holds the selected mxd item
    /// </summary>
    private Item _mxdItem = null;
    public Item MxdItem
    {
      get { return _mxdItem; }
      set
      {
        SetProperty(ref _mxdItem, value, () => MxdItem);
        if (_mxdItem == null) return;

        // import the map
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        ImportAMap(_mxdItem);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
      }
    }

    /// <summary>
    /// collection of template paths.  Bind to this property in the view.
    /// </summary>
    private IEnumerable<string> _templateFiles;
    public IEnumerable<string> TemplateFiles
    {
      get { return _templateFiles; }
    }

    private void RefreshTemplates ()
    {
      var templateFolder = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"ArcGIS\ProjectTemplates");
      _templateFiles = Directory.GetFiles(templateFolder, "*.aptx").ToList();
      BindingOperations.EnableCollectionSynchronization(TemplateFiles, _lockTemplateFilesCollection);
      NotifyPropertyChanged(() => TemplateFiles);
    }

    /// <summary>
    /// Holds the selected templateFile item
    /// </summary>
    private string _templateFile = null;
    public string TemplateFile
    {
      get
      {
        return _templateFile;
      }
      set
      {
        SetProperty(ref _templateFile, value, () => TemplateFile);
      }
    }

    private System.Windows.Visibility _fileTemplateVisibility = System.Windows.Visibility.Collapsed;
    /// <summary>
    /// Determines the FileTemplate entry visibility
    /// </summary>
    public System.Windows.Visibility FileTemplateVisibility
    {
      get
      {
        return _fileTemplateVisibility;
      }
      set
      {
        SetProperty(ref _fileTemplateVisibility, value, () => FileTemplateVisibility);
      }
    }

    #endregion

    #region Commands

    /// <summary>
    /// command to open a project
    /// </summary>
    private ICommand _openProjectCommand;
    public ICommand OpenProjectCommand
    {
      get
      {
        return _openProjectCommand ??
               (_openProjectCommand =
                   new RelayCommand(() => OpenProject(OpenProjectPath),
                       () => true));
      }
    }

    /// <summary>
    /// command to open a project
    /// </summary>
    private ICommand _addFolderToProjectCommand;
    public ICommand AddFolderToProjectCommand
    {
      get
      {
        return _addFolderToProjectCommand ??
               (_addFolderToProjectCommand =
                   new RelayCommand(() => AddFolderToProject(FolderPath),
                       () => true));
      }
    }
    #endregion

    #region Exercise

    private async void OpenProject(string openProjectPath)
    {
      try
      {
        await Project.OpenAsync(openProjectPath);
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error opening project: " + ex.ToString());
      }
    }


    private async void AddFolderToProject(string addFolderPath)
    {
      if (string.IsNullOrEmpty(addFolderPath)) return;
      try
      {
        // Add a folder to the Project
        var folderToAdd = ItemFactory.Instance.Create(addFolderPath);
        await QueuedTask.Run(() => Project.Current.AddItem(folderToAdd as IProjectItem));
        // find the folder project item
        FolderConnectionProjectItem folder = Project.Current.GetItems<FolderConnectionProjectItem>().FirstOrDefault(f => f.Path.Equals(addFolderPath, StringComparison.CurrentCultureIgnoreCase));
        if (folder == null)
          return;

        // do the search
        IEnumerable<Item> folderFiles = null;
        await QueuedTask.Run(() => folderFiles = folder.GetItems().Where(f => f.Path.EndsWith(Filter, StringComparison.CurrentCultureIgnoreCase)));
        
        // search MXDs
        lock (_lockMxdItemCollection)
        {
          _mxdItems.Clear();
          foreach (var newItem in folderFiles)
          {
            _mxdItems.Add(newItem);
          }
        }
        NotifyPropertyChanged(() => MxdItems);
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error adding folder to project: " + ex.ToString());
      }
    }

    private async Task ImportAMap(Item mxdToAdd)
    {
      try
      {
        // Add a folder to the Project
        await QueuedTask.Run(() => MapFactory.Instance.CreateMapFromItem(mxdToAdd));
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error importing mxd: " + ex.ToString());
      }
    }

    /// <summary>
    /// Creates a new project using the supplied name from a project template.
    /// </summary>
    /// <remarks>Exercise 3: This exercise creates a new project from a project template (*.aptx) using Pro SDK. This exercise introduces the 
    ///  CreateProjectSettings class used in project creation.</remarks>
    /// <param name="projectName">Name to be used for the new project.</param>
    /// <param name="locationPath">Location for the new project</param>
    /// <param name="templateType">Type of template to use for newly created project</param>
    /// <param name="templatePath">Template to use</param>
    /// <returns>A Task representing CreateProjectFromTemplate.</returns>
    private async Task CreateProjectFromTemplate(string projectName, string locationPath, TemplateType templateType, string templatePath)
    {
      try
      {
        // Set project creation settings
        var createProjectSettings = new CreateProjectSettings
        {
          Name = projectName,
          LocationPath = locationPath         
          
        };
        if (templateType == TemplateType.Untitled)
        {
          createProjectSettings.TemplatePath = templatePath;
        }
        else
        {
          createProjectSettings.TemplateType = templateType;
        }
        // Create project
        var newProject = await Project.CreateAsync(createProjectSettings);

        // ... and continue to do stuff
        //await ChangeSettings();

        // Save Project
        //await Project.Current.SaveAsync();

        // ... and/or continue to do stuff but allow user to continue to work with Pro

      }
      catch (Exception ex)
      {
        MessageBox.Show("Error importing mxd: " + ex.ToString());
      }
    }

    public static readonly string RegisrtyKeyUseInCaseOfEmergencyOnly = "ArcGISPro";

    private static string GetInstallDirFromReg()
    {
      var regKeyName = RegisrtyKeyUseInCaseOfEmergencyOnly;
      var regPath = string.Format(@"SOFTWARE\ESRI\{0}", regKeyName);

      var err1 = string.Format("Install location of ArcGIS Pro cannot be found. Please check your registry for {0}.", string.Format(@"HKLM\{0}\{1}", regPath, "InstallDir"));

      var path = "";
      try
      {
        var localKey = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
        var esriKey = localKey.OpenSubKey(regPath);

        if (esriKey == null)
        {
          localKey = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.CurrentUser, RegistryView.Registry64);
          esriKey = localKey.OpenSubKey(regPath);
        }
        if (esriKey == null)
        {
          //this is an error
          throw new System.InvalidOperationException(err1);
        }
        path = esriKey.GetValue("InstallDir") as string;
        if (path == null || path == string.Empty)
          //this is an error
          throw new InvalidOperationException(err1);
      }
      catch (InvalidOperationException ie)
      {
        //this is ours
        throw ie;
      }
      catch (Exception ex)
      {
        throw new System.Exception(err1, ex);
      }
      return path;
    }


    #endregion

    #region Manage Dockpane

    /// <summary>
    /// Show the DockPane.
    /// </summary>
    internal static void Show()
    {
      DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
      if (pane == null)
        return;

      pane.Activate();
    }

    /// <summary>
    /// Text shown near the top of the DockPane.
    /// </summary>
    private string _heading = "Exercising Project functions";
    public string Heading
    {
      get { return _heading; }
      set
      {
        SetProperty(ref _heading, value, () => Heading);
      }
    }

    #endregion
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
  internal class WorkWithProjects_ShowButton : Button
  {
    protected override void OnClick()
    {
      WorkWithProjectsViewModel.Show();
    }
  }
}
