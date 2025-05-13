/*

   Copyright 2025 Esri

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
using ArcGIS.Desktop.Internal.Mapping.Ribbon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPToolInspector.TreeHelpers
{
  internal class SearchWithDropDownViewModel : PropertyChangedBase
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public SearchWithDropDownViewModel()
    {
      TextList.Add("Item 1");
      TextList.Add("Item 2");

    }





    #region Properties

    /// <summary>
    /// Search Text
    /// </summary>
    private string _SearchText = "";
    public string SearchText
    {
      get => _SearchText;
      set {
        SetProperty(ref _SearchText, value);
      }
    }

    private ObservableCollection<string> _TextList = [];
    public ObservableCollection<string> TextList
    {
      get => _TextList;
      set {
        SetProperty(ref _TextList, value);
      }
    }

    private string _SelectedItem = null;
    public string SelectedItem
    {
      get => _SelectedItem;
      set {
        SetProperty(ref _SelectedItem, value);
      }
    }

    #endregion
  }
}
