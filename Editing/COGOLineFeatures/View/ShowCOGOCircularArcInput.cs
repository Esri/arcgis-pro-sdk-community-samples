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

namespace COGOLineFeatures.View
{
  internal class ShowCOGOCircularArcInput : Button
  {

    private COGOCircularArcInput _cogocirculararcinput = null;

    protected override void OnClick()
    {
      //already open?
      if (_cogocirculararcinput != null)
        return;
      _cogocirculararcinput = new COGOCircularArcInput();
      _cogocirculararcinput.Owner = FrameworkApplication.Current.MainWindow;
      _cogocirculararcinput.Closed += (o, e) => { _cogocirculararcinput = null; };
      _cogocirculararcinput.Show();
      //uncomment for modal
      //_cogocirculararcinput.ShowDialog();
    }

  }
}
