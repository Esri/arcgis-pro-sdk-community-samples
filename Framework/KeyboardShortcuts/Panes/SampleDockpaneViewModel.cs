//   Copyright 2023 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

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


namespace KeyboardShortcuts.Panes
{
  internal class SampleDockpaneViewModel : DockPane
  {
    private const string _dockPaneID = "KeyboardShortcuts_Panes_SampleDockpane";

    protected SampleDockpaneViewModel() { }

    /// <summary>
    /// Show the DockPane.
    /// </summary>
    internal static void Show()
    {
      DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
      if (pane == null)
        return;

      pane.Activate();
    }

    /// <summary>
    /// Text shown near the top of the DockPane.
    /// </summary>
    private string _heading = "Sample DockPane";
    public string Heading
    {
      get => _heading;
      set => SetProperty(ref _heading, value);
    }

    /// <summary>
    /// keyCommand implementation used in DockPane keyCommand
    /// example. A MessageBox is shown.
    /// </summary>
    /// <param name="commandID"></param>
    protected override void OnKeyCommand(string commandID)
    {
      switch (commandID)
      {
        case "Dock_Pane_Cmd":
          MessageBox.Show("Dockpane shortcut");
          break;
      }
    }
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class SampleDockpane_ShowButton : Button
  {
    protected override void OnClick()
    {
      SampleDockpaneViewModel.Show();
    }
  }
}
