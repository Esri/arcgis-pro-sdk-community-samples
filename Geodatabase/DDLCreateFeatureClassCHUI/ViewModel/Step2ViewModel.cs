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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace DDLCreateFeatureClassCHUI.ViewModel
{
  public class FieldDefinition
  {
    public string FieldName { get; set; }
    public string DataType { get; set; }
  }

  /// <summary>
  /// This class represents the second step in the wizard for creating feature classes.
  /// </summary>
  public class Step2ViewModel : PropertyChangedBase, StepViewModelBase
  {
    public ObservableCollection<FieldDefinition> Fields { get; set; } = new();

    private FieldDefinition _selectedField = null;
    public FieldDefinition SelectedField
    {
      get => _selectedField;
      set => SetProperty(ref _selectedField, value);
    }

    private string _newFieldName;
    public string NewFieldName
    {
      get => _newFieldName;
      set => SetProperty(ref _newFieldName, value);
    }

    private string _newDataType;
    public string NewDataType
    {
      get => _newDataType;
      set => SetProperty(ref _newDataType, value);
    }

    public List<string> DataTypes { get; } = new List<string> { "String", "Integer", "Double", "Date" };

    public ICommand AddFieldCommand { get; }
    public ICommand DeleteFieldCommand { get; }

    public Step2ViewModel()
    {
      AddFieldCommand = new RelayCommand(_ => AddField(), _ => !string.IsNullOrWhiteSpace(NewFieldName) && !string.IsNullOrWhiteSpace(NewDataType));
      DeleteFieldCommand = new RelayCommand(_ => DeleteField(), _ => _selectedField != null);
    }
    private void AddField()
    {
      Fields.Add(new FieldDefinition { FieldName = NewFieldName, DataType = NewDataType });
      NewFieldName = string.Empty;
      NewDataType = string.Empty;
      IsValid = Fields.Count > 0; // Update validity after adding a field
    }

    private void DeleteField()
    {
      var fieldToRemove = _selectedField;
      if (fieldToRemove != null)
      {
        Fields.Remove(fieldToRemove);
      }
      NewFieldName = string.Empty;
      NewDataType = string.Empty;
      IsValid = Fields.Count > 0; // Update validity after deleting a field
    }

    private bool _isValid = false;
    public bool IsValid
    {
      get => _isValid;
      set
      {
        SetProperty(ref _isValid, value);
        MainWindowVM.currentMainWindow.CurrentStepIsValid = IsValid;
      } 
    }
  }
}