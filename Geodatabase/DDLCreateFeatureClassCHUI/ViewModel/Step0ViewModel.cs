//   Copyright 2026 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.CIM;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Index = ArcGIS.Core.Data.Index;

namespace DDLCreateFeatureClassCHUI.ViewModel
{

  /// <summary>
  /// This class represents the first step in the wizard for creating feature classes.
  /// </summary>
  public class Step0ViewModel : PropertyChangedBase, StepViewModelBase
  {
    #region Private members

    private Datastore _datastore;
    private readonly object _lockCollection = new();
    private DatasetInfo _selectedDatasetInfo;
    private string _dataPath;
    private DatasetTypeCategory _datasetTypeCategory;
    private DatastoreCategory _datastoreCategory;
    private Visibility _cmdDataPathVisible = Visibility.Hidden;
    private string _cmdDataPathContent;
    private ICommand _cmdDataPath;
    private ICommand _cmdLoadData;

    //private readonly PropertyChangedBase prop = new();
    #endregion Private members

    public Step0ViewModel()
    {
      DatasetTypeCategories = [];
      BindingOperations.EnableCollectionSynchronization(DatasetTypeCategories, _lockCollection);
    }

    public static List<DatastoreCategory> DatastoreCategories => DatastoreCategory.AllDatastoreCategories;

    public DatastoreCategory DatastoreCategory
    {
      get { return _datastoreCategory; }
      set
      {
        SetProperty(ref _datastoreCategory, value);
        if (_datastoreCategory != null)
        {
          CmdDataPathVisible = string.IsNullOrEmpty(_datastoreCategory.PathCaption) ?
                                Visibility.Hidden : Visibility.Visible;
          CmdDataPathContent = _datastoreCategory.PathCaption;
        }
        else CmdDataPathVisible = Visibility.Hidden;
      }
    }

    public string DataPath
    {
      get { return _dataPath; }
      set
      {
        SetProperty(ref _dataPath, value);
      }
    }

    public Visibility CmdDataPathVisible
    {
      get { return _cmdDataPathVisible; }
      set
      {
        SetProperty(ref _cmdDataPathVisible, value);
      }
    }

    public string CmdDataPathContent
    {
      get { return _cmdDataPathContent; }
      set
      {
        SetProperty(ref _cmdDataPathContent, value);
      }
    }

    public ICommand CmdDataPath
    {
      get
      {
        return _cmdDataPath ??= new CommunityToolkit.Mvvm.Input.RelayCommand(() =>
        {
          switch (DatastoreCategory.Type)
          {
            case EnumDatastoreType.EnterpriseGDB:
              {
                OpenFileDialog openDialog = new()
                {
                  Title = DatastoreCategory.OpenDlgTitle,
                  Multiselect = false,
                  Filter = DatastoreCategory.OpenDlgFilter,
                  InitialDirectory = string.IsNullOrEmpty(DataPath) ? @"c:\data" : DataPath // updated from InitialLocation to InitialDirectory
                };
                if (openDialog.ShowDialog() == true)
                {
                  foreach (string filename in openDialog.FileNames)
                  {

                    var errValidation = DatastoreCategory.ValidateDataPath(filename); // updated from 'item.' to 'filename'
                    if (!string.IsNullOrEmpty(errValidation)) MessageBox.Show(errValidation, "Error");
                    else DataPath = filename; // updated from 'item' to 'filename'
                    break;
                  }
                }
              }
              break;
            case EnumDatastoreType.FileGDB:
              {
                OpenFolderDialog openFolderDialog = new()
                {
                  Title = DatastoreCategory.OpenDlgTitle,
                  InitialDirectory = string.IsNullOrEmpty(DataPath) ? @"c:\data" : DataPath // updated from InitialLocation to InitialDirectory
                };
                if (openFolderDialog.ShowDialog() == true)
                {
                  foreach (string foldername in openFolderDialog.FolderNames)
                  {
                    var errValidation = DatastoreCategory.ValidateDataPath(foldername);
                    if (!string.IsNullOrEmpty(errValidation))
                      MessageBox.Show(errValidation, "Error");
                    else
                      DataPath = foldername;
                    break;
                  }
                }
              }
              break;
            default:
              break;
          }
          IsValid = !string.IsNullOrWhiteSpace(DataPath) && DatastoreCategory != null;

        }, () => true);

      }
    }

    public ObservableCollection<DatasetTypeCategory> DatasetTypeCategories { get; set; }

    public DatasetTypeCategory DatasetTypeCategory
    {
      get
      { return _datasetTypeCategory; }
      set
      {
        SetProperty(ref _datasetTypeCategory, value);
        IsValid = !string.IsNullOrWhiteSpace(DataPath) && DatastoreCategory != null;
        MainWindowVM.currentMainWindow.CurrentStepIsValid = IsValid; // Update the CurrentStepIsValid property in the main window view model
      }
    }

    private bool _isValid = false;
    public bool IsValid
    {
      get => _isValid;
      set
      {
        SetProperty(ref _isValid, value);
        MainWindowVM.currentMainWindow.CanGoNext = IsValid; // Update the CanGoNext property in the main window view model
      }
    }
  }
}
