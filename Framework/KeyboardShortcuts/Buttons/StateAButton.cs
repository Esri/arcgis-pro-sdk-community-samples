//   Copyright 2023 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

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

namespace KeyboardShortcuts.Buttons
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

      if (activePane.State.Contains("StateA") && activePane.ID == "KeyboardShortcuts_Panes_SamplePane")
      {
        this.IsChecked = true;
      }
      else if (!activePane.State.Contains("StateA") && activePane.ID == "KeyboardShortcuts_Panes_SamplePane")
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

      if (activePane.State.Contains("StateA") && activePane.ID == "KeyboardShortcuts_Panes_SamplePane")
      {
        activePane.State.Deactivate("StateA");
      }
      else if (!activePane.State.Contains("StateA") && activePane.ID == "KeyboardShortcuts_Panes_SamplePane")
      {
        activePane.State.Activate("StateA");
      }
      else
      {
        MessageBox.Show("Sample Pane is not active");
      }
    }
  }
}
