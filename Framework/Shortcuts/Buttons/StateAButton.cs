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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shortcuts.Buttons
{
  /// <summary>
  /// This button is used to set "StateA" state in this sample. The state value is
  /// used in the conditional shortcut example.
  /// </summary>
  internal class StateAButton : Button
  {
    // The below method highlights/unhighlights the State A Button depending on whether
    // State A and the Sample Pane are activated.
    protected override void OnUpdate()
    {
      Pane activePane = FrameworkApplication.Panes.ActivePane;

      if (activePane == null) return;

      if (activePane.State.Contains("StateA") && activePane.ID == "Shortcuts_Panes_SamplePane")
      {
        this.IsChecked = true;
      }
      else if (!activePane.State.Contains("StateA") && activePane.ID == "Shortcuts_Panes_SamplePane")
      {
        this.IsChecked = false;
      }
    }
    // The below method activates/deactivates "StateA" on click depending on whether
    // State A is already activated and the Sample Pane is activated.
    protected override void OnClick()
    {
      Pane activePane = FrameworkApplication.Panes.ActivePane;

      //if (activePane == null) MessageBox.Show("SamplePane is not active");

      if (activePane.State.Contains("StateA") && activePane.ID == "Shortcuts_Panes_SamplePane")
      {
        activePane.State.Deactivate("StateA");
      }
      else if (!activePane.State.Contains("StateA") && activePane.ID == "Shortcuts_Panes_SamplePane")
      {
        activePane.State.Activate("StateA");
      }
      else
      {
        MessageBox.Show("SamplePane is not active");
      }
    }
  }
}
