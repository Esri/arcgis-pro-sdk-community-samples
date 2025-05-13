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

namespace GetRecentPortalProjects.UI
{
  /// <summary>
  /// Business logic for the StartPage
  /// </summary>
  internal class StartPageViewModel : PropertyChangedBase
  {

    #region Properties
    /// <summary>
    /// Collection of all recently opened ArcGIS Pro project files. This includes all local and portal projects.
    /// </summary>
    public ICollection<RecentProject> ProProjects
    {
      get
      {
        var result = Project.GetRecentProjectsEx();
        List<RecentProject> recentProjects= new List<RecentProject>();
        foreach (var project in result)
        {
          if (string.IsNullOrEmpty(project.Item1)) continue;
          string projectPath;
          string projectName;
          if (!string.IsNullOrEmpty(project.Item2)) 
          {
            //this is a portal project
            //Url
            projectPath = project.Item2;
          }
          else
          {
            //this is a local project
            //path to local project
            projectPath = project.Item1;
          }
          projectName = new FileInfo(project.Item1).Name;
          recentProjects.Add(new RecentProject() { Path = projectPath, Name = projectName });
        }
        var projectCollection = new Collection<RecentProject>(recentProjects);
        if (projectCollection.Count == 1) _selectedProjectFile = projectCollection[0];
        return projectCollection;
      }
    }

    private RecentProject _selectedProjectFile;
    /// <summary>
    /// ArcGIS Pro project file is selected and will be opened.
    /// </summary>
    public RecentProject SelectedProjectFile
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
        return new RelayCommand((args) => OpenSelectedProjectAsync(), () => SelectedProjectFile != null);
      }
    }
    /// <summary>
    /// Open selected project.
    /// </summary>
    /// <remarks>
    /// If project is not on disk, you will be prompted to remove the project from the list of recent projects.
    /// </remarks>
    private async Task OpenSelectedProjectAsync()
    {

      string docVer = string.Empty;
      #region Workflow to open an ArcGIS Pro project
      //Check if the project can be opened
      if (Project.CanOpen(SelectedProjectFile.Path, out docVer))
      {
        //Open the project
        await Project.OpenAsync(SelectedProjectFile.Path);
      }
      else //The project cannot be opened
      {
        //One possible reason: If the project is a portal project, the active portal must match the portal of the project
        //Check if this is a portal project
        bool isPortalProject = Uri.TryCreate(SelectedProjectFile.Path, UriKind.Absolute, out Uri uriResult)
             && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

        if (isPortalProject)
        {
          //Parse the project path to get the portal
          var uri = new Uri(SelectedProjectFile.Path);
          var portalUrlOfProjectToOpen = $"{uri.Scheme}://{uri.Host}/portal/";

          //Get the current active portal
          var activePortal = ArcGIS.Desktop.Core.ArcGISPortalManager.Current.GetActivePortal();
          //Compare to see if the active Portal is the same as the portal of the project
          bool isSamePortal = (activePortal != null && activePortal.PortalUri.ToString() == portalUrlOfProjectToOpen);
          if (!isSamePortal) //not the same. 
          {
            //Set new active portal to be the portal of the project
            #region Set the project's portal to be the active portal 
            //Find the portal to sign in with using its Uri...
            var projectPortal = ArcGISPortalManager.Current.GetPortal(new Uri(portalUrlOfProjectToOpen, UriKind.Absolute));
            await QueuedTask.Run(() => {
              if (!projectPortal.IsSignedOn())
              {
                //Calling "SignIn" will trigger the OAuth popup if your credentials are
                //not cached (eg from a previous sign in in the session)
                if (projectPortal.SignIn().success)
                {
                  //Set this portal as my active portal
                  ArcGISPortalManager.Current.SetActivePortal(projectPortal);
                  return;
                }
              }
              //Set this portal as my active portal
              ArcGISPortalManager.Current.SetActivePortal(projectPortal);
            });
            #endregion
            //Now try opening the project again
            if (Project.CanOpen(SelectedProjectFile.Path, out docVer))
            {
              await Project.OpenAsync(SelectedProjectFile.Path);
            }
            else
            {
              System.Diagnostics.Debug.WriteLine("The project cannot be opened.");
            }
          }
          else //The portals are the same. So the problem could be something else - permissions, portal is down?
          {
            System.Diagnostics.Debug.WriteLine("The project cannot be opened.");
            var remove = MessageBox.Show($"Cannot open {SelectedProjectFile.Path}. Project does not exist. Do you want to remove the project shortcut from your list of recent projects?", "Project not found", System.Windows.MessageBoxButton.YesNo);
            if (remove == System.Windows.MessageBoxResult.Yes)
            {
              Project.RemoveRecentProject(SelectedProjectFile.Path);
              NotifyPropertyChanged(nameof(ProProjects));
            }
            return;
          }
        }
        else //Project is on disk and cannot be opened. 
        {
          System.Diagnostics.Debug.WriteLine("The project cannot be opened.");
          var remove = MessageBox.Show($"Cannot open {SelectedProjectFile.Path}. Project does not exist. Do you want to remove the project shortcut from your list of recent projects?", "Project not found", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
          if (remove == System.Windows.MessageBoxResult.Yes)
          {
            Project.RemoveRecentProject(SelectedProjectFile.Path);
            NotifyPropertyChanged(nameof(ProProjects));
          }
          return;
        }
      }
      #endregion
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
      BrowseProjectFilter portalAndLocalProjectsFilter = new BrowseProjectFilter();
      //A filter to pick projects from the portal
      //This filter will allow selection of ppkx and portal project items on the portal
      portalAndLocalProjectsFilter.AddFilter(BrowseProjectFilter.GetFilter("esri_browseDialogFilters_projects_online_proprojects"));
      //A filter to pick projects from the local machine
      portalAndLocalProjectsFilter.AddFilter(BrowseProjectFilter.GetFilter("esri_browseDialogFilters_projects"));
      //Create the OpenItemDialog and set the filter to the one we just created
      var openDlg = new OpenItemDialog()
      {
        Title = "Select a Project",
        MultiSelect = false,
        BrowseFilter = portalAndLocalProjectsFilter,
        InitialLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"ArcGIS\Projects")
      };
      //Show the dialog
      //Check if the user clicked OK and selected an item
      bool? ok = openDlg.ShowDialog();
      if (!ok.HasValue || openDlg.Items.Count() == 0)
        return; //nothing selected
      var selectedItem = openDlg.Items.FirstOrDefault();
      //Open the project use the OpenAsync method.
      try
      {
        if (Project.CanOpen(selectedItem.Path, out string docVer))
        {
          //Open the project
          Project.OpenAsync(selectedItem.Path);
        }
        else
        {
          System.Diagnostics.Debug.WriteLine("The project cannot be opened.");
          MessageBox.Show($"Cannot open {selectedItem.Path}. Project does not exist. Do you want to remove the project shortcut from your list of recent projects?", "Project not found", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
        }
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine(ex.Message);
        //Handle the exception
        MessageBox.Show($"Error opening project: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
      }

    }

    internal static ICommand OpenProjectCommand
    {
      get
      {

        return new RelayCommand((args) => BrowseToProject(), () => true); ;
      }
    }
  }
  #endregion
}
