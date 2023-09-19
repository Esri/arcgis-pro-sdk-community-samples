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
  internal class StateAButton : Button
  {
    protected override void OnUpdate()
    {
      Pane activePane = FrameworkApplication.Panes.ActivePane;

      if (activePane == null) return;

      if (activePane.State.Contains("StateA"))
        this.IsChecked = true;
      else
        this.IsChecked = false;
    }
    protected override void OnClick()
    {
      Pane activePane = FrameworkApplication.Panes.ActivePane;

      if (activePane == null) return;

      if (activePane.State.Contains("StateA"))
      {
        activePane.State.Deactivate("StateA");
      }
      else
      {
        activePane.State.Activate("StateA");
      }
    }
  }
}
