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
using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LASDatasetAPISamples.FilterSettings
{
  public class CustomItemForCombo : PropertyChangedBase
{
    public CustomItemForCombo(string itemName, bool isChecked)
    {
      Name = itemName;
      //_isChecked = isSelected;
      IsChecked = isChecked;
    }
    private string _name;
    public string Name 
    { get {
        return _name;
      }  
      set
      {
        SetProperty(ref _name, value, () => Name);
        NotifyPropertyChanged(nameof(Name));
      }
    }
    private bool _isChecked;
    public bool IsChecked {
      get {
      return _isChecked;
      }
      set {
        SetProperty(ref _isChecked, value, () => IsChecked);
        NotifyPropertyChanged(nameof(IsChecked));
      }
    
    }
  }
}
