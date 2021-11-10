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
using System.Windows.Input;

namespace EditDiagramJunctionRotation
{
  /// <summary>
  /// Represents the ComboBox
  /// </summary>
  internal class EditRotation : EditBox
  {
    public EditRotation()
    {
      Text = string.Empty;
    }

    /// <summary>
    /// Validate the rotation value when the object lost focus
    /// </summary>
    /// <param name="e">KeyboardFocusChangedEventArgs</param>
    protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
      string newValue = Text;
      if (string.IsNullOrEmpty(newValue))
        EditDiagramJunctionRotationModule.Rotation = 0.0;
      else if (double.TryParse(newValue, out var val))
      {
        EditDiagramJunctionRotationModule.Rotation = val % 360.0;
        Text = EditDiagramJunctionRotationModule.Rotation.ToString("0.00");
        NotifyPropertyChanged("Text");
      }
    }
  }

}
