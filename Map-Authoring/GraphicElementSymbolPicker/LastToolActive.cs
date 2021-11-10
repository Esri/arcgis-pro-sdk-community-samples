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

namespace GraphicElementSymbolPicker
{
  internal class LastToolActive : CheckBox
  {
    protected override void OnClick()
    {
      try
      {
        Module1.Current.LayoutOptions.KeepLastToolActive = !Module1.Current.LayoutOptions.KeepLastToolActive;        
      }
      catch (Exception e) { ArcGIS.Desktop.Framework.Utilities.EventLog.Write(ArcGIS.Desktop.Framework.Utilities.EventLog.EventType.Warning, "Save ApplicationSetttings.LastToolActive: " + e.Message); }
    }
    protected override void OnUpdate()
    {
      IsChecked = Module1.Current.LayoutOptions.KeepLastToolActive;
    }
  }
}
