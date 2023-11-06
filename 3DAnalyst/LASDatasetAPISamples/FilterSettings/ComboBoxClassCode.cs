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
using ActiproSoftware.Windows.Extensions;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.Analyst3D;
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
  internal class ComboBoxClassCode : ComboBox
  {

    private bool _isInitialized;
    private List<int> _classCodesInLASDataset = new List<int>();
    /// <summary>
    /// Combo Box constructor
    /// </summary>
    public ComboBoxClassCode()
    {
      UpdateCombo();
    }

    /// <summary>
    /// Updates the combo box with all the items.
    /// </summary>

    internal void UpdateCombo()
    {
      
      //if (_isInitialized)
      //{
        
      //  var indexFirstItemChecked = Module1.Current.CustomItemsClassCodes.IndexOf(Module1.Current.CustomItemsClassCodes.FirstOrDefault(x => x.IsChecked == true));
      //  if (indexFirstItemChecked == -1)
      //    SelectedItem = ItemCollection.FirstOrDefault(); //set the default item in the comboBox
      //  else
      //    SelectedItem = ItemCollection[indexFirstItemChecked];
      //}

      if (!_isInitialized)
      {
        Clear();

        //ItemCollection.AddRange(Module1.Current.CustomItemsClassCodes);
        ////Add items to the combobox
        foreach (var item in Module1.Current.CustomItemsClassCodes)
        {
          Add(item);
          item.PropertyChanged += CustomComboBoxItem_PropertyChanged;
        }
        _isInitialized = true;
      }
      Enabled = true; //enables the ComboBox
      var indexFirstItemChecked = Module1.Current.CustomItemsClassCodes.IndexOf(Module1.Current.CustomItemsClassCodes.FirstOrDefault(x => x.IsChecked == true));
      if (indexFirstItemChecked == -1)
        SelectedItem = ItemCollection.FirstOrDefault(); //set the default item in the comboBox
      else
        SelectedItem = ItemCollection[indexFirstItemChecked];
    }

    private void CustomComboBoxItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      var comboItem = sender as CustomItemForCombo;
      if (comboItem == null || e.PropertyName != "IsChecked")
        return;

      int index = -1;

      if (comboItem.IsChecked)
      {
        index = Module1.Current._selectedClassCodes.IndexOf(Int32.Parse(comboItem.Name));
        if (index == -1) //list does not contain the item. This prevents duplicate items in the list
        {
          Module1.Current._selectedClassCodes.Add(Int32.Parse(comboItem.Name));
        }
        SelectedIndex = ItemCollection.IndexOf(comboItem);
      }
      else //Not selected
      {
        index = Module1.Current._selectedClassCodes.IndexOf(Int32.Parse(comboItem.Name));
        if (index != -1) //list contains the item
        {
          Module1.Current._selectedClassCodes.RemoveAt(index);
        }
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
