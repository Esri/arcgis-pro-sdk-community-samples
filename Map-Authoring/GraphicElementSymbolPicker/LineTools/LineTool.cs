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

namespace GraphicElementSymbolPicker.LineTools
{
  internal class LineTool : LayoutTool
  {
    public LineTool()
    {
      SketchType = SketchGeometryType.Line;
    }
    protected override Task OnToolActivateAsync(bool active)
    {
      
      //When tool activates, set the geometry type, clear the gallery, and re-populate with relevant symbols
      //if (Module1.ActiveToolGeometry != Module1.ToolType.Line)
      //{
      //  Module1.ActiveToolGeometry = Module1.ToolType.Line;
      //  //Populate with the symbols
      //  Module1.GallerySymbolItems.Clear();
      //   foreach (var lineSymbol in Module1.LineGallerySymbolItems)
      //  {
      //    Module1.GallerySymbolItems.Add(lineSymbol);
      //  }
      //}
      return base.OnToolActivateAsync(active);
    }

    protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      if (ActiveElementContainer == null)
        Task.FromResult(true);

      if (Module1.SelectedSymbol == null) return Task.FromResult(true);
      return QueuedTask.Run(() =>
      {

        var cimGraphicElement = new CIMLineGraphic
        {
          Line = geometry as Polyline,
          Symbol = Module1.SelectedSymbol.MakeSymbolReference()
        };
        LayoutElementFactory.Instance.CreateGraphicElement(this.ActiveElementContainer, cimGraphicElement);
        return true;
      });
    }
  }
}
