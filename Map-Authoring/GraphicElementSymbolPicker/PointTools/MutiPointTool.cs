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

namespace GraphicElementSymbolPicker.PointTools
{
  internal class MutiPointTool : LayoutTool
  {
    public MutiPointTool()
    {
      SketchType = SketchGeometryType.Multipoint;
    }
    protected override Task OnToolActivateAsync(bool active)
    {
      return base.OnToolActivateAsync(active);
    }
    protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      //var targetGraphicLayers = MapView.Active?.Map.TargetGraphicsLayer;
      //if (targetGraphicLayers == null) Task.FromResult(true);
      if (ActiveElementContainer == null)
        Task.FromResult(true);

      if (Module1.SelectedSymbol == null) return Task.FromResult(true);
      return QueuedTask.Run(() =>
      {

        var cimGraphicElement = new CIMMultipointGraphic
        {
          Multipoint = geometry as Multipoint,
          Symbol = Module1.SelectedSymbol.MakeSymbolReference()
        };
        LayoutElementFactory.Instance.CreateGraphicElement(this.ActiveElementContainer, cimGraphicElement);
        return true;
      });
    }
  }
}
