//   Copyright 2016 Esri
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using  ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.AddIns;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;
using System.Drawing;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Media.Imaging;
using ArcGIS.Desktop.Framework;
using System.Windows.Input;

namespace RemoveAddins
{
  /// <summary>
  /// Remove Add-in Backstage tab.
  /// </summary>
  internal class SampleBackstageTabViewModel : BackstageTab
  {
    public SampleBackstageTabViewModel()
    {
        //Get the path to the Add-In folders.
        RetrieveAddInPaths();
        SelectedAddInPath = _allAddInPaths[0];
        _deleteAddInsCommand = new RelayCommand(() => this.Del_AddIns(), () => CanDelete());
    }


    /// <summary>
    /// Initializes the tab.  
    /// </summary>
    /// <returns>A Task that represents InitializeAsync </returns>
    protected override async Task InitializeAsync()
    {
      await base.InitializeAsync();
    }

    /// <summary>
    ///  Uninitializes the tab. 
    /// </summary>
    /// <returns>A Task that represents UninitializeAsync</returns>
    protected override Task UninitializeAsync()
    {
      return base.UninitializeAsync();
    }

     #region Properties
    
    private ObservableCollection<FileInfo> _allAddInPaths = new ObservableCollection<FileInfo>();
    /// <summary>
    /// Collection of all well-known Add-in Folder paths.  Bind to this property in the view. 
    /// </summary>
    public ObservableCollection<FileInfo> AllAddInPaths
    {
        get { return _allAddInPaths; }
    }

    private FileInfo _selectedAddInPath;
    /// <summary>
    /// Holds the Selected add-in path
    /// </summary>
    public FileInfo SelectedAddInPath
    {
        get { return _selectedAddInPath; }
        set
        {
            SetProperty(ref _selectedAddInPath, value, () => SelectedAddInPath);
            if (_selectedAddInPath == null) return;
            //clear the AddIns Collection first
            AddIns.Clear();

            //Get the Add-ins for that path
            ProcessDirectory(value.FullName);

            NotifyPropertyChanged(() => SelectedAddInPath);
            NotifyPropertyChanged(() => AddIns);
        }
    }

    private ObservableCollection<AddInFileInfo> _AddIns = new ObservableCollection<AddInFileInfo>();
      /// <summary>
      /// Collection of all Add-ins in that seleted well-known folder
      /// </summary>
    public ObservableCollection<AddInFileInfo> AddIns
    {
        set {
            SetProperty(ref _AddIns, value, () => AddIns);
            NotifyPropertyChanged(() => AddIns);
        }
        
        get { return _AddIns; }
       
    }
  
    /// <summary>
    /// Indicates if an add-in can be deleted.
    /// </summary>
    private bool CanDelete() 
    {
        bool isAddInSelected = false;
        if (AddIns.Count == 0)
            return isAddInSelected;

        foreach (var addin in AddIns)
        {
            if (addin.IsSelected)
            {
                isAddInSelected = true;
                break;
            }
        }
        return isAddInSelected;
    }

    #endregion

    
    private RelayCommand _deleteAddInsCommand;
    /// <summary>
    /// Implement a RelayCommand to Delete the Addins
    /// </summary>
    public ICommand DeleteAddInsCommand
    {
        get
        {            
            return _deleteAddInsCommand;
        }
    }

   internal void Del_AddIns()
    {
        if (AddIns.Count == 0)
            return;
       foreach (var addin in AddIns)
          {
              if (addin.IsSelected)
              {
                  //MessageBox.Show(addin.AddInFullPath);
                  File.Delete(addin.AddInFullPath);
                  addin.IsSelected = false;
              }
          }
       //Clear the add in collection and then repopulate it. 
       _AddIns.Clear();
       ProcessDirectory(SelectedAddInPath.FullName); //re-populate
       NotifyPropertyChanged(() => AddIns); //notify collection changed.

    }

    private static readonly string AddInSubFolderPath = @"ArcGIS\AddIns\ArcGISPro";   
    private void RetrieveAddInPaths()
    {                
        string defaultAddInPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), AddInSubFolderPath);        
        List<string> AddInsPaths = Utils.GetAddInFolders();
        if (AddInsPaths.Count > 0)
        {
            foreach (string s in AddInsPaths)
            _allAddInPaths.Add(new FileInfo(s));
        }

        _allAddInPaths.Add(new FileInfo(defaultAddInPath));
        

    }

      /// <summary>
      /// Process all files in the directory passed in, recurse on any directories
      /// that are found, and process the files they contain. 
      /// </summary>
      /// <param name="targetDirectory"></param>
      private void ProcessDirectory(string targetDirectory)
    {
        
        // Process the list of files found in the directory. 
        string[] fileEntries = Directory.GetFiles(targetDirectory);
        foreach (string fileName in fileEntries)
            ProcessFile(fileName);

        // Recurse into subdirectories of this directory. 
        string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
        foreach (string subdirectory in subdirectoryEntries)
            ProcessDirectory(subdirectory);
    }


      private void ProcessFile(string path)
    {
        if (Path.GetExtension(path) == ".esriAddInX")
        {
            AddInFileInfo afi = new AddInFileInfo(path);            
            _AddIns.Add(afi);
        }
        else
            return;
    }
  }
}
