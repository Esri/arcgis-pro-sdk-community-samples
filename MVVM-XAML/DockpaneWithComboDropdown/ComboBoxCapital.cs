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
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DockpaneWithComboDropdown
{
  /// <summary>
  /// Represents the ComboBox
  /// </summary>
  internal class ComboBoxCapital : ComboBox
  {

    private bool _isInitialized;

    /// <summary>
    /// Combo Box constructor
    /// </summary>
    public ComboBoxCapital()
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
        // fill the comboBox with items
        foreach (var capital in DockpaneComboViewModel.CapitalList)
        {
          Add(new ComboBoxItem(capital));
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

      MessageBox.Show($"You selected {item.Text} as the capital.");
    }

  }
}
