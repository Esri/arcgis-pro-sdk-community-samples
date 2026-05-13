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

using ArcGIS.Core.Internal.CIM;
using System.Collections.Generic;

namespace DDLCreateFeatureClassCHUI.ViewModel
{

  /// <summary>
  /// This class represents the second step in the wizard for creating feature classes.
  /// </summary>
  public class Step1ViewModel : PropertyChangedBase, StepViewModelBase
  {
    private string _name;
    public string Name { 
      get => _name;
      set
      {
        _name = value;
        IsValid = !string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(FeatureClassType);
      }
    }
    public string Alias { get; set; }
    
    private string _featureClassType;
    public string FeatureClassType
    {
      get => _featureClassType;
      set
      {
        _featureClassType = value;
        IsValid = !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(value);
      }
    }
    public List<string> FeatureClassTypes { get; } = new List<string> { "Polygon", "Point", "Polyline" };
    public bool MValues { get; set; }
    public bool ZValues { get; set; }

    private bool _isValid = false;
    public bool IsValid
    {
      get => _isValid;
      set
      {
        SetProperty(ref _isValid, value);
        MainWindowVM.currentMainWindow.CurrentStepIsValid = IsValid; // Update the CurrentStepIsValid property in the main window view model
      }
    }
  }
}