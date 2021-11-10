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

namespace GraphicElementSymbolPicker.TextTools
{
  internal class CurvedTextTool : LayoutTool
  {
    public CurvedTextTool()
    {
      SketchType = SketchGeometryType.BezierLine;
    }
    protected override Task OnToolActivateAsync(bool active)
    {
      return base.OnToolActivateAsync(active);
    }
    protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      if (ActiveElementContainer == null)
        Task.FromResult(true);

      if (Module1.SelectedSymbol == null) return Task.FromResult(true);

      return QueuedTask.Run(() =>
      {

        var cimGraphicElement = new CIMTextGraphic
        {
          Shape = geometry as Polyline,
          Text = "Text",
          Symbol = Module1.SelectedSymbol.MakeSymbolReference()
        };
        LayoutElementFactory.Instance.CreateGraphicElement(this.ActiveElementContainer, cimGraphicElement);
        //The new text graphic element has been created
        //We now switch to Pro's out of box "esri_layouts_inlineEditTool" 
        //This will allow inline editing of the text 
        //This tool will work on graphics on both map view and layouts
        FrameworkApplication.SetCurrentToolAsync("esri_layouts_inlineEditTool");
        return true;
      });
    }
  }
}
