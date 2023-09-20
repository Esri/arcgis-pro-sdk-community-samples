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
  /// This button helps to illustrate condition. In DAML, it is only enabled when
  /// Condition A is met. If Condition A is met, then the conditional shortcut
  /// may also be invoked.
  /// </summary>
  internal class ConditionAButton : Button
  {
    protected override void OnClick()
    {
      MessageBox.Show("Condition A is met.");
    }
  }
}
