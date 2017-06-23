// Copyright 2017 Esri 
//
// 
//   Licensed under the Apache License, Version 2.0 (the "License"); 
//   you may not use this file except in compliance with the License. 
//   You may obtain a copy of the License at 
//
//       http://www.apache.org/licenses/LICENSE-2.0 
//
//   Unless required by applicable law or agreed to in writing, software 
//   distributed under the License is distributed on an "AS IS" BASIS, 
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//   See the License for the specific language governing permissions and 
//   limitations under the License. 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;


namespace RibbonControls
{
  /// <summary>
  /// Represents the ComboBox
  /// </summary>
  internal class ComboBox1 : ComboBox
  {

    private bool _isInitialized;

    /// <summary>
    /// Combo Box constructor
    /// </summary>
    public ComboBox1()
    {
      UpdateCombo();
    }

    /// <summary>
    /// Updates the combo box with all the items.
    /// </summary>

    private void UpdateCombo()
    {
      // TODO – customize this method to populate the combobox with your desired items  
      if (_isInitialized)
        SelectedItem = ItemCollection.FirstOrDefault(); //set the default item in the comboBox


      if (!_isInitialized)
      {
        Clear();

        //Add 6 items to the combobox
        for (int i = 0; i < 6; i++)
        {
          string name = string.Format("Item {0}", i);
          Add(new ComboBoxItem(name));
        }
        _isInitialized = true;
      }


      Enabled = true; //enables the ComboBox
      SelectedItem = ItemCollection.FirstOrDefault(); //set the default item in the comboBox

    }

    /// <summary>
    /// The on comboBox selection change event. 
    /// </summary>
    /// <param name="item">The newly selected combo box item</param>
    protected override void OnSelectionChange(ComboBoxItem item)
    {

      if (item == null)
        return;

      if (string.IsNullOrEmpty(item.Text))
        return;

      // TODO  Code behavior when selection changes.    
    }

  }
}
