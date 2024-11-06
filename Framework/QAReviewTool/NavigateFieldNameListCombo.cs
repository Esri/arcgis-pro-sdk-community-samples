/*

   Copyright 2019 Esri

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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;



namespace QAReviewTool
{
  /// <summary>
  /// Represents the ComboBox
  /// </summary>
  internal class NavigateFieldNameListCombo : ComboBox
  {
    /// <summary>
    /// Combo Box constructor
    /// </summary>
    public NavigateFieldNameListCombo()
    {
      Module1.Current.LayerFieldComboBox = this;
    }

    internal void AddItem(object comboitem)
    {
      Add(comboitem);
    }

    internal void ClearItems()
    {
      Clear();
    }

    private string _previousSelectedFieldName = string.Empty;

    /// <summary>
    /// The on comboBox selection change event. 
    /// </summary>
    /// <param name="item">The newly selected combo box item</param>
    protected override async void OnSelectionChange(ComboBoxItem item)
    {
      if (item == null)
        return;

      if (string.IsNullOrEmpty(item.Text))
      {
        Module1.Current.FieldValueComboBox.ClearItems();
        return;
      }
      if (item.Text == _previousSelectedFieldName)
        return;
      _previousSelectedFieldName = item.Text;

      // only refresh the field value combo box if it is empty
      if (Module1.Current.FieldValueComboBox.ItemCollection.Count == 0)
      {
        var result = await Module1.Current.RefreshFieldValueComboBoxAsync(item.Text);
        if (!string.IsNullOrEmpty(result))
          MessageBox.Show(result, "Error occurred", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
      }
    }

  }
}
