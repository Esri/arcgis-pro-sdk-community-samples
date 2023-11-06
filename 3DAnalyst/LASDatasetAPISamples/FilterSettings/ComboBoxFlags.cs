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
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LASDatasetAPISamples.FilterSettings
{
  /// <summary>
  /// Represents the ComboBox
  /// </summary>
  internal class ComboBoxFlags : ComboBox
  {

    private bool _isInitialized;

    private List<string> _flags = new List<string>()
    {
      "Key Points", "Overlap Points", "Synthetic Points", "Withheld Points"
    };

    /// <summary>
    /// Combo Box constructor
    /// </summary>
    public ComboBoxFlags()
    {
      UpdateCombo();
    }

    /// <summary>
    /// Updates the combo box with all the items.
    /// </summary>

    private void UpdateCombo()
    {
      if (_isInitialized)
        SelectedItem = ItemCollection.FirstOrDefault(); //set the default item in the comboBox


      if (!_isInitialized)
      {
        Clear();

        //Add flags
        foreach (var flag in _flags) 
        {
          var item = new CustomItemForCombo(flag, false);
          Add(item);
          item.PropertyChanged += OnItemPropertyChanged;  
        }

        _isInitialized = true;
      }


      Enabled = true; //enables the ComboBox
      SelectedItem = ItemCollection.FirstOrDefault(); //set the default item in the comboBox

    }

    private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      var comboItem = sender as CustomItemForCombo;
      if (comboItem == null || e.PropertyName != "IsChecked")
        return;

      if (comboItem.Name == "Key Points")
        Module1.Current._keyPoints = comboItem.IsChecked;
      if (comboItem.Name == "Overlap Points")
        Module1.Current._overlapPoints = comboItem.IsChecked;
      if (comboItem.Name == "Synthetic Points")
        Module1.Current._syntheticPoints = comboItem.IsChecked;
      if (comboItem.Name == "Withheld Points" )
        Module1.Current._withheldPoints = comboItem.IsChecked;

      if (comboItem.IsChecked)
      {
        SelectedIndex = ItemCollection.IndexOf(comboItem);
      }
     
    }

    /// <summary>
    /// The on comboBox selection change event. 
    /// </summary>
    /// <param name="item">The newly selected combo box item</param>
    protected override void OnSelectionChange(object item)
    {
     
    }

  }
}
