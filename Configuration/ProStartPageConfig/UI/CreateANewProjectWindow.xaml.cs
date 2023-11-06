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
using ProStartPageConfig.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProStartPageConfig.UI
{
  /// <summary>
  /// Interaction logic for CreateANewProjectWindow.xaml
  /// </summary>
  public partial class CreateANewProjectWindow : ArcGIS.Desktop.Framework.Controls.ProWindow, INotifyPropertyChanged
  {

    public event PropertyChangedEventHandler PropertyChanged = delegate { };

    public CreateANewProjectWindow()
    {
      InitializeComponent();
      (this.WindowContent as FrameworkElement).DataContext = this;
    }

    private string _projectName = "";

    public string ProjectName
    {
      get
      {
        if (string.IsNullOrEmpty(_projectName))
          _projectName = ProStartPageHelper.NextProjectName;
        return _projectName;
      }
      set
      {
        _projectName = value;
        NotifyPropertyChanged();
      }
    }

    private string _projectLocation = "";
    private bool _initProjLocationSetting = true;

    public string ProjectLocation
    {
      get
      {
        if (_initProjLocationSetting)
          _projectLocation = ProStartPageHelper.InitialLocation;
        _initProjLocationSetting = false;
        return _projectLocation;
      }
      set
      {
        _projectLocation = value;
        NotifyPropertyChanged();
      }
    }

    private bool _createFolder = true;
    private bool _initCreateSetting = true;

    public bool CreateFolderForProject
    {
      get
      {
        if (_initCreateSetting)
        {
          _initCreateSetting = false;
          _createFolder = ApplicationOptions.GeneralOptions.ProjectCreateInFolder;
        }
        return _createFolder;
      }
      set
      {
        _createFolder = value;
        NotifyPropertyChanged();
      }
    }

    private ICommand _browseForFolderCmd = null;

    public ICommand BrowseProjectLocationCmd
    {
      get
      {
        if (_browseForFolderCmd == null)
        {
          _browseForFolderCmd = new RelayCommand(() =>
          {
            var dlg = new OpenItemDialog();


            dlg.Title = "New Project Location";
            dlg.InitialLocation = ProStartPageHelper.InitialLocation;
            dlg.Filter = ArcGIS.Desktop.Catalog.ItemFilters.Folders;

            if (dlg.ShowDialog() ?? false)
            {
              var item = dlg.Items.FirstOrDefault();
              if (item != null)
              {
                this.ProjectLocation = item.Path;
              }
            }
          });
        }
        return _browseForFolderCmd;
      }
    }

    private ICommand _okCmd = null;

    public ICommand OkCmd
    {
      get
      {
        _okCmd = new RelayCommand(() =>
        {
          this.DialogResult = true;
          Validate();
          this.Close();
        });
        return _okCmd;
      }
    }

    public ImageSource MoveFolderImage => ProImageProvider.Instance["MoveFromFolder16"];

    private void Validate()
    {
      //make sure we do have a good name + location if the user
      //clicks ok and name or location were blanked out
      if (string.IsNullOrEmpty(this.ProjectLocation))
      {
        this.ProjectLocation = ProStartPageHelper.InitialLocation;
      }

      if (string.IsNullOrEmpty(this.ProjectName))
      {
        this.ProjectName = ProStartPageHelper.GetNextProjectName("",
                                                  this.ProjectLocation);
      }

    }

    private void NotifyPropertyChanged(
      [System.Runtime.CompilerServices.CallerMemberName] string name = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
  }
}
